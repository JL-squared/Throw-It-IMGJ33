using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class CustomAlphaCopy : MonoBehaviour {
    public Sprite source;
    public Sprite alpha;
    public Shader shader;
    public bool inverted;
    private Material copy;

    private void Start() {
        copy = new Material(shader);
        GetComponent<Image>().material = copy;
        copy.SetTexture("_MainTexture", source.texture);
        copy.SetTexture("_AlphaTexture", alpha.texture);
    }

    private void Update() {
        if (copy != null) {
            copy.SetInt("_Inverted", inverted ? 1 : 0);
            copy.SetVector("_LocalScale", GetComponent<RectTransform>().localScale);
        } else {
            Start();
        }
    }
}
