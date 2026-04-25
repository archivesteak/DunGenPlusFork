using DunGenPlus.DevTools.UIElements.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace DunGenPlus.DevTools.UIElements
{
  internal class StringInputField : BaseInputField<string> {

    public TMP_InputField inputField;

    public void SetupInputField(TitleParameter titleParameter, string baseValue, Action<string> setAction) {
      SetupBase(titleParameter);

      inputField.onValueChanged.AddListener((t) => SetValue(setAction, t));
      Set(baseValue);
    }

    private void SetValue(Action<string> setAction, string text) {
      Plugin.logger.LogInfo($"Setting {title} to {text}");
      setAction.Invoke(text);
    }

    public override void Set(string value){
      inputField.text = value;
    }

  }
}
