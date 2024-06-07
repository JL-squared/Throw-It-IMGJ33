using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowballThrower : MonoBehaviour {
    public GameObject snowball;
    public Transform spawnHolster;
    public GameObject particles;
    public float startingSpeed;
    public float offsetDistance = 2.0f;
    public new Collider collider;
    
    void Start() {
        collider = GetComponent<Collider>();
    }

    public void Throw(float forcePercentage = 1.0f) {
        Vector3 startingVelocity = startingSpeed * forcePercentage * spawnHolster.forward;
        Vector3 startingPos = spawnHolster.position + spawnHolster.forward * offsetDistance;
        GameObject spawned = Instantiate(snowball);

        EntityMovement em = GetComponent<EntityMovement>();
        if (em) {
            startingVelocity += em.cc.velocity;
        }

        Snowball snowballs = spawned.GetComponent<Snowball>();
        snowballs.ApplySpawn(startingPos, startingVelocity, this);
        snowballs.particles = particles;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawRay(spawnHolster.position, spawnHolster.forward * 2.0f);
    }
}
