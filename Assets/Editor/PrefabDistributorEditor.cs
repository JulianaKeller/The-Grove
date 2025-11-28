using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrefabDistributor))]
public class PrefabDistributorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PrefabDistributor distributor = (PrefabDistributor)target;

        if (GUILayout.Button("Distribute"))
        {
            distributor.Distribute();
        }

        if (GUILayout.Button("Clear Spawned"))
        {
            distributor.ClearSpawned();
        }

        if (distributor.spawnedObjects != null && distributor.spawnedObjects.Count > 0)
        {
            EditorGUILayout.LabelField("Spawned Objects: " + distributor.spawnedObjects.Count);
        }
    }
}
