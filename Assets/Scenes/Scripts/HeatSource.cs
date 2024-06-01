using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatSource : MonoBehaviour {
    public float sourceTemperature = 10f;
    public float maxRangeRadius = 20f;
    public float minRangeRadius = 2f;

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minRangeRadius);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, maxRangeRadius);
    }
}
