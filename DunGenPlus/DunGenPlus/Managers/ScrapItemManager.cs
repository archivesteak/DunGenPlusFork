using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DunGenPlus.Managers {
  public static class ScrapItemManager {

    internal static SelectableLevel previousLevel;
    internal static List<SpawnableItemWithRarity> previouslyAddedItems = new List<SpawnableItemWithRarity>();
    internal static List<SpawnableItemWithRarity> previouslyModifiedItems = new List<SpawnableItemWithRarity>();
    
    internal static void UndoPreviousChanges(){
      //
      if (previousLevel != null){
        Plugin.logger.LogDebug($"Undoing changes of ScrapItemManager for {previousLevel.PlanetName}");
        var levelList = previousLevel.spawnableScrap;
        
        if (previouslyAddedItems.Count > 0){
          // we doing it backwards since previously added items would be at the end of the list yuh?
          for(var j = previouslyAddedItems.Count - 1; j >= 0; j--){
            var previousItem = previouslyAddedItems[j];
            for(var i = levelList.Count - 1; i >= 0; i--){
              var levelItem = levelList[i];
              if (levelItem == previousItem){
                levelList.RemoveAt(i);
                Plugin.logger.LogDebug($"Properly removed temporary item {previousItem.spawnableItem.itemName}");
                goto RemovedItemCorrect;
              }
            }

            // 
            Plugin.logger.LogWarning($"Couldn't find/remove temporary item {previousItem.spawnableItem.itemName}");

            RemovedItemCorrect:
            continue;
          }
          previouslyAddedItems.Clear();
        }

        if (previouslyModifiedItems.Count > 0){
          for(var j = 0;  j < previouslyModifiedItems.Count; j++){
            var previousItem = previouslyModifiedItems[j];
            for(var i = 0;  i < levelList.Count; i++){
              if (levelList[i].spawnableItem == previousItem.spawnableItem){
                levelList[i] = previousItem;
                Plugin.logger.LogDebug($"Properly fixed modified item {previousItem.spawnableItem.itemName}");
                goto ModifiedItemCorrect;
              }
            }

             // 
            Plugin.logger.LogWarning($"Couldn't find/fix modified item {previousItem.spawnableItem.itemName}");

            ModifiedItemCorrect:
            continue;
          }
          previouslyModifiedItems.Clear();
        }

        previousLevel = null;
      }
    }

    internal static void Initialize(RoundManager roundManager){
      UndoPreviousChanges();
      previousLevel = roundManager.currentLevel;
      Plugin.logger.LogDebug($"Initialized ScrapItemManager to {previousLevel.PlanetName}");
    }

    public static void AddItems(IEnumerable<SpawnableItemWithRarity> newItems){
      foreach(var item in newItems){
        AddItem(item);
      }
    }

    public static void AddItem(SpawnableItemWithRarity newItem){
      var levelList = previousLevel.spawnableScrap;
      for(var i = 0; i < levelList.Count; ++i) {
        if (levelList[i].spawnableItem == newItem.spawnableItem) {
          if (levelList[i].rarity == newItem.rarity){
            Plugin.logger.LogDebug($"Skipping {newItem.spawnableItem.itemName} as it has the same rarity");
            return;
          }

          previouslyModifiedItems.Add(levelList[i]);
          levelList[i] = newItem;
          Plugin.logger.LogDebug($"Modifying already existing item {newItem.spawnableItem.itemName} to new weight {newItem.rarity}");
          return;
        }
      }

      previouslyAddedItems.Add(newItem);
      levelList.Add(newItem);
      Plugin.logger.LogDebug($"Adding temporary item {newItem.spawnableItem.itemName} with weight {newItem.rarity}");
    }

  }
}
