using DunGen;
using DunGen.Graph;
using DunGenPlus.DevTools.Panels.Collections;
using LethalLevelLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DunGenPlus.DevTools.Panels {
  internal abstract class BasePanel : MonoBehaviour {

    public DevDebugManager manager => DevDebugManager.Instance;
    protected RuntimeDungeon dungeon => manager.dungeon;
    protected ExtendedDungeonFlow selectedExtendedDungeonFlow => manager.selectedExtendedDungeonFlow;
    protected DungeonFlow selectedDungeonFlow => manager.selectedDungeonFlow;
    protected DungeonFlowCacheAssets selectedAssetCache => manager.selectedAssetCache;

    [Header("Renders")]  
    public GameObject mainGameObject;
    public PanelTab tab;

    public virtual void AwakeCall() {
      
    }

    public virtual void SetPanelVisibility(bool visible) {
      mainGameObject.SetActive(visible);
      tab.active = visible;
    }

    protected int ParseTextInt(string text, int defaultValue = 0) {
      if (int.TryParse(text, out var result)){
        return result;
      } else {
        Plugin.logger.LogWarning($"Couldn't parse {text} into an int");
        return defaultValue;
      }
    }

    protected float ParseTextFloat(string text, float defaultValue = 0f) {
      if (float.TryParse(text, out var result)){
        return result;
      } else {
        Plugin.logger.LogWarning($"Couldn't parse {text} into a float");
        return defaultValue;
      }
    }


  }
}
