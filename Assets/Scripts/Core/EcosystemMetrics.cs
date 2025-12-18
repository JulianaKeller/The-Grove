using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.Profiling.RawFrameDataView;

public class EcosystemMetrics : MonoBehaviour
{
    class SpeciesTrend
    {
        public int currentCount;
        public Queue<int> history = new();

        public void Record(int window) //records currentCount every N ticks
        {
            history.Enqueue(currentCount);
            if (history.Count > window)
                history.Dequeue();
        }

        public float GetStabilityScore()
        {
            if (history.Count < 2)
                return 1f;

            float avg = (float) history.Average();
            float variance = history.Select(v => Mathf.Pow(v - avg, 2)).Average();

            return 1f / (1f + variance);
        }
    }

    public static EcosystemMetrics Instance { get; private set; }

    [Range(0f, 100f)] public float diversity;
    [Range(0f, 100f)] public float balance;

    public UnityEvent<float> OnBalanceChanged = new UnityEvent<float>();
    public UnityEvent<float> OnDiversityChanged = new UnityEvent<float>();

    [Header("Balance Settings")]
    public int trendWindowTicks = 200;
    public float extinctionPenaltyWeight = 2f;

    private Dictionary<EntitySpeciesData, SpeciesTrend> trends = new();

    private int totalSpeciesCount;

    void Awake()
    {
        Instance = this;

        // total possible species (animals + plants)
        totalSpeciesCount =
            Resources.LoadAll<AnimalSpeciesData>("ScriptableObjects/AnimalSpecies").Length +
            Resources.LoadAll<PlantSpeciesData>("ScriptableObjects/PlantSpecies").Length;

        SetBalance(0);
        SetDiversity(0);
    }

    public float Balance
    {
        get
        {
            return balance;
        }
        private set
        {
            //if (Mathf.Approximately(balance, value)) return;
            balance = value;
            OnBalanceChanged?.Invoke(balance);
        }
    }

    public float Diversity
    {
        get
        {
            return diversity;
        }
        private set
        {
            //if (Mathf.Approximately(diversity, value)) return;
            diversity = value;
            OnDiversityChanged?.Invoke(diversity);
        }
    }

    public void SetBalance(float value)
    {
        Balance = value;
    }

    public void SetDiversity(float value)
    {
        Diversity = value;
    }

    public void RecalculateDiversity()
    {
        int aliveSpecies = trends.Values.Count(t => t.currentCount > 0);
        SetDiversity((float)aliveSpecies / totalSpeciesCount * 100f);
    }

    public void RegisterSpawn(EntitySpeciesData species)
    {
        GetTrend(species).currentCount++;
        RecalculateDiversity();
    }

    public void RegisterDeath(EntitySpeciesData species)
    {
        SpeciesTrend trend = GetTrend(species);
        trend.currentCount = (trend.currentCount - 1 >= 0 ? trend.currentCount - 1 : 0);
        RecalculateDiversity();
    }

    private SpeciesTrend GetTrend(EntitySpeciesData species)
    {
        if (!trends.TryGetValue(species, out var trend))
        {
            trend = new SpeciesTrend();
            trends[species] = trend;
        }
        return trend;
    }

    public void UpdateBalance()
    {
        if (trends.Count == 0)
        {
            SetBalance(0f);
            return;
        }

        float sum = 0f;
        int count = 0;

        foreach (var trend in trends.Values)
        {
            trend.Record(trendWindowTicks);

            float stability = trend.GetStabilityScore();

            // extinction penalty
            if (trend.currentCount == 0)
                stability /= extinctionPenaltyWeight;

            sum += stability;
            count++;
        }

        SetBalance(Mathf.Clamp01(sum / count) * 100f);
    }

}
