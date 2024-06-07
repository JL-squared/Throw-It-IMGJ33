using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EntityHealth))]
public class HealthCustomEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        EntityHealth health = (EntityHealth)target;

        if (GUILayout.Button("Deal 10 damage")) {
            health.Damage(10.0f);
        }

        if (GUILayout.Button("Kill")) {
            health.Damage(100000.0f);
        }
    }
}