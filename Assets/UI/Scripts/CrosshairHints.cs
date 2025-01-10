using UnityEngine;

public class CrosshairHints : MonoBehaviour {
    public GameObject interactKeyHint;
    public GameObject rightClickHint;
    public RectTransform chargeMeter;

    private void Awake() {
        SetRightClickHint(false);
        SetInteractKeyHint(false);
    }

    public void SetRightClickHint(bool b) {
        rightClickHint.SetActive(b);
    }

    public void SetInteractKeyHint(bool b) {
        interactKeyHint.SetActive(b);
    }

    public void UpdateChargeMeter(float charge) {
        float x = Mathf.InverseLerp(0.2f, 1.0f, charge);
        chargeMeter.localScale = new Vector3(x, 1.0f, 1.0f);
        chargeMeter.gameObject.SetActive(x != 0.0f);
    }
}
