using DunGen;
using DunGenPlus.DevTools.UIElements.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace DunGenPlus.DevTools.UIElements
{
  internal class IntRangeInputField : BaseInputField<IntRange> {

    public TMP_InputField minInputField;
    public TMP_InputField maxInputField;
    private IntRange _value;

    public void SetupInputField(TitleParameter titleParameter, IntRange baseValue, Action<IntRange> setAction) {
      SetupBase(titleParameter);

      minInputField.onValueChanged.AddListener((t) => SetMinValue(setAction, t));
      maxInputField.onValueChanged.AddListener((t) => SetMaxValue(setAction, t));

      Set(baseValue);
    }

    private void SetMinValue(Action<IntRange> setAction, string text){
      Plugin.logger.LogInfo($"Setting {title}.min to {text}");
      _value.Min = ParseTextInt(text);
      setAction.Invoke(_value);
    }

    private void SetMaxValue(Action<IntRange> setAction, string text){
      Plugin.logger.LogInfo($"Setting {title}.max to {text}");
      _value.Max = ParseTextInt(text);
      setAction.Invoke(_value);
    }

    public override void Set(IntRange value){
      _value = value;
      minInputField.text = value.Min.ToString();
      maxInputField.text = value.Max.ToString();
    }
  }
}
