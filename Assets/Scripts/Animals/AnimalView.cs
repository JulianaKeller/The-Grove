using UnityEngine;

public class AnimalView : EntityView
{
    public Animal data;
    private float interpolationFactor = 0f;

    [Header("Rotation")]
    float duration;
    private Quaternion prevRotation;
    private Quaternion targetRotation;

    [Header("Ground Following")]
    public bool useRaycast = true;
    public LayerMask groundLayer;
    public float raycastHeight = 10f;
    public float groundOffset = 0f;

    private Vector3 prevGroundedPosition = Vector3.zero;
    private Vector3 currGroundedPosition = Vector3.zero;

    private Vector3 prevSize;
    private Vector3 currSize;

    private void Start()
    {
        duration = WorldManager.Instance.timeStep * AnimalManager.Instance.updateSubsetCount;
    }

    void LateUpdate()
    {
        
        interpolationFactor += Time.deltaTime / duration;
        interpolationFactor = Mathf.Clamp01(interpolationFactor);

        transform.position = Vector3.Lerp(prevGroundedPosition, currGroundedPosition, interpolationFactor);

        transform.rotation = Quaternion.Slerp(prevRotation, targetRotation, interpolationFactor);

        transform.localScale = Vector3.Lerp(prevSize, currSize, interpolationFactor);
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
        prevGroundedPosition = ApplyGroundFollowing(data.lastRenderedPosition);
        currGroundedPosition = ApplyGroundFollowing(data.position);

        prevSize = data.prevSize;
        currSize = data.size;

        prevRotation = transform.rotation;
        targetRotation = Quaternion.LookRotation(data.facingDirection, Vector3.up);
    }

    public void FaceTowardsImmediate(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        direction.y = 0f;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * 100f
        );
    }

    public void FaceTowards(Vector3 lookTarget)
    {
        Vector3 dir = lookTarget - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.0001f)
            targetRotation = Quaternion.LookRotation(dir.normalized);
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

    public void SyncToCurrentVisualPosition()
    {
        Vector3 visualPos = transform.position;

        prevGroundedPosition = visualPos;
        currGroundedPosition = visualPos;

        prevRotation = transform.rotation;
        targetRotation = transform.rotation;

        interpolationFactor = 0f;
    }

}
