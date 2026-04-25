using DunGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using DunGenPlus.Collections;
using UnityEngine;

namespace DunGenPlus.Patches {
  internal class TileProxyPatch {
    
    public static Dictionary<TileProxy, TileExtenderProxy> TileExtenderProxyDictionary = new Dictionary<TileProxy, TileExtenderProxy>();
    
    public static void ResetDictionary(){
      TileExtenderProxyDictionary.Clear();
    }

    public static TileExtenderProxy GetTileExtenderProxy(TileProxy proxy){
      return TileExtenderProxyDictionary[proxy];
    }

    public static void AddTileExtenderProxy(TileProxy tileProxy, TileExtenderProxy tileExtenderProxy){
      TileExtenderProxyDictionary.Add(tileProxy, tileExtenderProxy);
    }


    [HarmonyPatch(typeof(TileProxy), MethodType.Constructor, new Type[] { typeof(GameObject), typeof(Func<Doorway, int, bool>) })]
    [HarmonyPostfix]
    public static void TileProxyConstructorNewPatch(ref TileProxy __instance){
      AddTileExtenderProxy(__instance, new TileExtenderProxy(__instance));
    }

    [HarmonyPatch(typeof(TileProxy), MethodType.Constructor, new Type[] { typeof(TileProxy) })]
    [HarmonyPostfix]
    public static void TileProxyConstructorExistingPatch(ref TileProxy __instance, TileProxy existingTile){
      AddTileExtenderProxy(__instance, new TileExtenderProxy(__instance, GetTileExtenderProxy(existingTile)));
    }

  }
}
