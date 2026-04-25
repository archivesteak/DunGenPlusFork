using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunGenPlus.DevTools.UIElements.Collections {
  internal struct TitleParameter {
    public string text;
    public float offset;
    public string hoverText;

    public TitleParameter(string text, float offset = 0f) {
      this.text = text;
      this.offset = offset;
      this.hoverText = null;
    }

    public TitleParameter(string text, string hoverText, float offset = 0f){
      this.text = text;
      this.offset = offset;
      this.hoverText = hoverText;
    }

    public static implicit operator TitleParameter(string text) => new TitleParameter(text);
    public static implicit operator TitleParameter((string text, string hoverText) pair) => new TitleParameter(pair.text, pair.hoverText);
  }

  internal struct IntParameter {
    public int baseValue;
    public int minValue;
    public int maxValue;
    public int defaultValue;


    public IntParameter(int baseValue, int defaultValue = 0) {
      this.baseValue = baseValue;
      this.minValue = int.MinValue;
      this.maxValue = int.MaxValue;
      this.defaultValue = defaultValue;
    }

    public IntParameter(int baseValue, int minValue, int maxValue, int defaultValue = 0) {
      this.baseValue = baseValue;
      this.minValue = minValue;
      this.maxValue = maxValue;
      this.defaultValue = defaultValue;
    }

    public static implicit operator IntParameter(int baseValue) => new IntParameter(baseValue);

  }

  internal struct FloatParameter {
    public float baseValue;
    public float minValue;
    public float maxValue;
    public float defaultValue;


    public FloatParameter(float baseValue, float defaultValue = 0f) {
      this.baseValue = baseValue;
      this.minValue = int.MinValue;
      this.maxValue = int.MaxValue;
      this.defaultValue = defaultValue;
    }

    public FloatParameter(float baseValue, float minValue, float maxValue, float defaultValue = 0f) {
      this.baseValue = baseValue;
      this.minValue = minValue;
      this.maxValue = maxValue;
      this.defaultValue = defaultValue;
    }

    public static implicit operator FloatParameter(float baseValue) => new FloatParameter(baseValue);

  }

}
