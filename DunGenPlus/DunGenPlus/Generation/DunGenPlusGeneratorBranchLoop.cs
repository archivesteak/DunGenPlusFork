using DunGen;
using DunGenPlus.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace DunGenPlus.Generation
{
  internal partial class DunGenPlusGenerator {

    public static Stopwatch GenerateBranchBoostedPathsStopWatch = new Stopwatch();
    public static float GenerateBranchBoostedPathsTime = 0f;

    public static Stopwatch GetTileResultStopwatch = new Stopwatch();
    public static float GetTileResultTime = 0f;

    public static Stopwatch DoorwayPairStopwatch = new Stopwatch();
    public static float DoorwayPairTime = 0f;

    public static Stopwatch CalculateWeightStopwatch = new Stopwatch();
    public static float CalculateWeightTime = 0f;

    private class BranchPathProxy {
      public TileProxy mainPathTile;
      public int mainPathIndex;

      public List<TilePlacementResultProxy> list;
      public float weight;

      public Dictionary<TileProxy, InjectedTile> injectedTiles;
      public List<InjectedTile> tilesPendingInjection;

      public BranchPathProxy(DungeonGenerator gen, TileProxy attachTileProxy){
        mainPathTile = attachTileProxy;
        mainPathIndex = GetMainPathIndexFromTileProxy(attachTileProxy);

        list = new List<TilePlacementResultProxy>();
        weight = 0f;

        injectedTiles = new Dictionary<TileProxy, InjectedTile>(gen.injectedTiles);
        tilesPendingInjection = new List<InjectedTile>(gen.tilesPendingInjection);
      }

      public void CalculateWeight(DungeonGenerator gen){
        var count = list.Count;
        if (count == 0) return;

        var lastNode = list[count - 1];
        var simProp = Properties.BranchPathMultiSimulationProperties;
        var depth = lastNode.tileProxy.Placement.BranchDepth;
        var normalizedDepth = lastNode.tileProxy.Placement.NormalizedBranchDepth;

        weight += simProp.GetWeightBase(depth, normalizedDepth);
        var allDungeonDoorways = gen.proxyDungeon.AllTiles.SelectMany(t => t.Doorways);
        foreach(var t in list) {
          foreach(var d in allDungeonDoorways) {
            if (d.TileProxy == mainPathTile) continue;
            var dIndex = GetMainPathIndexFromTileProxy(d.TileProxy);

            foreach(var l in t.tileProxy.doorways) {
              if (d.TileProxy == t.previousDoorway.TileProxy) continue;

              // favor paths that connect to other depths
              if (gen.DungeonFlow.CanDoorwaysConnect(new ProposedConnection(gen.proxyDungeon, d.TileProxy, l.TileProxy, d, l)) && Vector3.SqrMagnitude(d.Position - l.Position) < 1E-05){
                var diff = Mathf.Abs(d.TileProxy.Placement.PathDepth - l.TileProxy.Placement.PathDepth);
                var normalDiff = Mathf.Abs(d.TileProxy.Placement.NormalizedPathDepth - l.TileProxy.Placement.NormalizedPathDepth);
                var samePath = mainPathIndex == dIndex;

                weight += simProp.GetWeightPathConnection(samePath, diff, normalDiff);
              }
            }
          }
        }

        //Plugin.logger.LogInfo($"Path({lastNode.tileProxy.Placement.NormalizedBranchDepth}): {weight}");
      }

    }

    private class TilePlacementResultProxy {
      public TilePlacementResult result;
      public TileProxy tileProxy;
      public DoorwayProxy previousDoorway;
      public DoorwayProxy nextDoorway;

      public TilePlacementResultProxy(TilePlacementResult result) {
        this.result = result;
        tileProxy = null;
        previousDoorway = null;
        nextDoorway = null;
      }

      public TilePlacementResultProxy(TilePlacementResult result, TileProxy tile, DoorwayProxy previousDoorway, DoorwayProxy nextDoorway) {
        this.result = result;
        this.tileProxy = tile;
        this.previousDoorway = previousDoorway;
        this.nextDoorway = nextDoorway;
      }

    }

    public static IEnumerator GenerateMultiBranchPaths(DungeonGenerator gen){
      gen.ChangeStatus(GenerationStatus.Branching);
      var mainPathBranches = new int[gen.proxyDungeon.MainPathTiles.Count];
      BranchCountHelper.ComputeBranchCounts(gen.DungeonFlow, gen.RandomStream, gen.proxyDungeon, ref mainPathBranches);

      // do all nodes on main path
      for(var b = 0; b < mainPathBranches.Length; ++b){
        var tile = gen.proxyDungeon.MainPathTiles[b];
        var branchCount = mainPathBranches[b];

        // skip if not branchable tile
        if (tile.Placement.Archetype == null || branchCount == 0) {
          continue;
        }

        // the amount of branches per tile
        for(var i = 0; i < branchCount; ++i){
          // create a bunch of proxy paths
          // we evaulate later on the best one
          var pathProxys = new List<BranchPathProxy>();
          //Plugin.logger.LogInfo("New Path");
          for(var x = 0; x < Properties.BranchPathMultiSimulationProperties.SimulationCount; ++x){
            var currentPathProxy = new BranchPathProxy(gen, tile);
            var previousTile = tile;
            var branchDepth = tile.Placement.Archetype.BranchingDepth.GetRandom(gen.RandomStream);
            for(var depth = 0; depth < branchDepth; ++depth){

              // get tilesets, branch cap or regular
              List<TileSet> useableTileSets;
              if (depth == branchDepth - 1 && tile.Placement.Archetype.GetHasValidBranchCapTiles()){
                if (tile.Placement.Archetype.BranchCapType == BranchCapType.InsteadOf) {
                  useableTileSets = tile.Placement.Archetype.BranchCapTileSets;
                } else {
                  useableTileSets = tile.Placement.Archetype.TileSets.Concat(tile.Placement.Archetype.BranchCapTileSets).ToList();
                }
              } else {
                useableTileSets = tile.Placement.Archetype.TileSets;
              }

              // get potential tile to add
              var normalizedDepth = (branchDepth <= 1) ? 1f : (float)depth / (branchDepth - 1);

              GetTileResultStopwatch.Reset();
              GetTileResultStopwatch.Start();
              var tileResult = GetTileResult(gen, currentPathProxy, previousTile, useableTileSets, normalizedDepth, tile.Placement.Archetype);
              GetTileResultStopwatch.Stop();
              GetTileResultTime += (float)GetTileResultStopwatch.Elapsed.TotalMilliseconds;

              var tileProxy = tileResult.tileProxy;
              if (tileProxy == null) {
                // it's over, we done
                break;
              }

              // add
              currentPathProxy.list.Add(tileResult);
              tileProxy.Placement.BranchDepth = depth;
              tileProxy.Placement.NormalizedBranchDepth = normalizedDepth;
              previousTile = tileProxy;
            }

            // we can't save this path
            if (currentPathProxy.list.Count == 0) break;

            // record path
            CalculateWeightStopwatch.Reset();
            CalculateWeightStopwatch.Start();
            currentPathProxy.CalculateWeight(gen);
            CalculateWeightStopwatch.Stop();
            CalculateWeightTime += (float)CalculateWeightStopwatch.Elapsed.TotalMilliseconds;

            pathProxys.Add(currentPathProxy);
          }

          // time to evaulate best path then add
          var bestPath = pathProxys.OrderByDescending(p => p.weight).FirstOrDefault();
          if (bestPath != null) {
            //Plugin.logger.LogInfo($"Best path: {bestPath.weight}");
            //Plugin.logger.LogInfo("");
            foreach(var item in bestPath.list){
              MakeTileProxyConnection(gen, item);

              item.tileProxy.Placement.PlacementParameters = new TilePlacementParameters {
                Archetype = item.tileProxy.Placement.PlacementParameters?.Archetype,
                Node = item.previousDoorway.TileProxy.Placement.GraphNode,
                Line = item.previousDoorway.TileProxy.Placement.GraphLine
              };
            }

            gen.injectedTiles.Clear();
            foreach (var __kv in bestPath.injectedTiles) gen.injectedTiles[__kv.Key] = __kv.Value;
            gen.tilesPendingInjection = bestPath.tilesPendingInjection;

            AddMainPathTileProxies(bestPath.list.Select(x => x.tileProxy), bestPath.mainPathIndex);

            if (gen.ShouldSkipFrame(true)){
              yield return gen.GetRoomPause();
            }
          }
        }

        // why null?
        tile = null;
      }

      yield break;

    }

    private static TilePlacementResultProxy GetTileResult(DungeonGenerator gen, BranchPathProxy pathProxy, TileProxy attachTo, IEnumerable<TileSet> useableTileSets, float normalizedDepth, DungeonArchetype archetype){
      // get tile injection
      InjectedTile injectedTile = null;
      var index = -1;
      if (pathProxy.tilesPendingInjection != null) {
        var pathDepth = (float)attachTo.Placement.PathDepth / gen.targetLength - 1f;
        var branchDepth = normalizedDepth;
        for(var i = 0; i < pathProxy.tilesPendingInjection.Count; ++i){
          var injectedTile2 = pathProxy.tilesPendingInjection[i];
          if (injectedTile2.ShouldInjectTileAtPoint(false, pathDepth, branchDepth)) {
            injectedTile = injectedTile2;
            index = i;
            break;
          }
        }
      }

      // get tiles to consider
      IEnumerable<GameObjectChance> collection;
      if (injectedTile != null) {
        collection = new List<GameObjectChance>(injectedTile.TileSet.TileWeights.Weights);
      } else {
        collection = useableTileSets.SelectMany(t => t.TileWeights.Weights);
      }

      DoorwayPairStopwatch.Reset();
      DoorwayPairStopwatch.Start();
      var doorwayPairs = GetDoorwayPairs(gen, attachTo, collection, archetype, normalizedDepth);
      DoorwayPairStopwatch.Stop();
      DoorwayPairTime += (float)DoorwayPairStopwatch.Elapsed.TotalMilliseconds;

      var tilePlacementResult = new TilePlacementResultProxy(new NoMatchingDoorwayPlacementResult(attachTo));
      while(doorwayPairs.Count > 0) {
        var pair = doorwayPairs.Dequeue();
        tilePlacementResult = TryPlaceTileResult(gen, pathProxy, pair, archetype);
        if (tilePlacementResult.result is SuccessPlacementResult) break;
      }

      if (tilePlacementResult.result is SuccessPlacementResult){
        if (injectedTile != null) {
          var tileProxy = tilePlacementResult.tileProxy;
          tileProxy.Placement.InjectionData = injectedTile;
          pathProxy.injectedTiles[tileProxy] = injectedTile;
          pathProxy.tilesPendingInjection.RemoveAt(index);
        }
        return tilePlacementResult;
      }

      return new TilePlacementResultProxy(new NoMatchingDoorwayPlacementResult(attachTo));

    }

    private static Queue<DoorwayPair> GetDoorwayPairs(DungeonGenerator gen, TileProxy attachTo, IEnumerable<GameObjectChance> collection, DungeonArchetype archetype, float normalizedDepth){
      // get rotation state
      var value = attachTo.PrefabTile.AllowRotation;
      if (gen.OverrideAllowTileRotation) {
        value = gen.AllowTileRotation;
      }

      var doorwayPairFinder = new DoorwayPairFinder();
      doorwayPairFinder.DungeonFlow = gen.DungeonFlow;
      doorwayPairFinder.RandomStream = gen.RandomStream;
      doorwayPairFinder.PlacementParameters = new TilePlacementParameters { Archetype = archetype };
      doorwayPairFinder.GetTileTemplateDelegate = new GetTileTemplateDelegate(gen.GetTileTemplate);
      doorwayPairFinder.IsOnMainPath = false;
      doorwayPairFinder.NormalizedDepth = normalizedDepth;
      doorwayPairFinder.PreviousTile = attachTo;
      doorwayPairFinder.UpVector = gen.UpVector;
      doorwayPairFinder.AllowRotation = new bool?(value);
      doorwayPairFinder.TileWeights = new List<GameObjectChance>(collection);
      doorwayPairFinder.IsTileAllowedPredicate = (TileProxy prev, TileProxy next, ref float weight) => {
        var flag4 = prev != null && next.Prefab == prev.Prefab;
        var tileRepeatMode = TileRepeatMode.Allow;
        if (gen.OverrideRepeatMode) {
          tileRepeatMode = gen.RepeatMode;
        } else if (next != null) {
          tileRepeatMode = next.PrefabTile.RepeatMode;
        }
        bool result2;
        switch(tileRepeatMode) {
          case TileRepeatMode.Allow:
            result2 = true;
            break;
          case TileRepeatMode.DisallowImmediate:
            result2 = !flag4;
            break;
          case TileRepeatMode.Disallow:
            result2 = !gen.proxyDungeon.AllTiles.Where(x => x.Prefab == next.Prefab).Any();
            break;
          default:
            throw new NotImplementedException($"TileRepeatMode {tileRepeatMode} is not implemented");
        }
        return result2;
      };

      var maxCount = gen.UseMaximumPairingAttempts ? new int?(gen.MaxPairingAttempts) : null;
      return doorwayPairFinder.GetDoorwayPairs(maxCount);
    }

    private static Queue<DoorwayPair> GetDoorwayPairs(DungeonGenerator gen, TileProxy attachTo, IEnumerable<TileSet> useableTileSets, DungeonArchetype archetype, float normalizedDepth){
      IEnumerable<GameObjectChance> collection;
      collection = useableTileSets.SelectMany(t => t.TileWeights.Weights);
      return GetDoorwayPairs(gen, attachTo, collection, archetype, normalizedDepth);
    }

    private static TilePlacementResultProxy TryPlaceTileResult(DungeonGenerator gen, BranchPathProxy pathProxy, DoorwayPair pair, DungeonArchetype archetype){
      var nextTemplate = pair.NextTemplate;
      var previousDoorway = pair.PreviousDoorway;
      if (nextTemplate == null) return new TilePlacementResultProxy(new NullTemplatePlacementResult());

      var index = pair.NextTemplate.Doorways.IndexOf(pair.NextDoorway);

      var tile = new TileProxy(nextTemplate);
      tile.Placement.IsOnMainPath = false;
      tile.Placement.PlacementParameters = new TilePlacementParameters {
        Archetype = archetype,
        Node = tile.Placement.PlacementParameters?.Node,
        Line = tile.Placement.PlacementParameters?.Line
      };
      tile.Placement.TileSet = pair.NextTileSet;

      if (previousDoorway != null) {
        var myDoorway = tile.Doorways[index];
        tile.PositionBySocket(myDoorway, previousDoorway);
        var bounds = tile.Placement.Bounds;
        if (gen.RestrictDungeonToBounds && !gen.TilePlacementBounds.Contains(bounds)) return new TilePlacementResultProxy(new OutOfBoundsPlacementResult(tile));
        if (IsCollidingWithAnyTileAndTileResult(gen, pathProxy, tile, previousDoorway.TileProxy)) return new TilePlacementResultProxy(new TileIsCollidingPlacementResult(tile));
      }

      if (tile == null) return new TilePlacementResultProxy(new NullTemplatePlacementResult());

      tile.Placement.PathDepth = pair.PreviousTile.Placement.PathDepth;
      tile.Placement.NormalizedPathDepth = pair.PreviousTile.Placement.NormalizedPathDepth;
      tile.Placement.BranchDepth = pair.PreviousTile.Placement.IsOnMainPath ? 0 : (pair.PreviousTile.Placement.BranchDepth + 1);

      return new TilePlacementResultProxy(new SuccessPlacementResult(), tile, previousDoorway, tile.Doorways[index]);
    }

    private static bool IsCollidingWithAnyTileAndTileResult(DungeonGenerator gen, BranchPathProxy pathProxy, TileProxy newTile, TileProxy previousTile){
      foreach(var tileproxy in gen.proxyDungeon.AllTiles.Concat(pathProxy.list.Select(t => t.tileProxy))){
        var flag = previousTile == tileproxy;
        var maxOverlap = flag ? gen.OverlapThreshold : (-gen.Padding);
        if (gen.DisallowOverhangs && !flag){
          if (newTile.IsOverlappingOrOverhanging(tileproxy, gen.UpDirection, maxOverlap)) return true;
        } else if (newTile.IsOverlapping(tileproxy, maxOverlap)) return true;
      }
      return false;
    }

    private static void MakeTileProxyConnection(DungeonGenerator gen, TilePlacementResultProxy proxy) {
      if (proxy.previousDoorway != null) {
        gen.proxyDungeon.MakeConnection(proxy.previousDoorway, proxy.nextDoorway);
      }
      gen.proxyDungeon.AddTile(proxy.tileProxy);
    }

  }
}
