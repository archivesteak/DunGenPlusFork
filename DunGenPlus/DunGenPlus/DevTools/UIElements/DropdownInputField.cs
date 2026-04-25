using DunGenPlus.DevTools.UIElements.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DunGenPlus.DevTools.UIElements
{
  internal class DropdownInputField : BaseUIElement {

    public TMP_Dropdown dropDown;

    public void SetupDropdown<T>(TitleParameter titleParameter, int baseValue, Action<T> setAction, Func<int, T> convertIndex, IEnumerable<string> options) {
      SetupBase(titleParameter);

      var maxLength = (int)Mathf.LerpUnclamped(28f, 24f, layoutOffset / 24f);
      dropDown.options = options.Select(c => {
        return new TMP_Dropdown.OptionData(c.Substring(0, Math.Min(maxLength, c.Length)));
      }).ToList();

      dropDown.onValueChanged.AddListener((t) => SetValue(setAction, convertIndex, t));
      dropDown.value = baseValue;
    }

    private void SetValue<T>(Action<T> setAction, Func<int, T> convertIndex, int index) {
      var value = convertIndex.Invoke(index);
      Plugin.logger.LogInfo($"Setting {title} to {value}");
      setAction.Invoke(value);
    }

  }
}
