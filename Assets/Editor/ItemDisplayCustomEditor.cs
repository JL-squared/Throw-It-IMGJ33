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
    SerializedProperty interactable;
    SerializedProperty button;

    // Add preset thingy

    void OnEnable() {
        nameDisplay = serializedObject.FindProperty("nameDisplay");
        descriptionDisplay = serializedObject.FindProperty("descriptionDisplay");
        icon = serializedObject.FindProperty("icon");
        miniIcon = serializedObject.FindProperty("miniIcon");
        countDisplay = serializedObject.FindProperty("countDisplay");
        interactable = serializedObject.FindProperty("interactable");
    }

    public override void OnInspectorGUI() {
        ItemDisplay display = (ItemDisplay)target;

        serializedObject.Update();
        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), GetType(), false);
        string testItemID = EditorGUILayout.DelayedTextField("Test Item", "");
        if (testItemID != "") {
            ItemData itemData = Registries.items[testItemID];
            if (itemData != null) display.UpdateValues(itemData);
        }
        EditorGUILayout.PropertyField(nameDisplay);
        EditorGUILayout.PropertyField(descriptionDisplay);
        EditorGUILayout.PropertyField(icon);
        EditorGUILayout.PropertyField(miniIcon);
        EditorGUILayout.PropertyField(countDisplay);
        EditorGUILayout.PropertyField(interactable);
        serializedObject.ApplyModifiedProperties();
    }
}