using UnityEngine;

public class AnimalView : EntityView
{
    public Animal data;
    private float interpolationFactor = 0f;

    [Header("Ground Following")]
    public bool useRaycast = true;
    public LayerMask groundLayer;
    public float raycastHeight = 10f;
    public float groundOffset = 0f;
    private Vector3 prevGroundedPosition;
    private Vector3 currGroundedPosition;

    void LateUpdate()
    {
        // Interpolate between previous and current positions
        Vector3 interpolated = Vector3.Lerp(
            prevGroundedPosition,
            currGroundedPosition,
            interpolationFactor
        );

        transform.position = interpolated; //ToDo local or word space?

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

    public void RecalculateGroundedPositions()
    {
        prevGroundedPosition = ApplyGroundFollowing(data.prevPosition);
        currGroundedPosition = ApplyGroundFollowing(data.position);
    }

    public void FaceTowards(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        direction.y = 0f; // ignore vertical difference

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * 100f
        );
    }

    private Vector3 ApplyGroundFollowing(Vector3 pos)
    {
        if (useRaycast)
        {
            //Use raycast on terrain layer for groundfollowing, works with terrain and meshes
            
            Vector3 rayOrigin = pos + Vector3.up * raycastHeight;

            if (Physics.Raycast(
                rayOrigin,
                Vector3.down,
                out RaycastHit hit,
                raycastHeight * 2f,
                groundLayer,
                QueryTriggerInteraction.Ignore))
            {
                pos.y = hit.point.y + groundOffset;
            }

            return pos;
        }
        else
        {
            //Use terrain height values for groundfollowing, works only for terrains

            Terrain terrain = Terrain.activeTerrain;
            Vector3 terrainPos = terrain.transform.position;
            float y = terrain.SampleHeight(pos) + terrainPos.y;
            pos.y = y + groundOffset;
        }
        return pos;
    }
}
