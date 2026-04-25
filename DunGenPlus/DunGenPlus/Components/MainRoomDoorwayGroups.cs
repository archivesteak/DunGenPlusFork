using DunGen;
using DunGenPlus.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DunGenPlus.Components {
  public class MainRoomDoorwayGroups : MonoBehaviour {

    [System.Serializable]
    public class DoorwayList {
      [Tooltip("For organizing purposes. Has no effect.")]
      public string name;
      [Tooltip("The group of doorways.")]
      public List<Doorway> doorways;

      public bool Contains(Doorway target) {
        return doorways.Contains(target);
      }
    }

    public enum DoorwayGroupBehaviour { RemoveGroup, SetInOrder };

    [Tooltip("How the algorithm should treat these doorway groups during the main path(s) generation step.\n\nWith RemoveGroup, when a main path is being generated, it will get the doorway in this tile used to start the main path, find it's corresponding group below, and prevent the dungeon generation from using that group's doorways until all main paths are generated.\nThis is designed for the scenario where you would like the main paths to be generated more evenly throughout the MainRoomTilePrefab.\n\nWith SetInOrder, before a doorway is selected in this tile to start the main path, it will grab the first group below and only allow those doorways to start the main path. It will then select the next group for the next main path and repeat. If it cannot grab a next group, then the last group will be selected instead.\n\nIf you want this feature, this must be attached to the tile that will act as the MainRoomTilePrefab")]
    public DoorwayGroupBehaviour doorwayGroupBehaviour;

    public List<DoorwayList> doorwayLists;
    public List<Doorway> doorwayListFirst => doorwayLists.Count > 0 ? doorwayLists[0].doorways : null;

    public List<Doorway> GrabDoorwayGroup(int index){
      var count = doorwayLists.Count;
      if (count == 0) return null;
      if (index < count) return doorwayLists[index].doorways;
      return doorwayLists[count - 1].doorways;
    } 

    public List<Doorway> GrabDoorwayGroup(Doorway target){
      foreach(var a in doorwayLists){
        if (a.Contains(target)) return a.doorways;
      }
      return null;
    }

    public void OnlyUnlockGroup(TileProxy tileProxy, DoorwayProxy fakeDoorwayProxy, List<Doorway> selectedDoorways){
      if (selectedDoorways == null) return;

      foreach(var doorway in tileProxy.Doorways) {
        // it's part of the group, unlock if possible
        if (selectedDoorways.Contains(doorway.DoorwayComponent)){
          if (doorway.Used && doorway.ConnectedDoorway.Index == int.MaxValue) {
            doorway.ConnectedDoorway = null;
          }
        } 
        // it's not part of the group, lock unless already locked
        else {
          if (!doorway.Used) {
            doorway.ConnectedDoorway = fakeDoorwayProxy;
          }
        }
      }
    }

    public void OnlyLockGroup(TileProxy tileProxy, DoorwayProxy fakeDoorwayProxy){
      foreach(var d in tileProxy.UsedDoorways) {
        if (d.ConnectedDoorway.Index != int.MaxValue) {
          var groups = GrabDoorwayGroup(d.DoorwayComponent);
          if (groups == null) continue;

          foreach(var doorway in tileProxy.UnusedDoorways){
            if (groups.Contains(doorway.DoorwayComponent)){
              doorway.ConnectedDoorway = fakeDoorwayProxy;
            }
          }
        }
      }
    }

    public static void ModifyGroupBasedOnBehaviour(TileProxy tileProxy, int groupIndex){
      var doorwayGroups = tileProxy.Prefab.GetComponentInChildren<MainRoomDoorwayGroups>();
      if (doorwayGroups == null) return;

      // index of MaxValue is how we tell which doorway proxy is fake
      var fakeDoorwayProxy = new DoorwayProxy(tileProxy, int.MaxValue, tileProxy.doorways[0].DoorwayComponent, Vector3.zero, Quaternion.identity);
      if (doorwayGroups.doorwayGroupBehaviour == DoorwayGroupBehaviour.SetInOrder) {
        doorwayGroups.OnlyUnlockGroup(tileProxy, fakeDoorwayProxy, doorwayGroups.GrabDoorwayGroup(groupIndex));
      } else {
        doorwayGroups.OnlyLockGroup(tileProxy, fakeDoorwayProxy);
      }
    }

    public static bool ModifyGroupBasedOnBehaviourSimpleOnce = false;
    public static void ModifyGroupBasedOnBehaviourSimple(TileProxy tileProxy) {
      if (!DunGenPlusGenerator.Active || ModifyGroupBasedOnBehaviourSimpleOnce || tileProxy == null) return;

      var properties = DunGenPlusGenerator.Properties;
      var altCount = properties.MainPathProperties.MainPathCount - 1;
      var mainRoomTilePrefab = properties.MainPathProperties.MainRoomTilePrefab;

      // sanity check to prevent not properly configured dungeons from exploding
      if (altCount <= 0 || mainRoomTilePrefab == null) return;

      if (tileProxy.Prefab == mainRoomTilePrefab) {
        ModifyGroupBasedOnBehaviour(tileProxy, 0);
        ModifyGroupBasedOnBehaviourSimpleOnce = true;
      }
    }

    public static void RemoveFakeDoorwayProxies(TileProxy tileProxy){
      foreach(var doorway in tileProxy.UsedDoorways){
        if (doorway.ConnectedDoorway.Index == int.MaxValue) {
          doorway.ConnectedDoorway = null;
        }
      }
    }

  }
}
