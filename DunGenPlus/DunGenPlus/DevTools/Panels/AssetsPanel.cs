using DunGen;
using DunGenPlus.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DunGenPlus.DevTools.Panels
{
  internal class AssetsPanel : BasePanel {

    public static AssetsPanel Instance { get; internal set; }

    public override void AwakeCall() {
      Instance = this; 
    }

    public override void SetPanelVisibility(bool visible){
      base.SetPanelVisibility(visible);
      if (visible) UpdatePanel(false);
    }

    public void UpdatePanel(bool refreshPanel){
      if (refreshPanel) {
        ClearPanel();
        SetupPanel();
      }
    }

    public void SetupPanel(){
      var parentTransform = mainGameObject.transform;
      manager.CreateListExtendedUIField(parentTransform, "Archetypes", selectedAssetCache.archetypes.list.Select(t => t.Item).Where(t => t != null).ToList(), false);
      manager.CreateSpaceUIField(parentTransform);

      manager.CreateListExtendedUIField(parentTransform, "Tilesets", selectedAssetCache.tileSets.list.Select(t => t.Item).Where(t => t != null).ToList(), false);
      manager.CreateSpaceUIField(parentTransform);

      manager.CreateListExtendedUIField(parentTransform, "Tiles", selectedAssetCache.tiles.list.Select(t => t.Item).Where(t => t != null).ToList(), false);
      manager.CreateSpaceUIField(parentTransform);

      manager.CreateListExtendedUIField(parentTransform, "Main Paths", selectedAssetCache.mainPathExtenders.list.Select(t => t.Item).Where(t => t != null).ToList(), false);
    }

    public void ClearPanel(){
      manager.ClearTransformChildren(mainGameObject.transform);
    }

  }
}
