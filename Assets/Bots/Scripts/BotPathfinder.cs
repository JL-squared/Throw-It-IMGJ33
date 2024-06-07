using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BotPathfinder : MonoBehaviour {
    private EntityMovement em;
    public float rotationSmoothing;
    public float speedDistFalloff;
    private NavMeshPath path;
    public Vector3 target;

    public void Start() {
        em = GetComponent<EntityMovement>();
        path = new NavMeshPath();
    }

    private Vector3 GetAppropriateDir(Vector3[] corners) {
        for (int i = 0; i < Mathf.Min(3, corners.Length); i++) {
            Vector3 first = corners[i];
            Vector3 direction = -(transform.position - first);
            Vector2 local = new Vector2(direction.x, direction.z);

            if (local.magnitude > 0.05 && direction.magnitude > 1.0) {
                return direction;
            }
        }

        return Vector3.zero;
    }

    public void Update() {
        if (NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path)) {
            Vector3 direction = GetAppropriateDir(path.corners);
            direction.y = 0;
            Debug.DrawRay(transform.position, direction.normalized, Color.white, 1.0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction.normalized), rotationSmoothing * Time.deltaTime);
            float speedMult = Mathf.Clamp(direction.magnitude / speedDistFalloff, 0.5f, 1.0f);
            em.localWishMovement = Vector2.up * speedMult;
        } else {
            em.localWishMovement = Vector2.zero;
        }
    }
}
