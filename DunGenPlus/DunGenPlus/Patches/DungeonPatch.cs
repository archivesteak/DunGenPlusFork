using DunGen;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using DunGenPlus.Generation;

namespace DunGenPlus.Patches
{
  // The original Forloop-End transpiler injected AddTileToMainPathDictionary
  // inside the IL of Dungeon.FromProxy. New DunGen restructured FromProxy's
  // iterator so the proxy-to-tile dictionary is now a generated state-machine
  // field (<proxyToTileMap>5__2) and the IL pattern no longer exists.
  //
  // Instead, postfix the iterator's MoveNext: when MoveNext finally returns
  // false (iteration done), pull the dict out of the state machine via
  // reflection and hand it to DunGenPlusGenerator.
  internal class DungeonPatch {

    private static FieldInfo _proxyToTileMapField;
    private static bool _delivered;

    [HarmonyPatch(typeof(Dungeon), "FromProxy", MethodType.Enumerator)]
    [HarmonyPostfix]
    public static void FromProxyMoveNextPostfix(object __instance, bool __result) {
      if (__result) { _delivered = false; return; }
      if (_delivered) return;

      if (_proxyToTileMapField == null) {
        foreach (var f in __instance.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
          if (f.FieldType == typeof(Dictionary<TileProxy, Tile>)) { _proxyToTileMapField = f; break; }
        }
      }

      if (_proxyToTileMapField?.GetValue(__instance) is Dictionary<TileProxy, Tile> dict) {
        DunGenPlusGenerator.AddTileToMainPathDictionary(dict);
        _delivered = true;
      }
    }
  }
}
