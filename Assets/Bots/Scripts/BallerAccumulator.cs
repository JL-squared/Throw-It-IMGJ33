using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallerAccumulator : MonoBehaviour {
    public Transform ball;
    public float increaseFactor;
    public float rotationSpeed;
    private float volume;
    private float radius;
    private CharacterController cc;
    private EntityMovement movement;

    public void Start() {
        cc = GetComponent<CharacterController>();
        movement = GetComponent<EntityMovement>();
    }

    public void Update() {
        if (cc.isGrounded) {
            Vector2 mov2d = new Vector2(movement.wishMovement.x, movement.wishMovement.z);
            volume += increaseFactor * Time.deltaTime * mov2d.magnitude;
            
            if (mov2d.magnitude > 0.01f) {
                cc.Move(Vector3.up * radius * Time.deltaTime);
                radius = Mathf.Sqrt(volume / 4 * Mathf.PI);
                ball.transform.localScale = Vector3.one * radius * 2f;
                ball.transform.localPosition = -Vector3.up * (radius + 0.5f);
                cc.center = Vector3.up * (-radius);
                cc.height = 1f + 2 * radius;
                ball.Rotate(Vector3.right, rotationSpeed * Time.deltaTime / radius, Space.Self);
            }
        }
    }
}
