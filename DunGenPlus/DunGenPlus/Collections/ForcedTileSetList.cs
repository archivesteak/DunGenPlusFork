using DunGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DunGenPlus.Collections {
  [System.Serializable]
  public class ForcedTileSetList {

    internal const string TileSetsTooltip = "List of tiles to be forcefully spawned.";
    internal const string DepthWeightScaleTooltip = "The weight based on the path's depth.";
    internal const string MainPathWeightTooltip = "The weight for the tile spawning on the main path.";
    internal const string BranchPathWeightTooltip = "The weight for the tile spawning on the branch path.";

    [Tooltip(TileSetsTooltip)]
    public List<TileSet> TileSets = new List<TileSet>();
    [Tooltip(DepthWeightScaleTooltip)]
    public AnimationCurve DepthWeightScale = new AnimationCurve();
    [Tooltip(MainPathWeightTooltip)]
    public float MainPathWeight = 1f;
    [Tooltip(BranchPathWeightTooltip)]
    public float BranchPathWeight = 1f;

  }

    [System.Serializable]
  public class AdditionalTileSetList {

    internal const string TileSetsTooltip = "List of tiles to be generated.";
    internal const string DepthWeightScaleTooltip = "The weight based on the path's depth.";
    internal const string MainPathWeightTooltip = "The weight for the tile spawning on the main path.";
    internal const string BranchPathWeightTooltip = "The weight for the tile spawning on the branch path.";

    [Tooltip(TileSetsTooltip)]
    public List<TileSet> TileSets = new List<TileSet>();
    [Tooltip(DepthWeightScaleTooltip)]
    public AnimationCurve DepthWeightScale = new AnimationCurve();
    [Tooltip(MainPathWeightTooltip)]
    public float MainPathWeight = 1f;
    [Tooltip(BranchPathWeightTooltip)]
    public float BranchPathWeight = 1f;

    public static implicit operator AdditionalTileSetList(ForcedTileSetList item) {
      var copy = new AdditionalTileSetList();
      copy.TileSets = item.TileSets;
      copy.DepthWeightScale = item.DepthWeightScale;
      copy.MainPathWeight = item.MainPathWeight;
      copy.BranchPathWeight = item.BranchPathWeight;
      return copy;
    }

  }

}
