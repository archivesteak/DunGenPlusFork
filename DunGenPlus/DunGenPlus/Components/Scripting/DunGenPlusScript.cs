using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Soukoku.ExpressionParser;
using DunGen;

namespace DunGenPlus.Components.Scripting {

  public enum ScriptActionType {
    SwitchToConnector,
    SwitchToBlocker,
    SetNamedReferenceState
  }

  [System.Serializable]
  public struct ScriptAction {
    public ScriptActionType type;
    public string namedReference;
    public bool boolValue;

    public void CallAction(IDunGenScriptingParent parent){
      switch(type){
        case ScriptActionType.SwitchToConnector:
          parent.SetNamedGameObjectState("connectors", true);
          parent.SetNamedGameObjectState("blockers", false);
          break;
        case ScriptActionType.SwitchToBlocker:
          parent.SetNamedGameObjectState("connectors", false);
          parent.SetNamedGameObjectState("blockers", true);
          break;
        case ScriptActionType.SetNamedReferenceState:
          parent.SetNamedGameObjectState(namedReference, boolValue);
          break;
      }
    }
  }
  
  public class DunGenPlusScript : MonoBehaviour {

    public static bool InDebugMode = false;

    public string expression;
    public List<ScriptAction> actions;

    public bool EvaluateExpression(IDunGenScriptingParent parent){
      var context = parent.CreateContext();
      var evaluator = new Evaluator(context);
      try {
        InDebugMode = false;
        var results = evaluator.Evaluate(expression, true);
        return results.ToDouble(context) > 0;
      } catch (Exception e) {
        Plugin.logger.LogError($"Expression [{expression}] could not be parsed. Returning false");
        Plugin.logger.LogError(e.ToString());
      }
      
      return false;
    }

    [ContextMenu("Verify")]
    public void VerifyExpression(){
      var context = GetComponent<IDunGenScriptingParent>().CreateContext();
      var evaluator = new Evaluator(context);
      try {
        InDebugMode = true;
        var results = evaluator.Evaluate(expression, false);
        Debug.Log($"Expression parsed successfully: {results.ToString()} ({evaluator.ConvertTokenToFalseTrue(results).ToString()})");
      } catch (Exception e) {
        Debug.LogError($"Expression [{expression}] could not be parsed");
        Debug.LogError(e.ToString());
      } 
    }

    public void Call(IDunGenScriptingParent parent){
      if (EvaluateExpression(parent)){
        foreach(var action in actions) action.CallAction(parent);
      }
    }

  }
}
