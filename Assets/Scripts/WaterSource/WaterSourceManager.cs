using DistantLands.Cozy;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class WaterSourceManager : MonoBehaviour
{
    public static WaterSourceManager Instance { get; private set; }

    public int updateSubsetCount = 1;

    [Header("Radius")]

    [SerializeField]
    private float waterSourceRadius = 1;
    [SerializeField]
    private float radiusVariation = 0.5f;
    private float nextRadius;

    [Header("Prefabs")]

    public GameObject waterSourcePrefab;
    public GameObject waterSourcesParent;

    [Header("Influence Controls")]

    public float depth = 0.1f;
    public float baseEvaporationRate = 1f;
    public float influenceRadius = 5f;
    public float fertilityBonus = 0.05f;
    public float moistureBonus = 0.05f;

    private List<WaterSource> waterSources = new List<WaterSource>();
    private List<WaterSourceView> views = new List<WaterSourceView>();

    private static int nextId = 0;

    [Header("Weather Influence")]

    private bool lightRain = false;
    private bool heavyRain = false;
    private bool thunderstorm = false;

    public float refillLight = 0.5f;
    public float refillHeavy = 1.0f;
    public float refillThunderstorm = 1.0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public float GetNewWaterSourceRadius()
    {
        nextRadius = waterSourceRadius + Random.Range(-radiusVariation, radiusVariation);
        return nextRadius;
    }

    public void SpawnWaterSource(Vector3 pos, float radius)
    {
        WaterSource data = new WaterSource(pos, nextId++, radius);

        if(waterSourcePrefab != null)
        {
            GameObject obj = Instantiate(waterSourcePrefab, pos, Quaternion.identity, waterSourcesParent ? waterSourcesParent.transform : null);

            obj.transform.localScale = ComputeScaleForRadius(obj);

            WaterSourceView view = obj.GetComponent<WaterSourceView>();

            view.data = data;
            data.view = view;

            views.Add(view);
            waterSources.Add(data);
        }

        EnvironmentGrid.Instance.RegisterWaterSource(data);

        Debug.Log("Spwaned a water source at " + pos + " with radius " + radius);
    }

    private Vector3 ComputeScaleForRadius(GameObject obj)
    {
        float margin = 1.05f;

        Renderer r = obj.GetComponentInChildren<Renderer>();
        if (r == null)
            return Vector3.one;

        Bounds b = r.bounds;

        float meshRadius = Mathf.Max(b.extents.x, b.extents.z);

        if (meshRadius <= 0f)
            return Vector3.one;

        float scaleFactor = (nextRadius * margin) / meshRadius;
        return Vector3.one * scaleFactor;
    }

    public void UpdateWaterSources(float timeStep, int tick)
    {
        for (int i = waterSources.Count - 1; i >= 0; i--)
        {
            WaterSource ws = waterSources[i];
            if ((tick + ws.id) % updateSubsetCount == 0)
            {
                if (lightRain)
                {
                    ws.Refill(refillLight * timeStep);
                }
                else if (heavyRain)
                {
                    ws.Refill(refillHeavy * timeStep);
                }
                else if (thunderstorm)
                {
                    ws.Refill(refillThunderstorm * timeStep);
                }

                float evaporationRate = CalculateEvaporationRate();

                ws.view.ResetInterpolation();
                ws.UpdateWaterSource(timeStep * updateSubsetCount, evaporationRate);
            }
        }
    }

    private float CalculateEvaporationRate()
    {
        float temperature = CozyWeather.instance.climateModule.currentTemperature;

        float tempFactor = Mathf.InverseLerp(32f, 99f, temperature);

        return baseEvaporationRate * (0.3f + tempFactor);
    }

    public void RemoveWaterSource(WaterSource ws)
    {
        waterSources.Remove(ws);
        if (ws.view != null)
        {
            views.Remove(ws.view);
            Destroy(ws.view.gameObject);
        }
        EnvironmentGrid.Instance.DeregisterWaterSource(ws);
    }

    public void LogLightRain(bool raining)
    {
        lightRain = raining;
    }

    public void LogHeavyRain(bool raining)
    {
        heavyRain = raining;
    }

    public void LogThunderstorm(bool raining)
    {
        thunderstorm = raining;
    }
}
