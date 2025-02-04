using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBuilding : PlayerBehaviour {
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
    public float placementDistance = 10f;
    private bool altAction = false;
    public Piece currentOutline = null;
    public Color outlineColor = Color.black;
    [Range(0f, 10f)]
    public float outlineWidth = 2f;

    private void Awake() {
        placeRayMask = LayerMask.GetMask("Default", "Piece");
        //placeRayMask = ~LayerMask.GetMask("Player");
    }

    private void Start() {
        SetupPlacementTarget(selectedPiece.piecePrefab);
        placementTarget.SetActive(false);
    }

    private void LateUpdate() {
        if (placementTarget != null) {
            if (player.state == Player.State.Building)
                UpdatePlacementTarget();

            if (!noBuildingCost && !(player.inventory.CheckForItems(selectedPiece.requirement1) && player.inventory.CheckForItems(selectedPiece.requirement2) && player.inventory.CheckForItems(selectedPiece.requirement3))) {
                placementStatus = false;
            }

            MeshRenderer[] renderers = placementTarget.GetComponentsInChildren<MeshRenderer>();
            foreach (var item in renderers) {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetInt("_Valid", placementStatus ? 1 : 0);
                item.SetPropertyBlock(block);
            }
        }
    }

    public void PrimaryAction(InputAction.CallbackContext context) {
        if (Pressed(context)) {
            BuildActionPrimary();
        }
    }

    public void SecondaryAction(InputAction.CallbackContext context) {
        if (Pressed(context)) {
            UIScriptMaster.Instance.inGameHUD.ToggleBuilding();
        }
    }

    public void Scroll(float scroll) {
        placementRotation += scroll * 22.5f;
    }

    public void TertiaryAction(InputAction.CallbackContext context) {
        if (Pressed(context)) {
            DestroySelectedBuilding();
        }
    }

    public void TempActivateBuildingMode(InputAction.CallbackContext context) {
        if (Pressed(context) && (player.state == Player.State.Default || player.state == Player.State.Building)) {

            if (player.state == Player.State.Default) {
                player.state = Player.State.Building;
            } else if (player.state == Player.State.Building) {
                player.state = Player.State.Default;
            }

            placementTarget.SetActive(false);
            if (player.state != Player.State.Building) {
                ClearOutline();
            }
        }
    }

    public void DestroySelectedBuilding() {
        if (currentOutline != null) {
            Destroy(currentOutline.gameObject);
        }
    }

    // This is just the placement action
    private void BuildActionPrimary() {

        // HOW DOES THIS WORK!!!!!!!!
        GameObject builtPiece = Instantiate(selectedPiece.piecePrefab);
        builtPiece.transform.SetPositionAndRotation(placementTarget.transform.position, placementTarget.transform.rotation);
        builtPiece.SetActive(true);
        builtPiece.layer = LayerMask.NameToLayer("Piece");
        Utils.PlaySound(builtPiece.transform.position, Registries.snowBrickPlace);
        if (!noBuildingCost) {
            player.inventory.TakeItems(selectedPiece.requirement1);
            player.inventory.TakeItems(selectedPiece.requirement2);
            player.inventory.TakeItems(selectedPiece.requirement3);
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
        if (Physics.Raycast(player.camera.transform.position, player.camera.transform.forward, out var hitInfo, placementDistance, ~LayerMask.GetMask("Player"))) {
            Debug.DrawRay(player.camera.transform.position, player.camera.transform.forward * hitInfo.distance, Color.black);

            /* not sure what this does
            if ((bool)placementTarget) {
                Piece component = placementTarget.GetComponent<Piece>();
                if ((object)component != null) {
                    num += (float)component.m_extraPlacementDistance;
                }
            }
            */

            // if we hit something, return true and set the things
            if ((bool)hitInfo.collider && !hitInfo.collider.attachedRigidbody) {
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
}
