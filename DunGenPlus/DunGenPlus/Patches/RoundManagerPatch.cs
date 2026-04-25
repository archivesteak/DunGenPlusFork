using DunGen;
using DunGenPlus.Components.Scrap;
using DunGenPlus.DevTools;
using DunGenPlus.Generation;
using DunGenPlus.Managers;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace DunGenPlus.Patches {
  internal class RoundManagerPatch {

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RoundManager), "FinishGeneratingLevel")]
    public static void GenerateBranchPathsPatch(){
      if (DunGenPlusGenerator.Active) {
        Plugin.logger.LogDebug("Alt. InnerGenerate() function complete");
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RoundManager), "SetPowerOffAtStart")]
    public static void SetPowerOffAtStartPatch(){
      DoorwayManager.onMainEntranceTeleportSpawnedEvent.Call();
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(RoundManager), "Awake")]
    public static void AwakePatch(){
      if (PluginConfig.EnableDevDebugTools.Value){
        var devDebug = new GameObject("DevDebugOpen", typeof(DevDebugOpen));
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RoundManager), "Start")]
    public static void StartPatch(ref RoundManager __instance){
      ScrapItemManager.UndoPreviousChanges();
      EnemyManager.UndoPreviousChanges();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RoundManager), "waitForScrapToSpawnToSync")]
    public static void waitForScrapToSpawnToSyncPatch (ref RoundManager __instance, ref NetworkObjectReference[] spawnedScrap, ref int[] scrapValues) {
      if (DunGenPlusGenerator.Active && DunGenPlusGenerator.Properties.MiscellaneousProperties.UseRandomGuaranteedScrapSpawn) {
        var spawnedScrapList = spawnedScrap.ToList();
        var scrapValuesList = scrapValues.ToList();

        var sources = UnityEngine.Object.FindObjectsOfType<RandomGuaranteedScrapSpawn>();
        RandomGuaranteedScrapSpawn.ResetCache();
        foreach(var s in sources) {
          var result = s.CreateItem(__instance, __instance.currentLevel.spawnableScrap);
          if (result.itemReference != null) {
            Plugin.logger.LogDebug($"Created guaranteed item {result.itemReference.gameObject.name} w/ value {result.scrapValue}");
            spawnedScrapList.Add(result.itemReference);
            scrapValuesList.Add(result.scrapValue);
          }
        }

        spawnedScrap = spawnedScrapList.ToArray();
        scrapValues = scrapValuesList.ToArray();
      }
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(RoundManager), "SetLevelObjectVariables")]
    public static void SetLevelObjectVariablesPatch (ref RoundManager __instance) {
      DoorwayManager.SetLevelObjectVariablesFunction();
    }

  }
}
