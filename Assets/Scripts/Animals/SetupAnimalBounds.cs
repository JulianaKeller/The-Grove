using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SetupAnimalBounds : MonoBehaviour
{
    [Header("References")]
    public Transform modelRoot;          // child containing renderers
    public Transform thoughtBubble;      // world-space canvas root

    [Header("Offsets")]
    public float colliderPadding = 0.05f;
    public float thoughtBubbleOffset = 0.15f;

    private BoxCollider boxCollider;
    private Camera mainCamera;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        mainCamera = Camera.main;

        modelRoot = transform;

        RecalculateBounds();
    }

    void LateUpdate()
    {
        FaceCamera();
    }

    public void RecalculateBounds()
    {
        Renderer[] renderers = modelRoot.GetComponentsInChildren<SkinnedMeshRenderer>();

        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        //Convert world bounds to local space
        Vector3 localCenter = transform.InverseTransformPoint(bounds.center);

        Vector3 localSize = transform.InverseTransformVector(bounds.size) + Vector3.one * colliderPadding;

        boxCollider.center = localCenter;
        boxCollider.size = localSize;

        PositionThoughtBubble(bounds);
    }

    void PositionThoughtBubble(Bounds worldBounds)
    {
        if (thoughtBubble == null)
            return;

        Vector3 top = worldBounds.center + Vector3.up * worldBounds.extents.y;
        thoughtBubble.position = top + Vector3.up * thoughtBubbleOffset;
    }

    void FaceCamera()
    {
        if (thoughtBubble == null || mainCamera == null)
            return;

        thoughtBubble.forward = mainCamera.transform.forward;
    }
}
