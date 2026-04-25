using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using DunGenPlus;

namespace DunGenPlusEditor {
  [CustomPropertyDrawer(typeof(PropertyOverride<>), true)]
  public class PropertyOverridePropertyDrawer : PropertyDrawer {

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {

      var container = new VisualElement();

      var box = PropertyDrawerUtility.CreateBox(property.displayName);
      PropertyDrawerUtility.SetupItemsBoolProperty(box, property, "Override", "Using DungeonFlow's corresponding values");
      container.Add(box);

      return container;
    }

  }
}
