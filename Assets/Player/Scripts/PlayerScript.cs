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

    [Header("Death Stuff")]
    public AudioSource ambatakamChoir;
    public bool isDead;

    [Header("Temperature")]
    public const float TargetTemperature = 37.0f;
    private float outsideTemperature;
    private float heatSourcesTemperature;
    private float bodyTemperature = TargetTemperature;
    public float targetReachSpeed = 0.5f;
    public float outsideReachSpeed = 0.5f;
    public float shiverMeTimbers = 0.0f;
    public float shiveringCurrentTime = 10.0f;
    public float shiveringDelay = 10.0f;
    public float minShiveringTemp = 36.0f;
    public float shiveringShakeScale = 0.3f;
    public float shiveringShakeFactor = 2.0f;
    public float shiveringShakeRotationFactor = 2.0f;

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
    [HideInInspector]
    public List<Item> items = new List<Item>();

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
    public UnityEvent<List<Item>> inventoryUpdateEvent;
    public UnityEvent<int, bool> slotUpdateEvent;

    [Header("Movement")]
    [SerializeField]
    EntityMovement movement;
    Vector2 wishHeadDir;
    public Transform head;
    public Camera gameCamera;
    public float mouseSensitivity = 1.0f;

    [Header("Head Bobbing")]
    private float stepValue = 0f;
    private float bobbingStrengthCurrent = 0f;
    public float baseCameraHeight = 0.8f;
    public float bobbingStrength = 0.05f;
    public float bobbingSpeed = 2.5f;
    public float viewModelBobbingStrength = 3.0f;

    [Header("View Model")]
    public GameObject viewModelHolster;
    private GameObject instantiatedViewModel;
    private Vector3 viewModelRotationLocalOffset;
    private Vector3 viewModelPositionLocalOffset;
    public float viewModelSmoothingSpeed = 25f;
    public float viewModelRotationClampMagnitude = 0.2f;
    public float viewModelRotationStrength = -0.001f;
    public float viewModelPositionStrength = 0.2f;

    [Header("Throwing")]
    private float currentCharge;
    private bool isCharging;
    public float chargeSpeed;
    public float minCharge;
    public float maxThrowDelay;
    public float throwDelay;

    private Vector2 currentMouseDelta;
    private Vector2 targetMouseDelta;

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
        placementGhost.SetActive(false);

        EntityHealth health = GetComponent<EntityHealth>();
        health.OnHealthUpdated += (float p) => {
            UIMaster.Instance.healthBar.actualPosition = p;
        };

        health.OnKilled += OnKilled;

        for (int i = 0; i < 10; i++) {
            Item temp = new Item();
            items.Add(temp);
            temp.updateEvent?.AddListener(UpdateInventory);
        }

        addItem(new Item(1, (ItemData)Resources.Load("Items/Snowball")));
        addItem(new Item(1, (ItemData)Resources.Load("Items/Battery")));
    }

    private void OnKilled() {
        // Everything related to killing the actual player
        isDead = true;
        Debug.Log("Skill issue, you dead");
        ambatakamChoir.Play();
        Rigidbody rb = head.AddComponent<Rigidbody>();
        rb.AddForce(Random.insideUnitCircle, ForceMode.Impulse);
        head.AddComponent<SphereCollider>();
        head.transform.parent = null;
        GetComponent<CharacterController>().height = 0;
        Destroy(GetComponentInChildren<MeshRenderer>());

        GameManager.Instance.PlayerDeadLol();
    }

    public void UpdateInventory(Item item) {
        Debug.Log("Update inventory is being called!!");
        inventoryUpdateEvent.Invoke(items);
    }

    private void Update() {
        UpdateTemperature();
        UpdateShivering();
        UpdateCharging();

        Vector2 velocity2d = new Vector2(movement.cc.velocity.x, movement.cc.velocity.z);
        float targetBobbingStrength = 0f;
        if (velocity2d.magnitude > 0.01 && movement.cc.isGrounded) {
            stepValue += velocity2d.magnitude * Time.deltaTime;
            targetBobbingStrength = velocity2d.magnitude / movement.speed;
            targetBobbingStrength = Mathf.Clamp01(targetBobbingStrength);
        } else {
            targetBobbingStrength = 0f;
        }
        bobbingStrengthCurrent = Mathf.Lerp(bobbingStrengthCurrent, targetBobbingStrength, 25f * Time.deltaTime);

        float bobbing = Mathf.Sin(stepValue * bobbingSpeed) * bobbingStrength * bobbingStrengthCurrent;
        if (!isDead)
            head.transform.localPosition = new Vector3(0, baseCameraHeight + bobbing, 0);

        viewModelRotationLocalOffset = Vector3.ClampMagnitude(new Vector3(currentMouseDelta.x, currentMouseDelta.y, 0), viewModelRotationClampMagnitude) * viewModelRotationStrength;
        viewModelPositionLocalOffset = transform.InverseTransformDirection(-movement.cc.velocity) * viewModelPositionStrength;
    
        if (instantiatedViewModel != null) {
            instantiatedViewModel.transform.localPosition = Vector3.Lerp(instantiatedViewModel.transform.localPosition, viewModelRotationLocalOffset + viewModelPositionLocalOffset + Vector3.up * bobbing * viewModelBobbingStrength, Time.deltaTime * viewModelSmoothingSpeed);
        }

        currentMouseDelta = Vector2.Lerp(currentMouseDelta, targetMouseDelta, Time.deltaTime * 25);
    }

    private void LateUpdate() {
        if (isBuilding) UpdatePlacementGhost();
    }

    private void UpdateCharging() {
        if (isCharging) {
            currentCharge += Time.deltaTime * chargeSpeed;
            currentCharge = Mathf.Clamp01(currentCharge);
        } else if (throwDelay > 0.0f) {
            throwDelay -= Time.deltaTime * 1.0f;
        }

        throwDelay = Mathf.Clamp(throwDelay, 0.0f, maxThrowDelay);

        UIMaster.Instance.inGameHUD.UpdateChargeMeter(isCharging ? Mathf.InverseLerp(.2f, 1.0f, currentCharge) : Mathf.InverseLerp(0.0f, maxThrowDelay, throwDelay));
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
        outsideTemperature = GameManager.Instance.weatherManager.GetOutsideTemperature();

        // Actual value that we must try to reach
        float totalTemp = Mathf.Max(outsideTemperature, heatSourcesTemperature);

        // Body temperature calculations (DOES NOT CONSERVE ENERGY)
        bodyTemperature = Mathf.Lerp(bodyTemperature, totalTemp, outsideReachSpeed * Time.deltaTime);
        bodyTemperature = Mathf.Lerp(bodyTemperature, TargetTemperature, targetReachSpeed * Time.deltaTime);
    }

    private void UpdateShivering() {
        if (bodyTemperature < minShiveringTemp) {
            shiveringCurrentTime += Time.deltaTime;
        } else {
            shiveringCurrentTime = 0.0f;
        }

        if (shiveringCurrentTime > shiveringDelay) {
            shiverMeTimbers += Time.deltaTime;
        } else {
            shiverMeTimbers -= Time.deltaTime;
        }
        shiverMeTimbers = Mathf.Clamp01(shiverMeTimbers);


        Vector3 localCamPos = new Vector3(Mathf.PerlinNoise1D(Time.time * shiveringShakeScale + 32.123f) - 0.5f, Mathf.PerlinNoise1D(Time.time * shiveringShakeScale - 2.123f) - 0.5f, 0.0f);
        localCamPos *= shiverMeTimbers;
        localCamPos *= shiveringShakeFactor;
        gameCamera.transform.localPosition = localCamPos;
        gameCamera.transform.localRotation = Quaternion.Lerp(Quaternion.identity, Random.rotation, shiverMeTimbers * Time.deltaTime * shiveringShakeRotationFactor);
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
    /// DO NOT INSERT INVALID STACK COUNT ITEM,
    /// THIS WILL TAKE FROM THE ITEM YOU INSERT. YOU HAVE BEEN WARNED
    /// </summary>
    /// <param name="itemIn"></param>
    public void addItem(Item itemIn) {
        int firstEmpty = -1;
        int i = 0;
        foreach (Item item in items) {
            if (item.IsEmpty() && firstEmpty == -1) {
                firstEmpty = i;
            } else if (itemIn.Data == item.Data && !item.IsFull()) { // this will keep running for as many partial stacks as we can find
                int transferSize = item.Data.stackSize - item.Count; // amount we can fit in here
                transferSize = Mathf.Min(transferSize, itemIn.Count);

                item.Count += transferSize; // transfer
                itemIn.Count -= transferSize;
                if (itemIn.IsEmpty()) {
                    return; // break when done adding into partial stacks
                }
            }
            i++;
        }
        // by this point we should have exited if everything is handled, otherwise;
        if (firstEmpty != -1) {
            Debug.Log("oh yeah, slot was empty. It's sex time...");
            items[firstEmpty].CopyItem(itemIn.Clone()); // Don't know if we actually have to Clone this lol but wtv
            itemIn.MakeEmpty();
        }

        if (firstEmpty == selected) {
            SelectionChanged();
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
            if (item.Count >= count && item.Data.id == id) {
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
        if (!isDead) movement.localWishMovement = context.ReadValue<Vector2>();
    }

    public void ToggleInventory(InputAction.CallbackContext context) {
        if (context.performed && !isDead) {
            UIMaster.Instance.TabPressed();
        }
    }

    public void ExitButton(InputAction.CallbackContext context) {
        if (context.performed) {
            UIMaster.Instance.EscPressed();
        }
    }

    /// <summary>
    /// Input receiver for camera movement
    /// </summary>
    public void Look(InputAction.CallbackContext context) {
        if (Cursor.lockState != CursorLockMode.None && Performed()) {
            targetMouseDelta = context.ReadValue<Vector2>();
            wishHeadDir += targetMouseDelta * mouseSensitivity * 0.02f;
            wishHeadDir.y = Mathf.Clamp(wishHeadDir.y, -90f, 90f);
            head.localRotation = Quaternion.Euler(-wishHeadDir.y, 0f, 0f);
            movement.localWishRotation = Quaternion.Euler(0f, wishHeadDir.x, 0f).normalized;
            
            //lastMouseDelta = Vector2.Lerp(lastMouseDelta, mouseDelta, Time.deltaTime * 20.0f);
        }
    }

    public bool Performed(InputAction.CallbackContext context) {
        return context.performed && UIMaster.Instance.MovementPossible() && !isDead;
    }

    public bool Performed() {
        return UIMaster.Instance.MovementPossible() && !isDead;
    }

    /// <summary>
    /// Input receiver for jumping
    /// </summary>
    public void Jump(InputAction.CallbackContext context) {
        if (Performed(context)) {
            movement.Jump();
        }
    }

    /// <summary>
    /// Input receiver for hotbar 
    /// </summary>
    public void Scroll(InputAction.CallbackContext context) {
        if (Performed(context)) {
            float scroll = -context.ReadValue<float>();
            scroll /= 120;

            if (isBuilding) {
                placementRotation += scroll * 22.5f;
            } else {
                int newSelected = (Selected + scroll.ConvertTo<int>()) % 10;
                Selected = (newSelected < 0) ? 9 : newSelected;
                SelectionChanged();
            }

            //Debug.Log($"scroll action is being called?!?\nScroll: {scroll}");
        }
    }

    public void SelectSlot(InputAction.CallbackContext context) {
        if (Performed(context))
            Selected = (int)context.ReadValue<float>();
        SelectionChanged();
    }

    private void SelectionChanged() {
        Vector3 oldLocalPosition = default;
        Quaternion oldLocalRotation = default;
        if (instantiatedViewModel != null) {
            oldLocalPosition = instantiatedViewModel.transform.localPosition;
            oldLocalRotation = instantiatedViewModel.transform.localRotation;
            Destroy(instantiatedViewModel);
        }

        Item selectedItem = items[selected];
        if (selectedItem != null && selectedItem.Data != null) {
            GameObject viewModel = Instantiate(selectedItem.Data.viewModel, viewModelHolster.transform);
            viewModel.transform.localPosition = oldLocalPosition;
            viewModel.transform.localRotation = oldLocalRotation;
            instantiatedViewModel = viewModel;
        }
    }

    public void TempActivateBuildingMode(InputAction.CallbackContext context) {
        if (Performed(context)) {
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
                //Debug.Log("Fuck it's clipping");
                placementStatus = false;
            }

        } else {
            placementGhost.SetActive(false);
        }
    }

    public void PrimaryAction(InputAction.CallbackContext context) {
        if(!isDead)
        if (isBuilding) {
            if (placementStatus && Performed(context))
                BuildAction();
        } else {
            if (Performed(context)) {
                if(throwDelay == 0.0f) isCharging = true;
            } else {
                // need to have this check since it seems like unity
                // context.performed is FALSE when pressing down for the first time
                // that's cause performed runs on release right??
                // idk, weird
                if (isCharging) {
                    isCharging = false;
                    GetComponent<SnowballThrower>().Throw(currentCharge);
                    throwDelay = maxThrowDelay * currentCharge;
                    currentCharge = minCharge;
                }
                // this should eventually just point to the primary action of the currently selected item
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
        if(Performed(context)) {
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


    public void Craft(CraftingRecipe recipe) {
        recipe.CraftItem(items);
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
