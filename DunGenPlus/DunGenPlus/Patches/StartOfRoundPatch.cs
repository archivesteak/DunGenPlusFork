using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace DunGenPlus.Patches {

  // like could be it's own mod
  // but I don't wanna be the guy who messes with the original dungeon flows
  // and break someone else's mod cause of X edge case
  // and have to deal with that
  // nah bruh, let someone else deal with it

  internal class StartOfRoundPatch {

    /*

    public static readonly string[] validDungeonFlowTargets = new [] {
      "Level1Flow", "Level2Flow", "Level1FlowExtraLarge", "Level1Flow3Exits"
    };

    public static readonly Dictionary<string, int> validStartTileTargets = new Dictionary<string, int>() {
      { "StartRoom", 2 },
      { "ManorStartRoom", 3 }
    };

    [HarmonyPatch(typeof(RoundManager), "Awake")]
    [HarmonyPrefix]
    public static void AwakePatch(ref RoundManager __instance){
      var dungeonFlows = __instance.dungeonFlowTypes.Select(d => d.dungeonFlow);
      foreach(var d in dungeonFlows) {
        if (!validDungeonFlowTargets.Contains(d.name)) continue;
        if (API.ContainsDungeonFlow(d)) continue;

        Plugin.logger.LogInfo($"Creating DunGenExtender for {d.name}");

        var tiles = d.Lines
          .Select(i => i.DungeonArchetypes)
          .SelectMany(i => i)
          .Select(i => i.TileSets)
          .SelectMany(i => i)
          .Select(i => i.TileWeights.Weights)
          .SelectMany(i => i)
          .Select(i => i.Value);
        foreach(var t in tiles) {
          if (validStartTileTargets.TryGetValue(t.name, out var paths)) {
            var extender = API.CreateDunGenExtender(d);
            var props = extender.Properties;
            props.MainPathProperties.MainPathCount = paths;
            props.MainPathProperties.MainRoomTilePrefab = t;

            d.Length = new DunGen.IntRange(d.Length.Min / 2, d.Length.Max / 2);
            Plugin.logger.LogInfo($"New length: {d.Length}");

            if (t.name == "StartRoom") {
              var lines = d.Lines;
              lines[0].Length = 0.2f;
              lines[1].Length -= 0.2f - lines[1].Position;
              lines[1].Position = 0.2f;
            }

            API.AddDunGenExtender(extender);
            break;
          }
        }
      }
    }
    */

  }
}
