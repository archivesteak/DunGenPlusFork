using DunGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DunGenPlus.Collections {

  [System.Serializable]
  public class DetailedGlobalPropSettings {

    internal const string MinimumDistanceTooltip = "The minimum distance between each Global Props of this id.";
    internal const string GlobalCountMustBeReachedTooltip = "If true, when the global limit is not reached due to minimum distance, ignore it and try to reach the global limit anyway.";

    public int ID;

    [Space]
    [Tooltip(MinimumDistanceTooltip)]
    public float MinimumDistance;
    public bool GlobalCountMustBeReached = true;

    public DetailedGlobalPropSettings(int id, float minimumDistance, bool globalCountMustBeReached)
    {
      ID = id;
      MinimumDistance = minimumDistance;
      GlobalCountMustBeReached = globalCountMustBeReached;
    }
  }
}
