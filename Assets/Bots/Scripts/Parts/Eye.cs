using Tweens;
using UnityEngine;

// TODO: batching issue that occurs due to negative scaling!!!
// will try to fix when we rewrite the whole bot system after the demo
public class Eye : BotBehaviour {
    private bool trigerredDeathTransition = false;
    public MeshRenderer eyeRenderer;
    public Light eyeLight;

    // REASON WE DO THIS:
    // Unity URP batches draw calls that have the same properties, materials, and underlying shaders
    // If we make a unique Material for *each* instance of a bot's eye, none of the eye draw calls will be batched
    // Most of the time, an eye will either be completely lit or completely unlit (not in the "death" transition state), so we can help the batcher
    // by giving it two *unique* materials that are applied to a bunch of eyes instead, making it possible it to batch them!! 
    public static Material lit;
    public static Material unlit;

    public void Start() {
        if (lit == null && unlit == null) {
            lit = new Material(eyeRenderer.material);
            lit.SetFloat("_EyeLerp", 1.0f);
            unlit = new Material(eyeRenderer.material);
            unlit.SetFloat("_EyeLerp", 0.0f);
        }
        eyeRenderer.sharedMaterial = lit;
        eyeLight.intensity = 2.5f;
    }

    private void Update() {
        // This is ass, I know
        // I know we should be using evenst instead but wtv just tryna get it to work for the demo
        // Whole bot system is gonna get revisited anyways kekek
        if (deathFactor > 0 && !trigerredDeathTransition) {
            trigerredDeathTransition = true;

            gameObject.AddTween(new FloatTween() {
                from = 1f,
                to = 0f,
                duration = 1.0f,
                onUpdate = (TweenInstance<Transform, float> val, float aliveFactor) => {
                    Material mat;
                    if (aliveFactor == 0.0) {
                        mat = unlit;
                    } else {
                        mat = new Material(eyeRenderer.material);
                        mat.SetFloat("_EyeLerp", aliveFactor);
                    }

                    eyeRenderer.sharedMaterial = mat;
                    eyeLight.intensity = 2.5f * aliveFactor;
                },
            });
        }
    }
}
