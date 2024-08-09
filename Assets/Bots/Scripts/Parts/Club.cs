using UnityEngine;

public class Club : MonoBehaviour {
    public float rpm;
    public Transform rotatingPart;

    // Update is called once per frame
    void Update() {
        rotatingPart.Rotate(new Vector3(rpm * 360f * Time.deltaTime, 0, 0), Space.Self);
    }
}
