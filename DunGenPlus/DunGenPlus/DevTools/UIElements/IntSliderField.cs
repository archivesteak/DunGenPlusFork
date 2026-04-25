using DunGenPlus.DevTools.UIElements.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;

namespace DunGenPlus.DevTools.UIElements {

  internal class IntSliderField : BaseInputField<int> {

    public Slider inputField;
    public TextMeshProUGUI textMesh;

    public void SetupInputField(TitleParameter titleParameter, IntParameter intParameter, Action<int> setAction) {
      SetupBase(titleParameter);

      inputField.minValue = inputField.minValue;
      inputField.maxValue = inputField.maxValue;
      inputField.onValueChanged.AddListener((t) => SetValue(setAction, t));
      Set(intParameter.baseValue);
    }

    private void SetValue(Action<int> setAction, float value) {
      Plugin.logger.LogInfo($"Setting {title} to {value}");
      setAction.Invoke((int)value);
      textMesh.text = value.ToString();
    }

    public override void Set(int value){
      inputField.value = value;
      textMesh.text = value.ToString();
    }

  }
}
