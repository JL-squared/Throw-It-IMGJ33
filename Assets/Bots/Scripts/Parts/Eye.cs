using Tweens;
using UnityEngine;
using static UnityEditor.LightingExplorerTableColumn;

public class Eye : BotBehaviour {
    private bool tahinieed = false;
    public MeshRenderer eyeRenderer;
    public Light eyeLight;

    public void Start() {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetFloat("_EyeLerp", 1.0f);
        eyeRenderer.SetPropertyBlock(block);
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
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    block.SetFloat("_EyeLerp", amogus);
                    eyeRenderer.SetPropertyBlock(block);
                    eyeLight.intensity = 2.5f * amogus;
                },
            });
        }
    }
}
