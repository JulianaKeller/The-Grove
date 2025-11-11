using UnityEngine;

public class PlantView : MonoBehaviour
{
    public Plant data;
    private float interpolationFactor = 0f;

    void LateUpdate()
    {
        Vector3 interpolatedScale = Vector3.Lerp(
            data.prevSize,
            data.size,
            interpolationFactor
        );

        transform.localScale = interpolatedScale;

        // Update interpolation factor based on simulation timing
        interpolationFactor += Time.deltaTime / WorldManager.Instance.timeStep / PlantManager.Instance.updateSubsetCount;
        interpolationFactor = Mathf.Clamp01(interpolationFactor);
    }

    private Vector3 ClampScale(Vector3 scale)
    {
        float minScale = 0.01f;
        float maxScale = 50f;
        scale.x = Mathf.Clamp(scale.x, minScale, data.maxSize.x * maxScale);
        scale.y = Mathf.Clamp(scale.y, minScale, data.maxSize.y * maxScale);
        scale.z = Mathf.Clamp(scale.z, minScale, data.maxSize.z * maxScale);
        return scale;
    }

    public void ResetInterpolation()
    {
        interpolationFactor = 0f;
    }
}
