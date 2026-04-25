using DunGen;
using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DunGenPlus.Collections;

namespace DunGenPlus {

  [System.Serializable]
  public class PropertyOverride<T> {
    [Tooltip("If false, use the value found in DungeonFlow. If true, use this Value instead.")]
    public bool Override;
    public T Value;

    public PropertyOverride(bool _override, T value) {
      Override = _override;
      Value = value;
    }
  }

  [CreateAssetMenu(fileName = "Main Path Extender", menuName = "DunGenExtender/Main Path Extender", order = 2)]
  public class MainPathExtender : ScriptableObject {

    internal const string LocalMainPathGlobalPropsTooltip = "Limits the amount of Global Props that can spawn on a single main path.\n\nThis does not afffect the global limit defined in DungeonFlow.";

    public PropertyOverride<IntRange> Length = new PropertyOverride<IntRange>(false, new IntRange(5, 10));

    public PropertyOverride<BranchMode> BranchMode = new PropertyOverride<BranchMode>(false, DunGen.BranchMode.Local);
    public PropertyOverride<IntRange> BranchCount = new PropertyOverride<IntRange>(false, new IntRange(1, 5));

    public PropertyOverride<List<GraphNode>> Nodes = new PropertyOverride<List<GraphNode>>(false, new List<GraphNode>());
    public PropertyOverride<List<GraphLine>> Lines = new PropertyOverride<List<GraphLine>>(false, new List<GraphLine>());

    [Tooltip(LocalMainPathGlobalPropsTooltip)]
    public List<LocalGlobalPropSettings> LocalGroupProps = new List<LocalGlobalPropSettings>();

    [Header("DEV ONLY: DON'T TOUCH")]
    [Attributes.ReadOnly]
    public string Version = "0";

    public static IntRange GetLength(MainPathExtender extender, DungeonFlow flow) {
      if (extender && extender.Length.Override) return extender.Length.Value;
      return flow.Length;
    }

    public static BranchMode GetBranchMode(MainPathExtender extender, DungeonFlow flow) {
      if (extender && extender.BranchMode.Override) return extender.BranchMode.Value;
      return flow.BranchMode;
    }

    public static IntRange GetBranchCount(MainPathExtender extender, DungeonFlow flow) {
      if (extender && extender.BranchCount.Override) return extender.BranchCount.Value;
      return flow.BranchCount;
    }

    public static List<GraphNode> GetNodes(MainPathExtender extender, DungeonFlow flow) {
      if (extender && extender.Nodes.Override) return extender.Nodes.Value;
      return flow.Nodes;
    }

    public static List<GraphLine> GetLines(MainPathExtender extender, DungeonFlow flow) {
      if (extender && extender.Lines.Override) return extender.Lines.Value;
      return flow.Lines;
    }

  }
}
