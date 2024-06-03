using UnityEngine;

// Player temperature system using nearby heat sources and 
public class PlayerTemperature : MonoBehaviour {
    public const float TargetTemperature = 37.0f;
    private float outsideTemperature;
    private float heatSourcesTemperature; 
    private float bodyTemperature = TargetTemperature;
    public float targetReachSpeed = 0.5f;
    public float outsideReachSpeed = 0.5f;
    
    // Start is called before the first frame update
    void Start() {
        bodyTemperature = TargetTemperature;
    }

    // Update is called once per frame
    void Update() {
        // Calculate heat from sources
        HeatSource[] sources = GameObject.FindObjectsOfType<HeatSource>();
        heatSourcesTemperature = 0.0f;
        foreach (var source in sources) {
            float dist = Vector3.Distance(transform.position, source.transform.position);
            float invLerp = Mathf.InverseLerp(source.minRangeRadius, source.maxRangeRadius, dist);
            invLerp = Mathf.Pow(1 - Mathf.Clamp01(invLerp), 2);

            heatSourcesTemperature += source.sourceTemperature * invLerp;
        }

        // Get outside temp from weather system
        outsideTemperature = GameManager.Singleton.weatherManager.GetOutsideTemperature();

        // Actual value that we must try to reach
        float totalTemp = Mathf.Max(outsideTemperature, heatSourcesTemperature);

        // Body temperature calculations (DOES NOT CONSERVE ENERGY)
        bodyTemperature = Mathf.Lerp(bodyTemperature, totalTemp, outsideReachSpeed * Time.deltaTime);
        bodyTemperature = Mathf.Lerp(bodyTemperature, TargetTemperature, targetReachSpeed * Time.deltaTime);
    }

    private void OnGUI() {
        GUI.Label(new Rect(0, 0, 1000, 20), "Body Temperature: " + bodyTemperature.ToString("F3"));
        GUI.Label(new Rect(0, 20, 1000, 20), "Scene Temperature: " + outsideTemperature.ToString("F3"));
        GUI.Label(new Rect(0, 40, 1000, 20), "Sources Temperature: " + heatSourcesTemperature.ToString("F3"));
    }
}
