using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Tweens;
using System;
using System.Linq;
using UnityEngine.XR;

// Full Player script holding all necessary functions and variables
public class Player : MonoBehaviour, IEntitySerializer {
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
    public bool isBuilding; // irrelevant for now
    public bool noBuildingCost;
    List<Transform> tempSnapPoints1 = new List<Transform>(); // idk what this does
    List<Transform> tempSnapPoints2 = new List<Transform>(); // idk what this does either
    List<Piece> tempPieces = new List<Piece>(); // i also don't know what this does
    public PieceDefinition selectedPiece;
    public Material hologramMaterial;
    int placeRayMask; // need to know how this works
    [SerializeField]
    private GameObject placementTarget;
    private bool placementStatus = false;
    private float placementRotation = 0f;
    public float placementDistance;
    private bool altAction = false;
    public Piece currentOutline = null;
    public Color outlineColor = Color.black;
    [Range(0f,10f)]
    public float outlineWidth = 2f;
    #endregion

    #region Inventory
    [Header("Inventory")]
    [HideInInspector]
    public List<ItemStack> items = new List<ItemStack>();

    [SerializeField]
    private int equipped;
    public int Equipped {
        get {
            return equipped;
        }
        set {
            equipped = value;
            selectedEvent?.Invoke(equipped);
        }
    }
    public ItemStack EquippedItem { get { return items[equipped]; } }

    public UnityEvent<int> selectedEvent;
    public UnityEvent<List<ItemStack>> inventoryUpdateEvent;
    public UnityEvent<int, bool> slotUpdateEvent;
    #endregion

    #region Movement
    [Header("Movement")]
    [HideInInspector]
    public EntityMovement movement;
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
    public float defaultFOV;
    #endregion

    #region View Model & View Model Sway
    [Header("View Model")]
    public GameObject viewModelHolster;
    private GameObject viewModel;
    private Vector3 rotationLocalOffset;
    private Vector3 positionLocalOffset;
    public float holsterSwaySmoothingSpeed = 25f;
    public float holsterSwayMouseAccelSmoothingSpeed = 20f;
    public float viewModelRotationClampMagnitude = 0.2f;
    public float viewModelRotationStrength = -0.001f;
    public float viewModelPositionStrength = 0.2f;
    public Vector2 MouseDelta { private set; get; }
    public Vector2 SmoothedMouseDelta { private set; get; }
    #endregion

    #region Audio
    [Header("Audio")]
    public AudioSource footsteps;
    public AudioSource music;
    #endregion

    #region Interaction
    [HideInInspector]
    public RaycastHit? lookingAt;
    private IInteraction interaction;
    private IInteraction lastInteraction;
    [HideInInspector]
    public bool PrimaryHeld = false;
    [HideInInspector]
    public bool SecondaryHeld = false;
    #endregion

    [HideInInspector]
    public EntityHealth health;
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
        settings = Utils.Load<PlayerControlsSettings>("player.json");
        mouseSensitivity = settings.mouseSensivity;
        defaultFOV = settings.fov;
        bodyTemperature = targetTemperature;
        Cursor.lockState = CursorLockMode.Locked;
        movement = GetComponent<EntityMovement>();

        // Setup building stuff
        SetupPlacementTarget(selectedPiece.piecePrefab);
        placementTarget.SetActive(false);

        // Hook onto health component
        health = GetComponent<EntityHealth>();
        health.OnHealthChanged += (float p) => {
            UIMaster.Instance.healthBar.HealthChanged(p);
        };
        health.OnKilled += Killed;


        // Creates inventory items and hooks onto their update event
        for (int i = 0; i < 10; i++) {
            ItemStack temp = new ItemStack(null, 0);
            items.Add(temp);
            temp.updateEvent?.AddListener((ItemStack item) => { inventoryUpdateEvent.Invoke(items); });
        }

        // Add temp items at start
        AddItem(new ItemStack("snowball", 1));
        AddItem(new ItemStack("battery", 1));
        AddItem(new ItemStack("shovel", 1));
        AddItem(new ItemStack("wires", 1));

        gameCamera.fieldOfView = defaultFOV;
    }

    private void Killed() {
        Debug.Log("Skill issue, you dead");
        isDead = true;

        // Literal hell
        //ambatakamChoir.Play();

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
        
        if (!ReferenceEquals(lastInteraction, interaction) || (lastInteraction.IsNullOrDestroyed() ^ interaction.IsNullOrDestroyed())) {
            if (!interaction.IsNullOrDestroyed()) {
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
            movement.ToggleCollision(false);
        } else {
            movement.entityMovementFlags.AddFlag(EntityMovementFlags.ApplyMovement);
            movement.ToggleCollision(true);
        }

        EquippedItem.logic.EquippedUpdate(this);
        foreach (ItemStack item in items) {
            item.logic.Update(this);
        }

        if (viewModel != null && !EquippedItem.IsNullOrDestroyed() && !EquippedItem.IsEmpty()) {
            viewModel.transform.localPosition = EquippedItem.Data.viewModelPositionOffset;
            viewModel.transform.localRotation = EquippedItem.Data.viewModelRotationOffset;
            viewModel.transform.localScale = EquippedItem.Data.viewModelScaleOffset;
        }

        if(!music.isPlaying && !GameManager.Instance.paused) {
            PlayMusic();
        }

        test = false;
    }

    public void UpdateMovement() {
        if (isSprinting) {
            movement.speedModifier = sprintModifier;
        } else {
            movement.speedModifier = 1f;
        }
    }

    private void LateUpdate() {
        if (isBuilding) UpdatePlacementTarget();

        if (!noBuildingCost && !(CheckForItems(selectedPiece.requirement1) && CheckForItems(selectedPiece.requirement2) && CheckForItems(selectedPiece.requirement3))) {
            placementStatus = false;
        }

        MeshRenderer[] renderers = placementTarget.GetComponentsInChildren<MeshRenderer>();
        foreach (var item in renderers) {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetInt("_Valid", placementStatus ? 1 : 0);
            item.SetPropertyBlock(block);
        }
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
        rotationLocalOffset = Vector3.ClampMagnitude((new Vector3(SmoothedMouseDelta.x, SmoothedMouseDelta.y, 0) / (Time.deltaTime + 0.001f)) * 0.01f, viewModelRotationClampMagnitude) * viewModelRotationStrength;
        positionLocalOffset = transform.InverseTransformDirection(-movement.Velocity) * viewModelPositionStrength;
        Vector3 current = viewModelHolster.transform.localPosition;
        Vector3 target = rotationLocalOffset + positionLocalOffset;
        target += Vector3.up * bobbing * viewModelBobbingStrength;

        //if (itemLogic != null) {
        //    target += itemLogic.swayOffset;
        //}

        Vector3 localPosition = Vector3.Lerp(current, target, Time.deltaTime * holsterSwaySmoothingSpeed);
        viewModelHolster.transform.localPosition = Vector3.ClampMagnitude(localPosition, 1f);
        
        SmoothedMouseDelta = Vector2.Lerp(SmoothedMouseDelta, MouseDelta, Time.deltaTime * holsterSwayMouseAccelSmoothingSpeed);
    }

    bool stepped;
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

        if (verticalBobbing < 0f && !stepped) {
            if(isSprinting) {
                PlaySound(footsteps, Registries.rockRun);
            } else {
                PlaySound(footsteps, Registries.rockWalk);
            }
            stepped = true;
        } else if (verticalBobbing > 0f) {
            stepped = false;
        }

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
    // Works with invalid stack sizes
    // THIS WILL TAKE FROM THE ITEM YOU INSERT. YOU HAVE BEEN WARNED
    public void AddItemUnclamped(ItemStack itemIn) {
        int count = itemIn.Count;
        while (count > 0) {
            int tempCount = Mathf.Min(itemIn.Data.stackSize, count);
            AddItem(new ItemStack(itemIn.Data, tempCount));
            count -= itemIn.Data.stackSize;
        }
    }


    // Add item to inventory if possible,
    // DO NOT INSERT INVALID STACK COUNT ITEM, (clamped anyways)
    // THIS WILL TAKE FROM THE ITEM YOU INSERT. YOU HAVE BEEN WARNED
    public void AddItem(ItemStack itemIn) {
        if (itemIn.Count > itemIn.Data.stackSize) {
            Debug.LogWarning("Given item count was greater than stack size. Clamping. Use AddItemUnclamped for unclamped stack sizes");
            itemIn.Count = itemIn.Data.stackSize;
        }

        int firstEmpty = -1;
        int i = 0;
        foreach (ItemStack item in items) {
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
        } else {
            Debug.LogWarning("Could not find spot to add item, spawning as World Item!");
            for (int k = 0; k < itemIn.Count; k++) {
                WorldItem.Spawn(itemIn, transform.position, transform.rotation);
            }
        }

        if (firstEmpty == equipped) {
            SelectionChanged();
        }

        inventoryUpdateEvent.Invoke(items);
    }

    // Checks if we can fit a specific item
    // Count must be within stack count (valid item moment)
    public bool CanFitItem(ItemStack itemIn) {
        int emptyCounts = 0;

        foreach (ItemStack item in items) {
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
                SelectionChanged(() => { items[slot].Data = null; });
            }

            items[slot].Count = currentCount;

            return ogCount - currentCount;
        }

        return -1;
    }

    // Returns slot number (0-9) if we have item of specified count (default 1), otherwise returns -1
    public int CheckForUnfullMatchingStack(string id, int count = 1) {
        int i = 0;
        foreach (ItemStack item in items) {
            if (item.Count >= count && item.Data == Registries.items.data[id]) {
                return i;
            }
            i++;
        }

        return -1;
    }

    public bool CheckForItems(ItemStack stack) {
        if (stack.IsNullOrDestroyed() || stack.IsEmpty()) return true;
        return CheckForItemAmount(stack.data.name, stack.count);
    }

    public bool CheckForItemAmount(string id, int count) {
        foreach (ItemStack item in items) {
            if (count <= 0)
                return true;
           
            if (item.Data == Registries.items.data[id]) {
                count -= item.Count;
            }
        }
        return false;
    }

    // Both of these functions are super unsafe, fix later
    public void TakeItems(ItemStack stack) {
        if (!stack.IsNullOrDestroyed() && !stack.IsEmpty()) {
            TakeItemAmount(stack.data.name, stack.count);
        }
    }

    public void TakeItemAmount(string id, int count = 0) {
        foreach (ItemStack item in items) {
            if (count <= 0)
                return;

            if (item.Data == Registries.items.data[id]) {
                count -= item.Count;
                item.Count = count <= 0 ? -count : 0; // if count goes below 0, give it back to the item. othwerwise stack should become 0
            }
        }
    }

    // Only called when the following happens:
    // - User changes selected slot to new slot
    // - Item count gets changed from zero to positive value and vice versa
    private void SelectionChanged(Action function = null) {
        EquippedItem.logic.Unequipped(this);
        function?.Invoke();
        EquippedItem.logic.Equipped(this);

        if (viewModel != null) {
            Destroy(viewModel);
        }

        if (EquippedItem != null && !EquippedItem.IsEmpty()) {
            viewModel = Instantiate(EquippedItem.Data.prefab, viewModelHolster.transform);
            viewModel.transform.localPosition = EquippedItem.Data.viewModelPositionOffset;
            viewModel.transform.localRotation = EquippedItem.Data.viewModelRotationOffset;
            viewModel.transform.localScale = EquippedItem.Data.viewModelScaleOffset;
        }
    }
    #endregion

    #region Input System Callbacks
    public void Movement(InputAction.CallbackContext context) {
        if (Performed()) {
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

    public void AltAction(InputAction.CallbackContext context) {
        if(context.performed) {
            altAction = true;
        } else if (context.canceled) {
            altAction = false;
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
        }
    }

    public void ToggleDevConsole(InputAction.CallbackContext context) {
        if (Performed(context) && !GameManager.Instance.devConsole.consoleNation) {
            UIMaster.Instance.ToggleDevConsole();
        }
    }

    bool test;
    public void ApplyMouseDelta(Vector2 delta) {
        MouseDelta = delta;
        wishHeadDir += MouseDelta * mouseSensitivity * 0.02f;
        wishHeadDir.y = Mathf.Clamp(wishHeadDir.y, -90f, 90f);
        head.localRotation = Quaternion.Euler(-wishHeadDir.y, 0f, 0f);
        movement.localWishRotation = Quaternion.Euler(0f, wishHeadDir.x, 0f).normalized;

        //GetComponent<Rigidbody>().MoveRotation(movement.localWishRotation);
        if (!test) {
            test = true;
        }
    }

    public void Jump(InputAction.CallbackContext context) {
        if (Performed(context)) {
            if (movement.IsGrounded) PlaySound(footsteps, Registries.rockJump);
            movement.Jump();
        }
    }

    public void Scroll(InputAction.CallbackContext context) {
        if (Performed(context)) {
            float scroll = -context.ReadValue<float>();

            if (isBuilding) {
                placementRotation += scroll * 22.5f;
            } else {
                SelectionChanged( () => {
                    int newSelected = (Equipped + (int)scroll) % 10;
                    Equipped = (newSelected < 0) ? 9 : newSelected;
                });
            }
        }
    }

    public void SelectSlot(InputAction.CallbackContext context) {
        if (Performed(context))
            SelectionChanged(() => { Equipped = (int)context.ReadValue<float>(); });
    }

    public void PrimaryAction(InputAction.CallbackContext context) {
        if (!Performed() || context.performed)
            return;

        PrimaryHeld = !context.canceled && !isBuilding;

        if (isBuilding && placementStatus && !context.canceled) {
            BuildActionPrimary();
        } else if (!EquippedItem.IsEmpty() && !isBuilding) {
            EquippedItem.logic.PrimaryAction(context, this);
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
        if (context.performed)
            return;

        bool pressed = !context.canceled;
        if (isBuilding && pressed) {
            UIMaster.Instance.ToggleBuilding();
        } else if (!EquippedItem.IsEmpty()) {
            EquippedItem.logic.SecondaryAction(context, this);
        }
    }

    public void TertiaryAction(InputAction.CallbackContext context) {
        if (context.performed)
            return;

        bool pressed = !context.canceled;
        if (isBuilding && pressed) {
            DestroySelectedBuilding();
        }
    }

    public void DestroySelectedBuilding() {
        if (currentOutline != null) {
            Destroy(currentOutline.gameObject);
        }
    }

    public void TempActivateBuildingMode(InputAction.CallbackContext context) {
        if (Performed(context)) {
            isBuilding = !isBuilding;
            placementTarget.SetActive(false);
            if (!isBuilding) {
                ClearOutline();
                UIMaster.Instance.Clear();
            }
        }
    }

    public void DropItem(InputAction.CallbackContext context) {
        if (Performed(context)) {
            ItemStack item = items[equipped];
            if (item.Count > 0) {
                if (WorldItem.Spawn(items[equipped].NewCount(1), gameCamera.transform.position + gameCamera.transform.forward, Quaternion.identity, movement.Velocity))
                    RemoveItem(equipped, 1);
            }
        }
    }
    #endregion

    #region Building

    // This is just the placement action
    private void BuildActionPrimary() {

        // HOW DOES THIS WORK!!!!!!!!
        GameObject builtPiece = Instantiate(selectedPiece.piecePrefab);
        builtPiece.transform.SetPositionAndRotation(placementTarget.transform.position, placementTarget.transform.rotation);
        builtPiece.SetActive(true);
        builtPiece.layer = LayerMask.NameToLayer("Piece");
        PlaySound(builtPiece.transform.position, Registries.snowBrickPlace);
        if (!noBuildingCost) {
            TakeItems(selectedPiece.requirement1);
            TakeItems(selectedPiece.requirement2);
            TakeItems(selectedPiece.requirement3);
        }
    }

    // Creates the placement hologram by instantiating the prefab and then modifying it and its children (somehow)
    public void SetupPlacementTarget(GameObject prefab) {
        // This is probably where you do the thing JED

        if ((bool)placementTarget) {
            Destroy(placementTarget);
            placementTarget = null;
        }

        placementTarget = Instantiate(prefab);

        MeshRenderer[] renderers = placementTarget.GetComponentsInChildren<MeshRenderer>();
        foreach (var item in renderers) {
            int length = item.materials.Length;
            Material[] materials = new Material[length];
            Array.Fill(materials, hologramMaterial);
            item.SetMaterials(materials.ToList());
        }

        placementTarget.name = prefab.name;

        Collider[] componentsInChildren1 = placementTarget.GetComponentsInChildren<Collider>();
        foreach (Collider collider in componentsInChildren1) {
            if (((1 << collider.gameObject.layer) & placeRayMask) == 0) {
                Debug.Log("Disabling " + collider.gameObject.name + "  " + LayerMask.LayerToName(collider.gameObject.layer));
                collider.enabled = false;
            }
        }

        Transform[] componentsInChildren2 = placementTarget.GetComponentsInChildren<Transform>();
        int layer = LayerMask.NameToLayer("Ghost");
        Transform[] array = componentsInChildren2;
        for (int i = 0; i < array.Length; i++) {
            array[i].gameObject.layer = layer;
        }

        placementTarget.transform.position = transform.position;
    }

    /// <summary>
    /// Function for determining a valid placement destination forward from the camera.
    /// </summary>
    /// <param name="point"></param>
    /// <param name="normal"></param>
    /// <param name="piece"></param>
    /// <returns>
    /// Returns true if something is hit, false if not.
    /// Also pushes out the point, normal, and optional piece information of what was hit.
    /// </returns>
    private bool PieceRayTest(out Vector3 point, out Vector3 normal, out Piece piece) {
        int layerMask = placeRayMask;

        // Send a raycast
        if (Physics.Raycast(gameCamera.transform.position, gameCamera.transform.forward, out var hitInfo, 50f, layerMask)) {
            float num = placementDistance;

            /* not sure what this does
            if ((bool)placementTarget) {
                Piece component = placementTarget.GetComponent<Piece>();
                if ((object)component != null) {
                    num += (float)component.m_extraPlacementDistance;
                }
            }
            */

            // if we hit something, return true and set the things
            if ((bool)hitInfo.collider && !hitInfo.collider.attachedRigidbody && Vector3.Distance(head.position, hitInfo.point) < num) {
                point = hitInfo.point;
                normal = hitInfo.normal;
                piece = hitInfo.collider.GetComponentInParent<Piece>(); // this can either return a piece or just not do anything
                return true;
            }
        }

        // If raycast fails, return nothing
        point = Vector3.zero;
        normal = Vector3.zero;
        piece = null;
        return false;
    }

    /// <summary>
    /// Update function for building placement targeting, mostly involves the target object
    /// </summary>
    private void UpdatePlacementTarget() {
        bool manualPlacement = altAction; // this currently cannot be changed

        if (PieceRayTest(out var point, out var normal, out Piece piece)) { // check for a place first
            OutlineObject(piece);

            placementTarget.SetActive(true); // yess we found one get the hologram working
            placementStatus = true; // cant remember what this was used for
            Collider[] componentsInChildren = placementTarget.GetComponentsInChildren<Collider>();

            Quaternion quatPlacementRotation = Quaternion.Euler(new Vector3(0f, placementRotation, 0f));

            // Put it in the right place by offsetting it so it isn't inside of the point we found
            if (componentsInChildren.Length != 0) {
                placementTarget.transform.position = point + normal * 50f;
                placementTarget.transform.rotation = quatPlacementRotation;
                Vector3 offset = Vector3.zero;
                float maxPointsDistance = 999999f;
                Collider[] array = componentsInChildren;

                foreach (Collider collider in array) {
                    collider.enabled = true;
                    if (collider.isTrigger || !collider.enabled) {
                        // something should be here
                        continue;
                    }

                    MeshCollider meshCollider = collider as MeshCollider;
                    if (!(meshCollider != null) || meshCollider.convex) {
                        Vector3 closestPoint = collider.ClosestPoint(point);
                        float pointsDistance = Vector3.Distance(closestPoint, point);
                        if (pointsDistance < maxPointsDistance) {
                            offset = closestPoint;
                            maxPointsDistance = pointsDistance;
                        }
                        collider.enabled = false;
                    }
                }
                Vector3 positionOffset = placementTarget.transform.position - offset;
                placementTarget.transform.position = point + positionOffset;
            }

            // Snapping
            if (!manualPlacement) {
                tempPieces.Clear();
                if (FindClosestSnapPoints(placementTarget.transform, 0.5f, out var a, out var b, tempPieces)) {
                    _ = b.parent.position;
                    Vector3 vector4 = b.position - (a.position - placementTarget.transform.position);
                    placementTarget.transform.position = vector4;
                    if (!IsOverlappingOtherPiece(vector4, placementTarget.transform.rotation, placementTarget.name, tempPieces, true)) {
                        placementTarget.transform.position = vector4;
                    }
                }
            }

            if (TestGhostClipping(placementTarget, 0.2f)) {
                placementStatus = false;
            }

        } else {
            OutlineObject(null);
            placementTarget.SetActive(false);
        }
    }

    private void OutlineObject(Piece piece) {
        if (piece != currentOutline) {
            ClearOutline();
            currentOutline = piece;
            if (piece != null) {
                var outline = piece.gameObject.AddComponent<Outline>();
                outline.OutlineColor = outlineColor;
                outline.OutlineWidth = outlineWidth;
            }
        } else if (piece == null) {
            ClearOutline();
        }
    }

    private void ClearOutline() {
        if (currentOutline != null) {
            Destroy(currentOutline.gameObject.GetComponent<Outline>());
            currentOutline = null;
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

    private bool FindClosestSnappoint(Vector3 p, List<Transform> snapPoints, float maxDistance, out Transform closest, out float distance) { // i think this gets the closest snap point
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

    /// <summary>
    /// Checks the amount the ghost penetrates another piece
    /// </summary>
    /// <param name="ghost"></param>
    /// <param name="maxPenetration"></param>
    /// <returns></returns>
    private bool TestGhostClipping(GameObject ghost, float maxPenetration) {
        Collider[] componentsInChildren = ghost.GetComponentsInChildren<Collider>();
        Collider[] objectsClipping = Physics.OverlapSphere(ghost.transform.position, 10f, placeRayMask);
        Collider[] _componentsInChildren = componentsInChildren;
        foreach (Collider collider in _componentsInChildren) {
            collider.enabled = true;
            Collider[] array3 = objectsClipping;
            foreach (Collider collider2 in array3) {
                if (Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation, collider2, collider2.transform.position, collider2.transform.rotation, out var _, out var distance) && distance > maxPenetration) {
                    collider.enabled = false;
                    return true;
                }
            }
            collider.enabled = false;
        }
        return false;
    }

    // this function only kinda works
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
        SmoothedMouseDelta = Vector2.zero;
        MouseDelta = Vector2.zero;

        if (resetRotation) {
            movement.localWishRotation = Quaternion.identity;
            transform.localRotation = Quaternion.identity;
            head.localRotation = Quaternion.identity;
            wishHeadDir = Vector2.zero;
        }

        ApplyMouseDelta(Vector2.zero);

        stepValue = 0f;
    }

    public void ExitVehicle() {
        Quaternion cpy2 = transform.rotation;
        Vector2 cpy = wishHeadDir;
        vehicle.Exit();
        vehicle = null;
        lastInteraction = null;
        transform.SetParent(null);
        
        ResetMovement();
        Vector3 angles = cpy2.eulerAngles;
        wishHeadDir.x = angles.y;
        ApplyMouseDelta(Vector2.zero);
    }

    public void Serialize(EntityData data) {
        data.inventory = items;
        data.wishHeadDir = wishHeadDir;
    }

    public void Deserialize(EntityData data) {
        interaction = null;
        lastInteraction = null;
        stepValue = 0f;
        items = data.inventory;
        wishHeadDir = data.wishHeadDir.Value;
        ApplyMouseDelta(Vector2.zero);
        SelectionChanged();
        inventoryUpdateEvent?.Invoke(items);

        foreach (var item in items) {
            item.updateEvent?.AddListener((ItemStack item) => { inventoryUpdateEvent.Invoke(items); });
        }
    }

    
    public void PlaySound(AudioSource source, AddressablesRegistry<AudioClip> registry) {
        source.clip = registry.data.Random().Item2;
        source.pitch = UnityEngine.Random.Range(0.7f, 1.3f);
        source.Play();
    }

    public void PlaySound(Vector3 point, AddressablesRegistry<AudioClip> registry) {
        GameObject obj = new GameObject();
        obj.transform.position = point;
        AudioSource source = obj.AddComponent<AudioSource>();
        source.clip = registry.data.Random().Item2;
        source.pitch = UnityEngine.Random.Range(0.7f, 1.3f);
        source.Play();
        Destroy(obj, source.clip.length / source.pitch);
    }


    int lastPlayedMusicIndex = -1;
    public void PlayMusic() {
        var temp = Registries.music.data.Random(lastPlayedMusicIndex);
        music.clip = temp.Item2;
        lastPlayedMusicIndex = temp.Item1;
        music.Play();
    }

    public void Trace(int i = 0) {
        Debug.Log("is this calling " + i);
    }
    #endregion
}