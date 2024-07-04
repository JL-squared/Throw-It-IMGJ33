using UnityEngine;

public class HolsterGizmo : MonoBehaviour {
    private void OnDrawGizmos() {
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}
