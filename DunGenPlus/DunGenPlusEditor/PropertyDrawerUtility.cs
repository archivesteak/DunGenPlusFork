using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace DunGenPlusEditor {
  public static class PropertyDrawerUtility {

    public static VisualElement CreateBox(string displayName){
      var box = new Box();
      box.style.paddingBottom = 4f;
      box.style.paddingLeft = 4f;
      box.style.paddingRight = 4f;
      box.style.paddingTop = 4f;
      box.style.marginBottom = 8f;

      var label = new Label(displayName);
      var weight = label.style.unityFontStyleAndWeight;
      weight.value = UnityEngine.FontStyle.Bold;
      label.style.unityFontStyleAndWeight = weight;
      box.Add(label);

      return box;
    }

    public static (VisualElement parent, VisualElement container) CreateDropdown(SerializedProperty property, string displayName){
      var box = new Box();
      box.style.paddingBottom = 4f;
      box.style.paddingLeft = 4f;
      box.style.paddingRight = 4f;
      box.style.paddingTop = 4f;
      box.style.marginBottom = 8f;

      var foldout = new Foldout();
      foldout.text = displayName;
      foldout.style.marginLeft = 10f;
      foldout.viewDataKey = $"{property.serializedObject.targetObject.GetInstanceID()}.{property.name}";

      box.Add(foldout);

      return (box, foldout);
    }


    public static void SetupItemsBoolProperty(VisualElement container, SerializedProperty property, string togglePropertyName, string disabledLabelMessage) {
      SetupItems(container, property, togglePropertyName, disabledLabelMessage, (prop) => prop.boolValue);
    }

    public static void SetupItemsMainPathProperty(VisualElement container, SerializedProperty property, string togglePropertyName, string disabledLabelMessage) {
      SetupItems(container, property, togglePropertyName, disabledLabelMessage, (prop) => prop.intValue > 1);
    }

    public static void SetupItems(VisualElement container, SerializedProperty property, string togglePropertyName, string disabledLabelMessage, Func<SerializedProperty, bool> getDisplayStateFunction){
      
      SerializedProperty toggleSerializedProperty = null;
      PropertyField togglePropertyField = null;  
      var childrenPropertyFields = new List<VisualElement>();

      var enumerator = property.GetEnumerator();
      var depth = property.depth;

      while(enumerator.MoveNext()){
        var prop = enumerator.Current as SerializedProperty;
        if (prop == null || prop.depth > depth + 1) continue;

        var item = new PropertyField(prop);
        if (container is Box) item.style.marginLeft = 8f;

        if (prop.name == togglePropertyName) {
          toggleSerializedProperty = prop.Copy();
          togglePropertyField = item;
        } else {
          childrenPropertyFields.Add(item);
        }

        container.Add(item);
      }

      var defaultItem = new Label(disabledLabelMessage);
      if (container is Box) defaultItem.style.marginLeft = 11f;
      else defaultItem.style.marginLeft = 3f;
      container.Add(defaultItem);

      void SetDisplayState(bool state){
        foreach(var item in childrenPropertyFields){
          item.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
        }
        defaultItem.style.display = !state ? DisplayStyle.Flex : DisplayStyle.None;
      }

      SetDisplayState(getDisplayStateFunction(toggleSerializedProperty));
      togglePropertyField.RegisterValueChangeCallback(evt => SetDisplayState(getDisplayStateFunction(evt.changedProperty)));
    }

    public static void SetupItems(VisualElement container, SerializedProperty property){

      var enumerator = property.GetEnumerator();
      var depth = property.depth;

      while(enumerator.MoveNext()){
        var prop = enumerator.Current as SerializedProperty;
        if (prop == null || prop.depth > depth + 1) continue;

        var item = new PropertyField(prop);
        if (container is Box) item.style.marginLeft = 8f;

        container.Add(item);
      }
    }
  }
}
