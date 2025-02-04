using UnityEngine;

public class PlayerTemperature : PlayerBehaviour {
    /*
    public void Start() {
        bodyTemperature = targetTemperature;
    }

    private void UpdateTemperature() {
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
        outsideTemperature = GameManager.Instance.weatherManager.GetOutsideTemperature();

        // Actual value that we must try to reach
        float totalTemp = Mathf.Max(outsideTemperature, heatSourcesTemperature);

        // Body temperature calculations (DOES NOT CONSERVE ENERGY)
        bodyTemperature = Mathf.Lerp(bodyTemperature, totalTemp, outsideReachSpeed * Time.deltaTime);
        bodyTemperature = Mathf.Lerp(bodyTemperature, targetTemperature, targetReachSpeed * Time.deltaTime);
    }

    private void UpdateShivering() {
        if (bodyTemperature < minShiveringTemp) {
            shiveringCurrentTime += Time.deltaTime;
        } else {
            shiveringCurrentTime = 0.0f;
        }

        if (shiveringCurrentTime > shiveringDelay) {
            shiverMeTimbers += Time.deltaTime;
        } else {
            shiverMeTimbers -= Time.deltaTime;
        }
        shiverMeTimbers = Mathf.Clamp01(shiverMeTimbers);


        Vector3 localCamPos = new Vector3(Mathf.PerlinNoise1D(Time.time * shiveringShakeScale + 32.123f) - 0.5f, Mathf.PerlinNoise1D(Time.time * shiveringShakeScale - 2.123f) - 0.5f, 0.0f);
        localCamPos *= shiverMeTimbers;
        localCamPos *= shiveringShakeFactor;
        //gameCamera.transform.localPosition = localCamPos;
        //gameCamera.transform.localRotation = Quaternion.Lerp(Quaternion.identity, Random.rotation, shiverMeTimbers * Time.deltaTime * shiveringShakeRotationFactor);
    }
    */
}
