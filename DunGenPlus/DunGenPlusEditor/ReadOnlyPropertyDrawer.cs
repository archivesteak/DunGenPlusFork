using DunGenPlus.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace DunGenPlusEditor {
  [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
  public class ReadOnlyPropertyDrawer : PropertyDrawer {

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
      var item = new PropertyField(property);
      item.SetEnabled(false);
      return item;
    }
  }
}
