using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Full Player script holding all necessary functions and variables
public class PlayerScript : MonoBehaviour {
    public static PlayerScript singleton;

    [Header("Temperature")]
    public const float TargetTemperature = 37.0f;
    private float outsideTemperature;
    private float heatSourcesTemperature; 
    private float bodyTemperature = TargetTemperature;
    public float targetReachSpeed = 0.5f;
    public float outsideReachSpeed = 0.5f;

    [Header("Building")]
    public bool isBuilding;
    List<Transform> tempSnapPoints1 = new List<Transform>();
    List<Transform> tempSnapPoints2 = new List<Transform>();
    List<Piece> tempPieces = new List<Piece>();
    public GameObject selectedBuildPrefab;
    public GameObject selectedTemp2Prefab;

    int placeRayMask;

    [SerializeField]
    private GameObject placementGhost;
    private bool placementStatus = false;
    private float placementRotation = 0f;

    [Header("Inventory")]
    Item[] items = new Item[10];

    [SerializeField]
    private int selected;
    public int Selected {
        get {
            return selected;
        }
        set {
            selected = value;
            selectedEvent?.Invoke(selected);
        }
    }

    public UnityEvent<int> selectedEvent;

    public UnityEvent<int, bool> slotUpdateEvent;

    [Header("Movement")]
    [SerializeField]
    EntityMovement movement;
    Vector2 wishHeadDir;
    public Transform head;
    public Camera gameCamera;
    public float mouseSensitivity = 1.0f;

    [Header("Throwing")]
    private float currentCharge;
    private bool isCharging;
    public float chargeSpeed;
    public float minCharge;

    [Header("UI")]
    public bool shitted;

    private void Awake() {
        if (singleton != null && singleton != this) {
            Destroy(gameObject);
        } else {
            singleton = this;
        }

        placeRayMask = LayerMask.GetMask("Default", "Piece");
    }

    // Start is called before the first frame update
    void Start() {
        bodyTemperature = TargetTemperature;
        Cursor.lockState = CursorLockMode.Locked;
        movement = GetComponent<EntityMovement>();
        currentCharge = minCharge;

        SetupPlacementGhost(selectedBuildPrefab);
    }

    private void Update() {
        UpdateTemperature();
        UpdateCharging();
    }

    private void LateUpdate() {
        if(isBuilding) UpdatePlacementGhost();
    }

    private void UpdateCharging() {
        if (isCharging) {
            currentCharge += Time.deltaTime * chargeSpeed;
            currentCharge = Mathf.Clamp01(currentCharge);
        }
    }

    private void UpdateTemperature() {
        // Calculate heat from sources
        HeatSource[] sources = GameObject.FindObjectsOfType<HeatSource>();
        heatSourcesTemperature = 0.0f;
        foreach (var source in sources) {
            float dist = Vector3.Distance(transform.position, source.transform.position);
            float invLerp = Mathf.InverseLerp(source.minRangeRadius, source.maxRangeRadius, dist);
            invLerp = Mathf.Pow(1 - Mathf.Clamp01(invLerp), 2);

            heatSourcesTemperature += source.sourceTemperature * invLerp;
        }

        // Get outside temp from weather system
        outsideTemperature = GameManager.Singleton.weatherManager.GetOutsideTemperature();

        // Actual value that we must try to reach
        float totalTemp = Mathf.Max(outsideTemperature, heatSourcesTemperature);

        // Body temperature calculations (DOES NOT CONSERVE ENERGY)
        bodyTemperature = Mathf.Lerp(bodyTemperature, totalTemp, outsideReachSpeed * Time.deltaTime);
        bodyTemperature = Mathf.Lerp(bodyTemperature, TargetTemperature, targetReachSpeed * Time.deltaTime);
    }

    private void OnGUI() {
        GUI.Label(new Rect(0, 0, 1000, 20), "Body Temperature: " + bodyTemperature.ToString("F3"));
        GUI.Label(new Rect(0, 20, 1000, 20), "Scene Temperature: " + outsideTemperature.ToString("F3"));
        GUI.Label(new Rect(0, 40, 1000, 20), "Sources Temperature: " + heatSourcesTemperature.ToString("F3"));
        GUI.Label(new Rect(0, 60, 1000, 20), "Selected Piece: " + placementGhost.name);
        GUI.Label(new Rect(0, 80, 1000, 20), "Current Snowball Charge: " + currentCharge);
    }

    /// <summary>
    /// Add item to inventory if possible,
    /// DO NOT INSERT INVALID STACK COUNT ITEM
    /// </summary>
    /// <param name="itemIn"></param>
    public void addItem(ref Item itemIn) {
        int firstEmpty = -1;
        int i = 0;
        foreach (Item item in items) {
            if (itemIn.data.id == item.data.id && !item.IsFull()) { // this will keep running for as many partial stacks as we can find
                int transferSize = itemIn.Count - item.data.stackSize; // amount we can fit in here

                item.Count += transferSize; // transfer
                itemIn.Count -= transferSize;
                if (itemIn.IsEmpty()) {
                    return; // break when done adding into partial stacks
                }
            } else if (item.IsEmpty() && firstEmpty != -1) {
                firstEmpty = i;
            }
            i++;
        }
        // by this point we should have exited if everything is handled, otherwise;
        if(firstEmpty != -1) {
            items[firstEmpty] = itemIn.Clone();
            itemIn.MakeEmpty();
        }
    }

    /// <summary>
    /// Returns slot number (0-9) if we have item of specified count (default 1), otherwise returns -1
    /// </summary>
    /// <param name="id"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public int checkForItem(string id, int count = 1) {
        int i = 0;

        foreach (Item item in items) {
            if (item.Count >= count && item.data.id == id) {
                return i;
            }
            i++;
        }

        return -1;
    }

    /// <summary>
    /// Input receiver for movement
    /// </summary>
    public void Movement(InputAction.CallbackContext context) {
        movement.localWishMovement = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Input receiver for camera movement
    /// </summary>
    public void Look(InputAction.CallbackContext context) {
        if(Cursor.lockState != CursorLockMode.None) {
            wishHeadDir += context.ReadValue<Vector2>() * mouseSensitivity * 0.02f;
            wishHeadDir.y = Mathf.Clamp(wishHeadDir.y, -90f, 90f);
            head.localRotation = Quaternion.Euler(-wishHeadDir.y, 0f, 0f);
            movement.localWishRotation = Quaternion.Euler(0f, wishHeadDir.x, 0f).normalized;
        }
    }

    /// <summary>
    /// Input receiver for jumping
    /// </summary>
    public void Jump(InputAction.CallbackContext context) {
        if (context.performed && movement.cc.isGrounded) {
            movement.isJumping = true;
        }
    }

    /// <summary>
    /// Input receiver for hotbar 
    /// </summary>
    public void Scroll(InputAction.CallbackContext context) {
        if(context.performed) {
            float scroll = -context.ReadValue<float>();
            scroll /= 120;

            if (isBuilding) {
                placementRotation += scroll * 22.5f;
            } else {
                int newSelected = (Selected + scroll.ConvertTo<int>()) % 10;
                Selected = (newSelected < 0) ? 9 : newSelected;
            }

            //Debug.Log($"scroll action is being called?!?\nScroll: {scroll}");
        }
    }

    public void SelectSlot(InputAction.CallbackContext context) {
        if (context.performed)
        Selected = (int)context.ReadValue<float>();
    }

    public void TempActivateBuildingMode(InputAction.CallbackContext context) {
        if (context.performed) {
            isBuilding = !isBuilding;
            placementGhost.SetActive(false);
        }
    }

    // Test for placement location
    private bool PieceRayTest(out Vector3 point, out Vector3 normal, out Piece piece) {
        int layerMask = placeRayMask;

        // Send a raycast
        if (Physics.Raycast(gameCamera.transform.position, gameCamera.transform.forward, out var hitInfo, 50f, layerMask)) {
            float num = 30f;
            if ((bool)placementGhost) {
                Piece component = placementGhost.GetComponent<Piece>();
                //if ((object)component != null) {
                //    num += (float)component.m_extraPlacementDistance;
                //}
            }
            if ((bool)hitInfo.collider && !hitInfo.collider.attachedRigidbody && Vector3.Distance(head.position, hitInfo.point) < num) {
                point = hitInfo.point;
                normal = hitInfo.normal;
                piece = hitInfo.collider.GetComponentInParent<Piece>();
                return true;
            }
        }

        // If raycast fails, return nothing
        point = Vector3.zero;
        normal = Vector3.zero;
        piece = null;
        return false;
    }

    private void SetupPlacementGhost(GameObject prefab) {
        if ((bool)placementGhost) {
            Destroy(placementGhost);
            placementGhost = null;
        }

        placementGhost = Instantiate(prefab);
        placementGhost.name = prefab.name;

        Collider[] componentsInChildren1 = placementGhost.GetComponentsInChildren<Collider>();
        foreach (Collider collider in componentsInChildren1) {
            if (((1 << collider.gameObject.layer) & placeRayMask) == 0) {
                Debug.Log("Disabling " + collider.gameObject.name + "  " + LayerMask.LayerToName(collider.gameObject.layer));
                collider.enabled = false;
            }
        }

        Transform[] componentsInChildren2 = placementGhost.GetComponentsInChildren<Transform>();
        int layer = LayerMask.NameToLayer("Ghost");
        Transform[] array = componentsInChildren2;
        for (int i = 0; i < array.Length; i++) {
            array[i].gameObject.layer = layer;
        }

        placementGhost.transform.position = transform.position;
    }

    private void UpdatePlacementGhost() {
        bool flag = false; // manual placement mode btw

        if (PieceRayTest(out var point, out var normal, out Piece piece)) {
            placementGhost.SetActive(true);
            placementStatus = true;
            Collider[] componentsInChildren = placementGhost.GetComponentsInChildren<Collider>();

            Quaternion quaternion = Quaternion.Euler(new Vector3(0f, placementRotation, 0f));

            // now it's true time freaky true (piss)
            if (componentsInChildren.Length != 0) {
                placementGhost.transform.position = point + normal * 50f;
                placementGhost.transform.rotation = quaternion;
                Vector3 vector = Vector3.zero;
                float num2 = 999999f;
                Collider[] array = componentsInChildren;
                foreach (Collider collider in array) {
                    collider.enabled = true;
                    if (collider.isTrigger || !collider.enabled) {
                        //Debug.Log("this is porn");
                        continue;
                    }

                    MeshCollider meshCollider = collider as MeshCollider;
                    if (!(meshCollider != null) || meshCollider.convex) {
                        Vector3 vector2 = collider.ClosestPoint(point);
                        float num3 = Vector3.Distance(vector2, point);
                        if (num3 < num2) {
                            vector = vector2;
                            num2 = num3;
                        }
                        collider.enabled = false;
                    }
                }
                Vector3 vector3 = placementGhost.transform.position - vector;
                placementGhost.transform.position = point + vector3;
                placementGhost.transform.rotation = quaternion;
            }

            if (!flag) {
                tempPieces.Clear();
                if (FindClosestSnapPoints(placementGhost.transform, 0.5f, out var a, out var b, tempPieces)) {
                    _ = b.parent.position;
                    Vector3 vector4 = b.position - (a.position - placementGhost.transform.position);
                    placementGhost.transform.position = vector4;
                    if (!IsOverlappingOtherPiece(vector4, placementGhost.transform.rotation, placementGhost.name, tempPieces, true)) {
                        placementGhost.transform.position = vector4;
                    }
                }
            }

            if(TestGhostClipping(placementGhost, 0.2f)) {
                Debug.Log("Fuck it's clipping");
                placementStatus = false;
            }

        } else {
            placementGhost.SetActive(false);
        }
    }

    public void PrimaryAction(InputAction.CallbackContext context) {
        if (isBuilding) {
            if (placementStatus && context.performed)
                BuildAction();
        } else {
            if (context.performed) {
                isCharging = true;
            } else {
                // need to have this check since it seems like unity
                // context.performed is FALSE when pressing down for the first time
                // idk, weird
                if (isCharging) {
                    isCharging = false;
                    GetComponent<SnowballThrower>().Throw(currentCharge);
                    currentCharge = minCharge;
                }
            }
        }
    }

    private void BuildAction() {
        GameObject builtPiece = Instantiate(whichThing ? selectedBuildPrefab : selectedTemp2Prefab);
        builtPiece.transform.SetPositionAndRotation(placementGhost.transform.position, placementGhost.transform.rotation);
        builtPiece.SetActive(true);
        builtPiece.layer = LayerMask.NameToLayer("Piece");
    }

    bool whichThing = true;

    public void SecondaryAction(InputAction.CallbackContext context) {
        if(context.performed) {
            if (isBuilding) {
                BuildAction2();
            } else {
                // secondary action that isn't build
            }
        }
    }

    private void BuildAction2() {
        Debug.Log("switched");
        whichThing = !whichThing;
        SetupPlacementGhost(whichThing ? selectedBuildPrefab : selectedTemp2Prefab);
    }

    private bool FindClosestSnapPoints(Transform ghost, float maxSnapDistance, out Transform a, out Transform b, List<Piece> pieces) {
        tempSnapPoints1.Clear();
        ghost.GetComponent<Piece>().GetSnapPoints(tempSnapPoints1);
        tempSnapPoints2.Clear();
        tempPieces.Clear();
        Piece.GetSnapPoints(ghost.transform.position, 10f, tempSnapPoints2, tempPieces);
        float num = 9999999f;
        a = null;
        b = null;
        /*
        if (m_manualSnapPoint >= 0) {
            if (FindClosestSnappoint(tempSnapPoints1[manualSnapPoint].position, tempSnapPoints2, maxSnapDistance, out var closest, out var _)) {
                a = tempSnapPoints1[manualSnapPoint];
                b = closest;
                return true;
            }
            return false;
        }
        */
        foreach (Transform item in tempSnapPoints1) {
            if (FindClosestSnappoint(item.position, tempSnapPoints2, maxSnapDistance, out var closest2, out var distance2) && distance2 < num) {
                num = distance2;
                a = item;
                b = closest2;
            }
        }
        return a != null;
    }

    private bool FindClosestSnappoint(Vector3 p, List<Transform> snapPoints, float maxDistance, out Transform closest, out float distance) {
        closest = null;
        distance = 999999f;
        foreach (Transform snapPoint in snapPoints) {
            float num = Vector3.Distance(snapPoint.position, p);
            if (!(num > maxDistance) && num < distance) {
                closest = snapPoint;
                distance = num;
            }
        }
        return closest != null;
    }

    private bool TestGhostClipping(GameObject ghost, float maxPenetration) {
        Collider[] componentsInChildren = ghost.GetComponentsInChildren<Collider>();
        Collider[] array = Physics.OverlapSphere(ghost.transform.position, 10f, placeRayMask);
        Collider[] array2 = componentsInChildren;
        foreach (Collider collider in array2) {
            Collider[] array3 = array;
            foreach (Collider collider2 in array3) {
                if (Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation, collider2, collider2.transform.position, collider2.transform.rotation, out var _, out var distance) && distance > maxPenetration) {
                    Debug.Log("Distance: " + distance);
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsOverlappingOtherPiece(Vector3 p, Quaternion rotation, string pieceName, List<Piece> pieces, bool allowRotatedOverlap) {
        foreach (Piece tempPiece in tempPieces) {
            if (Vector3.Distance(p, tempPiece.transform.position) < 0.05f && (!allowRotatedOverlap || !(Quaternion.Angle(tempPiece.transform.rotation, rotation) > 10f))) {
                return true;
            }
        }
        return false;
    }
}
