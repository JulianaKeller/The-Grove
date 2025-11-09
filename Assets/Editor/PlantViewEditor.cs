using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlantView))]
public class PlantViewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Draw default inspector fields (like prefab, transforms, etc.)
        DrawDefaultInspector();

        PlantView view = (PlantView)target;

        if (view.data != null && view.data.species != null)
        {
            Plant p = view.data;
            PlantSpeciesData s = p.species;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("=== Plant Runtime Stats ===", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Species", s.speciesName);
            EditorGUILayout.LabelField("Age");
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), p.age / p.speciesLifespan, $"{p.age:F1}/{p.speciesLifespan:F1}");
            EditorGUILayout.LabelField("Health");
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), p.health / s.maxHP, $"{p.health:F1}/{s.maxHP}");
            EditorGUILayout.LabelField("Spread chance");
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), p.spreadChance / 1f, $"{p.spreadChance:F1}/{1f}");
            EditorGUILayout.LabelField("Size", p.size.ToString());
            EditorGUILayout.LabelField("Maximum Size", p.maxSize.ToString());
            EditorGUILayout.LabelField("Size Y");
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), p.size.y / p.maxSize.y, $"{p.size.y:F1}/{p.maxSize.y}");
            EditorGUILayout.LabelField("Current Cell Fertility", p.currentFertility.ToString("F1"));
            EditorGUILayout.LabelField("Current Cell Moisture", p.currentMoisture.ToString("F1"));
            EditorGUILayout.LabelField("Can Grow", p.canGrow.ToString());

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("=== Species Stats ===", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Water Need", s.waterNeed.ToString("F1"));
            EditorGUILayout.LabelField("Water Capacity", s.waterCapacity.ToString("F1"));
            EditorGUILayout.LabelField("Ground Fertility Usage", s.groundFertilityUsage.ToString("F1"));
            EditorGUILayout.LabelField("Minimum Ground Fertility", s.minGroundFertility.ToString("F1"));
            EditorGUILayout.LabelField("Maximum Size Variation", s.maxSizeVariation.ToString("F1"));
            EditorGUILayout.LabelField("Nutrition Base Value", s.nutritionBaseValue.ToString("F1"));
            EditorGUILayout.LabelField("Spread Chance", s.baseSpreadChance.ToString("F1"));
            EditorGUILayout.LabelField("Is Edible", s.isEdible.ToString());
        }
    }
}