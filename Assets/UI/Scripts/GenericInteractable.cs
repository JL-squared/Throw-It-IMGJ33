using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

public class GenericInteractable : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    public UnityEvent<PointerEventData> onPointerClick;
    public UnityEvent onPointerEnter;
    public UnityEvent onPointerExit;

    public void OnPointerClick(PointerEventData eventData) {
        onPointerClick?.Invoke(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        onPointerEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData) {
        onPointerExit?.Invoke();
    }

    public void OnDisable() {
        onPointerExit?.Invoke();
    }
}
