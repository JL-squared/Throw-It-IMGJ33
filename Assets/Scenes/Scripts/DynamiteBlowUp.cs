using UnityEngine;

public class DynamiteBlowUp : MonoBehaviour {
    public float timer;
    private bool blewUp = false;

    // Update is called once per frame
    void Update() {
        timer -= Time.deltaTime;

        if (timer < 0 && !blewUp) {
            blewUp = true;
            Destroy(gameObject);
            IVoxelEdit edit = new SphereVoxelEdit {
                center = transform.position,
                strength = -1000.0f,
                material = 0,
                radius = 7.0f,
                writeMaterial = false,
            };

            VoxelTerrain.Instance.ApplyVoxelEdit(edit);
        }
    }
}
