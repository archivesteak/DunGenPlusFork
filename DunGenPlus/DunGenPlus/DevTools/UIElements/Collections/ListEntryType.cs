using DunGen;
using DunGen.Graph;
using DunGenPlus.Collections;
using DunGenPlus.DevTools.Panels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DunGenPlus.DevTools.UIElements.Collections {
  internal abstract class ListEntryType {
    public abstract object CreateEmptyObject();
    public abstract void CreateEntry(IList list, int index, Transform parentTransform, float layoutOffset);

    public virtual bool UseCustomElementText() => false;

    public virtual string GetCustomElementText(IList list, int index) => string.Empty;

  }

  internal class ListEntryDungeonArchetype : ListEntryType {
    public override object CreateEmptyObject() => null;

    public override void CreateEntry(IList list, int index, Transform parentTransform, float layoutOffset) {
      var entry = (DungeonArchetype)list[index];
      var baseValue = DevDebugManager.Instance.selectedAssetCache.archetypes.dictionary[entry];
      DevDebugManager.Instance.CreateArchetypeOptionsUIField(parentTransform, new TitleParameter("Archetype", layoutOffset), baseValue, (t) => list[index] = t);
    }
  }

  internal class ListEntryDungeonArchetypeExtended : ListEntryType {
    public override object CreateEmptyObject() => null;

    public override void CreateEntry(IList list, int index, Transform parentTransform, float layoutOffset) {
      var entry = (DungeonArchetype)list[index];
      DevDebugManager.Instance.CreateListUIField(parentTransform, "Tile Sets", entry.TileSets);
      DevDebugManager.Instance.CreateEnumOptionsUIField<BranchCapType>(parentTransform, "Branch Cap Type", (int)entry.BranchCapType, (t) => entry.BranchCapType = t);
      DevDebugManager.Instance.CreateListUIField(parentTransform, "Branch Cap Tile Sets", entry.BranchCapTileSets);

      DevDebugManager.Instance.CreateIntRangeInputField(parentTransform, "Branch Count", entry.BranchCount, (t) => entry.BranchCount = t);
      DevDebugManager.Instance.CreateIntRangeInputField(parentTransform, "Branching Depth", entry.BranchingDepth, (t) => entry.BranchingDepth = t);
      
      DevDebugManager.Instance.CreateFloatInputField(parentTransform, "Straigten Chance", new FloatParameter(entry.StraightenChance, 0f, 1f), (t) => entry.StraightenChance = t);
      DevDebugManager.Instance.CreateBoolInputField(parentTransform, "Unique", entry.Unique, (t) => entry.Unique = t);

      DevDebugManager.Instance.CreateSpaceUIField(parentTransform);
    }

    public override bool UseCustomElementText() => true;

    public override string GetCustomElementText(IList list, int index) {
      var entry = (DungeonArchetype)list[index];
      return entry.name;
    }
  }

  internal class ListEntryTileSet : ListEntryType {
    public override object CreateEmptyObject() => null;

    public override void CreateEntry(IList list, int index, Transform parentTransform, float layoutOffset) {
      var entry = (TileSet)list[index];
      var baseValue = DevDebugManager.Instance.selectedAssetCache.tileSets.dictionary[entry];
      DevDebugManager.Instance.CreateTileSetsOptionsUIField(parentTransform, new TitleParameter("Tile Set", layoutOffset), baseValue, (t) => list[index] = t);
    }
  }

  internal class ListEntryTileExtended : ListEntryType {
    public override object CreateEmptyObject() => null;

    public override void CreateEntry(IList list, int index, Transform parentTransform, float layoutOffset) {
      var entry = (GameObject)list[index];
      var tile = entry.GetComponent<Tile>();

      DevDebugManager.Instance.CreateEnumOptionsUIField<TileRepeatMode>(parentTransform, "Repeat Mode", (int)tile.RepeatMode, (t) => tile.RepeatMode = t);
      DevDebugManager.Instance.CreateBoolInputField(parentTransform, "Allow Imm. Repeats", tile.allowImmediateRepeats, (t) => tile.allowImmediateRepeats = t);
      DevDebugManager.Instance.CreateBoolInputField(parentTransform, "Allow Rotation", tile.AllowRotation, (t) => tile.AllowRotation = t);
      DevDebugManager.Instance.CreateFloatInputField(parentTransform, "Connection Chance", new FloatParameter(tile.ConnectionChance, 0f, 1f), (t) => tile.ConnectionChance = t);  
      
      DevDebugManager.Instance.CreateSpaceUIField(parentTransform);
    }

    public override bool UseCustomElementText() => true;

    public override string GetCustomElementText(IList list, int index) {
      var entry = (GameObject)list[index];
      return entry.name;
    }
  }

  internal class ListEntryTileSetExtended : ListEntryType {
    public override object CreateEmptyObject() => null;

    public override void CreateEntry(IList list, int index, Transform parentTransform, float layoutOffset) {
      var entry = (TileSet)list[index];
      var weights = entry.TileWeights.Weights;

      DevDebugManager.Instance.CreateListUIField(parentTransform, "Weights", weights);

      DevDebugManager.Instance.CreateSpaceUIField(parentTransform);
    }

    public override bool UseCustomElementText() => true;

    public override string GetCustomElementText(IList list, int index) {
      var entry = (TileSet)list[index];
      return entry.name;
    }
  }

  internal class ListEntryGameObjectChance : ListEntryType {
    public override object CreateEmptyObject() {
      var item = new GameObjectChance();
      item.TileSet = null;
      item.DepthWeightScale = null;
      return item;
    }

    public override void CreateEntry(IList list, int index, Transform parentTransform, float layoutOffset) {
      var entry = (GameObjectChance)list[index];

      DevDebugManager.Instance.CreateTileOptionsUIField(parentTransform, "Tile", DevDebugManager.Instance.selectedAssetCache.tiles.dictionary[entry.Value], (t) => entry.Value = t);
      DevDebugManager.Instance.CreateFloatInputField(parentTransform, "Main Path Weight", entry.MainPathWeight, (t) => entry.MainPathWeight = t);
      DevDebugManager.Instance.CreateFloatInputField(parentTransform, "Branch Path Weight", entry.BranchPathWeight, (t) => entry.BranchPathWeight = t);
      DevDebugManager.Instance.CreateAnimationCurveOptionsUIField(parentTransform, "Depth Weight Scale", entry.DepthWeightScale, (t) => entry.DepthWeightScale = t);
    }
  }

  internal class ListEntryMainPathExtender : ListEntryType {
    public override object CreateEmptyObject() => null;

    public override void CreateEntry(IList list, int index, Transform parentTransform, float layoutOffset) {
      var entry = (MainPathExtender)list[index];
      var baseValue = DevDebugManager.Instance.selectedAssetCache.mainPathExtenders.dictionary[entry];
      DevDebugManager.Instance.CreateMainPathExtenderUIField(parentTransform, new TitleParameter("Main Path Extender", layoutOffset), baseValue, (t) => list[index] = t);
    }
  }

  internal class ListEntryMainPathExtenderExtended : ListEntryType {
    public override object CreateEmptyObject() => null;

    public override void CreateEntry(IList list, int index, Transform parentTransform, float layoutOffset) {
      var entry = (MainPathExtender)list[index];
      
      Transform CreateOverrideTransform<T>(PropertyOverride<T> property, string title){
        var transform = DevDebugManager.Instance.CreateVerticalLayoutUIField(parentTransform);
        DevDebugManager.Instance.CreateBoolInputField(parentTransform, new TitleParameter(title, layoutOffset), property.Override, (t) => {
          property.Override = t;
          transform.gameObject.SetActive(t);
          });
          transform.SetAsLastSibling();
        return transform;
      }

      var branchModeTransform = CreateOverrideTransform(entry.BranchMode, "Branch Mode Override");
      DevDebugManager.Instance.CreateEnumOptionsUIField<BranchMode>(branchModeTransform, new TitleParameter("Branch Mode", layoutOffset), (int)entry.BranchMode.Value, (t) => entry.BranchMode.Value = t);

      var branchCodeTransform = CreateOverrideTransform(entry.BranchCount, "Branch Count Override");
      DevDebugManager.Instance.CreateIntRangeInputField(branchCodeTransform, new TitleParameter("Branch Code", layoutOffset), entry.BranchCount.Value, (t) => entry.BranchCount.Value = t);

      var lengthTransform = CreateOverrideTransform(entry.Length, "Length Override");
      DevDebugManager.Instance.CreateIntRangeInputField(lengthTransform, new TitleParameter("Length", layoutOffset), entry.Length.Value, (t) => entry.Length.Value = t);

      var nodesTransform = CreateOverrideTransform(entry.Nodes, "Nodes Override");
      DevDebugManager.Instance.CreateListUIField(nodesTransform, new TitleParameter("Nodes", layoutOffset), entry.Nodes.Value);

      var linesTransform = CreateOverrideTransform(entry.Lines, "Lines Override");
      DevDebugManager.Instance.CreateListUIField(linesTransform, new TitleParameter("Lines", layoutOffset), entry.Lines.Value);

    }

    public override bool UseCustomElementText() => true;

    public override string GetCustomElementText(IList list, int index) {
      var entry = (MainPathExtender)list[index];
      return entry.name;
    }

  }

  internal class ListEntryNodeArchetype : ListEntryType {
    public override object CreateEmptyObject() => new NodeArchetype();

    public override void CreateEntry(IList list, int index, Transform parentTransform, float layoutOffset) {
      var entry = (NodeArchetype)list[index];
      DevDebugManager.Instance.CreateStringInputField(parentTransform, new TitleParameter("Label", NodeArchetype.LabelTooltip, layoutOffset), entry.Label, (t) => entry.Label = t);
      DevDebugManager.Instance.CreateListUIField(parentTransform, new TitleParameter("Archetypes", NodeArchetype.ArchetypesTooltip, layoutOffset), entry.Archetypes);
    }
  }

  internal class ListEntryAdditionalTileSetList : ListEntryType {
    public override object CreateEmptyObject() {
      var forcedTileset = new AdditionalTileSetList(); 
      forcedTileset.DepthWeightScale = null;
      return forcedTileset;
    }

    public override void CreateEntry(IList list, int index, Transform parentTransform, float layoutOffset) {
      var entry = (AdditionalTileSetList)list[index];
      DevDebugManager.Instance.CreateFloatInputField(parentTransform, new TitleParameter("Main Path Weight", AdditionalTileSetList.MainPathWeightTooltip, layoutOffset), entry.MainPathWeight, (t) => entry.MainPathWeight = t);
      DevDebugManager.Instance.CreateFloatInputField(parentTransform, new TitleParameter("Branch Path Weight", AdditionalTileSetList.BranchPathWeightTooltip, layoutOffset), entry.BranchPathWeight, (t) => entry.BranchPathWeight = t);

      // depth is weird cause we have to account for every entry's unique depth curve, even if they don't have one
      DevDebugManager.Instance.CreateAnimationCurveOptionsUIField(parentTransform, new TitleParameter("Depth Weight Scale", AdditionalTileSetList.DepthWeightScaleTooltip, layoutOffset), entry.DepthWeightScale, (t) => entry.DepthWeightScale = t);
      DevDebugManager.Instance.CreateListUIField(parentTransform, new TitleParameter("Tile Sets", AdditionalTileSetList.TileSetsTooltip, layoutOffset), entry.TileSets);
    }
  }

  internal class ListEntryTileInjectionRule : ListEntryType {

    public override object CreateEmptyObject() => new TileInjectionRule();

    public override void CreateEntry(IList list, int index, Transform parentTransform, float layoutOffset){
      var entry = (TileInjectionRule)list[index];
      var baseValue = DevDebugManager.Instance.selectedAssetCache.tileSets.dictionary[entry.TileSet];
      DevDebugManager.Instance.CreateTileSetsOptionsUIField(parentTransform, "Tile Set", baseValue, (t) => entry.TileSet = t);

      DevDebugManager.Instance.CreateFloatRangeInputField(parentTransform, "Norm. Path Depth", entry.NormalizedPathDepth, (t) => entry.NormalizedPathDepth = t);
      DevDebugManager.Instance.CreateFloatRangeInputField(parentTransform, "Norm. Branch Depth", entry.NormalizedBranchDepth, (t) => entry.NormalizedBranchDepth = t);

      DevDebugManager.Instance.CreateBoolInputField(parentTransform, "Appear On Main Path", entry.CanAppearOnMainPath, (t) => entry.CanAppearOnMainPath = t);
      DevDebugManager.Instance.CreateBoolInputField(parentTransform, "Appear On Branch Path", entry.CanAppearOnBranchPath, (t) => entry.CanAppearOnBranchPath = t);
      DevDebugManager.Instance.CreateBoolInputField(parentTransform, "Is Required", entry.IsRequired, (t) => entry.IsRequired = t);
    }
  }

  internal class ListEntryGraphNode : ListEntryType {

    public override object CreateEmptyObject() => new GraphNode(DevDebugManager.Instance.selectedDungeonFlow);

    public override void CreateEntry(IList list, int index, Transform parentTransform, float layoutOffset){
      var entry = (GraphNode)list[index];
      DevDebugManager.Instance.CreateListUIField(parentTransform, "Tile Sets", entry.TileSets);
      DevDebugManager.Instance.CreateEnumOptionsUIField<NodeType>(parentTransform, "Node Type", (int)entry.NodeType, (t) => entry.NodeType = t);
      DevDebugManager.Instance.CreateFloatInputField(parentTransform, "Position", new FloatParameter(entry.Position, 0f, 1f), (t) => entry.Position = t);
      DevDebugManager.Instance.CreateStringInputField(parentTransform, "Label", entry.Label, (t) => entry.Label = t);
    }
  }

  internal class ListEntryGraphLine : ListEntryType {

    public override object CreateEmptyObject() => new GraphLine(DevDebugManager.Instance.selectedDungeonFlow);

    public override void CreateEntry(IList list, int index, Transform parentTransform, float layoutOffset){
      var entry = (GraphLine)list[index];
      DevDebugManager.Instance.CreateListUIField(parentTransform, "Dungeon Archetypes", entry.DungeonArchetypes);
      DevDebugManager.Instance.CreateFloatInputField(parentTransform, "Position", new FloatParameter(entry.Position, 0f, 1f), (t) => entry.Position = t);
      DevDebugManager.Instance.CreateFloatInputField(parentTransform, "Length", new FloatParameter(entry.Length, 0f, 1f), (t) => entry.Length = t);
    }
  }

}
