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
using BepInEx.Logging;
using DunGenPlus.DevTools;
using DunGenPlus.Patches;

namespace DunGenPlus.Generation {
  internal partial class DunGenPlusGenerator {

    public static void PrintAddTileError(DungeonGenerator gen, TileProxy previousTile, DungeonArchetype archetype, IEnumerable<TileSet> useableTileSets, int branchId, int lineLength, float lineRatio){

      var prevName = previousTile != null ? previousTile.Prefab.name : "NULL";
      var archetypeName = archetype ? archetype.name : "NULL";
      var tileSetNames = string.Join(", ", useableTileSets);

      var stringList = new List<string>();  
      stringList.Add($"Main branch gen failed at Branch {branchId} (Length: {lineLength}, Ratio: {lineRatio})");
      stringList.Add($"Prev tile: {prevName}");
      stringList.Add($"Archetype: {archetypeName}");
      stringList.Add($"Tilesets: {tileSetNames}");
      stringList.Add($"Reason: {lastTilePlacementResult}");

      if (previousTile != null) {
        var availableDoorways = string.Join(", ", previousTile.UnusedDoorways.Select(d => d.DoorwayComponent.gameObject.name));
        var usedDoorways = string.Join(", ", previousTile.UsedDoorways.Select(d => d.DoorwayComponent.gameObject.name));

        stringList.Add($"Available Doorways: {availableDoorways}");
        stringList.Add($"Used Doorways: {usedDoorways}");

        if (API.IsDevDebugModeActive()){
          var allTiles = GetDoorwayPairs(gen, previousTile, useableTileSets, archetype, lineRatio);
          var uniqueTiles = string.Join(", ", allTiles.Select(t => t.NextTemplate.Prefab).Distinct().Select(d => d.name));

          stringList.Add($"Next Possible Tiles: {uniqueTiles}");
        }
      }

      stringList.Add(string.Empty);
      Plugin.logger.LogDebug(string.Join("\n", stringList));
    }

    public static void PrintAddTileErrorQuick(DungeonGenerator gen, int lineLength){
      PrintAddTileError(gen, DungeonGeneratorPatch.lastAttachTo, DungeonGeneratorPatch.lastArchetype, DungeonGeneratorPatch.lastUseableTileSets, 0, lineLength, DungeonGeneratorPatch.lastNormalizedDepth);
    }

    public static TilePlacementResult lastTilePlacementResult; 

    public static void RecordLastTilePlacementResult(DungeonGenerator gen, TilePlacementResult result){
      lastTilePlacementResult = result;
    }

  }
}
