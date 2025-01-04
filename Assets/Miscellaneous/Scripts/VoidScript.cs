using UnityEngine;

public class VoidScript : MonoBehaviour {
    void OnTriggerEnter(Collider other) {
        other.gameObject.transform.position = new Vector3 (0, 7, 0);
    }
}
