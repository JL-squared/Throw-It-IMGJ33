using System;
using Tweens;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class HealthBar : MonoBehaviour {
    public RectTransform bar;
    public RectTransform lingerBar;

    public float healthDuration = 0.2f;
    public float lingerDelay = 0.3f;
    public float lingerDuration = 0.1f;
    public EaseType type;

    public float currentPercent;
    TweenInstance<Transform, float> a;
    private float lingerCounter;
    private bool lingerate;

    // Start is called before the first frame update
    void Start() {
        currentPercent = 1f;
    }

    public void HealthChanged(float percent) {
        bool healed = percent > currentPercent;
        var tween = new FloatTween {
            from = bar.localScale.x,
            to = percent,
            duration = healthDuration,
            onUpdate = (instance, value) => {
                bar.localScale = new Vector3(value, 1f);

                // since the linger bar is underneath the health bar we have to set it exactly underneath it
                // otherwise the next time the player is damaged after a heal you won't be able to see the linger bar
                if (healed) {
                    lingerBar.localScale = new Vector3(value, 1f);
                }
            },
            easeType = type,
        };

        gameObject.AddTween(tween);
        lingerCounter = 0f;
        lingerate = !healed;
        currentPercent = percent;
    }

    // Update is called once per frame
    void Update() {
        lingerCounter += Time.deltaTime;

        if (lingerCounter > lingerDelay && lingerate) {
            lingerate = false;

            var secondTween = new FloatTween {
                from = lingerBar.localScale.x,
                to = currentPercent,
                duration = lingerDuration,
                onUpdate = (instance, value) => {
                    lingerBar.localScale = new Vector3(value, 1f);
                },
                easeType = type,
            };

            gameObject.AddTween(secondTween);
        }




        /*
        bar.localScale = new Vector3(Mathf.SmoothStep(lastPosition, actualPosition, tBar), 1f);
        lingerBar.localScale = new Vector3(Mathf.SmoothStep(lastPosition, actualPosition, tLinger), 1f);

        if (actualPosition != lastFrameActualPosition) {
            lastPosition = Mathf.SmoothStep(lastPosition, lastFrameActualPosition, tBar);
            
            if (Mathf.Abs(actualPosition - lastFrameActualPosition) < 0.05) {
                tBar = 1f;
            }
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
        */


        //bar.localScale = new Vector3(healthScale, 1f);
        //lingerBar.localScale = new Vector3(lingerScale, 1f);
    }
}
