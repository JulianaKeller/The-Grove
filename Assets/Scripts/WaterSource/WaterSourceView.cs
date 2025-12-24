using UnityEngine;

public class WaterSourceView : MonoBehaviour
{
    public GameObject waterSurfacePlane;

    public WaterSource data;
    private float interpolationFactor = 0f;

    private float prevHeight;
    private float newHeight;

    void LateUpdate()
    {
        float fill01 = Mathf.Clamp01(data.currentWater / data.capacity);

        prevHeight = newHeight;
        newHeight = Mathf.Lerp(-WaterSourceManager.Instance.depth, 0f, fill01);

        float y = Mathf.Lerp(prevHeight, newHeight, interpolationFactor);
        waterSurfacePlane.transform.localPosition = new Vector3(0f, y, 0f);

        // Update interpolation factor based on simulation timing
        interpolationFactor += Time.deltaTime / WorldManager.Instance.timeStep / PlantManager.Instance.updateSubsetCount;
        interpolationFactor = Mathf.Clamp01(interpolationFactor);
    }

    public void ResetInterpolation()
    {
        interpolationFactor = 0f;
    }
}
