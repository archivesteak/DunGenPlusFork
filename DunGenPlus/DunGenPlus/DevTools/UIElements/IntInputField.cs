using DunGenPlus.DevTools.UIElements.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace DunGenPlus.DevTools.UIElements {
  internal class IntInputField : BaseInputField<int> {

    public TMP_InputField inputField;
    internal int minValue;
    internal int maxValue;
    internal int defaultValue;

    public void SetupInputField(TitleParameter titleParameter, IntParameter intParameter, Action<int> setAction) {
      SetupBase(titleParameter);
      minValue = intParameter.minValue;
      maxValue = intParameter.maxValue;
      defaultValue = intParameter.defaultValue;

      inputField.onValueChanged.AddListener((t) => SetValue(setAction, t));
      Set(intParameter.baseValue);
    }

    private void SetValue(Action<int> setAction, string text) {
      Plugin.logger.LogInfo($"Setting {title} to {text}");
      var value = ParseTextInt(text, defaultValue);
      setAction.Invoke(Mathf.Clamp(value, minValue, maxValue));
    }

    public override void Set(int value){
      inputField.text = value.ToString();
    }

  }
}
