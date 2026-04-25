using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace DunGenPlus.DevTools {
  internal class PanelTab : MonoBehaviour {
    public bool active;

    [Header("References")]
    public RectTransform rectTransform;
    public Image image;

    void Update() {
      var targetHeight = active ? 48f : 36f;
      var targetColor = active ? new Color(100f / 255f, 100f / 255f, 100f / 255f, 1f) : new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

      var size = rectTransform.sizeDelta;
      size.y = Mathf.Lerp(size.y, targetHeight, Time.deltaTime * 10f);
      rectTransform.sizeDelta = size;
      
      var color = image.color;
      color = Color.Lerp(color, targetColor, Time.deltaTime * 10f);
      image.color = color;
    }
  }
}
