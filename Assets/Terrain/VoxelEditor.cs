using UnityEngine;

[ExecuteInEditMode]
public class VoxelEditor : MonoBehaviour {
    void Awake() {
        Debug.Log("Editor causes this Awake");
    }

    void Update() {
        Debug.Log("Editor causes this Update");
    }
}
