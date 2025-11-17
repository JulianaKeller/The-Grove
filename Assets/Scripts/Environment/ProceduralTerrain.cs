using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ProceduralTerrain : MonoBehaviour
{
    [Header("Terrain Settings")]
    public int size = 100;
    public float heightScale = 3f;
    public float noiseScale = 0.05f;
    public int resolution = 100;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    void Start()
    {
        GenerateTerrain();
    }

    void GenerateTerrain()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        GenerateVertices();
        GenerateTriangles();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void GenerateVertices()
    {
        vertices = new Vector3[(resolution + 1) * (resolution + 1)];

        for (int z = 0, i = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++, i++)
            {
                float worldX = (float)x / resolution * size;
                float worldZ = (float)z / resolution * size;

                float height = Mathf.PerlinNoise(worldX * noiseScale, worldZ * noiseScale);
                height *= heightScale;

                vertices[i] = new Vector3(worldX, height, worldZ);
            }
        }
    }

    void GenerateTriangles()
    {
        triangles = new int[resolution * resolution * 6];
        int vert = 0;
        int tris = 0;

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + resolution + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + resolution + 1;
                triangles[tris + 5] = vert + resolution + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + new Vector3(size / 2f, 0, size / 2f),
                            new Vector3(size, 0, size));
    }
}
