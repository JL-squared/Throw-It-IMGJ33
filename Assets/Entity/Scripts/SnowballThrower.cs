using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowballThrower : MonoBehaviour {
    public GameObject snowball;
    public Transform spawnHolster;
    public float startingSpeed;
    public float offsetDistance = 2.0f;

    public void Throw(float forcePercentage = 1.0f) {
        Vector3 startingVelocity = startingSpeed * forcePercentage * spawnHolster.forward;
        Vector3 startingPos = spawnHolster.position + spawnHolster.forward * offsetDistance;
        GameObject spawned = Instantiate(snowball);

        EntityMovement em = GetComponent<EntityMovement>();
        if (em) {
            startingVelocity += em.cc.velocity;
        }

        spawned.GetComponent<Snowball>().ApplySpawn(startingPos, startingVelocity);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawRay(spawnHolster.position, spawnHolster.forward * 2.0f);
    }
}
