using UnityEngine;
using UnityEngine.UI;

public class CustomAlphaCopy : MonoBehaviour {
    public Sprite source;
    public Sprite alpha;
    public Shader shader;
    private Material copy;

    private void Start() {
        copy = new Material(shader);
        GetComponent<Image>().material = copy;
        copy.SetTexture("_MainTexture", source.texture);
        copy.SetTexture("_AlphaTexture", alpha.texture);
    }

    private void Update() {
        copy.SetVector("Scaler", GetComponent<RectTransform>().localScale);
    }
}
