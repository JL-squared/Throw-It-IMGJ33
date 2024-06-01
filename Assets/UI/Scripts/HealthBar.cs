using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour {
    public float lastPosition;
    public float actualPosition;

    public float tBar;
    public RectTransform bar;

    public float tFlash;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        bar.localScale = new Vector3(Mathf.Lerp(lastPosition, actualPosition, tBar), 1, 1);

        if(tBar >= 1.0f) {
            lastPosition = actualPosition;
            tBar = 0.0f;
        } else {
            tBar += 0.5f * Time.deltaTime;
        }
    }

    public void ProcessHealthUpdate(float newHealth) {
        actualPosition = newHealth;
    }
}
