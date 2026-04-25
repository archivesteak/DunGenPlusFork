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

  [CustomPropertyDrawer(typeof(ScriptAction))]
  public class ScriptActionPropertyDrawer : PropertyDrawer {
    public override VisualElement CreatePropertyGUI(SerializedProperty property) {

      var container = new VisualElement();
      var typeProperty = property.FindPropertyRelative("type");
      container.Add(new PropertyField(typeProperty));

      switch((ScriptActionType)typeProperty.intValue){
        case ScriptActionType.SetNamedReferenceState:
          AddPropertyFields(container, property, ("namedReference", "Named Reference"), ("boolValue", "State"));
          break;
        default:
          break;
      }

      
      container.Add(new PropertyField(property.FindPropertyRelative("overrideState")));

      return container;
    }

    private void AddPropertyFields(VisualElement container, SerializedProperty property, params (string field, string label)[] pairs){
      foreach(var pair in pairs){
        container.Add(new PropertyField(property.FindPropertyRelative(pair.field), pair.label));
      }
    }
  }
}
