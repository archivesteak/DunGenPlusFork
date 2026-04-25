using DunGenPlus.DevTools.HoverUI;
using DunGenPlus.DevTools.UIElements.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DunGenPlus.DevTools.UIElements {
  internal abstract class BaseUIElement : MonoBehaviour {

    public TextMeshProUGUI titleTextMesh;
    internal string title;

    public LayoutElement layoutElement;
    internal float layoutOffset;
    internal float layoutWidthBase;

    public void SetupBase(TitleParameter titleParameter) {
      title = titleParameter.text;
      SetText(title);
      SetHoverText(titleParameter.hoverText);

      layoutOffset = titleParameter.offset;
      if (layoutElement) {
        layoutElement.minWidth -= layoutOffset;
        layoutWidthBase = layoutElement.minWidth;
      }  
    }

    void Update(){
      if (layoutElement) {
        var minWidth = layoutWidthBase;
        if (DevDebugManager.Instance.canvasExtended) minWidth += 40f;

        layoutElement.minWidth = Mathf.Lerp(layoutElement.minWidth, minWidth, Time.deltaTime * 10f);
      }
    }

    public void SetText(string value) {
      titleTextMesh.text = value;
    }

    public void SetHoverText(string value){
      var hoverChild = GetComponentInChildren<HoverUIChild>();
      if (hoverChild) {
        hoverChild.hoverText = value;
      }
    }

  }
}
