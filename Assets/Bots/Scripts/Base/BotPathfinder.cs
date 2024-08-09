using UnityEngine;
using UnityEngine.AI;

public class BotPathfinder : MonoBehaviour {
    private EntityMovement em;
    public float rotationSmoothing;
    public float speedDistFalloff;
    private NavMeshPath path;
    public LayerMask mask;
    public Vector3 target;
    private Vector3[] corners = new Vector3[0]; 
    public string agentType = "Bot";
    public bool pathfind = true;

    public void Start() {
        em = GetComponent<EntityMovement>();
        path = new NavMeshPath();
    }

    // https://forum.unity.com/threads/how-do-i-get-the-int-value-of-a-navigation-agent-type.472340/
    private int GetNavMeshAgentID(string name) {
        for (int i = 0; i < NavMesh.GetSettingsCount(); i++) {
            NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(index: i);
            if (name == NavMesh.GetSettingsNameFromID(agentTypeID: settings.agentTypeID)) {
                return settings.agentTypeID;
            }
        }

        Debug.LogError("Could not find navmesh agent id");
        return 0;
    }

    private Vector3 GetAppropriateDir(Vector3[] corners) {
        for (int i = 0; i < Mathf.Min(3, corners.Length); i++) {
            Vector3 first = corners[i];
            Vector3 direction = -(transform.position - first);
            Vector2 local = new Vector2(direction.x, direction.z);

            if (local.magnitude > 0.05) {
                return direction;
            }
        }

        return Vector3.zero;
    }

    public void Update() {
        if (pathfind && GameManager.Instance.pathfindingRebuilder.useNavMesh) {
            NavMeshQueryFilter filter = new NavMeshQueryFilter();
            filter.agentTypeID = GetNavMeshAgentID(agentType);
            filter.SetAreaCost(0, 1f);
            filter.areaMask = NavMesh.AllAreas;

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit info, 10000f, mask)) {
                Vector3 starting = info.point + Vector3.up * 0.2f;
                bool sampled = NavMesh.SamplePosition(starting, out NavMeshHit navmeshHit, 2.0f, NavMesh.AllAreas);
                Vector3 better = sampled ? navmeshHit.position : starting;
                DebugUtils.DrawSphere(better, 0.2f, Color.green);

                if (NavMesh.CalculatePath(better, target, filter, path) && sampled) {
                    Vector3 direction = GetAppropriateDir(path.corners);
                    corners = path.corners;
                    direction.y = 0;
                    Debug.DrawRay(transform.position, direction.normalized, Color.white, 1.0f);

                    if (direction.magnitude > 0.01) {
                        float speedMult = Mathf.Clamp(direction.magnitude / speedDistFalloff, 0.5f, 1.0f);
                        em.localWishMovement = Vector2.up * speedMult;
                        em.localWishRotation = Quaternion.LookRotation(direction.normalized);
                    } else {
                        em.localWishMovement = Vector2.zero;
                    }
                } else {
                    em.localWishMovement = Vector2.zero;
                }
            }
        } else {
            Vector3 direction = -(transform.position - target);
            direction.y = 0;
            Vector2 local = new Vector2(direction.x, direction.z);
            em.localWishMovement = Vector2.up;
            em.localWishRotation = Quaternion.LookRotation(direction.normalized);
        }
        
    }

    public void OnDrawGizmos() {
        for (int i = 0; i < corners.Length-1; i++) {
            Gizmos.DrawSphere(corners[i], 0.2f);
            Gizmos.DrawLine(corners[i], corners[i + 1]);
        }
    }
}
