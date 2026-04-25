using DunGen;
using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DunGenPlus.Generation {
  internal partial class DunGenPlusGenerator {

		public static MainPathExtender currentMainPathExtender;

		public static void SetCurrentMainPathExtender(int mainPathIndex){
			currentMainPathExtender = Properties.MainPathProperties.GetMainPathDetails(mainPathIndex);
		}

    public static GraphLine GetLineAtDepth(DungeonFlow flow, float depth) {
      if (!DunGenPlusGenerator.Active) {
				//Plugin.logger.LogInfo("LineDepth: Default");
				return flow.GetLineAtDepth(depth);
			}

			//var comment = currentMainPathExtender == null ? "NULL" : "ITEM";
			//Plugin.logger.LogInfo($"LineDepth: {comment}");
			var lines = MainPathExtender.GetLines(currentMainPathExtender, flow);
			return GetLineAtDepthHelper(lines, depth);
    }

    public static GraphLine GetLineAtDepthHelper(List<GraphLine> graphLines, float normalizedDepth) {
			normalizedDepth = Mathf.Clamp(normalizedDepth, 0f, 1f);
			if (normalizedDepth == 0f) return graphLines[0];
			if (normalizedDepth == 1f) return graphLines[graphLines.Count - 1];
			foreach (GraphLine graphLine in graphLines){
				if (normalizedDepth >= graphLine.Position && normalizedDepth < graphLine.Position + graphLine.Length) {
					return graphLine;
				}
			}
			Debug.LogError("GetLineAtDepth was unable to find a line at depth " + normalizedDepth.ToString() + ". This shouldn't happen.");
			return null;
		}

		public static List<GraphNode> GetNodes(DungeonFlow flow){
			if (!DunGenPlusGenerator.Active) {
				//Plugin.logger.LogInfo("Nodes: Default");
				return flow.Nodes;
			}

			//var comment = currentMainPathExtender == null ? "NULL" : "ITEM";
			//Plugin.logger.LogInfo($"Nodes: {comment}");
			return MainPathExtender.GetNodes(currentMainPathExtender, flow);
		}

		public static BranchMode GetBranchMode(DungeonFlow flow) {
			if (!DunGenPlusGenerator.Active) {
				//Plugin.logger.LogInfo("Branch Mode: Default");
				return flow.BranchMode;
			}

			//var comment = currentMainPathExtender == null ? "NULL" : "ITEM";
			//Plugin.logger.LogInfo($"Branch Mode: {comment}");
			return MainPathExtender.GetBranchMode(currentMainPathExtender, flow);
		}

		public static IntRange GetBranchCount(DungeonFlow flow) {
			if (!DunGenPlusGenerator.Active) {
				//Plugin.logger.LogInfo("Branch Count: Default");
				return flow.BranchCount;
			}

			//var comment = currentMainPathExtender == null ? "NULL" : "ITEM";
			//Plugin.logger.LogInfo($"Branch Count: {comment}");
			return MainPathExtender.GetBranchCount(currentMainPathExtender, flow);
		}

		public static IntRange GetLength(DungeonFlow flow) {
			if (!DunGenPlusGenerator.Active) {
				//Plugin.logger.LogInfo("Length: Default");
				return flow.Length;
			}

			//var comment = currentMainPathExtender == null ? "NULL" : "ITEM";
			//Plugin.logger.LogInfo($"Length: {comment}");
			return MainPathExtender.GetLength(currentMainPathExtender, flow);
		}

  }
}
