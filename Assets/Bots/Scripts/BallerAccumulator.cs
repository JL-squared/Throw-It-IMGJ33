using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallerAccumulator : MonoBehaviour {
    public Transform ball;
    public float increaseFactor;
    public float startingRadius;
    private float volume;
    private float radius;
    private CharacterController cc;
    private EntityMovement movement;
    private Quaternion rotation = Quaternion.identity;
    private Quaternion angularVelocity = Quaternion.identity;

    public void Start() {
        volume = (4f / 3f) * Mathf.PI * Mathf.Pow(startingRadius, 3);
        cc = GetComponent<CharacterController>();
        movement = GetComponent<EntityMovement>();
        UpdateBallParams();
    }

    void UpdateBallParams() {
        cc.Move(Vector3.up * radius * Time.deltaTime);
        radius = Mathf.Sqrt(volume / 4 * Mathf.PI);
        cc.center = Vector3.up * (-radius);
        cc.height = 1f + 2 * radius;
        ball.localScale = Vector3.one * radius * 2f;
        ball.localPosition = -Vector3.up * (radius + 0.5f);

        Vector2 mov2d = new Vector2(movement.wishMovement.x, movement.wishMovement.z);
        float lateralSpeed = mov2d.magnitude;
        float rotationSpeed = Mathf.Rad2Deg * lateralSpeed / radius;
        Vector3 dir = transform.TransformDirection(Vector3.right);
        angularVelocity = Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, dir);
    }

    public void Update() {

        if (cc.isGrounded) {
            Vector2 mov2d = new Vector2(movement.wishMovement.x, movement.wishMovement.z);
            volume += increaseFactor * Time.deltaTime * mov2d.magnitude;
            
            if (mov2d.magnitude > 0.01f) {
                UpdateBallParams();
            }
        } else {
            angularVelocity = Quaternion.identity;
        }

        rotation = angularVelocity.normalized * rotation;
        
        // I tried setting global rotation to default but that didn't work idk why
        ball.localRotation = Quaternion.Inverse(transform.rotation);
        ball.rotation = rotation;
    }
}
