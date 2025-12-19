using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

public class GenericInteractable : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {
    public UnityEvent<PointerEventData> onPointerClick;
    public UnityEvent<PointerEventData> onPointerEnter;
    public UnityEvent<PointerEventData> onPointerDown;
    public UnityEvent onPointerExit;

    public void OnPointerClick(PointerEventData eventData) {
        onPointerClick?.Invoke(eventData);
    }

    public void OnPointerDown(PointerEventData eventData) {
        onPointerDown?.Invoke(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        onPointerEnter?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData) {
        onPointerExit?.Invoke();
    }

    public void OnDisable() {
        onPointerExit?.Invoke();
    }
}
