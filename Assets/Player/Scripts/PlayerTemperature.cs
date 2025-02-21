using UnityEngine;

public class PlayerTemperature : PlayerBehaviour {
    public float shiveringShakeRotationFactor = 0.01f;
    public float bodyTemp;
    public float minBodyTemp;
    public float maxBodyTemp = 37.0f;
    public float maxSourcesTemp = 50f;
    public float maxAllowedSecondsHypothermia = 10f;
    public float currentHypothermiaSeconds;
    public float reachSpeedIncrease = 4.0f;
    public float reachSpeedDecrease = 0.25f;
    public float shivering;
    public float sourcesTemp;

    private void Start() {
        //bodyTemp = maxBodyTemp;
    }

    private void FixedUpdate() {
        // Calculate heat from sources
        HeatSource[] sources = FindObjectsByType<HeatSource>(FindObjectsSortMode.None);
        float outsideTemp = GameManager.Instance.weatherManager.GetOutsideTemperature() - 20.0f;
        sourcesTemp = outsideTemp;
        foreach (var source in sources) {
            float dist = Vector3.Distance(transform.position, source.transform.position);
            float invLerp = Mathf.InverseLerp(source.minRangeRadius, source.maxRangeRadius, dist);
            invLerp = Mathf.Pow(1 - Mathf.Clamp01(invLerp), 2);
            sourcesTemp = Mathf.Max(sourcesTemp, source.sourceTemperature * invLerp + (1-invLerp) * outsideTemp);
        }

        float targetTemp = sourcesTemp;
        targetTemp = Mathf.Min(targetTemp, maxSourcesTemp);

        float reachSpeed;
        if (targetTemp > bodyTemp) {
            reachSpeed = reachSpeedIncrease;
        } else {
            reachSpeed = reachSpeedDecrease;
        }
        
        bodyTemp = Mathf.Lerp(bodyTemp, targetTemp, reachSpeed * Time.fixedDeltaTime);
        bodyTemp = Mathf.Min(bodyTemp, maxBodyTemp);

        if (bodyTemp < minBodyTemp) {
            currentHypothermiaSeconds += Time.fixedDeltaTime;
        } else {
            currentHypothermiaSeconds = 0f;
        }

        if (currentHypothermiaSeconds > maxAllowedSecondsHypothermia) {
            shivering += Time.fixedDeltaTime;
        } else {
            shivering -= Time.fixedDeltaTime;
        }

        shivering = Mathf.Clamp01(shivering);
        cameraShake.shivering = shivering;

        UIScriptMaster.Instance.temperatureDisplay.SetDisplay(bodyTemp);
    }
}
