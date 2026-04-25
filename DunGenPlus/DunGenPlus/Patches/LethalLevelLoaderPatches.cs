using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DunGenPlus.DevTools;
using DunGenPlus.Managers;
using HarmonyLib;


namespace DunGenPlus.Patches {
  internal class LethalLevelLoaderPatches {

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LethalLevelLoader.Patches), "DungeonGeneratorGenerate_Prefix")]
    public static bool DungeonGeneratorGenerate_Prefix_Patches_Prefix(){
      return DevDebugManager.Instance == null;
    }


    [HarmonyPatch(typeof(LethalLevelLoader.EventPatches), "DungeonGeneratorGenerate_Prefix")]
    [HarmonyPrefix]
    public static void DungeonGeneratorGenerate_Prefix_EventPatches_Prefix(){
      ScrapItemManager.Initialize(LethalLevelLoader.Patches.RoundManager);
      EnemyManager.Initialize(LethalLevelLoader.Patches.RoundManager);
    }

  }
}
