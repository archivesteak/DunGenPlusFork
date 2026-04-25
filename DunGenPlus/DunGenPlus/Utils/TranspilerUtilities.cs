using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using GameNetcodeStuff;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.Logging;
using UnityEngine;
using DunGenPlus;

namespace DunGenPlus.Utils {

  internal class InjectionDictionary {

    public string name;
    public List<CodeInstruction> instructions;
    public CodeInstruction[] injections;

    int counter;

    public InjectionDictionary(string name, MethodInfo methodInjection, params CodeInstruction[] instructions) {
      this.name = name;
      this.injections = new CodeInstruction[] { new CodeInstruction(OpCodes.Call, methodInjection) } ;
      this.instructions = instructions.ToList();
    }

    public InjectionDictionary(string name, CodeInstruction[] codeInjections, params CodeInstruction[] instructions) {
      this.name = name;
      this.injections = codeInjections;
      this.instructions = instructions.ToList();
    }

    public void ResetCounter(){
      counter = 0;
    }

    public void AddCounter() {
      counter++;
    }

    public void Report(string debugFunction, int? expectedCounter){
      if (counter == 0) {
        Plugin.logger.LogError($"{debugFunction} could not inject {name}. Probably scary");
      } else if (!expectedCounter.HasValue) {
        Plugin.logger.LogDebug($"{debugFunction} inject {name} {counter} time(s)");
      } else if (expectedCounter.Value != counter){
        Plugin.logger.LogWarning($"{debugFunction} inject {name} {counter} time(s) (Expected {expectedCounter.Value}). Probably not an error but be warned");
      }
    }

  }

  internal abstract class InstructionSequence {
    protected List<Func<CodeInstruction, bool>> seq;
    protected string name;
    protected string extraErrorMessage;
    protected int stage;
    protected bool completed;
    protected bool single;

    public InstructionSequence(string name, bool single = true, string extraErrorMessage = default(string)) {
      seq = new List<Func<CodeInstruction, bool>>();
      stage = 0;
      completed = false;

      this.name = name;
      this.single = single;
      this.extraErrorMessage = extraErrorMessage;
    }

    public void Add(Func<CodeInstruction, bool> next){
      seq.Add(next);
    }

    public void AddBasic(OpCode opcode){
      seq.Add((i) => i.opcode == opcode);
    }

    public void AddBasic(OpCode opcode, object operand){
      seq.Add((i) => i.opcode == opcode && i.operand == operand);
    }

    public void AddBasicLocal(OpCode opcode, int operand){
      seq.Add((i) => i.opcode == opcode && (i.operand as LocalBuilder).LocalIndex == operand);
    }

    public void AddAny(){
      seq.Add(null);
    }

    public void AddOperandTypeCheck(OpCode opcode, Type operandType){
      seq.Add((i) => {
        var fieldInfo = i.operand as FieldInfo;
        if (i.opcode == opcode && fieldInfo != null) {
          return fieldInfo.FieldType == operandType;
        }
        return false;
      });
    }

    public void AddBasicWithAlternateMethodName(OpCode opcode, object operand, string methodName){
      seq.Add((i) => {
        if (i.opcode == opcode && i.operand == operand) return true;

        var mth = i.operand as MethodInfo;
        if (mth != null && mth.Name == methodName) return true;
        
        return false;
      });

    }

    public void AddSpecial(OpCode opcode, Func<CodeInstruction, bool> extra){
      seq.Add((i) => i.opcode == opcode && extra.Invoke(i));
    }

    public void ReportComplete(){
      if (completed == false){
        var errorM = string.IsNullOrWhiteSpace(extraErrorMessage) ? "BIG PROBLEM!" : extraErrorMessage;
        Plugin.logger.LogError($"HarmonyTranspiler for {name} has failed. {errorM}");
      }
    }

    protected enum AdvanceResult {
      Failed,
      Advanced,
      Finished
    }

    protected AdvanceResult AdvanceStage(CodeInstruction current) {
      var s = seq[stage];
      var result = AdvanceResult.Failed;

      // null is magic number to accept anything,
      // increase the counter if the NEXT sequence succeeds
      // but not reset the counter if it fails
      if (s == null) {
        s = seq[stage + 1];
        if (s.Invoke(current)) {
          stage += 2;
        }
        result = AdvanceResult.Advanced;
      } else {
        if (s.Invoke(current)) {
          stage++;
          result = AdvanceResult.Advanced;
        } else {
          stage = 0;
          result = AdvanceResult.Failed;
        }
      }

      if (stage >= seq.Count){

        if (completed && single){
          throw new Exception($"Found multiple valid {name} instructions"); 
        }

        stage = 0;
        completed = true;
        result = AdvanceResult.Finished;
      }

      return result;
    }

  }

  internal class InstructionSequenceStandard : InstructionSequence {

    public InstructionSequenceStandard(string name, bool single = true, string extraErrorMessage = default(string)) : base(name, single, extraErrorMessage) { }

    public bool VerifyStage(CodeInstruction current){
      return AdvanceStage(current) == AdvanceResult.Finished;
    }
  }

  internal class InstructionSequenceHold : InstructionSequence {
    
    public enum HoldResult {
      None,
      Hold,
      Release,
      Finished
    }

    public List<CodeInstruction> Instructions;
    List<Func<CodeInstruction, bool>> seq;
    string name;
    string extraErrorMessage;
    int stage = 0;
    bool completed = false;

    public InstructionSequenceHold(string name, bool single = true, string extraErrorMessage = default(string)) : base(name, single, extraErrorMessage) {
      Instructions = new List<CodeInstruction>();
    }

    public HoldResult VerifyStage(CodeInstruction current) {
      var result = AdvanceStage(current);
      if (result == AdvanceResult.Failed) {
        if (Instructions.Count > 0) {
          Instructions.Add(current);
          return HoldResult.Release;
        }
        return HoldResult.None;
      }
      else if (result == AdvanceResult.Advanced) {
        Instructions.Add(current);
        return HoldResult.Hold;
      }
      else {
        Instructions.Add(current);
        return HoldResult.Finished;
      }
    }

    public void ClearInstructions(){
      Instructions.Clear();
    }


  }

  internal class TranspilerUtilities {

    public static IEnumerable<CodeInstruction> InjectMethod(IEnumerable<CodeInstruction> instructions, InjectionDictionary injection, string debugFunction, int? expectedCounter = default){
      var targets = injection.instructions;
      var codeInjections = injection.injections;
      injection.ResetCounter();

      foreach(var i in instructions){
        foreach(var t in targets){
          if (i.opcode == t.opcode && i.operand == t.operand){
            yield return i;
            foreach(var c in codeInjections) yield return c;
            injection.AddCounter();
            goto GoNext;
          }
        }
        yield return i;

        GoNext:;
      }

      injection.Report(debugFunction, expectedCounter);
    }

    public static bool IsInstructionNearFloatValue(CodeInstruction instruction, float value){
      return Mathf.Abs((float)instruction.operand - value) < 0.1f;
    }

    public static void PrintInstructions(IEnumerable<CodeInstruction> instructions) {
      foreach(var i in instructions){
        PrintInstruction(i);
      }
    }

    public static void PrintInstruction(CodeInstruction inst) {
      var opString = inst.opcode.ToString();
      var objString = inst.operand != null ? inst.operand.ToString() : "NULL";
      Plugin.logger.LogInfo($"{opString}: {objString}");
    }

  }
}
