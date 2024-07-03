using UnityEngine;

public class EngineShake : MonoBehaviour {
    public float shakeStrengthVertical;
    public float shakeStrengthRoll;
    public Transform mesh;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        float shake = 2f * (Random.value - 0.5f);
        float shake2 = 2f * (Random.value - 0.5f);
        mesh.rotation = Quaternion.Euler(shake * shakeStrengthVertical, 0f, shake2 * shakeStrengthRoll);
    }
}
