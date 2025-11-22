using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraController : MonoBehaviour
{
    public enum ZoomMode
    {
        Forward,
        Vertical,
        PivotUnderMouse
    }

    [Header("Zoom Mode")]
    public ZoomMode zoomMode = ZoomMode.Forward;

    [Header("Rotation")]
    [Range(10f, 80f)] public float tiltAngle = 30;

    [Header("Movement")]
    public float moveSpeed = 20f;
    public float edgeScrollSpeed = 20f;
    public int edgeThickness = 20;
    public bool edgeScrolling = true;

    [Header("Smoothing")]
    public float smoothTime = 0.15f;
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition;

    [Header("Middle Mouse Panning")]
    public float panSpeed = 0.5f;
    private Vector3 lastMousePos;

    [Header("Zoom Distance")]
    public float zoomSpeed = 50f;
    public float minDistance = 10f;
    public float maxDistance = 80f;

    [Header("Depth Of Field")]
    public Volume volume;
    public float dofMinFocus = 3f;
    public float dofMaxFocus = 30f;

    private DepthOfField dof;
    private Camera cam;

    private float halfSize;


    void Start()
    {
        cam = Camera.main;
        transform.rotation = Quaternion.Euler(tiltAngle, transform.rotation.eulerAngles.y, 0f);

        float gridSize = EnvironmentGrid.Instance.gridSize * EnvironmentGrid.Instance.cellSize;
        halfSize = gridSize / 2f;

        targetPosition = transform.position;

        if (volume != null && volume.profile.TryGet(out DepthOfField depthOfField))
            dof = depthOfField;
    }


    void Update()
    {
        float distance = GetCameraDistanceToGround();
        float distanceFactor = Mathf.InverseLerp(minDistance, maxDistance, distance); // 0 = zoomed in, 1 = zoomed out

        float adaptiveMoveSpeed = moveSpeed * Mathf.Lerp(0.5f, 2f, distanceFactor);
        float adaptiveEdgeSpeed = edgeScrollSpeed * Mathf.Lerp(0.5f, 2f, distanceFactor);
        float adaptivePanSpeed = panSpeed * Mathf.Lerp(0.5f, 2f, distanceFactor);
        float adaptiveZoomSpeed = zoomSpeed * Mathf.Lerp(0.5f, 2f, distanceFactor);

        HandleKeyboardMovement(adaptiveMoveSpeed);
        HandleEdgeScrolling(adaptiveEdgeSpeed);
        HandleMiddleMousePan(adaptivePanSpeed);
        HandleZoom(adaptiveZoomSpeed);
        ApplyCameraBounds();
        ApplySmoothing();
        UpdateDepthOfField();
    }

    void HandleKeyboardMovement(float adaptiveSpeed)
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = (GetForward() * v + GetRight() * h).normalized * adaptiveSpeed * Time.deltaTime;
        targetPosition += move;
    }

    void HandleEdgeScrolling(float adaptiveSpeed)
    {
        if (!edgeScrolling) return;

        Vector3 move = Vector3.zero;

        Vector3 mouse = Input.mousePosition;

        if (mouse.x >= Screen.width - edgeThickness) move += GetRight();
        else if (mouse.x <= edgeThickness) move -= GetRight();

        if (mouse.y >= Screen.height - edgeThickness) move += GetForward();
        else if (mouse.y <= edgeThickness) move -= GetForward();

        targetPosition += move.normalized * adaptiveSpeed * Time.deltaTime;
    }

    void HandleMiddleMousePan(float adaptiveSpeed)
    {
        if (Input.GetMouseButtonDown(2))
            lastMousePos = Input.mousePosition;

        if (Input.GetMouseButton(2))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            lastMousePos = Input.mousePosition;

            targetPosition -= (GetRight() * delta.x + GetForward() * delta.y) * adaptiveSpeed * Time.deltaTime;
        }
    }

    private Vector3 GetForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();
        return forward;
    }

    private Vector3 GetRight()
    {
        Vector3 right = transform.right;
        right.y = 0;
        right.Normalize();
        return right;
    }

    void HandleZoom(float adaptiveSpeed)
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.001f) return;

        float distance = GetCameraDistanceToGround();

        float targetDistance = Mathf.Clamp(distance - scroll * adaptiveSpeed, minDistance, maxDistance);
        float delta = targetDistance - distance;

        switch (zoomMode)
        {
            case ZoomMode.Forward:
                ZoomForward(delta);
                break;

            case ZoomMode.Vertical:
                ZoomVertical(delta);
                break;

            case ZoomMode.PivotUnderMouse:
                ZoomPivotUnderMouse(delta);
                break;
        }
    }

    float GetCameraDistanceToGround()
    {
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        Ray ray = new Ray(transform.position, Vector3.down);
        if (ground.Raycast(ray, out float hitDist))
            return hitDist;
        return transform.position.y;
    }

    void ZoomForward(float delta)
    {
        targetPosition += transform.forward * delta;
        targetPosition.y = Mathf.Clamp(targetPosition.y, minDistance, maxDistance);
    }

    void ZoomVertical(float delta)
    {
        targetPosition += Vector3.up * delta;
        targetPosition.y = Mathf.Clamp(targetPosition.y, minDistance, maxDistance);
    }

    void ZoomPivotUnderMouse(float delta)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, Vector3.zero);

        if (ground.Raycast(ray, out float hit))
        {
            Vector3 pivot = ray.GetPoint(hit);

            Vector3 dir = (transform.position - pivot).normalized;
            targetPosition += dir * delta;
            targetPosition.y = Mathf.Clamp(targetPosition.y, minDistance, maxDistance);
        }
        else
        {
            ZoomForward(delta);
        }
    }

    void ApplyCameraBounds()
    {
        targetPosition.x = Mathf.Clamp(targetPosition.x, -halfSize, halfSize);
        targetPosition.z = Mathf.Clamp(targetPosition.z, -halfSize, halfSize);
    }

    void ApplySmoothing()
    {
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    void UpdateDepthOfField()
    {
        if (dof == null) return;

        float distance = GetCameraDistanceToGround();
        float t = Mathf.InverseLerp(maxDistance, minDistance, distance);
        float focus = Mathf.Lerp(dofMinFocus, dofMaxFocus, t);

        dof.focusDistance.Override(focus);
    }
}
