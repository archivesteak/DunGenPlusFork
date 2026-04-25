using DunGen;
using DunGen.Graph;
using DunGenPlus.DevTools;
using DunGenPlus.DevTools.Panels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DunGenPlus.Generation {
  internal partial class DunGenPlusGenerator {

    // Copied and pasted from DunGen
    // And heavily franksteined
    public static void ProcessGlobalPropsPerMainPath(DungeonGenerator dungeonGenerator){
      var localIDs = Properties.MainPathProperties.MainPathDetails.SelectMany(d => d.LocalGroupProps).Select(x => x.ID).ToHashSet();
      var detailedSettings = Properties.MainPathProperties.DetailedGlobalPropSettings;

      // first parameter int is the GlobalProp ID
      // second parameter is the List of GameObjectChanceTable indexed by the main path index
      var localDictionary = new Dictionary<int, Dictionary<int, GameObjectChanceTable>>();

      // default dictionary
      // first parmeter int is the Global Prop ID
      var globalDictionary = new Dictionary<int, GameObjectChanceTable>();

      foreach(var tile in dungeonGenerator.CurrentDungeon.AllTiles){
        foreach(var globalProp in tile.GetComponentsInChildren<GlobalProp>()){
          GameObjectChanceTable gameObjectChanceTable;
          if (localIDs.Contains(globalProp.PropGroupID)){
            if (!localDictionary.TryGetValue(globalProp.PropGroupID, out var dictionary)){
              dictionary = new Dictionary<int, GameObjectChanceTable>();
              localDictionary.Add(globalProp.PropGroupID, dictionary);
            }

            // The game will softlock if it can't find the tile
            // THIS IS SCARY!!!
            var mainPathIndex = GetMainPathIndexFromTile(tile);
            if (!dictionary.TryGetValue(mainPathIndex, out gameObjectChanceTable)){
              gameObjectChanceTable = new GameObjectChanceTable();
              dictionary.Add(mainPathIndex, gameObjectChanceTable);
            }
          } else {
            if (!globalDictionary.TryGetValue(globalProp.PropGroupID, out gameObjectChanceTable)){
              gameObjectChanceTable = new GameObjectChanceTable();
              globalDictionary.Add(globalProp.PropGroupID, gameObjectChanceTable);
            }
          }

          var num = tile.Placement.IsOnMainPath ? globalProp.MainPathWeight : globalProp.BranchPathWeight;
          num *= globalProp.DepthWeightScale.Evaluate(tile.Placement.NormalizedDepth);
          gameObjectChanceTable.Weights.Add(new GameObjectChance(globalProp.gameObject, num, 0f, null));
        }
      }

      // deactive in preparation
      foreach(var dictionary in localDictionary.Values){
        foreach(var table2 in dictionary.Values){
          foreach(var gameObjectChance in table2.Weights){
            gameObjectChance.Value.SetActive(false);
          }
        }
      }

      foreach(var table2 in globalDictionary.Values){
        foreach(var gameObjectChance in table2.Weights){
          gameObjectChance.Value.SetActive(false);
        }
      }

      var list = new List<int>(localDictionary.Count + globalDictionary.Count);

      // min distance behaviour
      void RemoveElementsTooClose(GameObjectChanceTable table, GameObjectChanceTable reserveTable, Vector3 position, float minDistance){
        if (minDistance <= 0f) return;

        var elementsToRemove = new List<GameObjectChance>();
        foreach(var item in table.Weights){
          if (Vector3.SqrMagnitude(position - item.Value.transform.position) < minDistance * minDistance) {
            elementsToRemove.Add(item);
          }
        }

        foreach(var e in elementsToRemove){
          table.Weights.Remove(e);
          reserveTable.Weights.Add(e);
        }
      }

      // normal default behaviour
      int ProcessGlobalPropID(GameObjectChanceTable table, GameObjectChanceTable reserveTable, int localMax, int globalCount, int globalMax, float minDistance){
        localMax = Mathf.Clamp(localMax, 0, table.Weights.Count);
        var i = 0;
        while(i < localMax && i + globalCount < globalMax){
          if (table.Weights.Count == 0) return i;

          var random = table.GetRandom(dungeonGenerator.RandomStream, true, 0f, null, true, true);
          if (random != null && random.Value != null) {
            random.Value.SetActive(true);
            RemoveElementsTooClose(table, reserveTable, random.Value.transform.position, minDistance);
          }
          ++i;
        }
        return i;
      }

      // UseToReachGlobalPropLimit behaviour
      int ProcessRemainingGlobalPropID(GameObjectChanceTable[] tables, GameObjectChanceTable reserveTable, int count, float minDistance){
        count = Mathf.Clamp(count, 0, tables.Sum(t => t.Weights.Count));
        var i = 0;
        while(i < count){
          if (tables.Sum(t => t.Weights.Count) == 0) return i;

          var random = GameObjectChanceTable.GetCombinedRandom(dungeonGenerator.RandomStream, true, 0f, tables);
          if (random != null) {
            random.SetActive(true);

            foreach(var t in tables){
              RemoveElementsTooClose(t, reserveTable, random.transform.position, minDistance);
            }
          }
          ++i;
        }
        return i;
      }

      // cause min distance can mean that global prop limit is not reached
      // fuck it, make it happen
      int ProcessRemainingGlobalPropIDNoMatterWhat(GameObjectChanceTable table, int count){
        count = Mathf.Clamp(count, 0, table.Weights.Count);
        var i = 0;
        while(i < count){
          if (table.Weights.Count == 0) return i;

          var random = table.GetRandom(dungeonGenerator.RandomStream, true, 0f, null, true, true);
          if (random != null) {
            random.Value.SetActive(true);
          }
          ++i;
        }
        return i;
      }

      using(var enumerator = globalDictionary.GetEnumerator()){
        while(enumerator.MoveNext()){
          var pair = enumerator.Current;
          if (list.Contains(pair.Key)){
            Plugin.logger.LogWarning("Dungeon Flow contains multiple entries for the global prop group ID: " + pair.Key.ToString() + ". Only the first entry will be used.");
          } else {

            var globalPropSettings = dungeonGenerator.DungeonFlow.GlobalProps
              .Where(x => x.ID == pair.Key)
              .FirstOrDefault();

            var detailedPropSettings = detailedSettings
                .Where(x => x.ID == pair.Key)
                .FirstOrDefault();

            var reserveTable = new GameObjectChanceTable();
            var minDistance = detailedPropSettings != null ? detailedPropSettings.MinimumDistance : -1f;
            var reachGlobalLimitNoMatterWhat = detailedPropSettings != null ? detailedPropSettings.GlobalCountMustBeReached : true;

            if (globalPropSettings != null){    
              var table = pair.Value;
              var globalMax = globalPropSettings.Count.GetRandom(dungeonGenerator.RandomStream);

              var spawned = ProcessGlobalPropID(table, reserveTable, globalMax, 0, globalMax, minDistance);
              Plugin.logger.LogDebug($"Global ID: {pair.Key} ({spawned} / {globalMax}). Min Dist: {minDistance}");
              list.Add(pair.Key);

              // failsafe
              if (reachGlobalLimitNoMatterWhat && spawned < globalMax) {
                Plugin.logger.LogDebug($"MinDistance be damned, forcing {globalMax - spawned} more to spawn");
                spawned += ProcessRemainingGlobalPropIDNoMatterWhat(reserveTable, globalMax - spawned);
                Plugin.logger.LogDebug($"Spawned remaining props ({spawned} / {globalMax})");
              }
            }
          }
        }
      }

      using(var enumerator = localDictionary.GetEnumerator()){
        while(enumerator.MoveNext()){
          var pair = enumerator.Current;
          var globalPropId = pair.Key;
          if (list.Contains(globalPropId)){
            Plugin.logger.LogWarning("Dungeon Flow contains multiple entries for the global prop group ID: " + pair.Key.ToString() + ". Only the first entry will be used.");
          } else {
            //Plugin.logger.LogWarning($"{pair.Key}: Local");

            var globalPropSettings = dungeonGenerator.DungeonFlow.GlobalProps
              .Where(x => x.ID == globalPropId)
              .FirstOrDefault();

            var detailedPropSettings = detailedSettings
                  .Where(x => x.ID == pair.Key)
                  .FirstOrDefault();

            var reserveTable = new GameObjectChanceTable();
            var minDistance = detailedPropSettings != null ? detailedPropSettings.MinimumDistance : -1f;
            var reachGlobalLimitNoMatterWhat = detailedPropSettings != null ? detailedPropSettings.GlobalCountMustBeReached : true;
            reachGlobalLimitNoMatterWhat = true;

            if (globalPropSettings != null){
              var globalCount = 0;
              var globalMax = globalPropSettings.Count.GetRandom(dungeonGenerator.RandomStream);
              var pathDictionary = pair.Value;
              Plugin.logger.LogDebug($"Local ID: {pair.Key} (Max {globalMax}). Min Dist: {minDistance}");

              var toRemoveKeys = new List<int>();

              foreach(var pathPair in pathDictionary){
                // try and get local main path properites based on key of Dictionary<MainPathIndex (int), GlobalProps (GameObjectChanceTable)> 
                var mainPathIndex = pathPair.Key;
                var localGroupProps = Properties.MainPathProperties.GetMainPathDetails(mainPathIndex).LocalGroupProps;
                var localPropSettings = localGroupProps
                  .Where(x => x.ID == globalPropId)
                  .FirstOrDefault();

                if (localPropSettings == null) {
                  Plugin.logger.LogDebug($"Main Path {mainPathIndex}: No local ID defined, skipping");
                  continue;
                }

                var tableClone = pathPair.Value; // this was cloned, why?
                var localMax = localPropSettings.Count.GetRandom(dungeonGenerator.RandomStream);

                var spawned = ProcessGlobalPropID(tableClone, reserveTable, localMax, globalCount, globalMax, minDistance);
                globalCount += spawned;
                Plugin.logger.LogDebug($"Main Path {mainPathIndex}: Local ({spawned} / {localMax}), Global ({globalCount} / {globalMax})");

                // all dictionaries that we done using get throw out
                if (!localPropSettings.UseToReachGlobalPropLimit) {
                  toRemoveKeys.Add(mainPathIndex); 
                  foreach(var w in tableClone.Weights){
                    reserveTable.Weights.Add(w);  // but we still have to include anything remaining back into the reserve
                  }
                }

                //Plugin.logger.LogError($"Spawned {spawned} ({globalCount}/{globalMax})");
              }

              foreach(var key in toRemoveKeys){ 
                pathDictionary.Remove(key);
              }

              // spawn the last remaining global props if possible
              if (globalCount < globalMax && pathDictionary.Count > 0) {
                var combine = string.Join(", ", pathDictionary.Keys);
                Plugin.logger.LogDebug($"Combining main paths ({combine}) into one GameObjectChanceTable");

                var combinedTable = pathDictionary.Select(d => d.Value).ToArray();
                var spawned = ProcessRemainingGlobalPropID(combinedTable, reserveTable, globalMax - globalCount, minDistance);
                globalCount += spawned;
                Plugin.logger.LogDebug($"Spawned remaining props ({globalCount} / {globalMax})");
              }

              // failsafe
              if (reachGlobalLimitNoMatterWhat && globalCount < globalMax) {
                Plugin.logger.LogDebug($"MinDistance be damned, forcing {globalMax - globalCount} more to spawn");
                globalCount += ProcessRemainingGlobalPropIDNoMatterWhat(reserveTable, globalMax - globalCount);
                Plugin.logger.LogDebug($"Spawned remaining props ({globalCount} / {globalMax})");
              }

              list.Add(pair.Key);
            }
          }
        }
      }

    }

  }
}
