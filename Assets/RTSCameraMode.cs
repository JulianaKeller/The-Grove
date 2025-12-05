using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static CameraController;

public class RTSCameraMode : MonoBehaviour
{
    public enum ZoomMode
    {
        Forward,
        Vertical,
        PivotUnderMouse
    }

    private CameraController cameraController;
    private Camera cam;

    [Header("Zoom Mode")]
    public ZoomMode zoomMode = ZoomMode.Forward;

    [Header("Rotation")]
    [Range(10f, 80f)] public float tiltAngle = 30;
    public float rtsRotateSpeed = 5f;

    [Header("Movement")]
    public float moveSpeed = 20f;
    public float edgeScrollSpeed = 20f;
    public int edgeThickness = 20;
    public bool edgeScrolling = true;

    [Header("Middle Mouse Panning")]
    public float panSpeed = 0.5f;

    [Header("Zoom Distance")]
    public float zoomSpeed = 50f;
    public float minDistance = 10f;
    public float maxDistance = 80f;


    private void Start()
    {
        cam = Camera.main;

        cameraController = transform.GetComponent<CameraController>();

        cameraController.targetPosition = transform.position;

        transform.rotation = Quaternion.Euler(tiltAngle, transform.rotation.eulerAngles.y, 0f);
    }

    public void updateRTSCameraMode()
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
        cameraController.ApplyCameraBounds();
        cameraController.ApplySmoothing();
        UpdateDepthOfField();
    }

    private void UpdateDepthOfField()
    {
        if (cameraController.dof == null) return;

        float distance = GetCameraDistanceToGround();
        float rt = Mathf.InverseLerp(maxDistance, minDistance, distance);
        float rFocus = Mathf.Lerp(cameraController.dofMinFocus, cameraController.dofMaxFocus, rt);

        cameraController.dof.focusDistance.Override(rFocus);
    }

    public float GetCameraDistanceToGround()
    {
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        Ray ray = new Ray(transform.position, Vector3.down);
        if (ground.Raycast(ray, out float hitDist))
            return hitDist;
        return transform.position.y;
    }

    void HandleKeyboardMovement(float adaptiveSpeed)
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = (GetForward() * v + GetRight() * h).normalized * adaptiveSpeed * Time.deltaTime;
        cameraController.targetPosition += move;
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

        cameraController.targetPosition += move.normalized * adaptiveSpeed * Time.deltaTime;
    }

    void HandleMiddleMousePan(float adaptiveSpeed)
    {
        if (Input.GetMouseButton(2) && !Input.GetKey(KeyCode.LeftShift))
        {
            // --- ROTATE RTS CAMERA ---
            Vector3 delta = Input.mousePosition - cameraController.lastMousePos;
            cameraController.lastMousePos = Input.mousePosition;

            float yaw = delta.x * rtsRotateSpeed;

            // Rotate around Y only
            transform.rotation = Quaternion.Euler(
                tiltAngle,
                transform.rotation.eulerAngles.y + yaw,
                0f
            );

            // do NOT treat this as panning
            return;
        }

        // --- MIDDLE MOUSE PANNING (Shift held) ---
        if (Input.GetMouseButtonDown(2))
            cameraController.lastMousePos = Input.mousePosition;

        if (Input.GetMouseButton(2) && Input.GetKey(KeyCode.LeftShift))
        {
            Vector3 delta = Input.mousePosition - cameraController.lastMousePos;
            cameraController.lastMousePos = Input.mousePosition;

            cameraController.targetPosition -= (GetRight() * delta.x + GetForward() * delta.y) * adaptiveSpeed * Time.deltaTime;
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

    void ZoomForward(float delta)
    {
        cameraController.targetPosition += transform.forward * delta;
        cameraController.targetPosition.y = Mathf.Clamp(cameraController.targetPosition.y, minDistance, maxDistance);
    }

    void ZoomVertical(float delta)
    {
        cameraController.targetPosition += Vector3.up * delta;
        cameraController.targetPosition.y = Mathf.Clamp(cameraController.targetPosition.y, minDistance, maxDistance);
    }

    void ZoomPivotUnderMouse(float delta)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, Vector3.zero);

        if (ground.Raycast(ray, out float hit))
        {
            Vector3 pivot = ray.GetPoint(hit);

            Vector3 dir = (transform.position - pivot).normalized;
            cameraController.targetPosition += dir * delta;
            cameraController.targetPosition.y = Mathf.Clamp(cameraController.targetPosition.y, minDistance, maxDistance);
        }
        else
        {
            ZoomForward(delta);
        }
    }
}
