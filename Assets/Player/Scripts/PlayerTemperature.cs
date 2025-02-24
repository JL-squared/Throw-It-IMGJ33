using NUnit.Framework.Constraints;
using Tweens;
using UnityEngine;

public class PlayerTemperature : PlayerBehaviour {
    public float bodyTemp;
    public float minBodyTemp;
    public float maxBodyTemp = 37.0f;
    public float maxSourcesTemp = 50f;
    public float reachSpeedIncrease = 4.0f;
    public float reachSpeedDecrease = 0.25f;
    public float sourcesTemp;
    public float stefan_boltzman_constant = 5.67e-8f;



    private void Start() {
        bodyTemp = maxBodyTemp;
    }

    // NOT PHYSICALLY ACCURATE BUT BETTER THAN THE OLD ONE!!!
    // TODO: should be executed every few ticks, not every tick
    // ugh atp rewrite this too this is ass
    private void UpdateHeatSources() {
        HeatSource[] sources = FindObjectsByType<HeatSource>(FindObjectsSortMode.None);

        sourcesTemp = GameManager.Instance.weatherManager.GetOutsideTemperature() - 15;
        foreach (var source in sources) {
            float sourceTemp = source.sourceTemperature * 1.5f;
            float distance = Vector3.Distance(transform.position, source.transform.position);
            float invSquareLaw = distance * distance;
            sourcesTemp += (sourceTemp / invSquareLaw) * Time.fixedDeltaTime;
        }
        Debug.Log(sourcesTemp);
    }

    private void FixedUpdate() {
        UpdateHeatSources();

        float transfer = bodyTemp - sourcesTemp;
        float speed = transfer > 0.0f ? reachSpeedIncrease : reachSpeedDecrease; 
        float change = speed * transfer * Time.fixedDeltaTime;
        bodyTemp += change;
        bodyTemp = Mathf.Clamp(bodyTemp, minBodyTemp, maxBodyTemp);
        UIScriptMaster.Instance.temperatureDisplay.SetDisplay(bodyTemp);

        /*
        float targetTemp = sourcesTemp;
        targetTemp = Mathf.Min(targetTemp, maxSourcesTemp);

        float reachSpeed;
        if (targetTemp > bodyTemp) {
            reachSpeed = reachSpeedIncrease;
        } else {
            reachSpeed = reachSpeedDecrease;
        }
        
        bodyTemp = Mathf.Lerp(bodyTemp, targetTemp, reachSpeed * Time.fixedDeltaTime);

        UIScriptMaster.Instance.temperatureDisplay.SetDisplay(bodyTemp);
        */

    }
}
