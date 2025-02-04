using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using static EntityHealth;

public class PlayerHealth : PlayerBehaviour {
    [HideInInspector]
    public EntityHealth health;

    private void Start() {
        // Hook onto health component
        health = GetComponent<EntityHealth>();
        health.OnHealthChanged += (float p) => {
            UIScriptMaster.Instance.healthBar.HealthChanged(p);
        };
        health.OnKilled += Killed;
    }

    private void Killed() {
        Debug.Log("Skill issue, you dead");
        player.state = Player.State.Dead;

        // Literal hell
        //ambatakamChoir.Play();

        // Make the camera a rigidbody
        /*
        Rigidbody rb = head.gameObject.AddComponent<Rigidbody>();
        rb.AddForce(UnityEngine.Random.insideUnitCircle, ForceMode.Impulse);
        head.gameObject.AddComponent<SphereCollider>();
        head.transform.parent = null;
        GetComponent<CharacterController>().height = 0;
        Destroy(GetComponentInChildren<MeshRenderer>());
        */

        // Lets others know that the player died
        GameManager.Instance.PlayerKilled();
    }
}