using DunGenPlus.DevTools.UIElements.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DunGenPlus.DevTools.UIElements {

  internal class Vector3InputField : BaseInputField<Vector3> {

    public TMP_InputField xInputField;
    public TMP_InputField yInputField;
    public TMP_InputField zInputField;
    private Vector3 _value;

    public void SetupInputField(TitleParameter titleParameter, Vector3 baseValue, Action<Vector3> setAction) {
      SetupBase(titleParameter);

      xInputField.onValueChanged.AddListener((t) => SetXValue(setAction, t));
      yInputField.onValueChanged.AddListener((t) => SetYValue(setAction, t));
      zInputField.onValueChanged.AddListener((t) => SetZValue(setAction, t));

      Set(baseValue);
    }

    private void SetXValue(Action<Vector3> setAction, string text){
      Plugin.logger.LogInfo($"Setting {title}.x to {text}");
      _value.x = ParseTextFloat(text);
      setAction.Invoke(_value);
    }

    private void SetYValue(Action<Vector3> setAction, string text){
      Plugin.logger.LogInfo($"Setting {title}.y to {text}");
      _value.y = ParseTextFloat(text);
      setAction.Invoke(_value);
    }

    private void SetZValue(Action<Vector3> setAction, string text){
      Plugin.logger.LogInfo($"Setting {title}.z to {text}");
      _value.z = ParseTextFloat(text);
      setAction.Invoke(_value);
    }

    public override void Set(Vector3 value){
      _value = value;
      xInputField.text = value.x.ToString();
      yInputField.text = value.y.ToString();
      zInputField.text = value.z.ToString();
    }
  }
}
