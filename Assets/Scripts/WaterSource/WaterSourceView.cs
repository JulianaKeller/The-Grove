using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class WaterSourceView : MonoBehaviour
{
    public GameObject waterSurfacePlane;
    public WaterSource data;

    //private float interpolationFactor = 0f;
    public float lerpSpeed = 0.5f;

    public float maxWaterHeight = 1f;
    public float minWaterHeight = 1f;

    private float initialHeight;
    private float terrainHeightAtCenter;

    private void Start()
    {
        initialHeight = waterSurfacePlane.transform.position.y;
        maxWaterHeight = initialHeight - 2f;
        terrainHeightAtCenter = Terrain.activeTerrain.SampleHeight(waterSurfacePlane.transform.position) + Terrain.activeTerrain.transform.position.y;
        minWaterHeight = terrainHeightAtCenter - 3f;

        Vector3 currentPos = waterSurfacePlane.transform.position;
        waterSurfacePlane.transform.position = new Vector3(currentPos.x, maxWaterHeight, currentPos.z);
    }

    void LateUpdate()
    {
        if (waterSurfacePlane == null || data == null) return;

        float fill01 = Mathf.Clamp01(data.currentWater / data.capacity);
        float targetY = Mathf.Lerp(minWaterHeight, maxWaterHeight, fill01);

        Vector3 currentPos = waterSurfacePlane.transform.position;
        waterSurfacePlane.transform.position = Vector3.Lerp(currentPos, new Vector3(currentPos.x, targetY, currentPos.z), Time.deltaTime * lerpSpeed);
    }

    /*public void ResetInterpolation()
    {
        interpolationFactor = 0f;
    }*/
}
