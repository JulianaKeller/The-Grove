using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;

[ExecuteAlways]
public class PrefabDistributor : MonoBehaviour
{
    [Header("Prefabs")]
    public List<GameObject> prefabs = new List<GameObject>();

    [Header("Distribution Settings")]
    public bool clearPreviouslySpawnedObjects = true;
    public int spawnCount = 10;

    [Header("Placement Settings")]
    public Vector3 minPosition = Vector3.zero;
    public Vector3 maxPosition = Vector3.one * 10f;

    public Terrain terrain; // Optional terrain reference
    public List<GameObject> surfaceObjects = new List<GameObject>();

    [Header("Randomization")]
    public Vector2 scaleRange = Vector2.one;
    public bool randomRotation = true;

    [Header("Distribution")]
    public int maxAttempts = 25;
    public float clumpingSize = 10f;
    [Range(0f, 1f)]
    public float clumpingAmount = 0.25f;
    public Texture2D densityMap; // Optional black/white texture for density control

    [Header("Spacing")]
    public float minimumDistance = 1.0f;

    [Header("Gizmos")]
    public bool shadeBoxFaces = true;

    [Header("Internal")]
    [SerializeField] public List<GameObject> spawnedObjects = new List<GameObject>();

    /// <summary>
    /// Clears previously spawned objects
    /// </summary>
    public void ClearSpawned()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
                DestroyImmediate(spawnedObjects[i]);
        }
        spawnedObjects.Clear();
    }

    private void OnValidate()
    {
        spawnedObjects.Clear();
        foreach (Transform child in transform)
            spawnedObjects.Add(child.gameObject);
    }

    /// <summary>
    /// Main distribution function
    /// </summary>
    public void Distribute()
    {
        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogWarning("No prefabs assigned for distribution.");
            return;
        }

        if (clearPreviouslySpawnedObjects)
        {
            ClearSpawned();
        }

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];

            Vector3 position = GetRandomPosition();

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, this.transform);
            instance.transform.localPosition = position;

            // Apply random rotation
            if (randomRotation)
                instance.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            // Apply random scale
            float scale = Random.Range(scaleRange.x, scaleRange.y);
            instance.transform.localScale = Vector3.one * scale;

            spawnedObjects.Add(instance);
        }
    }

    private Vector3 GetRandomPosition()
    {
        Vector3 pos;

        // Weighted distribution clumping
        if (spawnedObjects.Count > 0 && spawnedObjects.Count > spawnCount * (1- clumpingAmount))
        {
            pos = WeightedDistribution();
        }
        else
        {
            pos = RandomDistribution();
        }

        float highestY;
        if (terrain != null || (surfaceObjects != null && surfaceObjects.Count > 0))
        {
            highestY = 0;
        }
        else
        {
            highestY = pos.y;
        }

        if (terrain != null)
        {
            Vector3 worldPos = transform.TransformPoint(pos);

            float terrainHeight = terrain.SampleHeight(worldPos);

            if (terrainHeight > highestY) highestY = terrainHeight;
        }
        if (surfaceObjects != null && surfaceObjects.Count > 0)
        {
            Vector3 worldPos = transform.TransformPoint(pos);
            RaycastHit hit;

            foreach (var surface in surfaceObjects)
            {
                // Make sure surface has a collider
                Collider col = surface.GetComponent<Collider>();
                if (col == null) continue;

                // Cast a ray from above
                Ray ray = new Ray(new Vector3(worldPos.x, maxPosition.y + 10f, worldPos.z), Vector3.down);
                if (col.Raycast(ray, out hit, 100f))
                {
                    if (hit.point.y > highestY)
                        highestY = hit.point.y;
                }
            }
        }

        Vector3 finalWorldPos = transform.TransformPoint(pos);
        finalWorldPos.y = highestY;
        pos = transform.InverseTransformPoint(finalWorldPos);

        // Density map adjustment
        if (densityMap != null)
        {
            float uvX = Mathf.InverseLerp(minPosition.x, maxPosition.x, pos.x);
            float uvZ = Mathf.InverseLerp(minPosition.z, maxPosition.z, pos.z);
            Color pixel = densityMap.GetPixelBilinear(uvX, uvZ);
            float probability = pixel.grayscale;

            if (Random.value > probability)
                pos = GetRandomPosition(); // recursive retry
        }

        return pos;
    }

    private Vector3 WeightedDistribution()
    {
        Vector3 pos = transform.TransformPoint((minPosition + maxPosition) * 0.5f); ;

        int attempts = 0;
        bool valid = false;

        while (!valid && attempts < maxAttempts)
        {
            attempts++;

            GameObject referenceObj = spawnedObjects[Random.Range(0, spawnedObjects.Count)];

            Vector3 dir = Random.onUnitSphere;

            // Distance using Gaussian distribution (multiple small clumps naturally emerge)
            float distance = Mathf.Abs(NormalRandom()) * clumpingSize;

            pos = referenceObj.transform.localPosition + dir * distance;

            bool insideBounds = pos.x >= minPosition.x && pos.x <= maxPosition.x &&
                pos.y >= minPosition.y && pos.y <= maxPosition.y &&
                pos.z >= minPosition.z && pos.z <= maxPosition.z;

            // Test if candidate is inside the distribution bounds
            if (insideBounds && !IsTooClose(pos))
            {
                valid = true;
            }
        }

        if (!valid)
        {
            pos = RandomDistribution();
        }

        return pos;
    }

    private Vector3 RandomDistribution()
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 candidate = new Vector3(
                Random.Range(minPosition.x, maxPosition.x),
                Random.Range(minPosition.y, maxPosition.y),
                Random.Range(minPosition.z, maxPosition.z)
            );

            if (!IsTooClose(candidate))
                return candidate;
        }

        return new Vector3(
                Random.Range(minPosition.x, maxPosition.x),
                Random.Range(minPosition.y, maxPosition.y),
                Random.Range(minPosition.z, maxPosition.z)
            );
    }

    private float NormalRandom()
    {
        float u1 = 1f - Random.value;
        float u2 = 1f - Random.value;
        return Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Sin(2f * Mathf.PI * u2);
    }

    private bool IsTooClose(Vector3 candidate)
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            Vector3 other = spawnedObjects[i].transform.localPosition;
            if (Vector3.SqrMagnitude(candidate - other) < minimumDistance * minimumDistance)
                return true;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 size = new Vector3(
            Mathf.Abs(maxPosition.x - minPosition.x),
            Mathf.Abs(maxPosition.y - minPosition.y),
            Mathf.Abs(maxPosition.z - minPosition.z)
        );

        Vector3 center = transform.TransformPoint((minPosition + maxPosition) * 0.5f);

        // Draw the wireframe cube
        Gizmos.DrawWireCube(center, size);

        // Optionally draw a faint transparent fill
        if (shadeBoxFaces)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
            Gizmos.DrawCube(center, size);
        }
    }
}
