using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour {
    private static readonly Collider[] pieceColliders = new Collider[2000];

    static int pieceRayMask;

    void Awake() {
        pieceRayMask = LayerMask.GetMask("Piece");
    }

    // Get snap points on a piece
    public static void GetSnapPoints(Vector3 point, float radius, List<Transform> points, List<Piece> pieces) {
        int num = Physics.OverlapSphereNonAlloc(point, radius, pieceColliders, pieceRayMask); // overlap sphere to test for all piece colliders
        for (int i = 0; i < num; i++) {
            Piece componentInParent = pieceColliders[i].GetComponentInParent<Piece>();
            if (componentInParent != null) {
                componentInParent.GetSnapPoints(points);
                pieces.Add(componentInParent);
            }
        }
    }

    // Get snap points on a specific object
    public void GetSnapPoints(List<Transform> listOut) {
        for (int i = 0; i < transform.childCount; i++) {
            Transform child = transform.GetChild(i);
            if (child.CompareTag("Snappoint"))
                listOut.Add(child);
        }
    }
}
