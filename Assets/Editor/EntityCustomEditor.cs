using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Entity))]
public class EntityCustomEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        Entity entity = (Entity)target;

        if (GUILayout.Button("Regenerate GUID")) {
            //entity.guid = Guid.NewGuid();
        }
    }
}