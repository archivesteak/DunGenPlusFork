using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.UIElements;
using DunGenPlus;
using DunGenPlus.Collections;
using DunGenPlus.Components.Scripting;
using UnityEditor.UIElements;

namespace DunGenPlusEditor {

  [CustomPropertyDrawer(typeof(NamedGameObjectReference))]
  public class NamedGameObjectReferencePropertyDrawer : PropertyDrawer {
    public override VisualElement CreatePropertyGUI(SerializedProperty property) {

      var container = new VisualElement();
      container.Add(new PropertyField(property.FindPropertyRelative("name")));
      container.Add(new PropertyField(property.FindPropertyRelative("gameObjects")));
      container.Add(new PropertyField(property.FindPropertyRelative("overrideState")));

      return container;
    }
  }
}
