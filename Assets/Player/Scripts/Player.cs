using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Tweens.Core;
using Tweens;
using System;

// Full Player script holding all necessary functions and variables
public class Player : MonoBehaviour {
    public static Player Instance;

    #region Death Stuff
    [Header("Death Stuff")]
    public AudioSource ambatakamChoir;
    public bool isDead;
    #endregion

    #region Temperature
    [Header("Temperature")]
    public float targetTemperature = 37.0f;
    private float outsideTemperature;
    private float heatSourcesTemperature;
    private float bodyTemperature;
    public float targetReachSpeed = 0.5f;
    public float outsideReachSpeed = 0.5f;
    public float shiverMeTimbers = 0.0f;
    public float shiveringCurrentTime = 10.0f;
    public float shiveringDelay = 10.0f;
    public float minShiveringTemp = 36.0f;
    public float shiveringShakeScale = 0.3f;
    public float shiveringShakeFactor = 2.0f;
    public float shiveringShakeRotationFactor = 2.0f;
    #endregion

    #region Building
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
    #endregion

    #region Inventory
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
    public Item SelectedItem { get { return items[selected]; } }

    public UnityEvent<int> selectedEvent;
    public UnityEvent<List<Item>> inventoryUpdateEvent;
    public UnityEvent<int, bool> slotUpdateEvent;
    #endregion

    #region Movement
    [Header("Movement")]
    [SerializeField]
    EntityMovement movement;
    [HideInInspector]
    public Vector2 wishHeadDir;
    public Transform head;
    public Camera gameCamera;
    public float mouseSensitivity = 1.0f;
    [HideInInspector]
    public Vector2 localWishMovement;

    [Min(1.0f)]
    public float sprintModifier;

    public bool isSprinting;
    public bool isCrouching;
    #endregion

    #region Camera
    [Header("Camera")]
    private float stepValue = 0f;
    private float bobbingStrengthCurrent = 0f;
    public float baseCameraHeight = 0.8f;
    public float bobbingStrength = 0.05f;
    public float bobbingSpeed = 2.5f;
    public float bobbingSteppiness = 20f;
    public float viewModelBobbingStrength = 3.0f;
    public float defaultFOV = 90.0f;
    #endregion

    #region View Model & View Model Sway
    [Header("View Model")]
    public GameObject viewModelHolster;
    private GameObject viewModel;
    private Vector3 viewModelRotationLocalOffset;
    private Vector3 viewModelPositionLocalOffset;
    public float viewModelSmoothingSpeed = 25f;
    public float viewModelRotationClampMagnitude = 0.2f;
    public float viewModelRotationStrength = -0.001f;
    public float viewModelPositionStrength = 0.2f;
    private EquippedItemLogic itemLogic;
    private Item lastSelectedViewModelItem;
    private Vector2 currentMouseDelta;
    private Vector2 targetMouseDelta;
    #endregion

    #region Interaction
    [HideInInspector]
    public RaycastHit? lookingAt;
    private IInteraction interaction;
    private IInteraction lastInteraction;
    #endregion

    [HideInInspector]
    public Vehicle vehicle;
    private PlayerControlsSettings settings;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }

        placeRayMask = LayerMask.GetMask("Default", "Piece");
    }

    void Start() {
        settings = Utils.Load<PlayerControlsSettings>("player.json", new PlayerControlsSettings());
        mouseSensitivity = settings.mouseSensivity;
        bodyTemperature = targetTemperature;
        Cursor.lockState = CursorLockMode.Locked;
        movement = GetComponent<EntityMovement>();

        // Setup building stuff
        SetupPlacementGhost(selectedBuildPrefab);
        placementGhost.SetActive(false);

        // Hook onto health component
        EntityHealth health = GetComponent<EntityHealth>();
        health.OnHealthChanged += (float p) => {
            UIMaster.Instance.healthBar.HealthChanged(p);
        };
        health.OnKilled += Killed;


        // Creates inventory items and hooks onto their update event
        for (int i = 0; i < 10; i++) {
            Item temp = new Item(null, 0);
            items.Add(temp);
            temp.updateEvent?.AddListener((Item item) => { inventoryUpdateEvent.Invoke(items); });
        }

        // Add temp items at start
        AddItem(new Item("snowball", 1));
        AddItem(new Item("battery", 1));
        AddItem(new Item("shovel", 1));
        AddItem(new Item("wires", 1));
    }

    private void Killed() {
        Debug.Log("Skill issue, you dead");
        isDead = true;

        // Literal hell
        ambatakamChoir.Play();

        // Make the camera a rigidbody
        Rigidbody rb = head.gameObject.AddComponent<Rigidbody>();
        rb.AddForce(UnityEngine.Random.insideUnitCircle, ForceMode.Impulse);
        head.gameObject.AddComponent<SphereCollider>();
        head.transform.parent = null;
        GetComponent<CharacterController>().height = 0;
        Destroy(GetComponentInChildren<MeshRenderer>());

        // Lets others know that the player died
        GameManager.Instance.PlayerKilled();
    }

    private void Update() {
        UpdateTemperature();
        UpdateShivering();
        UpdateMovement();

        float bobbing = settings.cameraBobbing ? CalculateBobbing() : 0f;

        if (settings.viewModelSway) {
            ApplyHandSway(bobbing);
        } else {
            viewModel.transform.localPosition = Vector3.zero;
        }

        if (vehicle == null) {
            if (Physics.Raycast(gameCamera.transform.position, gameCamera.transform.forward, out RaycastHit info, 5f, ~LayerMask.GetMask("Player"))) {
                GameObject other = info.collider.gameObject;
                interaction = other.GetComponent<IInteraction>();
                lookingAt = info;
            } else {
                lookingAt = null;
                interaction = null;
            }
        }
        
        if (!UnityEngine.Object.ReferenceEquals(lastInteraction, interaction) || (lastInteraction.IsNullOrDestroyed() ^ interaction.IsNullOrDestroyed())) {
            if (!interaction.IsNullOrDestroyed()) {
                Debug.Log("test");
                interaction.StartHover(this);
            }
            
            if (!lastInteraction.IsNullOrDestroyed()) {
                lastInteraction.StopHover(this);
            }
        }

        UIMaster.Instance.inGameHUD.SetInteractHint(interaction != null && interaction.Interactable);
        lastInteraction = interaction;

        if (vehicle != null) {
            movement.entityMovementFlags.RemoveFlag(EntityMovementFlags.ApplyMovement);
            GetComponent<CharacterController>().enabled = false;
        } else {
            movement.entityMovementFlags.AddFlag(EntityMovementFlags.ApplyMovement);
            GetComponent<CharacterController>().enabled = true;
        }
    }

    public void UpdateMovement() {
        if (isSprinting) {
            movement.ModifySpeed(sprintModifier);
        } else {
            movement.ModifySpeed();
        }
    }

    private void LateUpdate() {
        if (isBuilding) UpdatePlacementGhost();
    }

    #region Polish & Effects
    public void FOVTween() {
        var tween = new FloatTween {
            from = gameCamera.fieldOfView,
            to = isSprinting && localWishMovement.magnitude > 0.0f ? defaultFOV + 10 : defaultFOV,
            duration = 0.2f,
            onUpdate = (instance, value) => {
                gameCamera.fieldOfView = value;
            },
            easeType = EaseType.QuadOut,
        };
        gameObject.AddTween(tween);
    }

    private void ApplyHandSway(float bobbing) {
        viewModelRotationLocalOffset = Vector3.ClampMagnitude((new Vector3(currentMouseDelta.x, currentMouseDelta.y, 0) / (Time.deltaTime + 0.001f)) * 0.01f, viewModelRotationClampMagnitude) * viewModelRotationStrength;
        viewModelPositionLocalOffset = transform.InverseTransformDirection(-movement.Velocity) * viewModelPositionStrength;
        if (viewModel != null) {
            Vector3 current = viewModel.transform.localPosition;
            Vector3 target = viewModelRotationLocalOffset + viewModelPositionLocalOffset;
            target += Vector3.up * bobbing * viewModelBobbingStrength;

            if (itemLogic != null) {
                target += itemLogic.swayOffset;
            }

            Vector3 localPosition = Vector3.Lerp(current, target, Time.deltaTime * viewModelSmoothingSpeed);
            viewModel.transform.localPosition = localPosition;
        }
        currentMouseDelta = Vector2.Lerp(currentMouseDelta, targetMouseDelta, Time.deltaTime * 25);
    }

    private float CalculateBobbing() {
        Vector2 velocity2d = new Vector2(movement.Velocity.x, movement.Velocity.z);

        // Calculate bobbing strength based on player velocity relative to their max speed
        float targetBobbingStrength = 0f;
        if (velocity2d.magnitude > 0.01 && movement.IsGrounded) {
            stepValue += velocity2d.magnitude * Time.deltaTime;
            targetBobbingStrength = velocity2d.magnitude / movement.speed;
            targetBobbingStrength = Mathf.Clamp01(targetBobbingStrength);
        } else {
            targetBobbingStrength = 0f;
        }

        // Interpolated so it doesn't come to an abrupt stop
        bobbingStrengthCurrent = Mathf.Lerp(bobbingStrengthCurrent, targetBobbingStrength, 25f * Time.deltaTime);

        // Vertical and horizontal bobbing values
        float effectiveBobbingStrength = bobbingStrength * bobbingStrengthCurrent;

        // https://www.desmos.com/calculator/bvzhohw3cu
        float verticalBobbing = (Utils.SmoothAbsClamped01(Mathf.Sin((0.5f * stepValue + Mathf.PI / 4f) * bobbingSpeed), 1f / bobbingSteppiness) * 2f - 1f) * effectiveBobbingStrength;
        float suace = Mathf.Sin(0.5f * stepValue * bobbingSpeed);
        float horizontalBobbing = Mathf.Pow(Mathf.Abs(suace), 1f / 1.5f) * Mathf.Sign(suace) * effectiveBobbingStrength;
        //float verticalBobbing = Mathf.Sin(stepValue * bobbingSpeed) * effectiveBobbingStrength;

        if (!isDead)
            head.transform.localPosition = new Vector3(horizontalBobbing, baseCameraHeight + verticalBobbing, 0);

        return verticalBobbing;
    }
    #endregion

    #region Temperature & Shivering
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
        bodyTemperature = Mathf.Lerp(bodyTemperature, targetTemperature, targetReachSpeed * Time.deltaTime);
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
        //gameCamera.transform.localPosition = localCamPos;
        //gameCamera.transform.localRotation = Quaternion.Lerp(Quaternion.identity, Random.rotation, shiverMeTimbers * Time.deltaTime * shiveringShakeRotationFactor);
    }
    #endregion

    #region Inventory
    // Add item to inventory if possible,
    // DO NOT INSERT INVALID STACK COUNT ITEM,
    // THIS WILL TAKE FROM THE ITEM YOU INSERT. YOU HAVE BEEN WARNED
    public void AddItem(Item itemIn) {
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
            //Debug.Log("oh yeah, slot was empty. It's sex time...");
            items[firstEmpty].CopyItem(itemIn.Clone()); // Don't know if we actually have to Clone this lol but wtv
            itemIn.MakeEmpty();
        }

        if (firstEmpty == selected) {
            SelectionChanged(force: true);
        }

        inventoryUpdateEvent.Invoke(items);
    }

    // Checks if we can fit a specific item
    // Count must be within stack count (valid item moment)
    public bool CanFitItem(Item itemIn) {
        int emptyCounts = 0;

        foreach (Item item in items) {
            if (item.IsEmpty()) {
                return true;
            } else if (itemIn.Data == item.Data && !item.IsFull() ) {
                emptyCounts += itemIn.Data.stackSize - item.Count;
            }
        }

        return emptyCounts >= itemIn.Count;
    }

    // Remove a specific count from a specific slot. Returns the number of items that successfully got removed
    public int RemoveItem(int slot, int count) {
        if (items[slot].Data != null && items[slot].Count > 0) {
            int ogCount = items[slot].Count;
            int currentCount = items[slot].Count;
            currentCount = Mathf.Max(currentCount - count, 0);
            //int missingCount = Mathf.Max(count - currentCount, 0);
            
            if (currentCount == 0) {
                items[slot].Data = null;
                SelectionChanged(force: true);
            }

            items[slot].Count = currentCount;

            return ogCount - currentCount;
        }

        return -1;
    }

    // Returns slot number (0-9) if we have item of specified count (default 1), otherwise returns -1
    public int CheckForItem(string id, int count = 1) {
        int i = 0;
        foreach (Item item in items) {
            if (item.Count >= count && item.Data == ItemUtils.itemDatas[id]) {
                return i;
            }
            i++;
        }

        return -1;
    }

    // Only called when the following happens:
    // - User changes selected slot to new slot
    // - Item count gets changed from zero to positive value and vice versa
    private void SelectionChanged(bool force = false) {
        if (UnityEngine.Object.ReferenceEquals(lastSelectedViewModelItem, SelectedItem) && !force) {
            return;
        }

        if (lastSelectedViewModelItem != null && itemLogic != null) {
            itemLogic.Unequipped();
        }

        Vector3 oldLocalPosition = default;
        Quaternion oldLocalRotation = default;
        if (viewModel != null) {
            oldLocalPosition = viewModel.transform.localPosition;
            oldLocalRotation = viewModel.transform.localRotation;
            Destroy(viewModel);
        }

        if (itemLogic != null) {
            Destroy(itemLogic.gameObject);
        }

        viewModel = null;
        lastSelectedViewModelItem = null;

        Item item = SelectedItem;
        if (item != null && item.Data != null && item.Count > 0) {
            if (item.Data.viewModel != null) {
                GameObject viewModel = Instantiate(item.Data.viewModel, viewModelHolster.transform);
                viewModel.transform.localPosition = oldLocalPosition;
                viewModel.transform.localRotation = oldLocalRotation;
                this.viewModel = viewModel;
                lastSelectedViewModelItem = item;
            }

            if (item.Data.equippedLogic != null) {
                GameObject equippedLogicGameObject = Instantiate(item.Data.equippedLogic, transform);
                itemLogic = equippedLogicGameObject.GetComponent<EquippedItemLogic>();
                itemLogic.equippedItem = item;
                itemLogic.player = this;
                itemLogic.Equipped();
                itemLogic.viewModel = item.Data.viewModel;
            }
        }
    }
    #endregion

    #region Input System Callbacks
    public void Movement(InputAction.CallbackContext context) {
        if (!isDead) {
            localWishMovement = context.ReadValue<Vector2>();
            movement.localWishMovement = localWishMovement;
            FOVTween();
        }
    }

    public void Sprint(InputAction.CallbackContext context) {
        if (!isCrouching && Performed()) {
            isSprinting = context.ReadValue<float>() > 0.5f;
            FOVTween();
        }
    }

    public void Crouch(InputAction.CallbackContext context) {
        if (!isSprinting && Performed()) {
            isCrouching = context.ReadValue<float>() > 0.5f;
        }
    }

    public void ToggleInventory(InputAction.CallbackContext context) {
        if (context.performed && !isDead) {
            UIMaster.Instance.ToggleInventory();
        }
    }

    public void ToggleMarket(InputAction.CallbackContext context) {
        if (context.performed && !isDead) {
            UIMaster.Instance.ToggleMarket();
        }
    }

    public void ExitButton(InputAction.CallbackContext context) {
        if (context.performed) {
            UIMaster.Instance.EscPressed();
        }
    }

    public void Look(InputAction.CallbackContext context) {
        if (Cursor.lockState != CursorLockMode.None && Performed()) {
            ApplyMouseDelta(context.ReadValue<Vector2>());

            //lastMouseDelta = Vector2.Lerp(lastMouseDelta, mouseDelta, Time.deltaTime * 20.0f);
        }
    }

    private void ApplyMouseDelta(Vector2 delta) {
        targetMouseDelta = delta;
        wishHeadDir += targetMouseDelta * mouseSensitivity * 0.02f;
        wishHeadDir.y = Mathf.Clamp(wishHeadDir.y, -90f, 90f);
        head.localRotation = Quaternion.Euler(-wishHeadDir.y, 0f, 0f);
        movement.localWishRotation = Quaternion.Euler(0f, wishHeadDir.x, 0f).normalized;
    }

    public void Jump(InputAction.CallbackContext context) {
        if (Performed(context)) {
            movement.Jump();
        }
    }

    public void Scroll(InputAction.CallbackContext context) {
        if (Performed(context)) {
            float scroll = -context.ReadValue<float>();
            scroll /= 120;

            if (isBuilding) {
                placementRotation += scroll * 22.5f;
            } else {
                int newSelected = (Selected + (int)scroll) % 10;
                Selected = (newSelected < 0) ? 9 : newSelected;
                SelectionChanged();
            }
        }
    }

    public void SelectSlot(InputAction.CallbackContext context) {
        if (Performed(context))
            Selected = (int)context.ReadValue<float>();
        SelectionChanged();
    }

    public void PrimaryAction(InputAction.CallbackContext context) {
        if (!Performed() || context.performed)
            return;

        // !context.canceled is equal to true when pressing and false when releasing
        bool pressed = !context.canceled;

        if (isBuilding && placementStatus && pressed) {
            BuildActionPrimary();
        } else {
            // fallback primary action if we can
            ItemData data = SelectedItem.Data;
            if (data != null && itemLogic != null) {
                itemLogic.PrimaryAction(pressed);
                return;
            }
        }
    }

    public void InteractAction(InputAction.CallbackContext context) {
        if (!Performed(context) || !lookingAt.HasValue)
            return;

        if (interaction != null && interaction.Interactable && vehicle == null) {
            interaction.Interact(this);
        } else if (vehicle != null) {
            ExitVehicle();
        }
    }

    public void SecondaryAction(InputAction.CallbackContext context) {
        if (!Performed() || context.performed)
            return;

        bool pressed = !context.canceled;
        if (isBuilding && pressed) {
            BuildActionSecondary();
        } else {
            // fallback secondary action if we can
            ItemData data = SelectedItem.Data;
            if (data != null && itemLogic != null) {
                itemLogic.SecondaryAction(pressed);
                return;
            }
        }
    }

    public void TempActivateBuildingMode(InputAction.CallbackContext context) {
        if (Performed(context)) {
            isBuilding = !isBuilding;
            placementGhost.SetActive(false);
        }
    }

    public void DropItem(InputAction.CallbackContext context) {
        if (Performed(context)) {
            Item item = items[selected];
            if (item.Count > 0) {
                if (item.Data.worldItem == null) {
                    Debug.LogWarning("World item is not set!");
                    return;
                }

                WorldItem.Spawn(items[selected].Data, gameCamera.transform.position + gameCamera.transform.forward, Quaternion.identity);
                RemoveItem(selected, 1);
            }
        }
    }
    #endregion

    #region Building
    private void BuildActionPrimary() {
        GameObject builtPiece = Instantiate(whichThing ? selectedBuildPrefab : selectedTemp2Prefab);
        builtPiece.transform.SetPositionAndRotation(placementGhost.transform.position, placementGhost.transform.rotation);
        builtPiece.SetActive(true);
        builtPiece.layer = LayerMask.NameToLayer("Piece");
    }

    
    bool whichThing = true;
    [Obsolete("DONT USE BuildActionSecondary THIS WE HAVE A BUILD MENU NOW")]
    private void BuildActionSecondary() {
        whichThing = !whichThing;
        SetupPlacementGhost(whichThing ? selectedBuildPrefab : selectedTemp2Prefab);
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

    // time to do a review of this
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

            if (TestGhostClipping(placementGhost, 0.001f)) {
                placementStatus = false;
            }

        } else {
            placementGhost.SetActive(false);
        }
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
    #endregion

    #region Util
    // Checks if the given input can be executed (using context.performed)
    private bool Performed(InputAction.CallbackContext context) {
        return context.performed && UIMaster.Instance.MovementPossible() && !isDead && GameManager.Instance.initialized;
    }

    // Checks if we can do *any* movement
    private bool Performed() {
        return UIMaster.Instance.MovementPossible() && !isDead && GameManager.Instance.initialized;
    }

    public void ResetMovement(bool resetRotation = false) {
        movement.localWishMovement = Vector2.zero;
        targetMouseDelta = Vector2.zero;
        currentMouseDelta = Vector2.zero;
        ApplyMouseDelta(Vector2.zero);

        if (resetRotation) {
            movement.localWishRotation = Quaternion.identity;
            transform.rotation = Quaternion.identity;
            wishHeadDir = Vector2.zero;
        }

        stepValue = 0f;
    }

    public void ExitVehicle() {
        vehicle.Exit();
        vehicle = null;
        lastInteraction = null;
        transform.SetParent(null);
        ResetMovement();
    }
    #endregion
}
