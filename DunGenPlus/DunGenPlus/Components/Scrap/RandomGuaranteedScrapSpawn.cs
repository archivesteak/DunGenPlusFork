using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;

namespace DunGenPlus.Components.Scrap {
  public class RandomGuaranteedScrapSpawn : MonoBehaviour {


    [Tooltip("Chance for the loot to even spawn.")]
    [Range(0f, 1f)]
    public float spawnChance = 1f;
    [Tooltip("Minimum scrap value of the scrap item.")]
    public int minimumScrapValue = 0;
    [Tooltip("Maximum scrap value of the scrap item.")]
    public int maximumScrapValue = 100;
    [Tooltip("Forces this item in particular to spawn. Overrides min/max scrap values")]
    public string specificScrapTarget;

    internal static Dictionary<(int, int), IEnumerable<SpawnableItemWithRarity>> scrapItemRarityValueCache;
    internal static Dictionary<string, IEnumerable<SpawnableItemWithRarity>> scrapItemRarityNameCache;

    internal static void ResetCache(){
      scrapItemRarityValueCache = new Dictionary<(int, int), IEnumerable<SpawnableItemWithRarity>>();
      scrapItemRarityNameCache = new Dictionary<string, IEnumerable<SpawnableItemWithRarity>>();
    }

    internal static IEnumerable<SpawnableItemWithRarity> GetCachedItemList(List<SpawnableItemWithRarity> allMoonItems, int minScrapValue, int maxScrapValue) {
      var pair = (minScrapValue, maxScrapValue);
      if (!scrapItemRarityValueCache.TryGetValue(pair, out var list)){
        list = allMoonItems.Where(i => i.spawnableItem.minValue >= minScrapValue && maxScrapValue <= i.spawnableItem.minValue).ToArray();
        scrapItemRarityValueCache.Add(pair, list);
      }
      return list;
    }

    internal static IEnumerable<SpawnableItemWithRarity> GetCachedItemList(List<SpawnableItemWithRarity> allMoonItems, string scrapName) {
      scrapName = scrapName.ToLowerInvariant();
      if (!scrapItemRarityNameCache.TryGetValue(scrapName, out var list)){
        list = allMoonItems.Where(i => i.spawnableItem.name.ToLowerInvariant().Contains(scrapName) || i.spawnableItem.itemName.ToLowerInvariant().Contains(scrapName)).ToArray();
        scrapItemRarityNameCache.Add(scrapName, list);
      }
      return list;
    }

    internal IEnumerable<SpawnableItemWithRarity> GetCachedItemList(List<SpawnableItemWithRarity> allMoonItems) {
      if (string.IsNullOrWhiteSpace(specificScrapTarget)) return GetCachedItemList(allMoonItems, minimumScrapValue, maximumScrapValue);
      return GetCachedItemList(allMoonItems, specificScrapTarget);
    }

    internal static Item GetRandomItem(IEnumerable<SpawnableItemWithRarity> list) {
      var weightList = new int[list.Count()];
      for(var i = 0; i < weightList.Length; ++i) {
        weightList[i] = list.ElementAt(i).rarity;
      }

      var randomIndex = RoundManager.Instance.GetRandomWeightedIndex(weightList);
      return list.ElementAt(randomIndex).spawnableItem;

    }

    internal (NetworkObject itemReference, int scrapValue) CreateItem(RoundManager roundManager, List<SpawnableItemWithRarity> allMoonItems){
      var anomalyRandom = roundManager.AnomalyRandom;
      if (anomalyRandom.NextDouble() >= spawnChance) return (null, 0);

      var itemList = GetCachedItemList(allMoonItems);
      var itemListCount = itemList.Count();
      if (itemListCount == 0) return (null, 0);

      var randomItem = GetRandomItem(itemList);
      var randomValue = (int)(anomalyRandom.Next(randomItem.minValue, randomItem.maxValue) * roundManager.scrapValueMultiplier);

      var gameObject = Instantiate(randomItem.spawnPrefab, transform.position, Quaternion.identity, roundManager.spawnedScrapContainer);
      var itemComp = gameObject.GetComponent<GrabbableObject>();
      
      gameObject.transform.rotation = Quaternion.Euler(randomItem.restingRotation);
      itemComp.fallTime = 0f;
      itemComp.scrapValue = randomValue;

      var networkComp = gameObject.GetComponent<NetworkObject>();
      networkComp.Spawn(false);
      return (networkComp, randomValue);
    }

  }
}
