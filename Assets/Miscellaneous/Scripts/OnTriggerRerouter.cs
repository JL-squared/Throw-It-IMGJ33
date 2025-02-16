using UnityEngine;
using UnityEngine.Events;

public class OnTriggerRerouter : MonoBehaviour {
    public UnityEvent<Collider> onTriggerEnter;

    private void OnTriggerEnter(Collider other) {
        onTriggerEnter?.Invoke(other);
    }
}
