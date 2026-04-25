using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace DunGenPlus.DevTools.HoverUI {
  internal class HoverUIManager : MonoBehaviour {

    public static HoverUIManager Instance { get; private set; }
  

    [Header("References/Default UI")]
    public RectTransform mainCanvasRectTransform;
    public Canvas canvas;
    public RectTransform background;

    public RectTransform textMeshRectTransform;
    public TextMeshProUGUI textMesh;
    public Vector2 preferredTextMeshSize = new Vector2(600f, 50f);

    [Header("Debug")]
    public HoverUIChild previousChild;

    private void Awake() {
      Instance = this;
    }

    public void UpdateDisplay(HoverUIChild child) {

      var text = child.GetHoverString;
      if (string.IsNullOrWhiteSpace(text)) {
        return;
      }

      previousChild = child;
      canvas.enabled = true;

      textMesh.text = text;
      textMeshRectTransform.sizeDelta = preferredTextMeshSize;
      textMesh.ForceMeshUpdate();

      var render = textMesh.GetRenderedValues();
      var margin = textMesh.margin;
      var sizeDelta = render + new Vector2(margin.x + margin.z, margin.y + margin.w);

      var posPivot = GetPositionAndPivot(sizeDelta);

      background.pivot = posPivot.pivot;
      background.position = posPivot.position;

      background.sizeDelta = sizeDelta;
      textMeshRectTransform.sizeDelta = sizeDelta;
      textMesh.ForceMeshUpdate();
    }

    public (Vector2 position, Vector2 pivot) GetPositionAndPivot(Vector2 sizeDelta){  
      if (previousChild == null) return (Vector2.zero, Vector2.zero);
      return GetPositionAndPivot(sizeDelta, previousChild.GetRenderPosition());
    }

    public (Vector2 position, Vector2 pivot) GetPositionAndPivot(Vector2 sizeDelta, (Vector2 pivot, Vector3 position) referencePos){  
      var scaledSizeDelta = sizeDelta * mainCanvasRectTransform.localScale.x;
    
      var pos = referencePos.position;
      var pivot = referencePos.pivot;
      var corners = new Vector3[4];
      mainCanvasRectTransform.GetWorldCorners(corners);

      var left = corners[0].x;
      var bottom = corners[0].y;
      var right = corners[2].x;
      var top = corners[2].y;

      pos.x = Mathf.Clamp(pos.x, left + scaledSizeDelta.x * pivot.x, right - scaledSizeDelta.x * (1f - pivot.x));
      pos.y = Mathf.Clamp(pos.y, bottom + scaledSizeDelta.y * pivot.y, top - scaledSizeDelta.y * (1f - pivot.y));

      return (pos, pivot);
    }


    public void RefreshDisplay(){
      if (previousChild) UpdateDisplay(previousChild);
    }

    public void ClearDisplay(HoverUIChild child){
      if (previousChild != child) return;
      previousChild = null;
      canvas.enabled = false;
    }

  }

}
