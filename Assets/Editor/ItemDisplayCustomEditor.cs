using UnityEngine;
using UnityEditor;
using UnityEditor.TerrainTools;
using TMPro;
using UnityEditor.UIElements;

[CustomEditor(typeof(ItemDisplay))]
[CanEditMultipleObjects]
public class ItemDisplayCustomEditor : Editor {
    SerializedProperty nameDisplay;
    SerializedProperty descriptionDisplay;
    SerializedProperty icon;
    SerializedProperty miniIcon;
    SerializedProperty countDisplay;

    // Add preset thingy

    void OnEnable() {
        nameDisplay = serializedObject.FindProperty("nameDisplay");
        descriptionDisplay = serializedObject.FindProperty("descriptionDisplay");
        icon = serializedObject.FindProperty("icon");
        miniIcon = serializedObject.FindProperty("miniIcon");
        countDisplay = serializedObject.FindProperty("countDisplay");
    }

    public override void OnInspectorGUI() {
        ItemDisplay display = (ItemDisplay)target;

        serializedObject.Update();
        string testItemID = EditorGUILayout.DelayedTextField("Test Item", "");
        if (testItemID != "") {
            ItemData itemData = Registries.itemsData[testItemID];
            if (itemData != null) display.UpdateValues(itemData);
        }
        EditorGUILayout.PropertyField(nameDisplay);
        EditorGUILayout.PropertyField(descriptionDisplay);
        EditorGUILayout.PropertyField(icon);
        EditorGUILayout.PropertyField(miniIcon);
        EditorGUILayout.PropertyField(countDisplay);
        serializedObject.ApplyModifiedProperties();
    }
}