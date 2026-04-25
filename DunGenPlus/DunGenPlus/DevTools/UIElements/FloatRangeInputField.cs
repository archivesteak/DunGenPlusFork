using DunGen;
using DunGenPlus.DevTools.UIElements.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace DunGenPlus.DevTools.UIElements
{
  internal class FloatRangeInputField : BaseInputField<FloatRange> {

    public TMP_InputField minInputField;
    public TMP_InputField maxInputField;
    private FloatRange _value;

    public void SetupInputField(TitleParameter titleParameter, FloatRange baseValue, Action<FloatRange> setAction) {
      SetupBase(titleParameter);

      minInputField.onValueChanged.AddListener((t) => SetMinValue(setAction, t));
      maxInputField.onValueChanged.AddListener((t) => SetMaxValue(setAction, t));

      Set(baseValue);
    }

    private void SetMinValue(Action<FloatRange> setAction, string text){
      Plugin.logger.LogInfo($"Setting {title}.min to {text}");
      _value.Min = ParseTextFloat(text);
      setAction.Invoke(_value);
    }

    private void SetMaxValue(Action<FloatRange> setAction, string text){
      Plugin.logger.LogInfo($"Setting {title}.max to {text}");
      _value.Max = ParseTextFloat(text);
      setAction.Invoke(_value);
    }

    public override void Set(FloatRange value){
      _value = value;
      minInputField.text = value.Min.ToString();
      maxInputField.text = value.Max.ToString();
    }
  }
}
