using UnityEngine;

public class MomoBillboard : MonoBehaviour {
    public GameObject momoTransform;

    // MOMO IS COMING
    void Update() {
        momoTransform.transform.rotation = Player.Instance.camera.transform.rotation * Quaternion.Euler(new Vector3(-90.0f, 0.0f, 0.0f)) * Quaternion.Euler(new Vector3(0.0f, 180.0f, 0.0f));
    }
}
