using DunGen;
using DunGenPlus.Managers;
using Soukoku.ExpressionParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DunGenPlus.Components.Scripting {

  public class DoorwayScriptingParent : DunGenPlusScriptingParent<Doorway> {

    [Header("Scripting Debug")]
    public Doorway connectedDoorwayDebug;

    public override void Awake(){
      base.Awake();

      if (targetReference == null) return;

      // steal the scene objects from the doorway and clear them
      // before the doorway messes with them before us
      // psycho energy
      AddNamedReference("connectors", targetReference.ConnectorSceneObjects);
      targetReference.ConnectorSceneObjects = new List<GameObject>();

      AddNamedReference("blockers", targetReference.BlockerSceneObjects);
      targetReference.BlockerSceneObjects = new List<GameObject>();
    } 

    public override void Call(){
      if (targetReference == null) return;

      // start up like in original
      var isConnected = targetReference.connectedDoorway != null;
      SetNamedGameObjectState("connectors", isConnected);
      SetNamedGameObjectState("blockers", !isConnected);

      base.Call();
    }

    Doorway GetDoorway(string name){
      switch(name) {
        case "self":
          return targetReference;
        case "other":
          return InDebugMode ? connectedDoorwayDebug : targetReference.ConnectedDoorway;
        default:
          Utils.Utility.PrintLog($"{name} is not valid doorway expression. Please use 'self' or 'other'", BepInEx.Logging.LogLevel.Error);
          return null;

      }
    }

    public override EvaluationContext CreateContext() {
      var context = base.CreateContext();
      context.RegisterFunction("doorwaySpawnedGameObject", new FunctionRoutine(2, doorwaySpawnedGameObjectFunction));
      return context;
    }

    ExpressionToken doorwaySpawnedGameObjectFunction(EvaluationContext context, ExpressionToken[] parameters) {
      var targetName = parameters[0].Value;
      var target = GetDoorway(targetName);
      if (target != null) {
        var name = parameters[1].Value;
        foreach(Transform child in target.transform) {
          if (child.gameObject.activeSelf && child.name.Contains(name)) return ExpressionToken.True;
        }
      } 
      return ExpressionToken.False;
    }

    public override (object, ValueTypeHint) GetFields(string field) {
      var split = field.Split('.');

      if (split.Length <= 1) {
        Utils.Utility.PrintLog($"{field} is not a valid field", BepInEx.Logging.LogLevel.Error);
        return (0, ValueTypeHint.Auto);
      }

      var targetName = split[0];
      var target = GetDoorway(targetName);
      var getter = split[1];

      switch(getter) {
        case "priority":
          if (target != null){
            return (target.DoorPrefabPriority, ValueTypeHint.Auto);
          }
          return (0, ValueTypeHint.Auto);
        case "exists":
          return (target != null, ValueTypeHint.Auto);
        default:
          Utils.Utility.PrintLog($"{getter} is not a valid getter", BepInEx.Logging.LogLevel.Error);
          return (0, ValueTypeHint.Auto);
      }
    }

  }
}
