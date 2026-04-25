using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DunGenPlus.Managers {
  public static class EnemyManager {

    internal static SelectableLevel previousLevel;
    internal static List<SpawnableEnemyWithRarity> previouslyAddedEnemies = new List<SpawnableEnemyWithRarity>();
    internal static List<SpawnableEnemyWithRarity> previouslyModifiedEnemies = new List<SpawnableEnemyWithRarity>();
    
    internal static void UndoPreviousChanges(){
      //
      if (previousLevel != null){
        Plugin.logger.LogDebug($"Undoing changes of EnemyManager for {previousLevel.PlanetName}");
        var levelList = previousLevel.Enemies;
        
        if (previouslyAddedEnemies.Count > 0){
          // we doing it backwards since previously added items would be at the end of the list yuh?
          for(var j = previouslyAddedEnemies.Count - 1; j >= 0; j--){
            var previousItem = previouslyAddedEnemies[j];
            for(var i = levelList.Count - 1; i >= 0; i--){
              var levelItem = levelList[i];
              if (levelItem == previousItem){
                levelList.RemoveAt(i);
                Plugin.logger.LogDebug($"Properly removed temporary enemy {previousItem.enemyType.enemyName}");
                goto RemovedItemCorrect;
              }
            }

            // 
            Plugin.logger.LogWarning($"Couldn't find/remove temporary enemy {previousItem.enemyType.enemyName}");

            RemovedItemCorrect:
            continue;
          }
          previouslyAddedEnemies.Clear();
        }

        if (previouslyModifiedEnemies.Count > 0){
          for(var j = 0;  j < previouslyModifiedEnemies.Count; j++){
            var previousItem = previouslyModifiedEnemies[j];
            for(var i = 0;  i < levelList.Count; i++){
              if (levelList[i].enemyType == previousItem.enemyType){
                levelList[i] = previousItem;
                Plugin.logger.LogDebug($"Properly fixed modified enemy {previousItem.enemyType.enemyName}");
                goto ModifiedItemCorrect;
              }
            }

             // 
            Plugin.logger.LogWarning($"Couldn't find/fix modified enemy {previousItem.enemyType.enemyName}");

            ModifiedItemCorrect:
            continue;
          }
          previouslyModifiedEnemies.Clear();
        }

        previousLevel = null;
      }
    }

    internal static void Initialize(RoundManager roundManager){
      UndoPreviousChanges();
      previousLevel = roundManager.currentLevel;
      Plugin.logger.LogDebug($"Initialized EnemyManager to {previousLevel.PlanetName}");
    }

    public static void AddEnemies(IEnumerable<SpawnableEnemyWithRarity> newEnemies){
      foreach(var item in newEnemies){
        AddEnemy(item);
      }
    }

    public static void AddEnemy(SpawnableEnemyWithRarity newEnemy){
      var levelList = previousLevel.Enemies;
      for(var i = 0; i < levelList.Count; ++i) {
        if (levelList[i].enemyType == newEnemy.enemyType) {

          if (levelList[i].rarity == newEnemy.rarity){
            Plugin.logger.LogDebug($"Skipping {newEnemy.enemyType.enemyName} as it has the same rarity");
            return;
          }

          previouslyModifiedEnemies.Add(levelList[i]);
          levelList[i] = newEnemy;
          Plugin.logger.LogDebug($"Modifying already existing enemy {newEnemy.enemyType.enemyName} to new weight {newEnemy.rarity}");
          return;
        }
      }

      previouslyAddedEnemies.Add(newEnemy);
      levelList.Add(newEnemy);
      Plugin.logger.LogDebug($"Adding temporary enemy {newEnemy.enemyType.enemyName} with weight {newEnemy.rarity}");
    }

  }
}
