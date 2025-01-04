using UnityEngine;

[CreateAssetMenu(fileName = "PieceDefinition", menuName = "Scriptable Objects/PieceDefinition")]
public class PieceDefinition : ScriptableObject {
    public GameObject piecePrefab;
    public Sprite icon;
}
