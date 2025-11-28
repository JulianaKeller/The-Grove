using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class GridTexture : MonoBehaviour
{

    [Header("Visual Settings")]
    public bool fullTexture = true;
    public Color lineColor = Color.black;
    public Color fillColor = Color.white;
    [Range(1, 10)]
    public int lineThickness = 2;

    //[Header("Texture Settings")]


    void Start()
    {
        // Scale the plane so that it covers the grid area exactly
        float worldSize = EnvironmentGrid.Instance.gridSize * EnvironmentGrid.Instance.cellSize;
        transform.localScale = new Vector3(worldSize / 10f, 1f, worldSize / 10f);

        transform.position = new Vector3(0,0,0);

        if(fullTexture)
        {
            FullGridTexture();
        }
        else
        {
            TiledGridTexture();
        }
    }

    private void FullGridTexture()
    {
        int cellsX = EnvironmentGrid.Instance.gridSize;
        int cellsZ = EnvironmentGrid.Instance.gridSize;

        int pixelsPerCell = 1+lineThickness*2;
        int textureResolutionX = cellsX * pixelsPerCell;
        int textureResolutionZ = cellsZ * pixelsPerCell;

        Texture2D gridTex = new Texture2D(textureResolutionX, textureResolutionZ);
        gridTex.wrapMode = TextureWrapMode.Clamp;

        for (int x = 0; x < textureResolutionX; x++)
        {
            for (int z = 0; z < textureResolutionZ; z++)
            {
                bool isLine = (x % pixelsPerCell < lineThickness) || (z % pixelsPerCell < lineThickness);
                gridTex.SetPixel(x, z, isLine ? lineColor : fillColor);
            }
        }

        gridTex.Apply();

        Renderer renderer = GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        renderer.material.mainTexture = gridTex;
        renderer.material.mainTextureScale = Vector2.one;
        //renderer.material.mainTextureScale = new Vector2(EnvironmentGrid.Instance.gridSize, EnvironmentGrid.Instance.gridSize);
        renderer.material.color = Color.white;
    }

    private void TiledGridTexture()
    {
        int cellTexSize = 10; // pixels per cell
        Texture2D cellTex = new Texture2D(cellTexSize, cellTexSize);
        cellTex.wrapMode = TextureWrapMode.Repeat;

        for (int x = 0; x < cellTexSize; x++)
        {
            for (int z = 0; z < cellTexSize; z++)
            {
                bool isLine =
                    x < lineThickness ||
                    x >= cellTexSize - lineThickness ||
                    z < lineThickness ||
                    z >= cellTexSize - lineThickness;

                cellTex.SetPixel(x, z, isLine ? lineColor : fillColor);
            }
        }
        cellTex.Apply();

        Renderer renderer = GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        renderer.material.mainTexture = cellTex;

        // Tile the texture so that each cell lines up with the world grid
        float cellsPerAxis = EnvironmentGrid.Instance.gridSize; // number of cells in grid
        renderer.material.mainTextureScale = new Vector2(cellsPerAxis, cellsPerAxis);

        SetTransparent(renderer.material);
    }

    private void SetTransparent(Material mat)
    {
        // Enable transparency
        mat.SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        // Set color with transparency
        Color semiTransparent = new Color(fillColor.r, fillColor.g, fillColor.b, 0.5f);
        mat.color = semiTransparent;
    }
}
