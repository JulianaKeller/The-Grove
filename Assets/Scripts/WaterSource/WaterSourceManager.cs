using DistantLands.Cozy;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class WaterSourceManager : MonoBehaviour
{
    public static WaterSourceManager Instance { get; private set; }

    public int updateSubsetCount = 1;

    [Header("Radius")]

    public int waterSourceRadius = 2;
    public int radiusVariation = 0;
    private int nextRadius;

    [Header("Prefabs")]

    public GameObject waterSourcePrefab;
    public GameObject waterSourcesParent;

    [Header("Influence Controls")]

    public float baseDepth = 1f;
    public float baseEvaporationRate = 1f;
    public float influenceRadius = 5f;
    public float fertilityBonus = 0.05f;
    public float moistureBonus = 0.05f;

    private List<WaterSource> waterSources = new List<WaterSource>();
    private List<WaterSourceView> views = new List<WaterSourceView>();

    private static int nextId = 0;

    [Header("Weather Influence")]

    public bool lightRain = false;
    public bool heavyRain = false;
    public bool thunderstorm = false;

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

    private void Start()
    {
        nextRadius = waterSourceRadius;
    }

    public int GetNewWaterSourceRadius()
    {
        nextRadius = waterSourceRadius + Random.Range(-radiusVariation, radiusVariation);

        Debug.Log("Next Radius is now " + nextRadius);
        return nextRadius;
    }

    public void SpawnWaterSource(Vector3 pos, int cellRadius)
    {
        WaterSource data = new WaterSource(nextId++, cellRadius, pos);

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

        Debug.Log("Spwaned a water source at " + pos + " with radius " + cellRadius);
    }

    /// <summary>
    /// Computes local scale so that the WaterSource covers the correct number of EnvironmentGrid cells.
    /// </summary>
    private Vector3 ComputeScaleForRadius(GameObject obj)
    {
        float margin = 1.005f;

        float cellSize = EnvironmentGrid.Instance.cellSize;

        float targetDiameterWorld = nextRadius * cellSize * 2f;

        Renderer r = obj.GetComponentInChildren<Renderer>();
        if (r == null)
            return Vector3.one * targetDiameterWorld;

        Bounds b = r.bounds;

        float meshDiameter = Mathf.Max(b.extents.x, b.extents.z);

        if (meshDiameter <= 0f)
            return Vector3.one;

        float scaleFactor = (targetDiameterWorld * margin) / meshDiameter;
        return new Vector3(scaleFactor, 1f, scaleFactor); ;
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

                //ws.view.ResetInterpolation();
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
