using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MarketManager))]
public class MarketCustomEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        MarketManager manager = (MarketManager)target;

        if (GUILayout.Button("Update stock market")) {
            manager.MarketUpdate();
        }
    }
}