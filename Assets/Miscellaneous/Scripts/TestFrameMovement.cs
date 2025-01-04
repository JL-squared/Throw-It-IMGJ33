using UnityEngine;

public class TestFrameMovement : MonoBehaviour {
    public float speed;
    private Vector3 start;

    private void Start() {
        start = transform.position;
    }


    // Update is called once per frame
    void Update() {
        float fac = 1.0f;
        if (Time.time % 2 < 1) {
            fac = -1;
        }

        transform.position = transform.position + Vector3.forward * speed * fac * Time.deltaTime;
    }
}
