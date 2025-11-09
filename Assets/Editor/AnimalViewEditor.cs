using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimalView))]
public class AnimalViewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Draw default inspector fields (like prefab, transforms, etc.)
        DrawDefaultInspector();

        AnimalView view = (AnimalView)target;

        if (view.data != null && view.data.species != null)
        {
            Animal a = view.data;
            AnimalSpeciesData s = a.species;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("=== Animal Runtime Stats ===", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Species", s.speciesName);
            EditorGUILayout.LabelField("Age");
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), a.age / a.speciesLifespan, $"{a.age:F1}/{a.speciesLifespan}");
            EditorGUILayout.LabelField("Health");
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), a.health / s.maxHP, $"{a.health:F1}/{s.maxHP}");
            EditorGUILayout.LabelField("Hunger");
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), a.hunger / 100f, $"{a.hunger:F1}/100");
            EditorGUILayout.LabelField("Thirst");
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), a.thirst / 100f, $"{a.thirst:F1}/100");
            EditorGUILayout.LabelField("Energy");
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), a.energy / 100f, $"{a.energy:F1}/100");
            EditorGUILayout.LabelField("Stamina");
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), a.stamina / s.stamina, $"{a.stamina:F1}/{s.stamina}");
            EditorGUILayout.LabelField("Mating Drive");
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), a.matingDrive / 100f, $"{a.matingDrive:F1}/100");
            EditorGUILayout.LabelField("Is Running", a.isRunning.ToString());
            EditorGUILayout.LabelField("Is Walking", a.isWalking.ToString());
            EditorGUILayout.LabelField("Is Female", a.isFemale.ToString());

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("=== Species Stats ===", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Walking Speed", s.walkingSpeed.ToString("F1"));
            EditorGUILayout.LabelField("Running Speed", s.runningSpeed.ToString("F1"));
            EditorGUILayout.LabelField("Base Dominance", s.baseDominance.ToString());
            EditorGUILayout.LabelField("Dominance Variation", s.dominanceVariation.ToString());
            EditorGUILayout.LabelField("Aggression", s.aggression.ToString("F2"));
            EditorGUILayout.LabelField("Awareness Range", s.awarenessRange.ToString("F1"));
            EditorGUILayout.LabelField("Stealth", s.stealth.ToString("F2"));
            EditorGUILayout.LabelField("Hunger Rate", s.hungerRate.ToString("F2"));
            EditorGUILayout.LabelField("Thirst Rate", s.thirstRate.ToString("F2"));
            EditorGUILayout.LabelField("Energy Depletion Rate", s.energyDepletionRate.ToString("F2"));
            EditorGUILayout.LabelField("HP Recovery Rate", s.hpRecoveryRate.ToString("F2"));
            EditorGUILayout.LabelField("Is Carnivore", s.isCarnivore.ToString());
            EditorGUILayout.LabelField("Is Herbivore", s.isHerbivore.ToString());
        }
    }
}