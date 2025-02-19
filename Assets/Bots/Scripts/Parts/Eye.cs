using Tweens;
using UnityEngine;

// TODO: batching issue that occurs due to negative scaling!!!
// will try to fix when we rewrite the whole bot system after the demo
public class Eye : BotBehaviour {
    private bool tahinieed = false;
    public MeshRenderer eyeRenderer;
    public Light eyeLight;

    // REASON WE DO THIS:
    // Unity URP batches draw calls that have the same properties, materials, and underlying shaders
    // If we make a unique MaterialPropertyBlock for *each* instance of a bot's eye, none of the eye draw calls will be batched
    // Most of the time, an eye will either be completely lit or completely unlit (not in the "death" transition state), so we can help the batcher
    // by giving it two *unique* material property blocks that are applied to a bunch of eyes instead, making it possible it to batch them!! 
    public static MaterialPropertyBlock lit;
    public static MaterialPropertyBlock unlit;

    public void Start() {
        if (lit == null && unlit == null) {
            lit = new MaterialPropertyBlock();
            lit.SetFloat("_EyeLerp", 1.0f);
            unlit = new MaterialPropertyBlock();
            unlit.SetFloat("_EyeLerp", 0.0f);
        }
        eyeRenderer.SetPropertyBlock(lit);
        eyeLight.intensity = 2.5f;
    }

    private void Update() {
        // This is ass, I know
        // I know we should be using evenst instead but wtv just tryna get it to work for the demo
        // Whole bot system is gonna get revisited anyways kekek
        if (deathFactor > 0 && !tahinieed) {
            tahinieed = true;

            gameObject.AddTween(new FloatTween() {
                from = 1f,
                to = 0f,
                duration = 1.0f,
                onUpdate = (TweenInstance<Transform, float> val, float amogus) => {
                    MaterialPropertyBlock block;
                    if (amogus == 0.0) {
                        block = unlit;
                    } else {
                        block = new MaterialPropertyBlock();
                        block.SetFloat("_EyeLerp", amogus);
                    }

                    eyeRenderer.SetPropertyBlock(block);
                    eyeLight.intensity = 2.5f * amogus;
                },
            });
        }
    }
}
