using DunGen;
using DunGenPlus.Components;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunGenPlus.Generation {

  internal static class DoorwaySistersRule {

    public class Data {
      public DoorwaySisters info;
      public List<DoorwayProxy> proxies;
    }

    public static Dictionary<Doorway, Data> doorwayDictionary;
    public static Dictionary<DoorwayProxy, Data> doorwayProxyDictionary;

    public static void UpdateCache(IEnumerable<DoorwayProxy> list){
      if (!DunGenPlusGenerator.Active || !DunGenPlusGenerator.Properties.MiscellaneousProperties.UseDoorwaySisters) return;

      Plugin.logger.LogDebug("Updating DoorwayProxy cache for DoorwaySistersRule");
      doorwayDictionary = new Dictionary<Doorway, Data>();
      doorwayProxyDictionary = new Dictionary<DoorwayProxy, Data>();

      foreach(var a in list){
        
        var doorway = a.DoorwayComponent;
        if (doorwayDictionary.TryGetValue(doorway, out var data)){

          data.proxies.Add(a);
          doorwayProxyDictionary.Add(a, data);

        } else {

          var proxies = new List<DoorwayProxy>();
          proxies.Add(a);
          var item = new Data { 
            info = doorway.GetComponent<DoorwaySisters>(), 
            proxies = proxies 
          };

          doorwayProxyDictionary.Add(a, item);
          doorwayDictionary.Add(a.DoorwayComponent, item);

        }
      }
    }

    public static bool CanDoorwaysConnect(bool result, TileProxy tileA, TileProxy tileB, DoorwayProxy doorwayA, DoorwayProxy doorwayB){
      //if (tileA.Prefab.name.ToLowerInvariant().Contains("mayor") || tileB.Prefab.name.ToLowerInvariant().Contains("mayor"))
      //Plugin.logger.LogInfo($"{tileA.Prefab.name} <-> {tileB.Prefab.name}: {(doorwayA.Position - doorwayB.Position).sqrMagnitude}");

      if (!result) return false; 
      if (!DunGenPlusGenerator.Active || !DunGenPlusGenerator.Properties.MiscellaneousProperties.UseDoorwaySisters) return true;

      var infoA = doorwayProxyDictionary[doorwayA].info;
      var infoB = doorwayProxyDictionary[doorwayB].info;

      // deny if any sister doorway is already in use
      // cause it feels like dumb otherwise
      if (CheckIfSisterActive(infoA, tileB)){
        return false;
      }

      if (CheckIfSisterActive(infoB, tileA)){
        return false;
      }

      // allow like normal
      return true;
    }

    public static bool CheckIfSisterActive(DoorwaySisters info, TileProxy targetTile){
      if (info == null || info.sisters == null) return false;

      foreach(var sis in info.sisters){
        var proxies = doorwayDictionary[sis].proxies;
        foreach(var proxy in proxies){
          var result = proxy.ConnectedDoorway != null && proxy.ConnectedDoorway.TileProxy == targetTile;
          if (result) return true;
        }
      }

      return false;
    }

  }
}
