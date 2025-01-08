using UnityEngine;

[CreateAssetMenu(fileName = "PieceDefinition", menuName = "Scriptable Objects/PieceDefinition")]
public class PieceDefinition : ScriptableObject {
    public GameObject piecePrefab;
    public Sprite icon;
    public string m_name;
    public string description;

    public ItemStack requirement1 = null;
    public ItemStack requirement2 = null;
    public ItemStack requirement3 = null;
}
