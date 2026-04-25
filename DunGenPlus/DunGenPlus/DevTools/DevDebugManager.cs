using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DunGen;
using UnityEngine.UI;
using TMPro;
using DunGen.Graph;
using LethalLevelLoader;
using UnityEngine.InputSystem;
using DunGenPlus.DevTools.Panels;
using DunGenPlus.DevTools.UIElements;
using DunGenPlus.Generation;
using DunGenPlus.DevTools.Panels.Collections;
using HarmonyLib;
using HarmonyLib.Public.Patching;

namespace DunGenPlus.DevTools {
  internal partial class DevDebugManager : MonoBehaviour {
    public static DevDebugManager Instance { get; private set; }

    public static bool IsActive => Instance != null;

    [Header("References")]
    public RuntimeDungeon dungeon;
    public GameObject devCamera;
    public BasePanel[] panels;
    public LayoutElement canvasLayoutElement;

    public TMP_Dropdown dungeonFlowSelectionDropDown;
    private ExtendedDungeonFlow[] dungeonFlows;

    internal ExtendedDungeonFlow selectedExtendedDungeonFlow;
    internal DungeonFlow selectedDungeonFlow;
    internal DungeonFlowCacheAssets selectedAssetCache;

    internal Dictionary<DungeonFlow, DungeonFlowCacheAssets> cacheDictionary = new Dictionary<DungeonFlow, DungeonFlowCacheAssets>();

    public TextMeshProUGUI statusTextMesh;
    public TextMeshProUGUI statsTextMesh;

    // fake
    private GameObject disabledGameObject;
    private RoundManager fakeRoundManager;

    // cache
    private Camera lastMainCamera;
    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;

    private Vector2 cameraYRange;

    // canvas
    public bool canvasExtended;
    private bool canvasExtendedInAnimation;
    private float canvasWidthTarget;

    void Awake(){
      Instance = this;

      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;

      CacheMainCamera();
      BeginDevCamera();
      GetAllDungeonFlows();
      
      foreach(var p in panels) p.AwakeCall();
      OpenPanel(0);
      UpdatePanels();

      dungeon.Generator.OnGenerationStatusChanged += OnDungeonFinished;

      disabledGameObject =  new GameObject("Disabled GOBJ");
      disabledGameObject.SetActive(false);
      disabledGameObject.transform.SetParent(transform);

      cameraYRange = new Vector2(devCamera.transform.position.y - 200f, devCamera.transform.position.y);

      canvasExtended = false;
      canvasWidthTarget = 440f;
    }

    void OnDestroy(){
      Instance = null;
      MainPanel.Instance = null;
      DunFlowPanel.Instance = null;
      DunGenPlusPanel.Instance = null;
      AssetsPanel.Instance = null;

      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;

      EndDevCamera();
    }

    void Update(){
      if (canvasExtendedInAnimation){
        var currentWidth = canvasLayoutElement.preferredWidth;
        if (Mathf.Abs(currentWidth - canvasWidthTarget) < 4f) {
          canvasLayoutElement.preferredWidth = canvasWidthTarget;
          canvasExtendedInAnimation = false;
        } else {
          canvasLayoutElement.preferredWidth = Mathf.Lerp(currentWidth, canvasWidthTarget, Time.deltaTime * 10f);
        }
      }
      

      statusTextMesh.text = dungeon.Generator.Status.ToString();

      if (!DevDebugOpen.IsSinglePlayerInShip()) {
        CloseDevDebug();
      }

      if (Mouse.current.middleButton.isPressed) {
        var delta = Mouse.current.delta.value;
        var movement = delta;
        devCamera.transform.position += new Vector3(-movement.x, 0f, -movement.y);
      }

      var scroll = Mouse.current.scroll.value.y;
      if (scroll != 0f) {
        var pos = devCamera.transform.position;
        pos.y = Mathf.Clamp(pos.y + scroll * -0.05f, cameraYRange.x, cameraYRange.y);
        devCamera.transform.position = pos;
      }
    }

    public void OpenPanel(int index) {
      for(var i = 0; i < panels.Length; ++i) {
        panels[i].SetPanelVisibility(i == index);
      }
    }

    public void ToggleCanvasExtended(){
      canvasExtendedInAnimation = true;
      canvasExtended = !canvasExtended;
      canvasWidthTarget = canvasExtended ? 800f : 440f;
    }

    public void SelectDungeonFlow(int index){
      selectedExtendedDungeonFlow = dungeonFlows[index];
      selectedDungeonFlow = selectedExtendedDungeonFlow.DungeonFlow;
      dungeon.Generator.DungeonFlow = selectedDungeonFlow;

      if (!cacheDictionary.TryGetValue(selectedDungeonFlow, out var cache)) {
        var extender = API.GetDunGenExtender(selectedDungeonFlow);
        cache = new DungeonFlowCacheAssets(selectedDungeonFlow, extender);
        cacheDictionary.Add(selectedDungeonFlow, cache);
      }
      selectedAssetCache = cache;

      UpdatePanels();
      Plugin.logger.LogInfo($"Selecting {selectedExtendedDungeonFlow.DungeonName}");
    }

    public void GenerateDungeon(){
      DeleteDungeon();
      Plugin.logger.LogInfo($"Generating dungeon: {dungeon.Generator.IsGenerating}");

      fakeRoundManager = disabledGameObject.AddComponent<RoundManager>();
      fakeRoundManager.dungeonGenerator = dungeon;

      selectedExtendedDungeonFlow.DungeonEvents.onBeforeDungeonGenerate?.Invoke(fakeRoundManager);
      DungeonManager.GlobalDungeonEvents?.onBeforeDungeonGenerate?.Invoke(fakeRoundManager);
      
      DunGenPlusGenerator.GenerateBranchBoostedPathsTime = 0f;
      DunGenPlusGenerator.GetTileResultTime = 0f;
      DunGenPlusGenerator.DoorwayPairTime = 0f;
      DunGenPlusGenerator.CalculateWeightTime = 0f;
      dungeon.Generate();
    }

    public void DeleteDungeon(){
      Plugin.logger.LogInfo($"Deleting dungeon");
      dungeon.Generator.Clear(true);
      dungeon.Generator.Cancel();

      dungeon.Generator.RestrictDungeonToBounds = false;

      if (fakeRoundManager) Destroy(fakeRoundManager);

      ClearTransformChildren(dungeon.Root.transform);
    }

    public void ClearTransformChildren(Transform root){
      var childCount = root.childCount;
      for(var i = childCount - 1; i >= 0; --i) {
        GameObject.Destroy(root.GetChild(i).gameObject);
      }
    }

    public void CloseDevDebug(){
      DeleteDungeon();
      Destroy(gameObject);
    }

    public void OnDungeonFinished(DungeonGenerator generator, GenerationStatus status) {
      // Albino said StringBuilder
      // I forget it even exists ngl
      var textList = new StringBuilder();

      // seeds
      textList.AppendLine($"Initial seed: {MainPanel.Instance.seedInputField.inputField.text}");
      textList.AppendLine($"Last seed: {generator.ChosenSeed}");

      if (status == GenerationStatus.Complete) {
        textList.AppendLine($"Tiles: {generator.CurrentDungeon.AllTiles.Count}");
        textList.AppendLine($"Main Tiles: {generator.CurrentDungeon.MainPathTiles.Count}");
        textList.AppendLine($"Branch Tiles: {generator.CurrentDungeon.BranchPathTiles.Count}");
        textList.AppendLine($"Doors: {generator.CurrentDungeon.Doors.Count}");
      } else if (status == GenerationStatus.Failed) {
        textList.AppendLine("<color=red>Failed</color>");
      }
      textList.AppendLine();

      var stats = generator.GenerationStats;
      textList.AppendLine("<u>DunGen</u>");
      textList.AppendLine($"Retrys: {stats.TotalRetries}");
      textList.AppendLine($"Pre Process Time: {stats.GetGenerationStepTime(GenerationStatus.PreProcessing):F2} ms");
      textList.AppendLine($"Main Path Time: {stats.GetGenerationStepTime(GenerationStatus.MainPath):F2} ms");
      textList.AppendLine($"Branch Path Time: {stats.GetGenerationStepTime(GenerationStatus.Branching):F2} ms");
      textList.AppendLine($"Post Process Time: {stats.GetGenerationStepTime(GenerationStatus.PostProcessing):F2} ms");
      textList.AppendLine($"Total Time: {stats.TotalTime:F2} ms");

      textList.AppendLine("");
      textList.AppendLine($"GenerateBranch Time: {DunGenPlusGenerator.GenerateBranchBoostedPathsTime:F2} ms");
      textList.AppendLine($"GetTileResult Time: {DunGenPlusGenerator.GetTileResultTime:F2} ms");
      textList.AppendLine($"DoorwayPair Time: {DunGenPlusGenerator.DoorwayPairTime:F2} ms");
      textList.AppendLine($"CalculateWeight Time: {DunGenPlusGenerator.CalculateWeightTime:F2} ms");

      /*
      var errors = generator.tilePlacementResultCounters;
      if (errors.Count > 0) {
        textList.AppendLine("");
        textList.AppendLine("Reasons for faliure:");
        foreach(var pair in errors){
          textList.AppendLine($"{pair.Key} (x{pair.Value})");
        }
      }
      */

      statsTextMesh.text = textList.ToString();
    }

    public void RecordNewSeed(int seed){
      MainPanel.Instance?.seedInputField.Set(seed);
    }

    private void UpdatePanels() {
      MainPanel.Instance?.UpdatePanel();
      DunFlowPanel.Instance?.UpdatePanel(true);
      DunGenPlusPanel.Instance?.UpdatePanel(true);
      AssetsPanel.Instance?.UpdatePanel(true);
    }

    public void UpdateDungeonBounds(){
      DunGenPlusPanel.Instance?.UpdateDungeonBoundsHelper();
    }

    private void GetAllDungeonFlows(){
      dungeonFlows = LethalLevelLoader.PatchedContent.ExtendedDungeonFlows.ToArray();
      dungeonFlowSelectionDropDown.options = dungeonFlows.Select(d => new TMP_Dropdown.OptionData(d.DungeonName)).ToList();
      SelectDungeonFlow(0);
    }

    private void CacheMainCamera() {
	    var main = Camera.main;
      if (main) {
        lastMainCamera = main;
        lastCameraPosition = main.transform.position;
        lastCameraRotation = main.transform.rotation;
      }
    }

    private void BeginDevCamera(){
      lastMainCamera?.gameObject.SetActive(false);
      devCamera.SetActive(true);
    }

    private void EndDevCamera(){
      devCamera.SetActive(false);
      if (lastMainCamera) {
        lastMainCamera.transform.position = lastCameraPosition;
        lastMainCamera.transform.rotation = lastCameraRotation;
        lastMainCamera.gameObject.SetActive(true);
      }
    }


  }
}
