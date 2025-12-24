using DistantLands.Cozy.Data;
using UnityEngine;
using UnityEngine.UI;

public class WeatherController : MonoBehaviour
{
    public WeatherProfile[] weatherProfiles;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        Debug.Log("Clicked Rain Button!");
        WeatherProfile weatherProfile = (weatherProfiles != null && weatherProfiles.Length > 0) ? weatherProfiles[Random.Range(0, weatherProfiles.Length)] : null;
        InteractionManager.Instance.ToggleSummonWeatherMode(weatherProfile);
    }
}
