using BepInEx.Logging;
using DunGenPlus.Components.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace DunGenPlus.Utils {
  public class ActionList {
    public string name;
    public List<(string name, Action action)> actionList;
    public List<(string name, Action action)> temporaryActionList;

    public ActionList(string name){
      this.name = name;
      actionList = new List<(string, Action)>();
      temporaryActionList = new List<(string name, Action action)>();
    }

    public void AddEvent(string name, Action act){
      actionList.Add((name, act));
    }

    public void AddTemporaryEvent(string name, Action act){
      temporaryActionList.Add((name, act)); 
    }

    public void Call(){
      foreach(var pair in actionList){
        try {
          pair.action.Invoke();
        } catch (Exception e) {
          Plugin.logger.LogError($"Error with event {name}/{pair.name}");
          Plugin.logger.LogError(e.ToString());
        }
      }

      foreach(var pair in temporaryActionList){
        try {
          pair.action.Invoke();
        } catch (Exception e) {
          Plugin.logger.LogError($"Error with event {name}/{pair.name}");
          Plugin.logger.LogError(e.ToString());
        }
      }

      ClearTemporaryActionList();
    }

    public void ClearTemporaryActionList(){
      temporaryActionList.Clear();
    }
  }

  public static class Utility {

    public static void PrintLog(string message, LogLevel logLevel){
      if (DunGenPlusScript.InDebugMode){
        switch(logLevel){
          case LogLevel.Error:
          case LogLevel.Fatal:
            Debug.LogError(message); 
            break;
          case LogLevel.Warning:
            Debug.LogWarning(message);
            break;
          default:
            Debug.Log(message);
            break;
        }
      } else {
        Plugin.logger.Log(logLevel, message);
      }
    }
  }

}
