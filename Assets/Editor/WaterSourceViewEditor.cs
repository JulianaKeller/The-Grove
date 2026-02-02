using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaterSourceView))]
public class WaterSourceViewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WaterSourceView view = (WaterSourceView)target;

        if (view.data != null)
        {
            WaterSource ws = view.data;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("=== Water Source Runtime Stats ===", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Capacity");
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), ws.currentWater / ws.capacity, $"{ws.currentWater:F1}/{ws.capacity:F1}");

            EditorGUILayout.LabelField("Radius", ws.radius.ToString());
        }
    }
}
