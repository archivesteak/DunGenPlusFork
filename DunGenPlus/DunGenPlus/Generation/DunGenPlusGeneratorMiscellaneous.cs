using DunGen;
using DunGen.Graph;
using DunGenPlus.DevTools;
using DunGenPlus.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DunGenPlus.Generation {
  internal partial class DunGenPlusGenerator {

    public static void AddForcedTiles(DungeonGenerator gen){
      if (!Properties.AdditionalTilesProperties.UseAdditionalTiles) return;

      var forcedTileSetLists = Properties.AdditionalTilesProperties.AdditionalTileSets.ToList();
      while(forcedTileSetLists.Count > 0){
        var item = forcedTileSetLists[forcedTileSetLists.Count - 1];
        
        // we have to check ALL tiles
        var allTiles = gen.proxyDungeon.AllTiles
          .Select(t => {
            var depthValue = item.DepthWeightScale.Evaluate(t.Placement.NormalizedDepth);
            var weight = t.Placement.IsOnMainPath ? item.MainPathWeight : item.BranchPathWeight;
            return (t, depthValue * weight * gen.RandomStream.NextDouble());
          })
          .Where(pair => pair.Item2 > 0f)
          .OrderBy(pair => pair.Item2);

        // try every tile, if we somehow fail than man that sucks
        foreach(var pair in allTiles){
          var t = pair.t;
          var tileProxy = gen.AddTile(t, item.TileSets, t.Placement.NormalizedDepth, new TilePlacementParameters());
          if (tileProxy == null) continue;

          AddTileProxy(tileProxy, GetMainPathIndexFromTileProxy(t));
          tileProxy.Placement.BranchDepth = t.Placement.BranchDepth;
					tileProxy.Placement.NormalizedBranchDepth = t.Placement.NormalizedDepth;
          tileProxy.Placement.PlacementParameters = new TilePlacementParameters {
            Archetype = tileProxy.Placement.PlacementParameters?.Archetype,
            Node = t.Placement.GraphNode,
            Line = t.Placement.GraphLine
          };

          Plugin.logger.LogDebug($"Forcefully added tile {tileProxy.Prefab.name}");
          break;
        }

        forcedTileSetLists.RemoveAt(forcedTileSetLists.Count - 1);
      }

      Plugin.logger.LogDebug($"Forcefully added all tiles");
    }

    public static void RandomizeLineArchetypes(DungeonGenerator gen, bool randomizeMainPath){
      if (!Properties.LineRandomizerProperties.UseLineRandomizer) return;  

      var flow = Instance.DungeonFlow;
      var lines = flow.Lines;
      var tilesetsUsed = new Dictionary<TileSet, int>();
      foreach(var t in Properties.LineRandomizerProperties.TileSets){
        tilesetsUsed.Add(t, 0);
      }

      foreach(var a in Properties.LineRandomizerProperties.Archetypes) {
        var tiles = randomizeMainPath ? a.TileSets : a.BranchCapTileSets;
        RandomizeArchetype(gen, tiles, tilesetsUsed);
      }
    }

    public static void RandomizeArchetype(DungeonGenerator gen, List<TileSet> targetTileSet, Dictionary<TileSet, int> tilesetsUsed){
      // get 3 random
      var newTiles = Properties.LineRandomizerProperties.TileSets
        .OrderBy(t => tilesetsUsed[t] + gen.RandomStream.NextDouble())
        .Take(Properties.LineRandomizerProperties.TileSetsTakeCount);

      var i = targetTileSet.Count - 1;
      foreach(var n in newTiles){
        targetTileSet[i] = n;
        --i;

        tilesetsUsed[n] += 1;
      }
    }

    public static DungeonArchetype ModifyMainBranchNodeArchetype(DungeonArchetype archetype, GraphNode node, RandomStream randomStream){
      if (!DunGenPlusGenerator.Active) return archetype;
      
      if (Properties.NormalNodeArchetypesProperties.AddArchetypesToNormalNodes && node.NodeType == NodeType.Normal) {
        return Properties.NormalNodeArchetypesProperties.GetRandomArchetype(node.Label, randomStream);;
      }
      return archetype;
    }

    public static bool AllowRetryStop(bool defaultState){
      return defaultState || API.IsDevDebugModeActive();
    }

    public static IEnumerable<DoorwayPair> GetPotentialDoorwayPairsForNonFirstTileAlternate(DoorwayPairFinder __instance){
      var previousTile = __instance.PreviousTile;
      var previousTileExtended = TileProxyPatch.GetTileExtenderProxy(previousTile);

      var tileOrder = __instance.tileOrder;

      
      IEnumerable<DoorwayProxy> validPrevExits;
      // default base behaviour
      if (!previousTileExtended.EntranceExitInterchangable){
        validPrevExits = previousTile.UnusedDoorways.Intersect(previousTileExtended.Exits);
      }
      // interchangable behaviour
      else {
        // check if previous tile used a specific exit/entrance
        // they used an exit as their entrance, so use entrances
        if (previousTile.UsedDoorways.Intersect(previousTileExtended.Exits).Any()){
          validPrevExits = previousTile.UnusedDoorways.Intersect(previousTileExtended.Entrances);
        } 
        // they used an entrance as their entrance, so use exits
        else if (previousTile.UsedDoorways.Intersect(previousTileExtended.Entrances).Any()){
          validPrevExits = previousTile.UnusedDoorways.Intersect(previousTileExtended.Exits);
        } 
        // uhh i guess it's fine?
        else {
          validPrevExits = new List<DoorwayProxy>();
        }
      }

      var requiresSpecExit = validPrevExits.Any();

      foreach(var previousDoor in previousTile.UnusedDoorways) {
        // overlapping doors aren't allowed to be potential doorway pairs since this function only finds entrance/exits
        // or have fun
        var isPrevOverlappingDoor = previousTileExtended.OverlappingDoorways.Contains(previousDoor);
        if (isPrevOverlappingDoor) {
          continue;
        }

        // only use allowed doorway exits
        if (requiresSpecExit && !validPrevExits.Contains(previousDoor)) continue;

        foreach(var tileWeight in __instance.TileWeights) {
          // skip if not ever considered
          if (!tileOrder.Contains(tileWeight)) continue;

          var nextTile = __instance.GetTileTemplateDelegate(tileWeight.Value);
          var nextTileExtended = TileProxyPatch.GetTileExtenderProxy(nextTile);
          var weight = (float)(tileOrder.Count - tileOrder.IndexOf(tileWeight));

          if (__instance.IsTileAllowedPredicate != null && !__instance.IsTileAllowedPredicate(previousTile, nextTile, ref weight)) continue;

          foreach(var nextDoor in nextTile.Doorways) {
            // check for previous and next
            // i forget next which causes problems obviously
            var isNextOverlappingDoor = nextTileExtended.OverlappingDoorways.Contains(nextDoor);
            if (isNextOverlappingDoor) {
              continue;
            }

            bool AllowEntranceAndExitPair(IEnumerable<DoorwayProxy> entrances, IEnumerable<DoorwayProxy> exits){
              // only use allowed doorway entrances
              if (entrances.Any() && !entrances.Contains(nextDoor)) return false;
              // skip if desiganted as an exit
              if (exits.Contains(nextDoor)) return false;
              return true;
            }

            // normal default behaviour
            if (!nextTileExtended.EntranceExitInterchangable){
              if (!AllowEntranceAndExitPair(nextTileExtended.Entrances, nextTileExtended.Exits)) continue;
            } 
            // interchangable behaviour
            else {
              var firstCheck = AllowEntranceAndExitPair(nextTileExtended.Entrances, nextTileExtended.Exits);
              var secondCheck = AllowEntranceAndExitPair(nextTileExtended.Exits, nextTileExtended.Entrances);
              if (!firstCheck && !secondCheck) continue;
            }

            var doorwayWeight = 0f;
            if (__instance.IsValidDoorwayPairing(previousDoor, nextDoor, previousTile, nextTile, ref doorwayWeight)) 
              yield return new DoorwayPair(previousTile, previousDoor, nextTile, nextDoor, tileWeight.TileSet, weight, doorwayWeight);
          }

        }

      }
    }

  }
}
