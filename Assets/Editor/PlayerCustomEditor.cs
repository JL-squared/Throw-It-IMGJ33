using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.Rendering;
using UnityEngine;

[CustomEditor(typeof(PlayerScript))]
public class PlayerCustomEditor : Editor {
    string text = "snowball";
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        PlayerScript player = (PlayerScript)target;

        /*
        ItemData snowball = ItemUtils.GetItemType("snowball");

        if (GUILayout.Button("Give Player snow")) {
            if (snowball == null) {
                Debug.LogWarning("Snowball wasn't loaded :(");
            } else {
                player.AddItem(new Item(1, snowball));
            }
        }
        */

        text = EditorGUILayout.TextField("Item ID: ", text);
        if (GUILayout.Button($"Give Player Item: '{text}'")) {
            ItemData item = ItemUtils.GetItemType(text);
            if (item == null) {
                Debug.LogWarning($"{text} wasn't loaded :(");
            } else {
                player.AddItem(new Item(1, item));
            }
        }
    }
}