using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {
    EntityMovement movement;
    Vector2 wishHeadDir;
    public Transform head;
    public float mouseSensivity = 1.0f;
    
    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        movement = GetComponent<EntityMovement>();
    }

    public void Movement(InputAction.CallbackContext context) {
        movement.localWishMovement = context.ReadValue<Vector2>();
    }

    public void Look(InputAction.CallbackContext context) {
        wishHeadDir += context.ReadValue<Vector2>() * mouseSensivity * 0.02f;
        wishHeadDir.y = Mathf.Clamp(wishHeadDir.y, -90f, 90f);
        head.localRotation = Quaternion.Euler(-wishHeadDir.y, 0f, 0f);
        movement.localWishRotation = Quaternion.Euler(0f, wishHeadDir.x, 0f).normalized;
    }

    public void Jump(InputAction.CallbackContext context) {
        if (context.performed) {
            movement.isJumping = true;
        }
    }
}
