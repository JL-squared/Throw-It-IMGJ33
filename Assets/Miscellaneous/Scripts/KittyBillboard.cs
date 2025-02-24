using UnityEngine;

public class KittyBillboard : MonoBehaviour {
    public GameObject kitty;

    // KITTY IS COMING!!
    void Update() {
        kitty.transform.rotation = Player.Instance.camera.transform.rotation * Quaternion.Euler(new Vector3(-90.0f, 0.0f, 0.0f)) * Quaternion.Euler(new Vector3(0.0f, 180.0f, 0.0f));
    }
}
