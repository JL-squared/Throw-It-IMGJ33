using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour {
    private static readonly Collider[] pieceColliders = new Collider[2000];

    static int pieceRayMask;

    // Start is called before the first frame update
    void Start()
    {
        pieceRayMask = LayerMask.GetMask("Piece");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void GetSnapPoints(Vector3 point, float radius, List<Transform> points, List<Piece> pieces) {
        int num = Physics.OverlapSphereNonAlloc(point, radius, pieceColliders, pieceRayMask);
        for (int i = 0; i < num; i++) {
            Debug.Log("Oh yeah we have snap points");
            Piece componentInParent = pieceColliders[i].GetComponentInParent<Piece>();
            if (componentInParent != null) {
                componentInParent.GetSnapPoints(points);
                pieces.Add(componentInParent);
            }
        }
    }

    public void GetSnapPoints(List<Transform> listOut) {
        for (int i = 0; i < transform.childCount; i++) {
            Transform child = transform.GetChild(i);
            Debug.Log("Oh yeah we have snap point");
            if (child.CompareTag("Snappoint"))
                listOut.Add(child);
        }
    }
}
