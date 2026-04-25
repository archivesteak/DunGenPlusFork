using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DunGen;
using DunGen.Adapters;
using DunGenPlus.Components;
using DunGenPlus.Components.Scripting;
using DunGenPlus.Generation;
using DunGenPlus.Utils;

namespace DunGenPlus.Managers {
  public static class DoorwayManager {

    public static ActionList onMainEntranceTeleportSpawnedEvent = new ActionList("onMainEntranceTeleportSpawned");
    //public static List<DoorwayCleanup> doorwayCleanupList;

    public class Scripts {
      public List<IDunGenScriptingParent> scriptList;
      public List<Action> actionList;

      public Scripts(){
        scriptList = new List<IDunGenScriptingParent>();
        actionList = new List<Action>();
      }

      public void Add(IDunGenScriptingParent script) {
        scriptList.Add(script);
      }

      public void Add(Action action) {
        actionList.Add(action);
      }

      public bool Call(){
        foreach(var s  in scriptList){
          s.Call();
        }

        foreach(var a in actionList){
          a.Invoke();
        }

        return scriptList.Count + actionList.Count > 0;
      }

    }

    public static Dictionary<DunGenScriptingHook, Scripts> scriptingLists;

    public static void ResetList(){
      //doorwayCleanupList = new List<DoorwayCleanup>();
      scriptingLists = new Dictionary<DunGenScriptingHook, Scripts>();
      foreach(DunGenScriptingHook e in Enum.GetValues(typeof(DunGenScriptingHook))){
        scriptingLists.Add(e, new Scripts());
      }
    }

    public static void AddDunGenScriptHook(IDunGenScriptingParent script){
      scriptingLists[script.GetScriptingHook].Add(script);
    }

    public static void AddActionHook(DunGenScriptingHook hook, Action action){
      scriptingLists[hook].Add(action);
    }

    public static void OnMainEntranceTeleportSpawnedFunction(){
      if (DunGenPlusGenerator.Active) {
          
        //foreach(var d in doorwayCleanupList){
        //  d.SetBlockers(false);
        //  d.Cleanup();
        //  Plugin.logger.LogWarning(d.GetComponentInParent<Tile>().gameObject.name);
        //}

        var anyFunctionCalled = false;
        foreach(var d  in scriptingLists.Values){
          anyFunctionCalled = anyFunctionCalled | d.Call();
        }

        // we can leave early if doorway cleanup is not used (most likely for most dungeons anyway)
        if (!anyFunctionCalled) return;

        try{
          var dungeonGen = RoundManager.Instance.dungeonGenerator;
          var navmesh = dungeonGen.transform.parent.GetComponentInChildren<UnityNavMeshAdapter>();
          navmesh.Run(dungeonGen.Generator);
          Plugin.logger.LogDebug("Rebuild nav mesh");
        } catch (Exception e){
          Plugin.logger.LogError("Failed to rebuild nav mesh");
          Plugin.logger.LogError(e.ToString());
        }
        
      }
    }

    public static void SetLevelObjectVariablesFunction(){
      if (DunGenPlusGenerator.Active) {
        scriptingLists[DunGenScriptingHook.SetLevelObjectVariables ].Call();
      }
    }

  }
}
