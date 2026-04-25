using DunGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DunGenPlus.Components {
  public class DoorwaySisters : MonoBehaviour {

    private Doorway _self;
    public Doorway Self {
      get {
        if (_self == null) {
          _self = GetComponent<Doorway>();
        }
        return _self;
      }
    }

    [Tooltip("The list of 'sister' doorways.\n\nUseDoorwaySisters must be toggled in DunGenExtender for this component to be used.\n\nThis doorway will not generate if it's an intersecting doorway, any of it's 'sister' doorways are generated, and both this doorway and the 'sister' doorway lead to the same tile.")]
    public List<Doorway> sisters = new List<Doorway>();

    void OnValidate(){
      var sis = sisters.Select(s => s.GetComponent<DoorwaySisters>());
      foreach(var s in sis) {
        if (s == null) continue;

        s.TryAddSisterDoorway(Self);
      }
    }

    public void TryAddSisterDoorway(Doorway doorway){
      if (sisters.Contains(doorway)) return;
      sisters.Add(doorway);
    }

    public void OnDrawGizmosSelected(){
      var center = transform.position + Vector3.up;
      if (sisters == null) return;

      foreach(var sis in sisters){
        var target = sis.transform.position + Vector3.up;
        var comp = sis.GetComponent<DoorwaySisters>();

        var self = Self;
        if (self == null) {
          Gizmos.color = Color.magenta;
        } else if (comp == null || comp.sisters == null){
          Gizmos.color = Color.yellow;
        } else if (!comp.sisters.Contains(self)) {
          Gizmos.color = Color.red;
        } else {
          Gizmos.color = Color.green;
        }

        Gizmos.DrawLine(center, target);
        Gizmos.DrawSphere((center + target) * 0.5f, 0.25f);
      }
    }

  }
}
