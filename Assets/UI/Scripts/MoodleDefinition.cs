using UnityEngine;

[CreateAssetMenu(fileName = "New Moodle", menuName = "Scriptable Objects/New Moodle Definition", order = 2)]
public class MoodleDefinition : ScriptableObject {
    public string id;
    public Sprite sprite;

    [Header("Neutral")]
    public string neutralName;
    public string neutralDesc;

    [Header("Weak")]
    public string weakName;
    public string weakDesc;

    [Header("Medium")]
    public string mediumName;
    public string mediumDesc;

    [Header("Bad")]
    public string badName;
    public string badDesc;
}
