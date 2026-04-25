using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.UIElements;
using DunGenPlus;
using DunGenPlus.Collections;

namespace DunGenPlusEditor {

  [CustomPropertyDrawer(typeof(DunGenExtenderProperties))]
  public class DunGenExtenderPropertiesPropertyDrawer : PropertyDrawer {

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {

      var container = new VisualElement();
      PropertyDrawerUtility.SetupItems(container, property);
      return container;
    }

  }

  [CustomPropertyDrawer(typeof(MainPathProperties))]
  public class MainPathPropertiesPropertyDrawer : PropertyDrawer {

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {

      var container = new VisualElement();

      var box = PropertyDrawerUtility.CreateDropdown(property, "Main Path");
      PropertyDrawerUtility.SetupItemsMainPathProperty(box.container, property, "MainPathCount", "Generating the default one main path");
      container.Add(box.parent);

      return container;
    }

  }

  [CustomPropertyDrawer(typeof(DungeonBoundsProperties))]
  public class DungeonBoundsPropertiesPropertyDrawer : PropertyDrawer {

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {

      var container = new VisualElement();

      var box = PropertyDrawerUtility.CreateDropdown(property, "Dungeon Bounds");
      PropertyDrawerUtility.SetupItemsBoolProperty(box.container, property, "UseDungeonBounds", "Disabled");
      container.Add(box.parent);

      return container;
    }

  }

  [CustomPropertyDrawer(typeof(NormalNodeArchetypesProperties))]
  public class NormalNodeArchetypesPropertiesPropertyDrawer : PropertyDrawer {

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {

      var container = new VisualElement();

      var box = PropertyDrawerUtility.CreateDropdown(property, "Normal Nodes Archetypes");
      PropertyDrawerUtility.SetupItemsBoolProperty(box.container, property, "AddArchetypesToNormalNodes", "Disabled");
      container.Add(box.parent);

      return container;
    }

  }

  [CustomPropertyDrawer(typeof(AdditionalTilesProperties))]
  public class ForcedTilesPropertiesPropertyDrawer : PropertyDrawer {

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {

      var container = new VisualElement();

      var box = PropertyDrawerUtility.CreateDropdown(property, "Additional Tiles");
      PropertyDrawerUtility.SetupItemsBoolProperty(box.container, property, "UseAdditionalTiles", "Disabled");
      container.Add(box.parent);

      return container;
    }

  }

  [CustomPropertyDrawer(typeof(BranchPathMultiSimulationProperties))]
  public class BranchPathMultiSimulationPropertiesPropertyDrawer : PropertyDrawer {

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {

      var container = new VisualElement();

      var box = PropertyDrawerUtility.CreateDropdown(property, "Branch Path Multi Simulation");
      PropertyDrawerUtility.SetupItemsBoolProperty(box.container, property, "UseBranchPathMultiSim", "Disabled");
      container.Add(box.parent);

      return container;
    }

  }

  [CustomPropertyDrawer(typeof(LineRandomizerProperties))]
  public class LineRandomizerPropertiesPropertyDrawer : PropertyDrawer {

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {

      var container = new VisualElement();

      var box = PropertyDrawerUtility.CreateDropdown(property, "Line Randomizer");
      PropertyDrawerUtility.SetupItemsBoolProperty(box.container, property, "UseLineRandomizer", "Disabled");
      container.Add(box.parent);

      return container;
    }

  }

  [CustomPropertyDrawer(typeof(MiscellaneousProperties))]
  public class MiscellaneousPropertiesPropertyDrawer : PropertyDrawer {

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {

      var container = new VisualElement();

      var box = PropertyDrawerUtility.CreateDropdown(property, "Miscellaneous");
      PropertyDrawerUtility.SetupItems(box.container, property);
      container.Add(box.parent);

      return container;
    }

  }

}
