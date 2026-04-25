using DunGen;
using DunGenPlus.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DunGenPlus.Generation;

namespace DunGenPlus.Patches {
  internal class DungeonProxyPatch {

    [HarmonyPatch(typeof(DungeonProxy), "ConnectOverlappingDoorways")]
    [HarmonyPrefix]
    public static void ConnectOverlappingDoorwaysPrePatch(ref DungeonProxy __instance){
      var enumerable = __instance.AllTiles.SelectMany(t => t.Doorways);
      DoorwaySistersRule.UpdateCache(enumerable);
    }


    [HarmonyTranspiler]
    [HarmonyPatch(typeof(DungeonProxy), "ConnectOverlappingDoorways")]
    public static IEnumerable<CodeInstruction> ConnectOverlappingDoorwaysPatch(IEnumerable<CodeInstruction> instructions){
      var callFunction = typeof(DunGen.Graph.DungeonFlow).GetMethod("CanDoorwaysConnect", BindingFlags.Instance | BindingFlags.Public);

      var sequence = new InstructionSequenceStandard("doorway connect", false);
      sequence.AddBasic(OpCodes.Callvirt, callFunction);
      sequence.AddBasic(OpCodes.Brfalse);

      foreach(var instruction in instructions){

        if (sequence.VerifyStage(instruction)){

          var method = typeof(DoorwaySistersRule).GetMethod("CanDoorwaysConnect", BindingFlags.Static | BindingFlags.Public);
          var getTileProxy = typeof(DoorwayProxy).GetMethod("get_TileProxy", BindingFlags.Instance | BindingFlags.Public);

          yield return new CodeInstruction(OpCodes.Ldloc_2);
          yield return new CodeInstruction(OpCodes.Callvirt, getTileProxy);
          yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
          yield return new CodeInstruction(OpCodes.Callvirt, getTileProxy);

          yield return new CodeInstruction(OpCodes.Ldloc_2);
          yield return new CodeInstruction(OpCodes.Ldloc_S, 4);

          yield return new CodeInstruction(OpCodes.Call, method);

          yield return instruction;

          continue;
        }

        yield return instruction;
      }

      sequence.ReportComplete();
    }


  }
}
