using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeciesButton : MonoBehaviour
{
    public EntitySpeciesData speciesData;
    public TMP_Text label;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        Debug.Log("Clicked Species Button!");
        MenuManager.Instance.CloseAll();
        InteractionManager.Instance.ToggleSpawnMode(speciesData);
    }

    public void AssignSpecies(EntitySpeciesData data)
    {
        speciesData = data;
        if(label != null)
        {
            label.text = data.speciesName;

        }
    }
}
