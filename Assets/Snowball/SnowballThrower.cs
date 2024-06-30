using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowballThrower : MonoBehaviour {
    public SnowballItemData itemData;
    public Transform spawnHolster;
    public float startingSpeed;
    public float offsetDistance = 2.0f;
    public new Collider collider;
    [HideInInspector]
    public EntityMovement inheritVelocityMovement;
    
    void Start() {
        collider = GetComponent<Collider>();
        inheritVelocityMovement = GetComponent<EntityMovement>();
    }

    public void Throw(float forcePercentage = 1.0f) {
        Vector3 startingVelocity = startingSpeed * forcePercentage * spawnHolster.forward * itemData.speedFactor;
        Vector3 startingPos = spawnHolster.position + spawnHolster.forward * offsetDistance;
        GameObject spawned = Instantiate(itemData.snowball);

        if (inheritVelocityMovement != null) {
            startingVelocity += inheritVelocityMovement.cc.velocity;
        }

        Snowball snowball = spawned.GetComponent<Snowball>();
        snowball.dataParent = itemData;
        snowball.ApplySpawn(startingPos, startingVelocity, this);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawRay(spawnHolster.position, spawnHolster.forward * 2.0f);
    }
}
