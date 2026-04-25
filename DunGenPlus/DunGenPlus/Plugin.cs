using BepInEx;
using BepInEx.Logging;
using DunGen;
using DunGen.Graph;
using DunGenPlus.Collections;
using DunGenPlus.Components.Scripting;
using DunGenPlus.Generation;
using DunGenPlus.Managers;
using DunGenPlus.Patches;
using HarmonyLib;
using LethalLevelLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace DunGenPlus {

  [BepInPlugin(modGUID, modName, modVersion)]
  [BepInDependency("imabatby.lethallevelloader", "1.4.0")]
  [BepInProcess("Lethal Company.exe")]
  public class Plugin : BaseUnityPlugin {

    internal const string modGUID = "dev.ladyalice.dungenplus";
    private const string modName = "Dungeon Generation Plus";
    private const string modVersion = "1.4.1";

    internal readonly Harmony Harmony = new Harmony(modGUID);

    internal static Plugin Instance {get; private set;}

    internal static ManualLogSource logger { get; private set; }

    internal static Dictionary<DungeonFlow, DunGenExtender> DunGenExtenders = new Dictionary<DungeonFlow, DunGenExtender>();

    void Awake() {
      if (Instance == null)  Instance = this;

      logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);
      logger.LogInfo($"Plugin {modName} has been added!");

      PluginConfig.SetupConfig(Config);

      Harmony.PatchAll(typeof(DungeonGeneratorPatch));
      Harmony.PatchAll(typeof(DungeonPatch));
      Harmony.PatchAll(typeof(DungeonProxyPatch));
      Harmony.PatchAll(typeof(RoundManagerPatch));
      Harmony.PatchAll(typeof(BranchCountHelperPatch));
      Harmony.PatchAll(typeof(TileProxyPatch));
      Harmony.PatchAll(typeof(DoorwayPairFinderPatch));
      
      try {
        Harmony.PatchAll(typeof(LethalLevelLoaderPatches));
      } catch (Exception e) {
        Plugin.logger.LogError("Failed to patch LLL for dev debug. You can ignore this.");
        Plugin.logger.LogError(e);
      }

      //Harmony.PatchAll(typeof(StartOfRoundPatch));

      Assets.LoadAssets();
      Assets.LoadAssetBundle();
      DoorwayManager.onMainEntranceTeleportSpawnedEvent.AddEvent("DoorwayCleanup", DoorwayManager.OnMainEntranceTeleportSpawnedFunction);
    }

  }
}
