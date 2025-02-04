using UnityEditor;
using UnityEditor.Experimental.Rendering;
using UnityEngine;

[CustomEditor(typeof(Player))]
public class PlayerCustomEditor : Editor {
    string text = "snowball";
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        Player player = (Player)target;

        text = EditorGUILayout.TextField("Item ID: ", text);
        if (GUILayout.Button($"Give Player Item: '{text}'")) {
            ItemData item = Registries.items[text];
            if (item == null) {
                Debug.LogWarning($"{text} wasn't loaded :(");
            } else {
                player.inventory.AddItem(new ItemStack(item, 1));
            }
        }
    }
}