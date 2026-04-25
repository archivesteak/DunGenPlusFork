using DunGen.Graph;
using DunGen;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using DunGenPlus.Collections;
using DunGenPlus.Components;
using System.Security.Permissions;
using DunGenPlus.Managers;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using BepInEx.Logging;
using DunGenPlus.DevTools;
using DunGenPlus.Patches;
using DunGenPlus.DevTools.Panels;

[assembly: SecurityPermission( SecurityAction.RequestMinimum, SkipVerification = true )]
namespace DunGenPlus.Generation {
  internal partial class DunGenPlusGenerator {
    public static DunGenExtender Instance { get; internal set; }
    public static DunGenExtenderProperties Properties { get; internal set; }

    public static bool Active { get; internal set; }
    public static bool ActiveAlternative { get; internal set; }

    internal static HDRenderPipelineAsset previousHDRPAsset;
    internal static HDRenderPipelineAsset newHDRPAsset;

    public static void Activate(DungeonGenerator generator, DunGenExtender extender){
      Instance = extender;
      Active = true;
      ActiveAlternative = true;

      var props = extender.Properties.Copy(extender.Version);
      var callback = new EventCallbackScenario(DunGenPlusPanel.Instance && DunGenPlusPanel.Instance.eventCallbackValue);
      Instance.Events.OnModifyDunGenExtenderProperties.Invoke(props, callback);
      props.NormalNodeArchetypesProperties.SetupProperties(generator);
      Properties = props;

      if (Properties.DungeonBoundsProperties.UseDungeonBounds) {
        generator.DebugRender = true;
        generator.RestrictDungeonToBounds = true;
        var bounds = Properties.DungeonBoundsProperties.GetDungeonBounds(generator.LengthMultiplier);
        generator.TilePlacementBounds = bounds;
        Plugin.logger.LogDebug($"Dungeon Bounds: {bounds}");
      }

      if (Properties.MiscellaneousProperties.UseMaxShadowsRequestUpdate) {
        Plugin.logger.LogDebug($"Updating HDRP asset: setting max shadows request to {Properties.MiscellaneousProperties.MaxShadowsRequestCount}");
        try {
          previousHDRPAsset = QualitySettings.renderPipeline as HDRenderPipelineAsset;
          newHDRPAsset = ScriptableObject.Instantiate(previousHDRPAsset);
      
          var settings = newHDRPAsset.currentPlatformRenderPipelineSettings;
          settings.hdShadowInitParams.maxScreenSpaceShadowSlots = Properties.MiscellaneousProperties.MaxShadowsRequestCount;
          newHDRPAsset.currentPlatformRenderPipelineSettings = settings;

          QualitySettings.renderPipeline = newHDRPAsset;
        } catch (Exception e) {
          Plugin.logger.LogError("Failed to update HDRP asset");
          Plugin.logger.LogError(e.ToString());
        }
      }


      DoorwayManager.ResetList();
      DoorwayManager.onMainEntranceTeleportSpawnedEvent.ClearTemporaryActionList();
    }

    public static void Deactivate(){
      Instance = null;
      Properties = null;
      Active = false;
      ActiveAlternative = false;

      if (previousHDRPAsset && QualitySettings.renderPipeline == newHDRPAsset) {
        Plugin.logger.LogDebug("Restoring original HDRP asset");

        QualitySettings.renderPipeline = previousHDRPAsset;
        previousHDRPAsset = null;
        newHDRPAsset = null;
      }
    }

    private static Dictionary<TileProxy, int> tileProxyMainPath = new Dictionary<TileProxy, int>();
    private static Dictionary<Tile, int> tileMainPath = new Dictionary<Tile, int>();

    public static int GetMainPathIndexFromTileProxy(TileProxy tileProxy){
      return tileProxyMainPath[tileProxy];
    }

    public static int GetMainPathIndexFromTile(Tile tile){
      if (tileMainPath.TryGetValue(tile, out var value)){
        return value;
      }

      Plugin.logger.LogWarning("Error with GetMainPathIndexFromTile.\nPLEASE REPORT TO MR. DEV WITH YOUR MOD PACK. THIS SHOULD NOT BE HAPPENING!");
      return 0;
    }

    public static void AddTileProxy(TileProxy tileProxy, int index) {  
      //Plugin.logger.LogWarning($"Adding: {tileProxy.Prefab.name} ({tileProxy.Placement.IsOnMainPath}): {index}");
      tileProxyMainPath.Add(tileProxy, index);
    }

    public static void AddMainPathTileProxies(IEnumerable<TileProxy> tileProxies, int index){
      var totalLength = (float)tileProxies.Last().Placement.PathDepth;
      foreach(var t in tileProxies) {
        AddTileProxy(t, index);
        t.Placement.NormalizedPathDepth = t.Placement.PathDepth / totalLength;
      }
    }

    public static void BuildBranchPathTileProxiesDictionary(DungeonProxy dungeonProxy){
      var lastIndex = 0;
      foreach(var connection in dungeonProxy.Connections){
        var aTileProxy = connection.A.TileProxy;
        if (aTileProxy.Placement.IsOnMainPath) {
          lastIndex = GetMainPathIndexFromTileProxy(aTileProxy);
        }

        var bTileProxy = connection.B.TileProxy;
        if (!bTileProxy.Placement.IsOnMainPath){
          AddTileProxy(bTileProxy, lastIndex);
        }
      }
    }

    public static void AddTileToMainPathDictionary(Dictionary<TileProxy, Tile> dictionary){
      if (!DunGenPlusGenerator.Active) return;

      foreach(var pair in dictionary){
        tileMainPath.Add(pair.Value, GetMainPathIndexFromTileProxy(pair.Key));
      }
    }

    public static IEnumerator GenerateAlternativeMainPaths(DungeonGenerator gen) {

      // default behaviour
      if (!Active) {
        ActiveAlternative = false;
        yield return gen.Wait(gen.GenerateBranchPaths());
        ActiveAlternative = true;
        yield break;
      }

      var altCount = Properties.MainPathProperties.MainPathCount - 1;
      tileProxyMainPath.Clear();
      tileMainPath.Clear();

      var firstMainPathTiles = gen.proxyDungeon.MainPathTiles.ToList();
      AddMainPathTileProxies(firstMainPathTiles, 0);

      var mainRoomTilePrefab = Properties.MainPathProperties.MainRoomTilePrefab;
      var copyNodeBehaviour = Properties.MainPathProperties.CopyNodeBehaviour;

      if (altCount <= 0) {
        yield return gen.Wait(GenerateBranchPaths(gen, null, $"MainPathCount being {altCount + 1}", LogLevel.Info));
        yield break;
      }

      if (mainRoomTilePrefab == null) {
        yield return gen.Wait(GenerateBranchPaths(gen, null, $"MainRoomTilePrefab being null", LogLevel.Warning));
        yield break;
      }

      var allMainPathTiles = new List<List<TileProxy>>();
      allMainPathTiles.Add(firstMainPathTiles);

      // main room is the true main room and not the fake room
      // this MUST have multiple doorways as you can imagine
      var mainRoom = gen.proxyDungeon.MainPathTiles.FirstOrDefault(t => t.Prefab == mainRoomTilePrefab);
      if (mainRoom == null) {
        yield return gen.Wait(GenerateBranchPaths(gen, null, $"MainRoomTilePrefab not spawning on the main path", LogLevel.Warning));
        yield break;
      }
      var mainRoomStartingLengthIndex = mainRoom.Placement.Depth + 1;
      Plugin.logger.LogDebug($"Main Room Length Index: {mainRoomStartingLengthIndex}");

      //FixDoorwaysToAllFloors(mainRoom, doorwayGroups);

      gen.ChangeStatus(GenerationStatus.MainPath);

			for (var b = 0; b < altCount; ++b) {
        SetCurrentMainPathExtender(b + 1);
        RandomizeLineArchetypes(gen, true);

        var previousTile = mainRoom;
        var targetLength = Mathf.RoundToInt(GetLength(gen.DungeonFlow).GetRandom(gen.RandomStream) * gen.LengthMultiplier);
        var archetypes = new List<DungeonArchetype>(targetLength);

        var newMainPathTiles = new List<TileProxy>();
        // this causes the main room to create three sets of branch paths
        // newMainPathTiles.Add(mainRoom);

        // nodes
        var nodesSorted = GetNodes(gen.DungeonFlow).OrderBy(n => n.Position).ToList();
        int startingNodeIndex;
        if (copyNodeBehaviour == DunGenExtenderProperties.CopyNodeBehaviour.CopyFromNodeList) {
          var index = nodesSorted.FindIndex(n => n.TileSets.SelectMany(t => t.TileWeights.Weights).Any(t => t.Value == mainRoomTilePrefab));

          if (index == -1) {
            yield return gen.Wait(GenerateBranchPaths(gen, mainRoom, $"CopyNodeBehaviour being CopyFromNodeList AND MainRoomTilePrefab not existing in the Nodes' tilesets", LogLevel.Warning));
            yield break;
          }

          startingNodeIndex = index + 1;
        } else if (copyNodeBehaviour == DunGenExtenderProperties.CopyNodeBehaviour.CopyFromMainPathPosition) {
          var lineDepthRatio = Mathf.Clamp01(1f / (targetLength - 1));
          startingNodeIndex = nodesSorted.FindIndex(n => n.Position >= lineDepthRatio);
        } else {
          Plugin.logger.LogFatal($"{copyNodeBehaviour} is not yet defined. Really really bad");
          startingNodeIndex = -1;
        }

        var nodes = nodesSorted.Skip(startingNodeIndex);
        var nodesVisited = new List<GraphNode>(nodes.Count());

        // places fake doorways at the first node
        MainRoomDoorwayGroups.ModifyGroupBasedOnBehaviour(mainRoom, b + 1);

        // most of this code is a mix of the GenerateMainPath()
        // and GenerateBranch() code
        var reachedLastNode = false;
        var lastNode = nodes.ElementAt(nodes.Count() - 1);
        for(var t = mainRoomStartingLengthIndex; t < targetLength && !reachedLastNode; ++t){
          var lineDepthRatio = Mathf.Clamp01((float)t / (targetLength - 1));
          var lineAtDepth = GetLineAtDepth(gen.DungeonFlow, lineDepthRatio);
          if (lineAtDepth == null){
            yield return gen.Wait(gen.InnerGenerate(true));
            yield break;
          }

          if (lineAtDepth != gen.previousLineSegment){
            gen.currentArchetype = lineAtDepth.GetRandomArchetype(gen.RandomStream, archetypes);
            gen.previousLineSegment = lineAtDepth;
          }

          GraphNode graphNode = null;
          DungeonArchetype archetype = null;
          foreach(var g in nodes) {
            if (lineDepthRatio >= g.Position && !nodesVisited.Contains(g)) {
              graphNode = g;
              nodesVisited.Add(g);
              break;
            }
          }

          List<TileSet> useableTileSets;
          if (graphNode != null) {
            archetype = ModifyMainBranchNodeArchetype(null, graphNode, gen.RandomStream);
            useableTileSets = graphNode.TileSets;

            // Zaggy wants the last node to stop dungeon generation
            if (graphNode == lastNode) {
              reachedLastNode = true;
            }
          } else {
            archetype = gen.currentArchetype;
            useableTileSets = archetype.TileSets;
          }

          var placementParams = new TilePlacementParameters {
            Archetype = archetype,
            Node = graphNode,
            Line = graphNode != null ? null : lineAtDepth
          };
          var tileProxy = gen.AddTile(previousTile, useableTileSets, lineDepthRatio, placementParams);

          if (tileProxy == null) {
            PrintAddTileError(gen, previousTile, archetype, useableTileSets, b + 1, t, lineDepthRatio);
            yield return gen.Wait(gen.InnerGenerate(true));
						yield break;
          }

          if (reachedLastNode || lineDepthRatio >= 1f){
            Plugin.logger.LogDebug($"Alt. main branch at {b} ended with {tileProxy.PrefabTile.name}");
          }

					tileProxy.Placement.BranchDepth = t;
					tileProxy.Placement.NormalizedBranchDepth = lineDepthRatio;
					tileProxy.Placement.PlacementParameters = placementParams;
					
					previousTile = tileProxy;
          newMainPathTiles.Add(tileProxy);

					if (gen.ShouldSkipFrame(true)) yield return gen.GetRoomPause();
        }

        AddMainPathTileProxies(newMainPathTiles, b + 1);
        allMainPathTiles.Add(newMainPathTiles);
			}

      // okay lets fix the fakes
      MainRoomDoorwayGroups.RemoveFakeDoorwayProxies(mainRoom);

      ActiveAlternative = false;
      Plugin.logger.LogDebug($"Created {altCount} alt. paths, creating branches now");
      gen.ChangeStatus(GenerationStatus.Branching);

      // this is major trickery and it works still
      for(var b = 0; b < altCount + 1; ++b){
        Plugin.logger.LogDebug($"Branch {b}");
        SetCurrentMainPathExtender(b);
        RandomizeLineArchetypes(gen, false);
        gen.proxyDungeon.MainPathTiles = allMainPathTiles[b];

        if (Properties.BranchPathMultiSimulationProperties.UseBranchPathMultiSim) {
          GenerateBranchBoostedPathsStopWatch.Reset();
          GenerateBranchBoostedPathsStopWatch.Start();
          yield return gen.Wait(GenerateMultiBranchPaths(gen));
          GenerateBranchBoostedPathsStopWatch.Stop();
          GenerateBranchBoostedPathsTime += (float)GenerateBranchBoostedPathsStopWatch.Elapsed.TotalMilliseconds;
        } else {
          yield return gen.Wait(gen.GenerateBranchPaths());
        }
      }

      ActiveAlternative = true;
      gen.proxyDungeon.MainPathTiles = allMainPathTiles[0];
      
      if (!Properties.BranchPathMultiSimulationProperties.UseBranchPathMultiSim){
        BuildBranchPathTileProxiesDictionary(gen.proxyDungeon);
      }

      AddForcedTiles(gen);
		}

    private static IEnumerator GenerateBranchPaths(DungeonGenerator gen, TileProxy mainRoom, string message, LogLevel logLevel){
      Plugin.logger.Log(logLevel, $"Switching to default dungeon branch generation: {message}");

      ActiveAlternative = false;
      SetCurrentMainPathExtender(0);
      if (mainRoom != null) MainRoomDoorwayGroups.RemoveFakeDoorwayProxies(mainRoom);

      yield return gen.Wait(gen.GenerateBranchPaths());
      ActiveAlternative = true;

      BuildBranchPathTileProxiesDictionary(gen.proxyDungeon);
      AddForcedTiles(gen);
    }

    public static void FixDoorwaysToAllFloors(TileProxy mainRoom, MainRoomDoorwayGroups doorwayGroups) {
      var first = doorwayGroups.doorwayListFirst;
      if (first == null) return;

      foreach(var target in mainRoom.UsedDoorways){
        if (target.ConnectedDoorway.Index == int.MaxValue && !first.Contains(target.DoorwayComponent)) {
          target.ConnectedDoorway = null;
        }
      }
    }

    /*
    public static GraphNode ModifyGraphNode(GraphNode node) {
      if (!Patch.active) return node;

      if (node.Label == "Hallway Entrance 1") {
        return Assets.networkObjectList.gardenEntranceGraphNode;
      }
      return node;
    }

    public static TileProxy FixTilesToAllFloors(TileProxy mainTile) {
      if (!Patch.active) return mainTile;

      var groups = mainTile.Prefab.GetComponentInChildren<MainRoomDoorwayGroups>();
      var first = groups.groupFirst;

      foreach(var target in mainTile.doorways){
        if (target.ConnectedDoorway != null && target.ConnectedDoorway.Index == int.MaxValue && !first.Contains(target.DoorwayComponent)) {
          target.ConnectedDoorway = null;
        }
      }

      return mainTile;
    }
    */

  }
}
