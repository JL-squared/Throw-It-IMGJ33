using TMPro;
using UnityEngine;

public class TemperatureDisplay : MonoBehaviour {
    public TextMeshProUGUI display;
    public GameObject sourceParent;

    void Start() {
        SetDisplay(0.0f);
    }

    private string TempFormat(float temperature) {
        return temperature.ToString("F1") + "°C";
    }

    public void SetDisplay(float temperature) {
        display.text = TempFormat(temperature);
    }
}
