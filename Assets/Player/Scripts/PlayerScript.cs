using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

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

    [SerializeField]
    private GameObject placementGhost;

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

    private void Awake() {
        if (singleton != null && singleton != this) {
            Destroy(gameObject);
        } else {
            singleton = this;
        }
    }

    // Start is called before the first frame update
    void Start() {
        bodyTemperature = TargetTemperature;
        Cursor.lockState = CursorLockMode.Locked;
        movement = GetComponent<EntityMovement>();
    }

    private void Update() {
        UpdateTemperature();
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
    }

    /// <summary>
    /// Add item to inventory if possible
    /// </summary>
    /// <param name="itemIn"></param>
    public void addItem(ref Item itemIn) {
        foreach (Item item in items) {
            if (itemIn.data.id == item.data.id) {
                item.Count += itemIn.Count; // TODO ; ACTUAL STACK SIZE LOL
                itemIn.Count = 0;
            }
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
            if (item.Count == count && item.data.id == id) {
                return i;
            }
            i++;
        }

        return -1;
    }

    public void Movement(InputAction.CallbackContext context) {
        movement.localWishMovement = context.ReadValue<Vector2>();
    }

    public void Look(InputAction.CallbackContext context) {
        wishHeadDir += context.ReadValue<Vector2>() * mouseSensitivity * 0.02f;
        wishHeadDir.y = Mathf.Clamp(wishHeadDir.y, -90f, 90f);
        head.localRotation = Quaternion.Euler(-wishHeadDir.y, 0f, 0f);
        movement.localWishRotation = Quaternion.Euler(0f, wishHeadDir.x, 0f).normalized;
    }

    public void Jump(InputAction.CallbackContext context) {
        if (context.performed && movement.cc.isGrounded) {
            movement.isJumping = true;
        }
    }

    public void Scroll(InputAction.CallbackContext context) {
        if(context.performed) {
            float scroll = -context.ReadValue<float>();
            scroll /= 120;

            if (isBuilding) {
                // do the rotato
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
        if (context.performed)
            isBuilding = !isBuilding;
    }

    private bool PieceRayTest(out Vector3 point, out Vector3 normal, out Piece piece) {
        //int layerMask = m_placeRayMask;

        if (Physics.Raycast(gameCamera.transform.position, gameCamera.transform.forward, out var hitInfo, 50f)) {
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
        point = Vector3.zero;
        normal = Vector3.zero;
        piece = null;
        return false;
    }
}
