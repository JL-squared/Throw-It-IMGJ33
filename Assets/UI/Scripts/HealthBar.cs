using UnityEngine;

public class HealthBar : MonoBehaviour {
    [Range(0f, 1f)]
    public float lastPosition;
    [Range(0f, 1f)]
    public float actualPosition;

    public float lastFrameActualPosition;

    public float tBar;
    public float tLinger;
    public RectTransform bar;
    public RectTransform lingerBar;

    public float lerpSpeed;

    public float tFlash;

    public float testHealth;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        bar.localScale = new Vector3(Mathf.SmoothStep(lastPosition, actualPosition, tBar), 1f);
        lingerBar.localScale = new Vector3(Mathf.SmoothStep(lastPosition, actualPosition, tLinger), 1f);

        if (actualPosition != lastFrameActualPosition) {
            lastPosition = Mathf.SmoothStep(lastPosition, lastFrameActualPosition, tBar);
            tBar = 0;
        }

        if(tBar >= 1.0f) {
            if (tLinger >= 1.0f) {
                // Reset state
                lastPosition = actualPosition;
                tBar = 0.0f;
                tLinger = 0.0f;
            } else {
                tLinger += 5.0f * Time.deltaTime;
            }
        } else if (lastPosition != actualPosition) {
            tBar += lerpSpeed * Time.deltaTime;
        }

        lastFrameActualPosition = actualPosition;
    }

    public void ProcessHealthUpdate(float newHealth) {
        actualPosition = newHealth;
    }
}
