using UnityEngine;

public class ProcHolster : MonoBehaviour {
    private void OnDrawGizmos() {
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}
