using System.Collections.Generic;
using UnityEngine;

public class TerrainDeformer : MonoBehaviour
{
    public static TerrainDeformer Instance { get; private set; }

    [SerializeField] private LayerMask terrainLayer;

    public Terrain terrain;

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

    public struct TerrainPoint
    {
        public Vector3 worldPos;
        public int x;
        public int z;
        public float height;
    }

    public IEnumerable<TerrainPoint> GetTerrainPointsInRadius(Vector3 center, float radius)
    {
        if(terrain == null)
            terrain = Terrain.activeTerrain;
        if (terrain == null)
            yield break;

        TerrainData data = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        int heightmapWidth = data.heightmapResolution;
        int heightmapHeight = data.heightmapResolution;

        float[,] heights = data.GetHeights(0, 0, heightmapWidth, heightmapHeight);

        Vector3 size = data.size;

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

                if (Vector3.Distance(worldPos, center) <= radius)
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

    public void LowerTerrain(Vector3 center, float radius, float depth, float irregularity)
    {
        if(terrain == null)
        {
            terrain = Terrain.activeTerrain;
        }
        if (terrain == null)
        {
            return;
        }

        TerrainData data = terrain.terrainData;

        float[,] heights = data.GetHeights(0, 0, data.heightmapResolution, data.heightmapResolution);

        foreach (var point in GetTerrainPointsInRadius(center, radius))
        {
            float dist01 = Vector3.Distance(point.worldPos, center) / radius;
            float noise = Mathf.PerlinNoise(point.x * 0.3f, point.z * 0.3f);
            float falloff = Mathf.SmoothStep(1f, 0f, dist01);

            float deformation = depth * falloff * Mathf.Lerp(1f, noise, irregularity);

            heights[point.z, point.x] = Mathf.Max(0f, heights[point.z, point.x] - deformation / data.size.y);
        }

        data.SetHeights(0, 0, heights);
    }

}
