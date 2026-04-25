using DunGen;
using DunGenPlus.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DunGenPlus.Collections {
  internal class TileExtenderProxy {
    public TileProxy TileProxy { get; internal set; }
    public TileExtender PrefabTileExtender { get; internal set; }
    public List<DoorwayProxy> Entrances { get; internal set; }
    public List<DoorwayProxy> Exits { get; internal set; }
    public List<DoorwayProxy> OverlappingDoorways { get; internal set; }
    public bool EntranceExitInterchangable { get; internal set; }

    public TileExtenderProxy(TileProxy tileProxy, TileExtenderProxy existingTileExtenderProxy) {
      TileProxy = tileProxy;
      PrefabTileExtender = existingTileExtenderProxy.PrefabTileExtender;

      Entrances = new List<DoorwayProxy>();
      Exits = new List<DoorwayProxy>();
      OverlappingDoorways = new List<DoorwayProxy>();

      EntranceExitInterchangable = existingTileExtenderProxy.EntranceExitInterchangable;

      var existingTile = existingTileExtenderProxy.TileProxy;
      for(var i = 0; i < tileProxy.doorways.Count; ++i){
        var doorway = tileProxy.doorways[i];
        var existingDoorway = existingTile.doorways[i];
        if (existingTileExtenderProxy.Entrances.Contains(existingDoorway)) Entrances.Add(doorway);
        if (existingTileExtenderProxy.Exits.Contains(existingDoorway)) Exits.Add(doorway);
        if (existingTileExtenderProxy.OverlappingDoorways.Contains(existingDoorway)) OverlappingDoorways.Add(doorway);
      }
    }

    public TileExtenderProxy(TileProxy tileProxy) {
      TileProxy = tileProxy;
      PrefabTileExtender = tileProxy.Prefab.GetComponent<TileExtender>();

      Entrances = new List<DoorwayProxy>();
      Exits = new List<DoorwayProxy>();
      OverlappingDoorways = new List<DoorwayProxy>();

      if (PrefabTileExtender == null) {
        if (tileProxy.Entrances.FirstOrDefault() != null) Entrances.Add(tileProxy.Entrances.FirstOrDefault());
        if (tileProxy.Exits.FirstOrDefault() != null) Exits.Add(tileProxy.Exits.FirstOrDefault());
        EntranceExitInterchangable = false;
        return;
      }

      foreach(var proxyDoorway in tileProxy.doorways) {
        if (PrefabTileExtender.entrances.Contains(proxyDoorway.DoorwayComponent)) Entrances.Add(proxyDoorway);
        if (PrefabTileExtender.exits.Contains(proxyDoorway.DoorwayComponent)) Exits.Add(proxyDoorway);
        if (PrefabTileExtender.overlappingDoorways.Contains(proxyDoorway.DoorwayComponent)) OverlappingDoorways.Add(proxyDoorway);
      }

      EntranceExitInterchangable = PrefabTileExtender.entranctExitInterchangable;
    }
  }
}
