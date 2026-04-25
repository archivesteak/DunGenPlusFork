using DunGen;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Collections;
using DunGenPlus.Utils;
using DunGenPlus.Generation;
using DunGenPlus.Managers;
using DunGenPlus.DevTools;
using DunGen.Graph;
using DunGenPlus.Components;
using UnityEngine;

namespace DunGenPlus.Patches {
  internal class DungeonGeneratorPatch {

    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(DungeonGenerator), "Generate")]
    [HarmonyPrefix]
    public static void GeneratePatch(ref DungeonGenerator __instance){
      DunGenPlusGenerator.Deactivate();

      var flow = __instance.DungeonFlow;
      var extender = API.GetDunGenExtender(flow);
      if (extender && extender.Active) {
        Plugin.logger.LogInfo($"Loading DunGenExtender for {flow.name}");
        DunGenPlusGenerator.Activate(__instance, extender);
        return;
      }

      Plugin.logger.LogInfo($"Did not load a DunGenExtenderer");
      DunGenPlusGenerator.Deactivate();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonGenerator), "InnerGenerate")]
    public static void InnerGeneratePatch(ref DungeonGenerator __instance, bool isRetry){
      if (API.IsDevDebugModeActive() && !isRetry) {
        DevDebugManager.Instance.RecordNewSeed(__instance.ChosenSeed);
      }

      if (DunGenPlusGenerator.Active && DunGenPlusGenerator.ActiveAlternative) {
        TileProxyPatch.ResetDictionary();
        DunGenPlusGenerator.SetCurrentMainPathExtender(0);
        MainRoomDoorwayGroups.ModifyGroupBasedOnBehaviourSimpleOnce = false;
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonGenerator), "GenerateMainPath")]
    public static void GenerateMainPathPatch(ref DungeonGenerator __instance){
      if (DunGenPlusGenerator.Active && DunGenPlusGenerator.ActiveAlternative) {
        DunGenPlusGenerator.RandomizeLineArchetypes(__instance, true);
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonGenerator), "GenerateBranchPaths")]
    public static void GenerateBranchPathsPatch(ref DungeonGenerator __instance, ref IEnumerator __result){
      if (DunGenPlusGenerator.Active && DunGenPlusGenerator.ActiveAlternative) {
        __result = DunGenPlusGenerator.GenerateAlternativeMainPaths(__instance);
      }
    }

    // Transpiler that injects ModifyGroupBasedOnBehaviourSimple after the
    // attachTo local is captured in GenerateMainPath's enumerator. The other
    // half of this transpiler (the "archetype node" sequence) is gone — that
    // pattern (list.Add(null) for archetype) no longer exists in current
    // DunGen; archetype overrides are now handled in AddTilePrefix below
    // via TilePlacementParameters instead.
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(DungeonGenerator), "GenerateMainPath", MethodType.Enumerator)]
    public static IEnumerable<CodeInstruction> GenerateMainPathPatch(IEnumerable<CodeInstruction> instructions){
      var attachToSequence = new InstructionSequenceStandard("attach to");
      attachToSequence.AddBasicLocal(OpCodes.Stloc_S, 13);

      foreach(var instruction in instructions){
        if (attachToSequence.VerifyStage(instruction)){
          yield return instruction;
          var modifyMethod = typeof(MainRoomDoorwayGroups).GetMethod("ModifyGroupBasedOnBehaviourSimple", BindingFlags.Public | BindingFlags.Static);
          yield return new CodeInstruction(OpCodes.Ldloc_S, 13);
          yield return new CodeInstruction(OpCodes.Call, modifyMethod);
          continue;
        }
        yield return instruction;
      }

      attachToSequence.ReportComplete();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(DungeonGenerator), "GenerateMainPath", MethodType.Enumerator)]
    public static IEnumerable<CodeInstruction> GenerateMainPathGetLineAtDepthPatch(IEnumerable<CodeInstruction> instructions){
      var getLineFunction = typeof(DungeonFlow).GetMethod("GetLineAtDepth", BindingFlags.Instance | BindingFlags.Public);
      var nodesField = typeof(DungeonFlow).GetField("Nodes", BindingFlags.Instance | BindingFlags.Public);

      var lineSequence = new InstructionSequenceStandard("GetLineAtDepth");
      lineSequence.AddBasic(OpCodes.Callvirt, getLineFunction);

      var nodesSequence = new InstructionSequenceStandard("Nodes", false);
      nodesSequence.AddBasic(OpCodes.Ldfld, nodesField);

      foreach(var instruction in instructions){
        if (lineSequence.VerifyStage(instruction)) {
          var specialFunction = typeof(DunGenPlusGenerator).GetMethod("GetLineAtDepth", BindingFlags.Static | BindingFlags.Public);
          yield return new CodeInstruction(OpCodes.Call, specialFunction);
          continue;
        }

        if (nodesSequence.VerifyStage(instruction)) {
          var specialFunction = typeof(DunGenPlusGenerator).GetMethod("GetNodes", BindingFlags.Static | BindingFlags.Public);
          yield return new CodeInstruction(OpCodes.Call, specialFunction);
          continue;
        }

        yield return instruction;
      }

      lineSequence.ReportComplete();
      nodesSequence.ReportComplete();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(DungeonGenerator), "InnerGenerate", MethodType.Enumerator)]
    public static IEnumerable<CodeInstruction> InnerGenerateLengthPatch(IEnumerable<CodeInstruction> instructions){
      var lengthField = typeof(DungeonFlow).GetField("Length", BindingFlags.Instance | BindingFlags.Public);
      var getIsEditor = typeof(Application).GetMethod("get_isEditor", BindingFlags.Static | BindingFlags.Public);

      var lengthSequence = new InstructionSequenceStandard("Length");
      lengthSequence.AddBasic(OpCodes.Ldfld, lengthField);

      var editorCheck = new InstructionSequenceStandard("Editor");
      editorCheck.AddBasic(OpCodes.Call, getIsEditor);

      foreach(var instruction in instructions){
        if (lengthSequence.VerifyStage(instruction)) {
          var specialFunction = typeof(DunGenPlusGenerator).GetMethod("GetLength", BindingFlags.Static | BindingFlags.Public);
          yield return new CodeInstruction(OpCodes.Call, specialFunction);
          continue;
        }

        if (editorCheck.VerifyStage(instruction)){
          var specialFunction = typeof(DunGenPlusGenerator).GetMethod("AllowRetryStop", BindingFlags.Static | BindingFlags.Public);
          yield return instruction;
          yield return new CodeInstruction(OpCodes.Call, specialFunction);
          continue;
        }

        yield return instruction;
      }

      lengthSequence.ReportComplete();
      editorCheck.ReportComplete();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonGenerator), "PostProcess")]
    public static void PostProcessPatch(ref DungeonGenerator __instance){
      if (DunGenPlusGenerator.Active) {
        var value = __instance.RandomStream.Next(999);
        Components.Props.SpawnSyncedObjectCycle.UpdateCycle(value);
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DungeonGenerator), "ProcessGlobalProps")]
    public static bool ProcessGlobalPropsPatch(ref DungeonGenerator __instance){
      if (DunGenPlusGenerator.Active){
        var anyGlobalSettings = DunGenPlusGenerator.Properties.MainPathProperties.DetailedGlobalPropSettings.Count > 0;
        var anyLocalSettings = DunGenPlusGenerator.Properties.MainPathProperties.MainPathDetails.Any(d => d.LocalGroupProps.Count > 0);
        if (anyGlobalSettings || anyLocalSettings){
          Plugin.logger.LogDebug("Performing Local Global Props algorithm");
          DunGenPlusGenerator.ProcessGlobalPropsPerMainPath(__instance);
          return false;
        }
      }
      return true;
    }

    public static TileProxy lastAttachTo;
    public static IEnumerable<TileSet> lastUseableTileSets;
    public static float lastNormalizedDepth;
    public static DungeonArchetype lastArchetype;

    // Replaces both the old AddTileDebugPatch prefix (arg capture) AND the
    // archetype-node transpiler. Modifying placementParams.Archetype here is
    // equivalent to the old transpiler's "list.Add(modifiedArchetype)" trick,
    // and works against the new DunGen architecture where archetype lives on
    // TilePlacementParameters.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(DungeonGenerator), "AddTile")]
    public static void AddTilePrefix(DungeonGenerator __instance, TileProxy attachTo, IEnumerable<TileSet> useableTileSets, float normalizedDepth, TilePlacementParameters placementParams){
      lastAttachTo = attachTo;
      lastUseableTileSets = useableTileSets;
      lastNormalizedDepth = normalizedDepth;
      lastArchetype = placementParams?.Archetype;

      if (placementParams?.Node != null) {
        placementParams.Archetype = DunGenPlusGenerator.ModifyMainBranchNodeArchetype(
          placementParams.Archetype, placementParams.Node, __instance.RandomStream);
      }
    }

    // Replaces the old "Add Tile Placement" transpiler (which read the
    // result local at a specific IL offset) and the "TileProxyNull" debug
    // transpiler. Reads the most recent TilePlacementResult off the
    // generator's tilePlacementResults list and feeds it to the debug
    // helpers, with no IL pattern matching required.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonGenerator), "AddTile")]
    public static void AddTilePostfix(DungeonGenerator __instance, TileProxy __result){
      var listField = AccessTools.Field(typeof(DungeonGenerator), "tilePlacementResults");
      if (listField?.GetValue(__instance) is IList list && list.Count > 0) {
        if (list[list.Count - 1] is TilePlacementResult last) {
          DunGenPlusGenerator.RecordLastTilePlacementResult(__instance, last);
        }
      }

      if (__result == null) {
        DunGenPlusGenerator.PrintAddTileErrorQuick(__instance, 0);
      }
    }
  }
}
