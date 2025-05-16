using UnityEngine;

public class HeatSource : MonoBehaviour {
    public float sourceTemperature = 10f;
    public float minRangeRadius = 2f;

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minRangeRadius);
    }

    public void Start() {
        Player.Instance.temperature.sources.Add(this);
    }

    public void OnDestroy() {
        Player.Instance.temperature.sources.Remove(this);
    }
}
