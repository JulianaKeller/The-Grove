using UnityEngine;

public class PlantView : MonoBehaviour
{
    public Plant data;
    private float interpolationFactor = 0f;

    [Header("Ground Following")]
    public LayerMask groundLayer;
    public float raycastHeight = 10f;
    public float groundOffset = 0f;

    void Start()
    {
        SnapToGround();
    }

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

    private void SnapToGround()
    {
        Vector3 pos = transform.position;
        Vector3 origin = pos + Vector3.up * raycastHeight;

        if (Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit hit,
            raycastHeight * 2f,
            groundLayer,
            QueryTriggerInteraction.Ignore))
        {
            pos.y = hit.point.y + groundOffset;
            transform.position = pos;
        }
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
