using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.Rendering;
using UnityEngine;

[CustomEditor(typeof(PlayerScript))]
public class PlayerCustomEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        PlayerScript player = (PlayerScript)target;
        ItemData snowball = (ItemData)AssetDatabase.LoadAssetAtPath("Assets/Items/ScriptableObjects/Snowball.asset", typeof(ScriptableObject));

        
        if (GUILayout.Button("Give Player snow")) {
            if (snowball == null) {
                Debug.Log("Snowball wasn't loaded :(");
            } else {
                player.addItem(new Item(1, snowball));
            }
        }
    }
}