using UnityEngine;
using UnityEngine.UI;

public class SpawnButton : MonoBehaviour
{
    public GameObject indicatorPrefab;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        InteractionManager.Instance.SetSpawnIndicator(indicatorPrefab);
    }
}
