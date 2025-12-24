using UnityEngine;

public class FollowMouseAndClamp : MonoBehaviour
{
    [Header("Mode")]
    public bool clampToTerrain = true;
    public bool alignWithSurfaceNormal = false;

    [Header("Raycast Settings")]
    public LayerMask terrainLayer;
    public float maxRayDistance = 1000f;
    public bool IsOnTerrain { get; private set; }

    [Header("Offset")]
    public float surfaceOffset = 0.1f;

    Camera cam;
    Renderer[] renderers;

    void Awake()
    {
        cam = Camera.main;
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        UpdatePosition();
    }

    void UpdatePosition()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (!clampToTerrain)
        {
            IsOnTerrain = true;
            SetVisible(true);

            transform.position = ray.origin + ray.direction * 10f;
            return;
        }

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, terrainLayer))
        {
            IsOnTerrain = true;

            SetVisible(true);

            transform.position = hit.point + hit.normal * surfaceOffset;

            if (alignWithSurfaceNormal)
            {
                transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
        }
        else
        {
            IsOnTerrain = false;

            // Hide indicator when not valid
            SetVisible(false);
        }
    }

    void SetVisible(bool visible)
    {
        foreach (var r in renderers)
            r.enabled = visible;
    }
}
