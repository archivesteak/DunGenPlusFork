using DunGen.Graph;
using DunGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DunGenPlus.Collections {

  [System.Serializable]
  public class DunGenExtenderProperties {

    public enum CopyNodeBehaviour {
      /// <summary>
      /// Nodes will copy from the MainRoomTilePrefab's position in the main path
      /// </summary>
      CopyFromMainPathPosition,
      /// <summary>
      /// Nodes will copy from the MainRoomTilePrefab's location from the node list + 1
      /// </summary>
      CopyFromNodeList
    }

    public MainPathProperties MainPathProperties = new MainPathProperties();
    
    public DungeonBoundsProperties DungeonBoundsProperties = new DungeonBoundsProperties();

    public NormalNodeArchetypesProperties NormalNodeArchetypesProperties = new NormalNodeArchetypesProperties();

    public AdditionalTilesProperties AdditionalTilesProperties = new AdditionalTilesProperties();

    [HideInInspector]
    [Obsolete("Variable field renamed to AdditionalTilesProperties. This field will be removed in future updates.")]
    public ForcedTilesProperties ForcedTilesProperties = new ForcedTilesProperties();

    public BranchPathMultiSimulationProperties BranchPathMultiSimulationProperties = new BranchPathMultiSimulationProperties();

    public LineRandomizerProperties LineRandomizerProperties = new LineRandomizerProperties();

    public MiscellaneousProperties MiscellaneousProperties = new MiscellaneousProperties();

    [Header("Asset Cache (FOR DEV DEBUG PURPOSES ONLY)")]
    public List<GameObject> AssetCacheTileList = new List<GameObject>();
    public List<TileSet> AssetCacheTileSetList = new List<TileSet>();
    public List<DungeonArchetype> AssetCacheArchetypeList = new List<DungeonArchetype>();

    internal void CopyFrom(DunGenExtenderProperties props, string version) {
      MainPathProperties = props.MainPathProperties.Copy();
      DungeonBoundsProperties = props.DungeonBoundsProperties.Copy();
      NormalNodeArchetypesProperties = props.NormalNodeArchetypesProperties.Copy();

      // for backwards support
      ForcedTilesProperties = props.ForcedTilesProperties.Copy();
      AdditionalTilesProperties = props.AdditionalTilesProperties.Copy();
      if (version == "0") {
        AdditionalTilesProperties.CopyFrom(ForcedTilesProperties);
      }

      BranchPathMultiSimulationProperties = props.BranchPathMultiSimulationProperties.Copy();
      LineRandomizerProperties = props.LineRandomizerProperties.Copy();
      MiscellaneousProperties = props.MiscellaneousProperties.Copy();
    }

    internal DunGenExtenderProperties Copy(string version) {
      var copy = new DunGenExtenderProperties();
      copy.CopyFrom(this, version);
      return copy;
    }

  }
}
