using Unity.Mathematics;
using UnityEngine;

// Sample script for editing terrain, loading/saving, and modifying props
public class PlayerControllerScript : MonoBehaviour {
    public GameObject head;
    public GameObject indicator;
    private float sizeRadius = 1.0F;
    private int target = 0;

    private void Start() {
    }

    private void Callback(int[] changed) {
        Debug.Log(changed[0]);
    }

    // Update is called once per frame
    void Update() {
        if (Physics.Raycast(head.transform.position + head.transform.forward * 1, head.transform.forward * 2, out RaycastHit hit, 5000.0F)) {
            bool add = Input.GetKey(KeyCode.LeftShift);
            float temp = add ? -1F : 1F * Time.deltaTime * 20.0f;
            target += Input.GetKeyDown(KeyCode.K) ? 1 : 0;
            target = target % 3;

            if (Input.GetMouseButtonDown(0)) {
                switch (target) {
                    case 0:
                        var edit = new FlattenVoxelEdit {
                            center = hit.point,
                            normal = hit.normal,
                            radius = sizeRadius,
                            strength = temp * 10,
                        };

                        VoxelTerrain.Instance.ApplyVoxelEdit(edit, callback: Callback);
                        break;
                    case 1:
                        var edit2 = new AddVoxelEdit {
                            center = math.float3(hit.point.x, hit.point.y, hit.point.z),
                            radius = sizeRadius,
                            strength = 10.0F * temp,
                            writeMaterial = true,
                            material = 0,
                        };

                        VoxelTerrain.Instance.ApplyVoxelEdit(edit2, callback: Callback);
                        break;
                    case 2:
                        var edit3 = new SphereVoxelEdit {
                            center = math.float3(hit.point.x, hit.point.y, hit.point.z),
                            radius = sizeRadius,
                            strength = 10.0F * temp,
                            writeMaterial = true,
                            material = 0,
                        };

                        VoxelTerrain.Instance.ApplyVoxelEdit(edit3, callback: Callback);
                        break;
                }
            }

            if (Input.GetMouseButton(1)) {
                /*
                var edit2 = new FlattenVoxelEdit {
                    center = hit.point,
                    normal = hit.normal,
                    radius = sizeRadius,
                    strength = temp * 10,
                };
                */

                var edit2 = new AddVoxelEdit {
                    center = math.float3(hit.point.x, hit.point.y, hit.point.z),
                    radius = sizeRadius,
                    strength = 0.7F * temp,
                    writeMaterial = true,
                    material = 0,
                };

                VoxelTerrain.Instance.ApplyVoxelEdit(edit2, callback: Callback);
            }

            indicator.transform.position = Vector3.Lerp(indicator.transform.position, hit.point, 50f * Time.deltaTime);
            indicator.transform.rotation = Quaternion.Lerp(indicator.transform.rotation, Quaternion.LookRotation(hit.normal), 50f * Time.deltaTime);
        } else {
            indicator.transform.position = Vector3.Lerp(indicator.transform.position, head.transform.forward * 200.0F + head.transform.position, 50f * Time.deltaTime);
        }

        indicator.transform.localScale = Vector3.one * sizeRadius * 2;

        sizeRadius += Input.mouseScrollDelta.y * Time.deltaTime * 45.0F;
        sizeRadius = Mathf.Clamp(sizeRadius, 0, 500);
    }
}