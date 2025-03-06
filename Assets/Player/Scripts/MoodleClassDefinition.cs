using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Moodle", menuName = "Scriptable Objects/New Moodle Definition", order = 2)]
public class MoodleClassDefinition : ScriptableObject {
    public string id;
    public Sprite sprite;

    // sigma

    [Header("Neutral")]
    public MoodleDefinition neutral;

    [Header("Weak")]
    public MoodleDefinition weak;

    [Header("Medium")]
    public MoodleDefinition medium;

    [Header("Bad")]
    public MoodleDefinition bad;
}
