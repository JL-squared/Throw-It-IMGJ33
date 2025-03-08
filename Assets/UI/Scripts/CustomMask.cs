using UnityEngine;
using UnityEngine.UI;

// Creates an internal mask that only allows the children of this gameobject to be visible within the bounds of the parent
// (using custom image apha)
[ExecuteAlways]
[ExecuteInEditMode]
public class CustomMask : MonoBehaviour {
    public Shader shader;
    public bool inverted;

    // Forgive me my children, for I have failed to bring you salvation...
    // from this cold... dark... world....
    // TODO: REWRITE!!! TS ASS!!! SO SLOW!!!
    public void Update() {
        Texture alphaTexture = GetComponent<Image>().mainTexture;
        MaskableGraphic[] children = transform.GetComponentsInChildren<MaskableGraphic>(false);
        RectTransform parent = GetComponent<RectTransform>();
        Rect parentRect = parent.rect;
        
        float width = (parentRect.max.x - parentRect.min.x) / Screen.width;
        float height = (parentRect.max.y - parentRect.min.y) / Screen.height;
        float x = parentRect.min.x / Screen.width;
        float y = parentRect.min.y / Screen.height;

        foreach (var child in children) {
            // because for SOME fucking reason GetComponentsInChildren also gives self!!!
            if (child.transform == transform) {
                continue;
            }

            if (!child.maskable) {
                continue;
            }

            Texture mainTexture = child.GetComponent<Image>().mainTexture;
            Vector4 parentRectConverted = new Vector4(x, y, x+width, y+height);

            if (child.material.shader != shader) {
                child.material = new Material(shader);
            }

            child.material.SetInt("_Inverted", inverted ? 1 : 0);
            child.material.SetVector("_ParentRect", parentRectConverted);
            child.material.SetTexture("_MainTexture", mainTexture);
            child.material.SetTexture("_AlphaTexture", alphaTexture);
        }
    }
}
