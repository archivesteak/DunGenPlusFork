using DunGen;
using DunGen.Graph;
using DunGenPlus.Collections;
using LethalLevelLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using DunGenPlus.DevTools.UIElements.Collections;

namespace DunGenPlus.DevTools.Panels {
  internal class DunGenPlusPanel : BasePanel {

    public static DunGenPlusPanel Instance { get; internal set; }

    internal DunGenExtender selectedExtenderer;

    [Header("Panel References")]
    public GameObject createGameObject;
    public GameObject selectedGameObject;
    public GameObject selectedListGameObject;

    [Header("Dungeon Bounds Helper")]
    public GameObject dungeonBoundsHelperGameObject;

    private GameObject mainPathParentGameobject;
    private GameObject dungeonBoundsParentGameobject;
    private GameObject archetypesNodesParentGameobject;
    private GameObject forcedTilesParentGameobject;
    private GameObject branchLoopBoostParentGameobject;
    private GameObject maxShadowsParentGameobject;
    public bool eventCallbackValue = true;

    public override void AwakeCall() {
      Instance = this;

      dungeonBoundsHelperGameObject.SetActive(false);
    }

    public override void SetPanelVisibility(bool visible) {
      base.SetPanelVisibility(visible);
      if (visible) UpdatePanel(false);
    }

    public void CreateDunGenPlusExtenderer(){
      var asset = API.CreateDunGenExtender(selectedDungeonFlow);
      selectedDungeonFlow.TileInjectionRules = new List<TileInjectionRule>();
      API.AddDunGenExtender(asset);

      UpdatePanel(true);
    }

    public void UpdatePanel(bool refreshPanel){
      var hasAsset = API.ContainsDungeonFlow(selectedDungeonFlow);
      selectedGameObject.SetActive(hasAsset);
      createGameObject.SetActive(!hasAsset);

      if (refreshPanel) {
        ClearPanel();
        if (hasAsset) {
          SetupPanel(); 
        } else {
          selectedExtenderer = null;
          dungeonBoundsHelperGameObject.SetActive(false);
        }
      }
      
    }

    internal const string ActivateDunGenPlusTooltip = "If disabled, the dungeon generation will ignore this DunGenPlusExtender asset and simply create a vanilla dungeon instead when generating.";
    internal const string EventCallbackScenarioTooltip = "Sets the EventCallbackScenario.IsDevDebug value";

    public void SetupPanel() {
      selectedExtenderer = API.GetDunGenExtender(selectedDungeonFlow);

      var parentTransform = selectedListGameObject.transform;
      var properties = selectedExtenderer.Properties;
      manager.CreateBoolInputField(parentTransform, ("Activate DunGenPlus", ActivateDunGenPlusTooltip), selectedExtenderer.Active, SetActivateDunGenPlus);
      manager.CreateBoolInputField(parentTransform, ("EventCallbackScenario state", EventCallbackScenarioTooltip), eventCallbackValue, SetDebugCallbackState);
      manager.CreateSpaceUIField(parentTransform);

      var mainPathTransform = manager.CreateVerticalLayoutUIField(parentTransform);
      mainPathParentGameobject = mainPathTransform.gameObject;
      manager.CreateHeaderUIField(parentTransform, "Main Path");
      manager.CreateIntSliderField(parentTransform, ("Main Path Count", MainPathProperties.MainPathCountTooltip), new IntParameter(properties.MainPathProperties.MainPathCount, 1, 10), SetMainPathCount);
      mainPathTransform.SetAsLastSibling();
      manager.CreateTileOptionsUIField(mainPathTransform, ("Main Room Tile Prefab", MainPathProperties.MainRoomTilePrefabTooltip), selectedAssetCache.tiles.dictionary[properties.MainPathProperties.MainRoomTilePrefab], SetMainRoom);
      manager.CreateEnumOptionsUIField<DunGenExtenderProperties.CopyNodeBehaviour>(mainPathTransform, ("Copy Node Behaviour", MainPathProperties.CopyNodeBehaviourTooltip), (int)properties.MainPathProperties.CopyNodeBehaviour, SetCopyNodeBehaviour);
      manager.CreateListUIField(mainPathTransform, ("Main Path Details", MainPathProperties.MainPathDetailsTooltip), properties.MainPathProperties.MainPathDetails);
      manager.CreateSpaceUIField(parentTransform);

      var dungeonBoundsTransform = manager.CreateVerticalLayoutUIField(parentTransform);
      dungeonBoundsParentGameobject = dungeonBoundsTransform.gameObject;
      manager.CreateHeaderUIField(parentTransform, "Dungeon Bounds");
      manager.CreateBoolInputField(parentTransform, ("Use Dungeon Bounds", DungeonBoundsProperties.UseDungeonBoundsTooltip), properties.DungeonBoundsProperties.UseDungeonBounds, SetUseDungeonBounds);
      dungeonBoundsTransform.SetAsLastSibling();
      manager.CreateVector3InputField(dungeonBoundsTransform, ("Size Base", DungeonBoundsProperties.SizeBaseTooltip), properties.DungeonBoundsProperties.SizeBase, SetDungeonBoundsSizeBase);
      manager.CreateVector3InputField(dungeonBoundsTransform, ("Size Factor", DungeonBoundsProperties.SizeFactorTooltip), properties.DungeonBoundsProperties.SizeFactor, SetDungeonBoundsSizeFactor);
      manager.CreateVector3InputField(dungeonBoundsTransform, ("Position Offset", DungeonBoundsProperties.PositionOffsetTooltip), properties.DungeonBoundsProperties.PositionOffset, SetDungeonBoundsPosOffset);
      manager.CreateVector3InputField(dungeonBoundsTransform, ("Position Pivot", DungeonBoundsProperties.PositionPivotTooltip), properties.DungeonBoundsProperties.PositionPivot, SetDungeonBoundsPosPivot);
      manager.CreateSpaceUIField(parentTransform);

      var archetypesTransform = manager.CreateVerticalLayoutUIField(parentTransform);
      archetypesNodesParentGameobject = archetypesTransform.gameObject;
      manager.CreateHeaderUIField(parentTransform, "Archetypes Normal Nodes");
      manager.CreateBoolInputField(parentTransform, ("Add Archetypes", NormalNodeArchetypesProperties.AddArchetypesToNormalNodesTooltip), properties.NormalNodeArchetypesProperties.AddArchetypesToNormalNodes, SetAddArchetypes);
      archetypesTransform.SetAsLastSibling();
      manager.CreateListUIField(archetypesTransform, "Normal Node Archetypes", properties.NormalNodeArchetypesProperties.NormalNodeArchetypes);
      manager.CreateSpaceUIField(parentTransform);

      var forcedTilesTransform = manager.CreateVerticalLayoutUIField(parentTransform);
      forcedTilesParentGameobject = forcedTilesTransform.gameObject;
      manager.CreateHeaderUIField(parentTransform, "Additional Tiles");
      manager.CreateBoolInputField(parentTransform, ("Use Additional Tiles", AdditionalTilesProperties.UseAdditionalTilesTooltip), properties.AdditionalTilesProperties.UseAdditionalTiles, SetUseForcedTiles);
      forcedTilesTransform.SetAsLastSibling();
      manager.CreateListUIField(forcedTilesTransform, ("Additional Tile Sets", AdditionalTilesProperties.AdditionalTileSetsTooltip), properties.AdditionalTilesProperties.AdditionalTileSets);
      manager.CreateSpaceUIField(parentTransform);

      var branchLoopTransform = manager.CreateVerticalLayoutUIField(parentTransform);
      branchLoopBoostParentGameobject = branchLoopTransform.gameObject;
      manager.CreateHeaderUIField(parentTransform, "Branch Path Multi Sim");
      manager.CreateBoolInputField(parentTransform, ("Use Branch Path Multi Sim", BranchPathMultiSimulationProperties.UseBranchPathMultiSimTooltip), properties.BranchPathMultiSimulationProperties.UseBranchPathMultiSim, SetUseBranchPathMultiSim);
      branchLoopTransform.SetAsLastSibling();
      manager.CreateIntInputField(branchLoopTransform, ("Simulation Count", BranchPathMultiSimulationProperties.SimulationCountTooltip), new IntParameter(properties.BranchPathMultiSimulationProperties.SimulationCount, 1, 10, 1), SetSimulationCount);
      manager.CreateFloatInputField(branchLoopTransform, ("Length Weight Scale", BranchPathMultiSimulationProperties.LengthWeightScaleTooltip), new FloatParameter(properties.BranchPathMultiSimulationProperties.LengthWeightScale, 0f, 2f, 0f), SetLengthScale);
      manager.CreateFloatInputField(branchLoopTransform, ("Norm. Length Weight Scale", BranchPathMultiSimulationProperties.NormalizedLengthWeightScaleTooltip), new FloatParameter(properties.BranchPathMultiSimulationProperties.NormalizedLengthWeightScale, 0f, 2f, 0f), SetNormalizedLengthScale);
      manager.CreateSpaceUIField(branchLoopTransform);

      manager.CreateTextUIField(branchLoopTransform, ("Same Path Connect", "Weight scale for branch paths that start from a main path and connect later to the same main path."));
      manager.CreateFloatInputField(branchLoopTransform, ("Base Weight Scale", BranchPathMultiSimulationProperties.SamePathBaseWeightScaleTooltip), new FloatParameter(properties.BranchPathMultiSimulationProperties.SamePathBaseWeightScale, 0f, 2f, 0f), SamePathBaseConnectScale);
      manager.CreateFloatInputField(branchLoopTransform, ("Depth Weight Scale", BranchPathMultiSimulationProperties.SamePathDepthWeightScaleTooltip), new FloatParameter(properties.BranchPathMultiSimulationProperties.SamePathDepthWeightScale, 0f, 2f, 0f), SamePathConnectDepthScale);
      manager.CreateFloatInputField(branchLoopTransform, ("Norm. Depth Weight Scale", BranchPathMultiSimulationProperties.SamePathNormalizedDepthWeightTooltip), new FloatParameter(properties.BranchPathMultiSimulationProperties.SamePathNormalizedDepthWeightScale, 0f, 2f, 0f), SamePathConnectNormalizedDepthScale);
      manager.CreateSpaceUIField(branchLoopTransform);

      manager.CreateTextUIField(branchLoopTransform, ("Diff Path Connect", "Weight scale for branch paths that start from a main path and connect later to a different main path."));
      manager.CreateFloatInputField(branchLoopTransform, ("Base Weight Scale", BranchPathMultiSimulationProperties.DiffPathBaseWeightScaleTooltip), new FloatParameter(properties.BranchPathMultiSimulationProperties.DiffPathBaseWeightScale, 0f, 2f, 0f), DiffPathBaseConnectScale);
      manager.CreateFloatInputField(branchLoopTransform, ("Depth Weight Scale", BranchPathMultiSimulationProperties.DiffPathDepthWeightScaleTooltip), new FloatParameter(properties.BranchPathMultiSimulationProperties.DiffPathDepthWeightScale, 0f, 2f, 0f), DiffPathConnectDepthScale);
      manager.CreateFloatInputField(branchLoopTransform, ("Norm. Depth Weight Scale", BranchPathMultiSimulationProperties.DiffPathNormalizedDepthWeightTooltip), new FloatParameter(properties.BranchPathMultiSimulationProperties.DiffPathNormalizedDepthWeightScale, 0f, 2f, 0f), DiffPathConnectNormalizedDepthScale);
      manager.CreateSpaceUIField(branchLoopTransform);

      manager.CreateHeaderUIField(parentTransform, "Miscellaneous");
      var maxShadowTransform = manager.CreateVerticalLayoutUIField(parentTransform);
      maxShadowsParentGameobject = maxShadowTransform.gameObject;
      manager.CreateBoolInputField(parentTransform, ("Use Max Shadows Request", MiscellaneousProperties.UseMaxShadowsRequestUpdateTooltip), properties.MiscellaneousProperties.UseMaxShadowsRequestUpdate, SetUseMaxShadows);
      manager.CreateIntInputField(maxShadowTransform, ("Shadows Request Amount", MiscellaneousProperties.MaxShadowsRequestCountTooltip), new IntParameter(properties.MiscellaneousProperties.MaxShadowsRequestCount, 4, 20, 4), SetMaxShadowsCount);
      maxShadowTransform.SetAsLastSibling();

      manager.CreateBoolInputField(parentTransform, ("Use Doorway Sisters", MiscellaneousProperties.UseDoorwaySistersTooltip), properties.MiscellaneousProperties.UseDoorwaySisters, SetUseDoorwaySisters);
      manager.CreateBoolInputField(parentTransform, ("Use Random Guaranteed Scrap", MiscellaneousProperties.UseRandomGuaranteedScrapSpawnTooltip), properties.MiscellaneousProperties.UseRandomGuaranteedScrapSpawn, SetUseRandomGuaranteedScrap);
      manager.CreateSpaceUIField(parentTransform);

      mainPathParentGameobject.SetActive(properties.MainPathProperties.MainPathCount > 1);
      dungeonBoundsParentGameobject.SetActive(properties.DungeonBoundsProperties.UseDungeonBounds);
      dungeonBoundsHelperGameObject.SetActive(properties.DungeonBoundsProperties.UseDungeonBounds);
      archetypesNodesParentGameobject.SetActive(properties.NormalNodeArchetypesProperties.AddArchetypesToNormalNodes);
      forcedTilesParentGameobject.SetActive(properties.AdditionalTilesProperties.UseAdditionalTiles);
      branchLoopBoostParentGameobject.SetActive(properties.BranchPathMultiSimulationProperties.UseBranchPathMultiSim);
      maxShadowsParentGameobject.SetActive(properties.MiscellaneousProperties.UseMaxShadowsRequestUpdate);

      UpdateDungeonBoundsHelper();
    }

    public void ClearPanel(){
      manager.ClearTransformChildren(selectedListGameObject.transform);
    }

    public void SetActivateDunGenPlus(bool state){
      selectedExtenderer.Active = state;
    }

    public void SetDebugCallbackState(bool state){
      eventCallbackValue = state;
    }

    public void SetMainPathCount(int value) {
      selectedExtenderer.Properties.MainPathProperties.MainPathCount = value;
      mainPathParentGameobject.SetActive(value > 1);
    }

    public void SetMainRoom(GameObject value) {
      selectedExtenderer.Properties.MainPathProperties.MainRoomTilePrefab = value;
    }

    public void SetCopyNodeBehaviour(DunGenExtenderProperties.CopyNodeBehaviour value) {
      selectedExtenderer.Properties.MainPathProperties.CopyNodeBehaviour = value;
    }

    public void SetUseDungeonBounds(bool state){
      selectedExtenderer.Properties.DungeonBoundsProperties.UseDungeonBounds = state;
      dungeonBoundsHelperGameObject.SetActive(state);
      dungeonBoundsParentGameobject.SetActive(state);
    }

    public void UpdateDungeonBoundsHelper(){
      if (selectedExtenderer == null) return;

      var t = dungeonBoundsHelperGameObject.transform;
      var result = selectedExtenderer.Properties.DungeonBoundsProperties.GetDungeonBounds(dungeon.Generator.LengthMultiplier);
      t.localPosition = result.center;
      t.localScale = result.size;
    }

    public void SetDungeonBoundsSizeBase(Vector3 value) {
      selectedExtenderer.Properties.DungeonBoundsProperties.SizeBase = value;
      UpdateDungeonBoundsHelper();
    }

    public void SetDungeonBoundsSizeFactor(Vector3 value) {
      selectedExtenderer.Properties.DungeonBoundsProperties.SizeFactor = value;
      UpdateDungeonBoundsHelper();
    }

    public void SetDungeonBoundsPosOffset(Vector3 value) {
      selectedExtenderer.Properties.DungeonBoundsProperties.PositionOffset = value;
      UpdateDungeonBoundsHelper();
    }

    public void SetDungeonBoundsPosPivot(Vector3 value) {
      selectedExtenderer.Properties.DungeonBoundsProperties.PositionPivot = value;
      UpdateDungeonBoundsHelper();
    }

    public void SetAddArchetypes(bool state){
      selectedExtenderer.Properties.NormalNodeArchetypesProperties.AddArchetypesToNormalNodes = state;
      archetypesNodesParentGameobject.SetActive(state);
    }

    public void SetUseForcedTiles(bool state){
      selectedExtenderer.Properties.AdditionalTilesProperties.UseAdditionalTiles = state;
      forcedTilesParentGameobject.SetActive(state);
    }

    public void SetUseBranchPathMultiSim(bool state){
      selectedExtenderer.Properties.BranchPathMultiSimulationProperties.UseBranchPathMultiSim = state;
      branchLoopBoostParentGameobject.SetActive(state);
    }

    public void SetSimulationCount(int value){
      selectedExtenderer.Properties.BranchPathMultiSimulationProperties.SimulationCount = value;
    }

    public void SetLengthScale(float value){
      selectedExtenderer.Properties.BranchPathMultiSimulationProperties.LengthWeightScale = value;
    }

    public void SetNormalizedLengthScale(float value){
      selectedExtenderer.Properties.BranchPathMultiSimulationProperties.NormalizedLengthWeightScale = value;
    }

    public void SamePathBaseConnectScale(float value){
      selectedExtenderer.Properties.BranchPathMultiSimulationProperties.SamePathBaseWeightScale = value;
    }

    public void DiffPathBaseConnectScale(float value){
      selectedExtenderer.Properties.BranchPathMultiSimulationProperties.DiffPathBaseWeightScale = value;
    }

    public void SamePathConnectDepthScale(float value){
      selectedExtenderer.Properties.BranchPathMultiSimulationProperties.SamePathDepthWeightScale = value;
    }

    public void DiffPathConnectDepthScale(float value){
      selectedExtenderer.Properties.BranchPathMultiSimulationProperties.DiffPathDepthWeightScale = value;
    }

    public void SamePathConnectNormalizedDepthScale(float value){
      selectedExtenderer.Properties.BranchPathMultiSimulationProperties.SamePathNormalizedDepthWeightScale = value;
    }

    public void DiffPathConnectNormalizedDepthScale(float value){
      selectedExtenderer.Properties.BranchPathMultiSimulationProperties.DiffPathNormalizedDepthWeightScale = value;
    }


    public void SetUseMaxShadows(bool state){
      selectedExtenderer.Properties.MiscellaneousProperties.UseMaxShadowsRequestUpdate = state;
      maxShadowsParentGameobject.SetActive(state);
    }

    public void SetMaxShadowsCount(int value){
      selectedExtenderer.Properties.MiscellaneousProperties.MaxShadowsRequestCount = value;
    }

    public void SetUseDoorwaySisters(bool state){
      selectedExtenderer.Properties.MiscellaneousProperties.UseDoorwaySisters = state;
    }

    public void SetUseRandomGuaranteedScrap(bool state){
      selectedExtenderer.Properties.MiscellaneousProperties.UseRandomGuaranteedScrapSpawn = state;
    }

    public void RestoreOriginalState(){
      selectedExtenderer.Properties.CopyFrom(selectedAssetCache.originalProperties, DunGenExtender.CURRENT_VERSION);
      ClearPanel();
      SetupPanel();
    }

  }
}
