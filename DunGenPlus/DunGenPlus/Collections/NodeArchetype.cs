using DunGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DunGenPlus.Collections {

  [System.Serializable]
  public class NodeArchetype {

    internal const string LabelTooltip = "The normal node with this label will gain a randomly chosen archetype.\n\nIf empty, this becomes the default choice for any normal node without a NodeArchetype specified in this list.";
    internal const string ArchetypesTooltip = "The list of archetypes. One will be randomly chosen.";

    [Tooltip(LabelTooltip)]
    public string Label;
    [Tooltip(ArchetypesTooltip)]
    public List<DungeonArchetype> Archetypes = new List<DungeonArchetype>();
  }

}
