using UnityEngine;

public class GroundFertilityTexture : MonoBehaviour
{
    public float updateInterval = 10f;  // Update every x timesteps
    public int texSizeMultiplier = 4;

    private Texture2D fertilityTexture;
    private Renderer groundRenderer;
    private int texSize = 0;

    public Color lowFertilityDry = new Color(0.7f, 0.6f, 0.2f);   // light brown
    public Color lowFertilityWet = new Color(0.5f, 0.4f, 0.2f);   // medium brown
    public Color highFertilityDry = new Color(0.5f, 0.9f, 0.5f);  // light green
    public Color highFertilityWet = new Color(0.3f, 0.7f, 0.3f);  // medium lush green

    void Start()
    {
        groundRenderer = GetComponent<Renderer>();

        texSize = EnvironmentGrid.Instance.gridSize * texSizeMultiplier;
        fertilityTexture = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
        fertilityTexture.wrapMode = TextureWrapMode.Clamp;

        UpdateTexture();
        groundRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        groundRenderer.material.mainTexture = fertilityTexture;
        groundRenderer.material.mainTextureScale = new Vector2(1,1);
        groundRenderer.material.color = Color.white;
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
                        // ALT fertilityTexture.SetPixel(gridSize - 1 - x, gridSize - 1 - z, finalColor);
                    }
                }
            }
        }

        // Apply blur for smooth blending
        SmoothTexture(fertilityTexture);
        fertilityTexture.Apply();

        groundRenderer.material.mainTexture = fertilityTexture;
    }

    private void SmoothTexture(Texture2D tex)
    {
        int w = tex.width;
        int h = tex.height;
        Color[] pixels = tex.GetPixels();
        Color[] blurred = new Color[pixels.Length];

        int kernelRadius = 2; // 1 = 3x3 box blur
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
}
