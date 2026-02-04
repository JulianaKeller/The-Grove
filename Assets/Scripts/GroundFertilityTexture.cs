using UnityEngine;

public class GroundFertilityTexture : MonoBehaviour
{
    public float updateInterval = 20f;  // Update every x timesteps
    public int texSizeMultiplier = 4;
    public int kernelRadius = 2; // 1 = 3x3 box blur

    private Texture2D fertilityTexture;
    private Renderer groundRenderer;
    private int texSize = 0;
    public Terrain terrain;

    public Color lowFertilityDry = new Color(0.7f, 0.6f, 0.2f);   // light brown
    public Color lowFertilityWet = new Color(0.5f, 0.4f, 0.2f);   // medium brown
    public Color highFertilityDry = new Color(0.5f, 0.9f, 0.5f);  // light green
    public Color highFertilityWet = new Color(0.3f, 0.7f, 0.3f);  // medium lush green

    void Start()
    {
        if(terrain == null)
            terrain = GetComponent<Terrain>();

        //groundRenderer = GetComponent<Renderer>();

        texSize = EnvironmentGrid.Instance.gridSize * texSizeMultiplier;
        fertilityTexture = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
        fertilityTexture.wrapMode = TextureWrapMode.Clamp;

        UpdateTexture();
        ApplyTextureToTerrain();
        
        //ALT
        /*
        groundRenderer.material = new Material(Shader.Find("Toon"));
        groundRenderer.material.mainTexture = fertilityTexture;
        groundRenderer.material.mainTextureScale = new Vector2(1,1);
        groundRenderer.material.color = Color.white;
        */
    }

    public void UpdateFertilityTexture()
    {
        UpdateTexture();
    }

    private void UpdateTexture()
    {
        var grid = EnvironmentGrid.Instance.grid;
        int gridSize = EnvironmentGrid.Instance.gridSize;

        for (int z = 0; z < gridSize; z++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                float fertility = grid[x, z].fertility;
                float moisture = grid[x, z].moisture;

                // Interpolate between dry and wet based on moisture
                Color lowFertilityColor = Color.Lerp(lowFertilityDry, lowFertilityWet, moisture);
                Color highFertilityColor = Color.Lerp(highFertilityDry, highFertilityWet, moisture);

                // Interpolate between low and high fertility based on fertility
                Color finalColor = Color.Lerp(lowFertilityColor, highFertilityColor, fertility);

                // Fill texSizeMultiplier × texSizeMultiplier block of pixels
                for (int dz = 0; dz < texSizeMultiplier; dz++)
                {
                    for (int dx = 0; dx < texSizeMultiplier; dx++)
                    {
                        int px = x * texSizeMultiplier + dx;
                        int pz = z * texSizeMultiplier + dz;

                        // flip Z for Unity texture coordinates
                        fertilityTexture.SetPixel(texSize -1 - px, texSize - 1 - pz, finalColor);
                    }
                }
            }
        }

        // Apply blur for smooth blending
        SmoothTexture(fertilityTexture);
        fertilityTexture.Apply();

        //ALT
        //fertilityTexture.Apply();
        //groundRenderer.material.mainTexture = fertilityTexture;

        ApplyTextureToTerrain();
    }

    private void SmoothTexture(Texture2D tex)
    {
        int w = tex.width;
        int h = tex.height;
        Color[] pixels = tex.GetPixels();
        Color[] blurred = new Color[pixels.Length];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color sum = Color.black;
                int count = 0;
                for (int ky = -kernelRadius; ky <= kernelRadius; ky++)
                {
                    for (int kx = -kernelRadius; kx <= kernelRadius; kx++)
                    {
                        int sx = Mathf.Clamp(x + kx, 0, w - 1);
                        int sy = Mathf.Clamp(y + ky, 0, h - 1);
                        sum += pixels[sy * w + sx];
                        count++;
                    }
                }
                blurred[y * w + x] = sum / count;
            }
        }

        tex.SetPixels(blurred);
    }

    private void ApplyTextureToTerrain()
    {
        TerrainData terrainData = terrain.terrainData;

        int terrainWidth = terrainData.alphamapWidth;
        int terrainHeight = terrainData.alphamapHeight;
        int numLayers = terrainData.alphamapLayers;

        if (numLayers < 4)
        {
            Debug.LogError("Terrain must have at least 4 layers for lowDry, lowWet, highDry, highWet.");
            return;
        }

        float[,,] alphamaps = new float[terrainHeight, terrainWidth, numLayers];

        // Get pixels from fertilityTexture (resize to match terrain alpha map)
        Texture2D resizedTex = ResizeTexture(fertilityTexture, terrainWidth, terrainHeight);
        Color[] pixels = resizedTex.GetPixels();

        for (int y = 0; y < terrainHeight; y++)
        {
            for (int x = 0; x < terrainWidth; x++)
            {
                Color c = pixels[y * terrainWidth + x];

                // Compute contribution for each layer
                float lowDry = (1 - c.g) * (1 - c.b); // roughly map dryness and fertility
                float lowWet = (1 - c.g) * c.b;
                float highDry = c.g * (1 - c.b);
                float highWet = c.g * c.b;

                // Normalize
                float sum = lowDry + lowWet + highDry + highWet;
                if (sum > 0)
                {
                    lowDry /= sum;
                    lowWet /= sum;
                    highDry /= sum;
                    highWet /= sum;
                }

                alphamaps[y, x, 0] = lowDry;
                alphamaps[y, x, 1] = lowWet;
                alphamaps[y, x, 2] = highDry;
                alphamaps[y, x, 3] = highWet;
            }
        }

        terrainData.SetAlphamaps(0, 0, alphamaps);
    }

    private Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        Graphics.Blit(source, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D newTex = new Texture2D(newWidth, newHeight);
        newTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        newTex.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        return newTex;
    }
}
