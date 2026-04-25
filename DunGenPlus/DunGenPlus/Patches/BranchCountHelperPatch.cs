using DunGen;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using DunGenPlus.Utils;
using DunGenPlus.Generation;
using DunGenPlus.Managers;
using DunGenPlus.Collections;
using DunGenPlus.DevTools;
using DunGen.Graph;
using UnityEngine;
using DunGenPlus.Components;

namespace DunGenPlus.Patches
{
  internal class BranchCountHelperPatch {
    
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(BranchCountHelper), "ComputeBranchCounts")]
    public static IEnumerable<CodeInstruction> ComputeBranchCountsPatch(IEnumerable<CodeInstruction> instructions){

      var branchModeField = typeof(DungeonFlow).GetField("BranchMode", BindingFlags.Instance | BindingFlags.Public);

      var branchSequence = new InstructionSequenceStandard("BranchMode", false);
      branchSequence.AddBasic(OpCodes.Ldfld, branchModeField);

      foreach(var instruction in instructions){
        if (branchSequence.VerifyStage(instruction)) {
          var specialFunction = typeof(DunGenPlusGenerator).GetMethod("GetBranchMode", BindingFlags.Static | BindingFlags.Public);

          yield return new CodeInstruction(OpCodes.Call, specialFunction);

          continue;
        }

        yield return instruction;
      }

      branchSequence.ReportComplete();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(BranchCountHelper), "ComputeBranchCountsGlobal")]
    public static IEnumerable<CodeInstruction> ComputeBranchCountsGlobalPatch(IEnumerable<CodeInstruction> instructions){

      var branchCountField = typeof(DungeonFlow).GetField("BranchCount", BindingFlags.Instance | BindingFlags.Public);

      var branchSequence = new InstructionSequenceStandard("BranchCount");
      branchSequence.AddBasic(OpCodes.Ldfld, branchCountField);

      foreach(var instruction in instructions){
        if (branchSequence.VerifyStage(instruction)) {
          var specialFunction = typeof(DunGenPlusGenerator).GetMethod("GetBranchCount", BindingFlags.Static | BindingFlags.Public);

          yield return new CodeInstruction(OpCodes.Call, specialFunction);

          continue;
        }

        yield return instruction;
      }

      branchSequence.ReportComplete();
    }

  }
}
