using DunGen;
using DunGenPlus.DevTools.UIElements;
using DunGenPlus.DevTools.UIElements.Collections;
using LethalLevelLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DunGenPlus.DevTools.Panels {
  internal class MainPanel : BasePanel {

    public static MainPanel Instance { get; internal set; }

    internal IntInputField seedInputField;
    internal TextUIElement lengthMultiplierField;
    internal TextUIElement mapSizeMultiplierField;
    internal TextUIElement factorySizeMultiplierField;
    internal TextUIElement mapTileSizeField;

    internal ExtendedLevel[] levels;
    internal IEnumerable<string> levelOptions;
    private ExtendedLevel selectedLevel;

    private GameObject asyncParentGameobject;

    public override void AwakeCall(){
      Instance = this;

      GetAllLevels();

      var gen = dungeon.Generator;
      var parentTransform = mainGameObject.transform;

      manager.CreateHeaderUIField(parentTransform, "Dungeon Generator");
      seedInputField = manager.CreateIntInputField(parentTransform, "Seed", gen.Seed, SetSeed);
      manager.CreateBoolInputField(parentTransform, ("Randomize Seed", "If true, creates and saves a new seed when generating the dungeon."), gen.ShouldRandomizeSeed, SetRandomSeed);
      manager.CreateSpaceUIField(parentTransform);

      manager.CreateIntInputField(parentTransform, ("Max Attempts", "Maximum number of dungeon generation attempts before giving up."), new IntParameter(gen.MaxAttemptCount, 0, 500, 0), SetMaxAttempts);
      manager.CreateSpaceUIField(parentTransform);

      var asyncTransform = manager.CreateVerticalLayoutUIField(parentTransform);
      asyncParentGameobject = asyncTransform.gameObject;
      manager.CreateBoolInputField(parentTransform, ("Generate Async", "If true, visually generates the dungeon tile by tile."), gen.GenerateAsynchronously, SetGenerateAsync);
      manager.CreateFloatInputField(asyncTransform, "Max Async (ms)", new FloatParameter(gen.MaxAsyncFrameMilliseconds, 0f, float.MaxValue), SetMaxAsync);
      manager.CreateFloatInputField(asyncTransform, "Pause Between Rooms", new FloatParameter(gen.PauseBetweenRooms, 0f, float.MaxValue), SetPauseBetweenRooms);
      asyncTransform.SetAsLastSibling();
      manager.CreateSpaceUIField(parentTransform);

      manager.CreateHeaderUIField(parentTransform, "Levels");
      manager.CreateLevelOptionsUIField(parentTransform, "Level", 0, SetLevel);
      lengthMultiplierField = manager.CreateTextUIField(parentTransform, ("Length Multiplier", "Dungeon generation length multiplier based on the numerous factors."));
      mapSizeMultiplierField = manager.CreateTextUIField(parentTransform, ("Map Size Multiplier", "Map size multiplier based on the round manager (fixed)."));
      factorySizeMultiplierField = manager.CreateTextUIField(parentTransform, ("Factory Size Multiplier", "Factory size multiplier based on the level."));
      mapTileSizeField = manager.CreateTextUIField(parentTransform, ("Map Tile Size", "Map tile size based on the dungeon."));

      SetLevel(levels[0]);

      asyncParentGameobject.SetActive(gen.GenerateAsynchronously);
    }

    public void UpdatePanel(){
      SetLevel(selectedLevel);
    }

    public void SetSeed(int value) {
      dungeon.Generator.Seed = value;
    }

    public void SetRandomSeed(bool state) {
      dungeon.Generator.ShouldRandomizeSeed = state;
    }

    public void SetMaxAttempts(int value) {
      dungeon.Generator.MaxAttemptCount = value;
    }

    public void SetGenerateAsync(bool state) {
      dungeon.Generator.GenerateAsynchronously = state;
      asyncParentGameobject.SetActive(state);
    }

    public void SetMaxAsync(float value) {
      dungeon.Generator.MaxAsyncFrameMilliseconds = value;
    }

    public void SetPauseBetweenRooms(float value) {
      dungeon.Generator.PauseBetweenRooms = value;
    }

    private void GetAllLevels(){
      levels = LethalLevelLoader.PatchedContent.ExtendedLevels.ToArray();
      levelOptions = levels.Select(l => l.NumberlessPlanetName);
    }

    public void SetLevel(ExtendedLevel level){
      var currentValues = GetLevelMultiplier(level);
      dungeon.Generator.LengthMultiplier = currentValues.lengthMultiplier;
      manager.UpdateDungeonBounds();

      var lengthString = currentValues.lengthMultiplier.ToString("F2");
      var mapSizeString = currentValues.mapSizeMultiplier.ToString("F2");
      var factoryString = currentValues.factorySizeMultiplier.ToString("F2");
      var tileString = currentValues.mapTileSize.ToString("F2");

      lengthMultiplierField.SetText($"Length multiplier: {lengthString} [{mapSizeString} / {tileString} * {factoryString}]");
      mapSizeMultiplierField.SetText($"Map size multiplier: {mapSizeString}");
      factorySizeMultiplierField.SetText($"Factory size multiplier: {factoryString}");
      mapTileSizeField.SetText($"Map tile size: {tileString}");

      selectedLevel = level;
    }

    private (float lengthMultiplier, float mapSizeMultiplier, float factorySizeMultiplier, float mapTileSize) GetLevelMultiplier(ExtendedLevel level){
      var roundManager = RoundManager.Instance;
      var mapSizeMultiplier = 1f;
      if (roundManager != null) {
        mapSizeMultiplier = roundManager.mapSizeMultiplier;
      } else {
        Plugin.logger.LogError("RoundManager somehow null.");
      }

      var factorySizeMultiplier = level.SelectableLevel.factorySizeMultiplier;
      var mapTileSize = selectedExtendedDungeonFlow.MapTileSize;

      var num2 = mapSizeMultiplier / mapTileSize * factorySizeMultiplier; 
      num2 = (float)((double)Mathf.Round(num2 * 100f) / 100f);
      return (num2, mapSizeMultiplier, factorySizeMultiplier, mapTileSize);
    }

  }
}
