using DunGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DunGenPlus.DevTools.Panels {
  internal class DunFlowPanel : BasePanel {
    public static DunFlowPanel Instance { get; internal set; }

    private GameObject branchPathParentGameobject;

    public override void AwakeCall(){
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

      var branchPathParentTransform = manager.CreateVerticalLayoutUIField(parentTransform);
      branchPathParentGameobject = branchPathParentTransform.gameObject;
      manager.CreateHeaderUIField(parentTransform, "Paths");
      manager.CreateIntRangeInputField(parentTransform, "Main Path Length", selectedDungeonFlow.Length, SetLength);
      manager.CreateEnumOptionsUIField<DunGen.BranchMode>(parentTransform, "Branch Mode", (int)selectedDungeonFlow.BranchMode, SetBranchMode);
      manager.CreateIntRangeInputField(branchPathParentTransform, "Branch Path Count", selectedDungeonFlow.BranchCount, SetBranchCount);
      branchPathParentTransform.SetAsLastSibling();
      manager.CreateSpaceUIField(parentTransform);

      manager.CreateHeaderUIField(parentTransform, "Generation");
      manager.CreateListUIField(parentTransform, "Tile Injection Rules", selectedDungeonFlow.TileInjectionRules);
      manager.CreateSpaceUIField(parentTransform);
      manager.CreateListUIField(parentTransform, "Nodes", selectedDungeonFlow.Nodes);
      manager.CreateSpaceUIField(parentTransform);
      manager.CreateListUIField(parentTransform, "Lines", selectedDungeonFlow.Lines);
      manager.CreateSpaceUIField(parentTransform);

      branchPathParentGameobject.SetActive(selectedDungeonFlow.BranchMode == BranchMode.Global);
    }

    public void ClearPanel(){
      manager.ClearTransformChildren(mainGameObject.transform);
    }


    public void SetLength(IntRange value){
      selectedDungeonFlow.Length = value;
    }

    public void SetBranchMode(DunGen.BranchMode value){
      selectedDungeonFlow.BranchMode = value;
      branchPathParentGameobject.SetActive(value == BranchMode.Global);
    }

    public void SetBranchCount(IntRange value){
      selectedDungeonFlow.BranchCount = value;
    }
  }
}
