using DunGenPlus.DevTools.UIElements.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace DunGenPlus.DevTools.UIElements
{
  internal class FloatInputField : BaseInputField<float> {

    public TMP_InputField inputField;
    internal float minValue;
    internal float maxValue;
    internal float defaultValue;

    public void SetupInputField(TitleParameter titleParameter, FloatParameter floatParameter, Action<float> setAction) {
      SetupBase(titleParameter);
      minValue = floatParameter.minValue;
      maxValue = floatParameter.maxValue;
      defaultValue = floatParameter.defaultValue;

      inputField.onValueChanged.AddListener((t) => SetValue(setAction, t));
      Set(floatParameter.baseValue);
    }

    private void SetValue(Action<float> setAction, string text) {
      Plugin.logger.LogInfo($"Setting {title} to {text}");
      var value = ParseTextFloat(text, defaultValue);
      setAction.Invoke(Mathf.Clamp(value, minValue, maxValue));
    }

    public override void Set(float value){
      inputField.text = value.ToString();
    }
  }
}
