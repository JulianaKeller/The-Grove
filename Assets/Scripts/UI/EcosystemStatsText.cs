using TMPro;
using UnityEngine;

public class EcosystemStatsText : MonoBehaviour
{
    public enum StatType { Balance, Diversity }

    public StatType statType;
    public TMP_Text text;
    public TextMeshProUGUI text01;

    void Awake()
    {

    }

    void Start()
    {
        if (EcosystemMetrics.Instance == null || text == null || text01 == null) return;

        if (statType == StatType.Balance)
        {
            EcosystemMetrics.Instance.OnBalanceChanged.AddListener(UpdateText);
            UpdateText(EcosystemMetrics.Instance.Balance);
        }
        else
        {
            EcosystemMetrics.Instance.OnDiversityChanged.AddListener(UpdateText);
            UpdateText(EcosystemMetrics.Instance.Diversity);
        }
    }

    void OnDisable()
    {
        if (EcosystemMetrics.Instance == null) return;

        if (statType == StatType.Balance)
            EcosystemMetrics.Instance.OnBalanceChanged.RemoveListener(UpdateText);
        else
            EcosystemMetrics.Instance.OnDiversityChanged.RemoveListener(UpdateText);
    }

    private void UpdateText(float value)
    {
        string newText = $"{statType}: {Mathf.RoundToInt(value)}";
        text.text = newText;
        text01.text = newText;

        Debug.Log($"{statType} set to {value}");
    }
}