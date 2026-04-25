using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DunGenPlus.Collections.DunGenExtenderProperties;
using UnityEngine;
using DunGen;

namespace DunGenPlus.Collections {

  [System.Serializable]
  public class MainPathProperties {

    internal const string MainPathCountTooltip = "The number of main paths.\n\n1 means no additional main paths\n3 means two additional main paths\netc.";
    internal const string MainRoomTilePrefabTooltip = "The Tile prefab where the additional main paths will start from.\n\nCannot be null if MainPathCount is more than 1.";
    internal const string CopyNodeBehaviourTooltip = "Defines how the nodes list is copied onto the additional main paths.\n\nCopyFromMainPathPosition: nodes will copy based on the MainRoomTilePrefab's position in the main path.\nCopyFromNodeList: nodes will copy based on the MainRoomTilePrefab's position in the node list + 1.";
    internal const string MainPathDetailsTooltip = "Overrides certain DungeonFlow values during the main path generation.\n\nThe order of items in this list correspond to the order of the main paths being generated.\nThe first item in this list will activate for the first main path, the second item for the second main path, and so on. If there are more main paths than items in this list, the last item is used instead.";
    internal const string DetailedGlobalPropSettingsTooltip = "Additional settings for how Global Props spawn.";

    [Tooltip(MainPathCountTooltip)]
    [Range(1, 9)]
    public int MainPathCount = 1;
    [Tooltip(MainRoomTilePrefabTooltip)]
    public GameObject MainRoomTilePrefab;
    [Tooltip(CopyNodeBehaviourTooltip)]
    public CopyNodeBehaviour CopyNodeBehaviour = CopyNodeBehaviour.CopyFromMainPathPosition;
    [Tooltip(MainPathDetailsTooltip)]
    public List<MainPathExtender> MainPathDetails = new List<MainPathExtender>();
    [Tooltip(DetailedGlobalPropSettingsTooltip)]
    public List<DetailedGlobalPropSettings> DetailedGlobalPropSettings = new List<DetailedGlobalPropSettings>();

    public MainPathExtender GetMainPathDetails(int index) {
      var count = MainPathDetails.Count;
      if (count == 0) return null;
      if (index < count) return MainPathDetails[index];
      return MainPathDetails[count - 1];
    }

    internal void CopyFrom(MainPathProperties props) {
      MainPathCount = props.MainPathCount;
      MainRoomTilePrefab = props.MainRoomTilePrefab;
      CopyNodeBehaviour = props.CopyNodeBehaviour;
      MainPathDetails = props.MainPathDetails;
      DetailedGlobalPropSettings = props.DetailedGlobalPropSettings;
    }

    internal MainPathProperties Copy() {
      var copy = new MainPathProperties();
      copy.CopyFrom(this);
      return copy;
    }
  }

  [System.Serializable]
  public class DungeonBoundsProperties {

    internal const string UseDungeonBoundsTooltip = "If enabled, restricts the dungeon's generation to the bounds described below.\n\nThis will help in condensing the dungeon, but it will increase the chance of dungeon generation failure (potentially guarantees failure if the bounds is too small).";
    internal const string SizeBaseTooltip = "The base size of the bounds.";
    internal const string SizeFactorTooltip = "The factor that's multiplied with the base size AND the dungeon's size. The resulting value is added to the base size of the bounds.\n\n0 means that the bound size is not influenced by the dungeon's size and is therefore a constant.";
    internal const string PositionOffsetTooltip = "The base positional offset of the bounds.";
    internal const string PositionPivotTooltip = "The pivot of the bounds.";

    [Tooltip(UseDungeonBoundsTooltip)]
    public bool UseDungeonBounds = false;
    [Tooltip(SizeBaseTooltip)]
    public Vector3 SizeBase = new Vector3(120f, 40f, 80f);
    [Tooltip(SizeFactorTooltip)]
    public Vector3 SizeFactor = new Vector3(1f, 0.5f, 1f);
    [Tooltip(PositionOffsetTooltip)]
    public Vector3 PositionOffset = Vector3.zero;
    [Tooltip(PositionPivotTooltip)]
    public Vector3 PositionPivot = new Vector3(0.5f, 0.5f, 0.5f);

    internal void CopyFrom(DungeonBoundsProperties props) {
      UseDungeonBounds = props.UseDungeonBounds;
      SizeBase = props.SizeBase;
      SizeFactor = props.SizeFactor;
      PositionOffset = props.PositionOffset;
      PositionPivot = props.PositionPivot;
    }

    internal DungeonBoundsProperties Copy() {
      var copy = new DungeonBoundsProperties();
      copy.CopyFrom(this);
      return copy;
    }

    internal Bounds GetDungeonBounds(float dungeonScale) {
      var size = SizeBase + Vector3.Scale(SizeBase * (dungeonScale - 1), SizeFactor);
      var offset = PositionOffset + Vector3.Scale(size, PositionPivot - Vector3.one * 0.5f);
      return new Bounds(offset, size);
    }

  }

  [System.Serializable]
  public class NormalNodeArchetypesProperties {

    internal const string AddArchetypesToNormalNodesTooltip = "If enabled, adds archetypes to the normal nodes in the DungeonFlow.\n\nBy default, nodes cannot have branching paths since they don't have archetype references. This allows nodes to have branching paths.";

    [Tooltip(AddArchetypesToNormalNodesTooltip)]
    public bool AddArchetypesToNormalNodes = false;
    public List<NodeArchetype> NormalNodeArchetypes = new List<NodeArchetype>();
    internal Dictionary<string, NodeArchetype> _normalNodeArchetypesDictioanry;
    internal NodeArchetype _defaultNodeArchetype;

    internal void CopyFrom(NormalNodeArchetypesProperties props)
    {
      AddArchetypesToNormalNodes = props.AddArchetypesToNormalNodes;
      NormalNodeArchetypes = props.NormalNodeArchetypes;
    }

    internal NormalNodeArchetypesProperties Copy()
    {
      var copy = new NormalNodeArchetypesProperties();
      copy.CopyFrom(this);
      return copy;
    }

    internal void SetupProperties(DungeonGenerator generator)
    {
      _normalNodeArchetypesDictioanry = new Dictionary<string, NodeArchetype>();
      _defaultNodeArchetype = null;

      foreach (var n in NormalNodeArchetypes)
      {
        if (_normalNodeArchetypesDictioanry.ContainsKey(n.Label))
        {
          Plugin.logger.LogError($"Label {n.Label} already exists. Ignoring latest entry.");
          continue;
        }
        _normalNodeArchetypesDictioanry.Add(n.Label, n);

        if (string.IsNullOrWhiteSpace(n.Label))
        {
          _defaultNodeArchetype = n;
        }
      }
    }

    internal DungeonArchetype GetRandomArchetype(string label, RandomStream randomStream) {
      NodeArchetype node;
      if (!_normalNodeArchetypesDictioanry.TryGetValue(label, out node))
      {
        node = _defaultNodeArchetype;
      }

      if (node != null)
      {
        var archetypes = node.Archetypes;
        var count = archetypes.Count;
        if (count == 0) return null;

        var index = randomStream.Next(0, count);
        return archetypes[index];
      }

      return null;
    }

  }

  [System.Serializable]
  public class ForcedTilesProperties {

    internal const string UseForcedTilesTooltip = "If enabled, attempts to forcefully spawn tiles from ForcedTileSets after branching paths are generated.";
    internal const string ForcedTileSetsTooltip = "The list of tiles that will be attempted to forcefully spawn. Each entry will spawn only one tile from it's list.\n\nIf the tile cannot be forcefully spawned, the dungeon generation will not restart.";

    [Tooltip(UseForcedTilesTooltip)]
    public bool UseForcedTiles = false;
    [Tooltip(ForcedTileSetsTooltip)]
    public List<ForcedTileSetList> ForcedTileSets = new List<ForcedTileSetList>();

    internal void CopyFrom(ForcedTilesProperties props) {
      UseForcedTiles = props.UseForcedTiles;
      ForcedTileSets = props.ForcedTileSets;
    }

    internal ForcedTilesProperties Copy() {
      var copy = new ForcedTilesProperties();
      copy.CopyFrom(this);
      return copy;
    }
  }

    [System.Serializable]
  public class AdditionalTilesProperties {

    internal const string UseAdditionalTilesTooltip = "If enabled, attempts to generate additional tiles from AdditionalTileSets after main and branching paths are generated.";
    internal const string AdditionalTileSetsTooltip = "The list of tiles that will be attempted to generate. Each entry will spawn only one tile from it's list.\n\nEven if the tile cannot be generated, the dungeon generation will not restart.";

    [Tooltip(UseAdditionalTilesTooltip)]
    public bool UseAdditionalTiles = false;
    [Tooltip(AdditionalTileSetsTooltip)]
    public List<AdditionalTileSetList> AdditionalTileSets = new List<AdditionalTileSetList>();

    internal void CopyFrom(AdditionalTilesProperties props) {
      UseAdditionalTiles = props.UseAdditionalTiles;
      AdditionalTileSets = props.AdditionalTileSets;
    }

    internal void CopyFrom(ForcedTilesProperties props) {
      UseAdditionalTiles = props.UseForcedTiles;
      AdditionalTileSets = props.ForcedTileSets.Select(t => (AdditionalTileSetList)t).ToList();
    }

    internal AdditionalTilesProperties Copy() {
      var copy = new AdditionalTilesProperties();
      copy.CopyFrom(this);
      return copy;
    }
  }

  [System.Serializable]
  public class BranchPathMultiSimulationProperties {

    internal const string UseBranchPathMultiSimTooltip = "If enabled, dungeon generation will simulate a number of paths for each branch path, then choose the path based on the highest weight and generate it. The weight is decided by the following criteria below.\n\nCan slow down Branch Path Generation Times by a second or two.";
    internal const string SimulationCountTooltip = "The number of simulations per branch path.\n\nIncreasing this value can increase your chances of finding your desired path, but will increase Branch Path Times and vice versa.";
    internal const string LengthWeightScaleTooltip = "The weight scale for the branch path's length. The length of the branch path is multiplied by the scale and is added to the branch path's weight.\n\nIncreasing this value will prioritize very long branch paths.";
    internal const string NormalizedLengthWeightScaleTooltip = "The weight scale for the branch path's normalized length. The normalized length (0 -> 1) of the branch path (PathLength / MaxPathLength) is multiplied by the scale and is added to the branch path's weight.\n\nIncreasing this value will prioritize branch paths who meet their maximum path length.";

    [Tooltip(UseBranchPathMultiSimTooltip)]
    public bool UseBranchPathMultiSim = false;
    [Tooltip(SimulationCountTooltip)]
    public int SimulationCount = 5;

    [Space()]
    [Tooltip(LengthWeightScaleTooltip)]
    public float LengthWeightScale = 0.125f;
    [Tooltip(NormalizedLengthWeightScaleTooltip)]
    public float NormalizedLengthWeightScale = 1f;

    internal const string SamePathBaseWeightScaleTooltip = "The weight scale for the branch path's number of connections to the same main path. The number of possible connections is multiplied by the scale and is added to the branch path's weight.\n\nIncreasing this value will prioritize branch paths who make path loops in their main path in general.";
    internal const string SamePathDepthWeightScaleTooltip = "The weight scale for the branch path's number of connections to the same main path. For each possible connection, the main path depth difference is multiplied by the scale and is added to the branch path's weight.\n\nIncreasing this value will prioritize branch paths who make deep path loops to their main paths.";
    internal const string SamePathNormalizedDepthWeightTooltip = "The weight scale for the branch path's number of connections to the same main path. For each possible connection, the main path normalized depth difference is multiplied by the scale and is added to the branch path's weight.\n\nIncreasing this value will prioritize branch paths who make generally deep path loops to their main paths.";

    [Space()]
    [Header("Same Path")]
    [Tooltip(SamePathBaseWeightScaleTooltip)]
    public float SamePathBaseWeightScale = 0.125f;
    [Tooltip(SamePathDepthWeightScaleTooltip)]
    public float SamePathDepthWeightScale = 0.125f;
    [Tooltip(SamePathNormalizedDepthWeightTooltip)]
    public float SamePathNormalizedDepthWeightScale = 1f;

    internal const string DiffPathBaseWeightScaleTooltip = "The weight scale for the branch path's number of connections to other main paths. The number of possible connections is multiplied by the scale and is added to the branch path's weight.\n\nIncreasing this value will prioritize branch paths who make path loops to other main paths in general.";
    internal const string DiffPathDepthWeightScaleTooltip = "The weight scale for the branch path's number of connections to other main paths. For each possible connection, the main path depth difference is multiplied by the scale and is added to the branch path's weight.\n\nIncreasing this value will prioritize branch paths who make deep path loops to other main paths.";
    internal const string DiffPathNormalizedDepthWeightTooltip = "The weight scale for the branch path's number of connections to other main paths. For each possible connection, the main path normalized depth difference is multiplied by the scale and is added to the branch path's weight.\n\nIncreasing this value will prioritize branch paths who make generally deep path loops to other main paths.";

    [Space()]
    [Header("Different Path")]
    [Tooltip(DiffPathBaseWeightScaleTooltip)]
    public float DiffPathBaseWeightScale = 0.25f;
    [Tooltip(DiffPathDepthWeightScaleTooltip)]
    public float DiffPathDepthWeightScale = 0.25f;
    [Tooltip(DiffPathNormalizedDepthWeightTooltip)]
    public float DiffPathNormalizedDepthWeightScale = 2f;

    public float GetWeightBase(float length, float normalizedLength){
      var weight = 0f;
      weight += length * LengthWeightScale;
      weight += normalizedLength * NormalizedLengthWeightScale;
      return weight;
    }

    public float GetWeightPathConnection(bool samePath, float depthDifference, float normalizedDepthDifference){
      var weight = 0f;
      if (samePath) {
        weight += SamePathBaseWeightScale;
        weight += depthDifference * SamePathDepthWeightScale;
        weight += normalizedDepthDifference * SamePathNormalizedDepthWeightScale;
      } else {
        weight += DiffPathBaseWeightScale;
        weight += depthDifference * DiffPathDepthWeightScale;
        weight += normalizedDepthDifference * DiffPathNormalizedDepthWeightScale;
      }
      return weight;
    }

    internal void CopyFrom(BranchPathMultiSimulationProperties props) {
      UseBranchPathMultiSim = props.UseBranchPathMultiSim;
      SimulationCount = props.SimulationCount;
      LengthWeightScale = props.LengthWeightScale;
      NormalizedLengthWeightScale = props.NormalizedLengthWeightScale;
      SamePathBaseWeightScale = props.SamePathBaseWeightScale;
      DiffPathBaseWeightScale = props.DiffPathBaseWeightScale;
      SamePathDepthWeightScale = props.SamePathDepthWeightScale;
      DiffPathDepthWeightScale = props.DiffPathDepthWeightScale;
      SamePathNormalizedDepthWeightScale = props.SamePathNormalizedDepthWeightScale;
      DiffPathNormalizedDepthWeightScale = props.DiffPathNormalizedDepthWeightScale;
    }

    internal BranchPathMultiSimulationProperties Copy() {
      var copy = new BranchPathMultiSimulationProperties();
      copy.CopyFrom(this);
      return copy;
    }
  }

  [System.Serializable]
  public class LineRandomizerProperties {

    internal const string UseLineRandomizerTooltip = "If enabled, every archetype in LineRandomizerArchetypes will have the last LineRandomizerTakeCount tilesets replaced by a randomly selected set of tilesets from LineRandomizerTileSets. This applies for both archetype's TileSets and BranchCapTileSets.\n\nThis is designed for the scenario where dungeon generation takes a long time due to the combination of too many tiles and/or doorways in those tiles. This can reduce dungeon generation time while keeping some of the randomness of dungeon generation.\n\nAs stated previously, this WILL replace the last LineRandomizerTakeCount tilesets in the archetype's TileSets and BranchCapTileSets. As such you must guarantee that those elements can be replaced.";
    internal const string ArchetypesTooltip = "The archetypes whose tilesets will be replaced.\n\nThese archetypes should ideally used in the Lines section of DungeonFlow, but it's a free country.";
    internal const string TileSetsTooltip = "The tilesets that will be used for replacement.";
    internal const string TileSetsTakeCountTooltip = "The amount of tilesets that will be replaced from the archetypes, starting from the last element to the first element.\n\nAs stated previously, this WILL replace the tilesets in the archetype's TileSets and BranchCapTileSets. As such you must guarantee that those elements can be replaced.";

    [Tooltip(UseLineRandomizerTooltip)]
    public bool UseLineRandomizer = false;
    [Tooltip(ArchetypesTooltip)]
    public List<DungeonArchetype> Archetypes = new List<DungeonArchetype>();
    [Tooltip(TileSetsTooltip)]
    public List<TileSet> TileSets = new List<TileSet>();
    [Tooltip(TileSetsTakeCountTooltip)]
    public int TileSetsTakeCount = 3;

    internal void CopyFrom(LineRandomizerProperties props) {
      UseLineRandomizer = props.UseLineRandomizer;
      Archetypes = props.Archetypes;
      TileSets = props.TileSets;
    }

    internal LineRandomizerProperties Copy() {
      var copy = new LineRandomizerProperties();
      copy.CopyFrom(this);
      return copy;
    }

  }

  [System.Serializable]
  public class MiscellaneousProperties {

    internal const string UseMaxShadowsRequestUpdateTooltip = "If enabled, updates the MaxShadowsRequest to MaxShadowsRequestAmount when your dungeon loads.\n\nThis is designed for the scenario where your dungeon, for whatever reason, has too many lights nearby and causes the annoying 'Max shadow requests count reached' warning to spam the logs.";
    internal const string MaxShadowsRequestCountTooltip = "The amount of MaxShadowsRequest.\n\n4 is the game's default value. I find 8 to be more than acceptable.";
    internal const string UseDoorwaySistersTooltip = "If enabled, the DoorwaySisters component will become active.\n\nThe component prevents an intersecting doorway from generating if it's 'sister' doorway already generated and both doorways would lead to the same neighboring tile.\n\nThis is designed for the scenario where, two neighboring doorways would lead to the same tile, one doorway is a locked door and the other is an open doorway. This would defeat the purpose of the locked door, and such as, this feature exists if needed.\n\nThis feature slows down dungeon generation slightly when enabled.";
    internal const string UseRandomGuaranteedScrapSpawnTooltip = "If enabled, the RandomGuaranteedScrapSpawn component will be come active.\n\nThe component allows random scrap of a specified minimum value to always be spawned on a specific point.";

    [Tooltip(UseMaxShadowsRequestUpdateTooltip)]
    public bool UseMaxShadowsRequestUpdate = false;
    [Tooltip(MaxShadowsRequestCountTooltip)]
    public int MaxShadowsRequestCount = 8;
    [Tooltip(UseDoorwaySistersTooltip)]
    public bool UseDoorwaySisters = false;
    [Tooltip(UseRandomGuaranteedScrapSpawnTooltip)]
    public bool UseRandomGuaranteedScrapSpawn = false;

    internal void CopyFrom(MiscellaneousProperties props) {
      UseMaxShadowsRequestUpdate = props.UseMaxShadowsRequestUpdate;
      MaxShadowsRequestCount = props.MaxShadowsRequestCount;
      UseDoorwaySisters = props.UseDoorwaySisters;
      UseRandomGuaranteedScrapSpawn = props.UseRandomGuaranteedScrapSpawn;
    }

    internal MiscellaneousProperties Copy() {
      var copy = new MiscellaneousProperties();
      copy.CopyFrom(this);
      return copy;
    }

  }


}
