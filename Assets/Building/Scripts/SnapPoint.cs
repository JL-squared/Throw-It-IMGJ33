using UnityEngine;

public class SnapPoint : MonoBehaviour {
    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.05f);
    }
}
