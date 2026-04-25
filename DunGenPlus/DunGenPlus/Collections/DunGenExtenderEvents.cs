using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunGenPlus.Collections {
  
  [System.Serializable]
  public  class DunGenExtenderEvents {

    /// <summary>
    /// Event handler called after <see cref="DunGenExtender.DungeonFlow"/> is selected for dungeon generation,
    /// but before the dungeon generation process begins.
    /// Allows the parameter passed, <see cref="DunGenExtenderProperties"/>, to be modified.
    /// <para>
    /// The parameter passed, <see cref="DunGenExtenderProperties"/>, is a shallow copy of <see cref="DunGenExtender.Properties"/>.
    /// Field values can be replaced without affecting the original properties. Editing a reference value will affect the original value.
    /// </para>
    /// </summary>
    public ExtenderEvent<DunGenExtenderProperties> OnModifyDunGenExtenderProperties = new ExtenderEvent<DunGenExtenderProperties>();

  }
}
