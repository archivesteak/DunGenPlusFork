using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace DunGenPlus.DevTools.UIElements {
  internal abstract class BaseInputField<T> : BaseUIElement {

    public abstract void Set(T value);

    protected int ParseTextInt(string text, int defaultValue = 0) {
      if (int.TryParse(text, out var result)){
        return result;
      } else {
        Plugin.logger.LogWarning($"Couldn't parse {text} into an int");
        return defaultValue;
      }
    }

    protected float ParseTextFloat(string text, float defaultValue = 0f) {
      if (float.TryParse(text, out var result)){
        return result;
      } else {
        Plugin.logger.LogWarning($"Couldn't parse {text} into a float");
        return defaultValue;
      }
    }

  }
}
