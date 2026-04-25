using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine;

namespace DunGenPlus.DevTools.HoverUI {
  internal class HoverUIChild: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public enum DisplayDirection { Up, Down, Left, Right };
  
    [Header("Display Values")]
    public DisplayDirection direction = DisplayDirection.Up;
    public float directionDistance = 16f;
    public RectTransform rectTransform;
    public bool hovering;

    [Header("Display")]
    [TextArea(2, 4)]
    public string hoverText;

    void Reset(){
      rectTransform = GetComponent<RectTransform>();
    }

    public string GetHoverString => hoverText;

    public void OnPointerEnter(PointerEventData eventData) {
      HoverUIManager.Instance.UpdateDisplay(this);
      hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
      HoverUIManager.Instance.ClearDisplay(this);
      hovering = false;
    }

    private void OnDisable() {
      HoverUIManager.Instance.ClearDisplay(this);
      hovering = false;
    }

    public (Vector2 pivot, Vector3 position) GetRenderPosition(){
      return GetRenderPosition(direction, directionDistance);
    }

    public (Vector2 pivot, Vector3 position) GetRenderPosition(DisplayDirection direction, float directionDistance){
      Vector2 pivot;
      Vector3 position;

      var corners = new Vector3[4];
      rectTransform.GetWorldCorners(corners);

      if (direction == DisplayDirection.Up){
        pivot = new Vector2(0.5f, 0f);  
        position = (corners[1] + corners[2]) * 0.5f + new Vector3(0f, directionDistance);
      } else if (direction == DisplayDirection.Down){
        pivot = new Vector2(0.5f, 1f);
        position = (corners[0] + corners[3]) * 0.5f - new Vector3(0f, directionDistance);
      } else if (direction == DisplayDirection.Left){
        pivot = new Vector2(1f, 0.5f);
        position = (corners[0] + corners[1]) * 0.5f - new Vector3(directionDistance, 0f);
      } else if (direction == DisplayDirection.Right){
        pivot = new Vector2(0f, 0.5f);
        position = (corners[2] + corners[3]) * 0.5f + new Vector3(directionDistance, 0f);
      } else {
        pivot = Vector2.zero;
        position = Vector3.zero;
      }

      return (pivot, position);
    }

  }
}
