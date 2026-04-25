using DunGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DunGenPlus.Components {
  public class TileExtender : MonoBehaviour {

    public List<Doorway> entrances = new List<Doorway>();
    public List<Doorway> exits = new List<Doorway>();
    public bool entranctExitInterchangable;
    public List<Doorway> overlappingDoorways = new List<Doorway>();

  }
}
