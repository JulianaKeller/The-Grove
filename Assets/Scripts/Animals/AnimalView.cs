using UnityEngine;

public class AnimalView : MonoBehaviour
{
    public Animal data;
    private float interpolationFactor = 0f;

    void LateUpdate()
    {
        // Interpolate between previous and current positions
        Vector3 interpolated = Vector3.Lerp(
            data.prevPosition,
            data.position,
            interpolationFactor
        );

        transform.position = interpolated;

        // Update interpolation factor relative to simulation time step
        interpolationFactor += Time.deltaTime / WorldManager.Instance.timeStep / AnimalManager.Instance.updateSubsetCount;
        interpolationFactor = Mathf.Clamp01(interpolationFactor);
    }

    public Vector3 GetInterpolatedPosition()
    {
        return Vector3.Lerp(data.prevPosition, data.position, interpolationFactor);
    }

    public void ResetInterpolation()
    {
        interpolationFactor = 0f;
    }
}
