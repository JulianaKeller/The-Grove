using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainHeightmapSave
{
    public int resolution;
    public float[] heights;
}

public class TerrainDeformer : MonoBehaviour
{
    public static TerrainDeformer Instance { get; private set; }

    [SerializeField] private LayerMask terrainLayer;

    public Terrain terrain;

    [Header("Runtime Terrain Handling")]

    private TerrainData originalTerrainData;
    private TerrainData runtimeTerrainData;

    [Header("Terrain Serialization")]

    private string SavePathRuntimeTerrain =>
    System.IO.Path.Combine(Application.persistentDataPath, "terrain_heightmap_runtime.json");
    private string SavePathOriginalTerrain =>
    System.IO.Path.Combine(Application.persistentDataPath, "terrain_heightmap_original.json");


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

    void Start()
    {
        InitializeRuntimeTerrain();
    }

    public struct TerrainPoint
    {
        public Vector3 worldPos;
        public int x;
        public int z;
        public float height;
    }

    private void InitializeRuntimeTerrain()
    {
        if (terrain == null)
            terrain = Terrain.activeTerrain;

        if (terrain == null)
            return;

        originalTerrainData = terrain.terrainData;

        // Clone for runtime-only modification
        runtimeTerrainData = Instantiate(originalTerrainData);
        runtimeTerrainData.name = originalTerrainData.name + "_Runtime";

        terrain.terrainData = runtimeTerrainData;
    }

    public IEnumerable<TerrainPoint> GetTerrainPointsInRadius(Vector3 center, int cellRadius)
    {
        if(terrain == null)
            terrain = Terrain.activeTerrain;
        if (terrain == null)
            yield break;

        float radius = cellRadius * EnvironmentGrid.Instance.cellSize;

        TerrainData data = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        int heightmapWidth = data.heightmapResolution;
        int heightmapHeight = data.heightmapResolution;

        float[,] heights = data.GetHeights(0, 0, heightmapWidth, heightmapHeight);

        Vector3 size = data.size;

        Vector2 centerXZ = new Vector2(center.x, center.z);

        int minX = Mathf.Max(0, Mathf.FloorToInt(((center.x - radius - terrainPos.x) / size.x) * heightmapWidth));
        int maxX = Mathf.Min(heightmapWidth - 1, Mathf.CeilToInt(((center.x + radius - terrainPos.x) / size.x) * heightmapWidth));

        int minZ = Mathf.Max(0, Mathf.FloorToInt(((center.z - radius - terrainPos.z) / size.z) * heightmapHeight));
        int maxZ = Mathf.Min(heightmapHeight - 1, Mathf.CeilToInt(((center.z + radius - terrainPos.z) / size.z) * heightmapHeight));

        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                Vector3 worldPos = new Vector3(
                    terrainPos.x + (x / (float)(heightmapWidth - 1)) * size.x,
                    terrainPos.y + heights[z, x] * size.y,
                    terrainPos.z + (z / (float)(heightmapHeight - 1)) * size.z
                );

                Vector2 pointXZ = new Vector2(worldPos.x, worldPos.z);

                if (Vector3.Distance(pointXZ, centerXZ) <= radius)
                {
                    yield return new TerrainPoint
                    {
                        worldPos = worldPos,
                        x = x,
                        z = z,
                        height = heights[z, x]
                    };
                }
            }
        }
    }

    public void ApplyTerrain()
    {
        if(terrain == null)
        {
            terrain = Terrain.activeTerrain;
        }
            
        if (terrain == null)
        {
            return;
        }

        terrain.terrainData.SetHeightsDelayLOD(0, 0,
            terrain.terrainData.GetHeights(0, 0,
            terrain.terrainData.heightmapResolution,
            terrain.terrainData.heightmapResolution));

        //terrain.ApplyDelayedHeightmapModification();
        terrain.terrainData.SyncHeightmap();
    }

    public void LowerTerrain(Vector3 center, int cellRadius, float irregularity, out float spawnHeight)
    {
        spawnHeight = 0;

        if(terrain == null)
        {
            terrain = Terrain.activeTerrain;
        }
        if (terrain == null)
        {
            return;
        }

        float radius = cellRadius * EnvironmentGrid.Instance.cellSize;

        TerrainData data = terrain.terrainData;

        Debug.Log("Height of Terrain: " + data.size.y);
        float normalizedDepth = WaterSourceManager.Instance.baseDepth / data.size.y;

        float[,] heights = data.GetHeights(0, 0, data.heightmapResolution, data.heightmapResolution);

        float localMaxHeight = float.MinValue;
        float localMinHeight = float.MaxValue;

        Vector2 centerXZ = new Vector2(center.x, center.z);

        foreach (var point in GetTerrainPointsInRadius(center, cellRadius))
        {
            localMinHeight = Mathf.Min(localMinHeight, point.height);
            localMaxHeight = Mathf.Max(localMaxHeight, point.height);
        }

        spawnHeight = terrain.transform.position.y + localMinHeight * data.size.y; //Spawn height in world units

        
        float targetHeight = localMinHeight - normalizedDepth;

        foreach (var point in GetTerrainPointsInRadius(center, cellRadius))
        {
            Vector2 pointXZ = new Vector2(point.worldPos.x, point.worldPos.z);

            float dist01 = Vector2.Distance(pointXZ, centerXZ) / radius;
            float falloff = Mathf.SmoothStep(1f, 0f, dist01);

            // Interpolate deformation from current height to target height
            float newHeight = Mathf.Lerp(point.height, targetHeight, falloff);

            heights[point.z, point.x] = Mathf.Clamp01(newHeight);
        }

        data.SetHeights(0, 0, heights);
    }

    #region Serialize, Save, Load Original Terrain

    private void SaveOriginalTerrain()
    {
        SaveTerrainToJSON(originalTerrainData, SavePathOriginalTerrain);
    }

    private void SaveTerrainToJSON(TerrainData terrainToSave, string savePath)
    {
        if (terrain == null || terrain.terrainData == null)
            return;

        int res = terrainToSave.heightmapResolution;

        float[,] heights2D = terrainToSave.GetHeights(0, 0, res, res);
        float[] heights1D = new float[res * res];

        int i = 0;
        for (int z = 0; z < res; z++)
            for (int x = 0; x < res; x++)
                heights1D[i++] = heights2D[z, x];

        TerrainHeightmapSave snapshot = new TerrainHeightmapSave
        {
            resolution = res,
            heights = heights1D
        };

        string json = JsonUtility.ToJson(snapshot);
        System.IO.File.WriteAllText(savePath, json);
    }

    #endregion

    #region Serialize, Save, Load Runtime Terrain

    public void SaveRuntimeTerrain()
    {
        SaveTerrainToJSON(terrain.terrainData, SavePathRuntimeTerrain);
    }

    public bool LoadRuntimeTerrainIfExists()
    {
        if (!System.IO.File.Exists(SavePathRuntimeTerrain))
            return false;

        if (terrain == null || terrain.terrainData == null)
            return false;

        string json = System.IO.File.ReadAllText(SavePathRuntimeTerrain);
        TerrainHeightmapSave save = JsonUtility.FromJson<TerrainHeightmapSave>(json);

        TerrainData data = terrain.terrainData;

        if (data.heightmapResolution != save.resolution)
            return false;

        float[,] heights2D = new float[save.resolution, save.resolution];

        int i = 0;
        for (int z = 0; z < save.resolution; z++)
            for (int x = 0; x < save.resolution; x++)
                heights2D[z, x] = save.heights[i++];

        data.SetHeights(0, 0, heights2D);
        data.SyncHeightmap();

        return true;
    }

    #endregion

    #region Restoring Original Terrain

    private void RestoreOriginalTerrain()
    {
        if (terrain != null && originalTerrainData != null)
        {
            terrain.terrainData = originalTerrainData;
        }

        if (runtimeTerrainData != null)
        {
            DestroyImmediate(runtimeTerrainData);
            runtimeTerrainData = null;
        }
    }

    //Call when starting a new save game in build!
    public void ResetTerrainForNewGame()
    {
        RestoreOriginalTerrain();
        InitializeRuntimeTerrain();
    }

    void OnDestroy()
    {
        RestoreOriginalTerrain();
    }

    #if UNITY_EDITOR
        void OnDisable()
        {
            RestoreOriginalTerrain();
        }
    #endif

    #endregion
}
