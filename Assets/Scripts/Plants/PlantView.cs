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

    public void ResetInterpolation()
    {
        interpolationFactor = 0f;
    }
}
