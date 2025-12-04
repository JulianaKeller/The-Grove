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

    [Header("Focus Mode")]
    public bool inFocusMode = false;
    public Transform focusTarget;
    public float focusDistance = 6f;
    public float orbitSensitivity = 3f;
    public float zoomExitThreshold = 25f;

    private float currentAngleY = 0f;
    private float currentAngleX = 20f;
    public float currentFocusDistance = 6f;

    [Header("Focus Transition")]
    public float focusTransitionDuration = 0.7f;
    private bool focusing = false;
    private float focusTransitionT = 0f;

    private Vector3 focusStartPos;
    private Quaternion focusStartRot;
    private float focusStartDistance;

    private bool leavingFocus = false;
    private float leaveTransitionT = 0f;

    public float leaveTransitionDuration = 0.6f;

    private Vector3 leaveStartPos;
    private Quaternion leaveStartRot;
    private float leaveStartDistance;

    private bool transitionInputLocked = false;
    private Vector3 focusTransitionAnchor;


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
        if (focusing)
        {
            HandleFocusTransition();
            UpdateDepthOfField();
            return;
        }

        if (leavingFocus)
        {
            HandleLeaveFocusTransition();
            UpdateDepthOfField();
            return;
        }

        if (inFocusMode)
        {
            HandleFocusMode();
            UpdateDepthOfField();
            return;
        }

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

    public void ToggleFocusMode(Transform target)
    {
        // If already in a transition for the same target, cancel (explicit user intent)
        if (focusing || leavingFocus)
        {
            // only cancel if target matches or target is null (user forced cancel)
            if (target == focusTarget || target == null)
            {
                CancelTransitions();
            }
            return;
        }

        if (inFocusMode)
            BeginLeaveFocus();
        else
            EnterFocusMode(target);
    }

    public void ExitFocusModeRequest()
    {
        if (focusing || leavingFocus)
        {
            CancelTransitions();
            return;
        }

        if (inFocusMode)
            BeginLeaveFocus();
    }

    void HandleLeaveFocusTransition()
    {
        leaveTransitionT += Time.deltaTime / leaveTransitionDuration;
        float t = Mathf.SmoothStep(0f, 1f, leaveTransitionT);

        transform.position = Vector3.Lerp(leaveStartPos, targetPosition, t);
        transform.rotation = Quaternion.Slerp(leaveStartRot, Quaternion.Euler(tiltAngle, transform.rotation.eulerAngles.y, 0f), t);

        if (t >= 1f)
        {
            leavingFocus = false;
            inFocusMode = false;
        }
    }

    private void CancelTransitions()
    {
        focusing = false;
        leavingFocus = false;
        inFocusMode = false;
    }

    void HandleFocusMode()
    {
        if (focusTarget == null)
        {
            ExitFocusMode();
            return;
        }

        // Orbit with middle mouse, but only after transitions are unlocked
        if (!transitionInputLocked)
        {
            if (Input.GetMouseButton(2))
            {
                Vector3 delta = Input.mousePosition - lastMousePos;
                currentAngleY += delta.x * orbitSensitivity;
                currentAngleX = Mathf.Clamp(currentAngleX - delta.y * orbitSensitivity, 5f, 80f);
            }
            lastMousePos = Input.mousePosition;

            // Zoom in/out
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0005f)
                currentFocusDistance = Mathf.Clamp(currentFocusDistance - scroll * zoomSpeed * 0.5f, minDistance, maxDistance);
        }

        // Exit when zoomed too far (only when actually in focus mode and not transitioning)
        if (!transitionInputLocked && currentFocusDistance > zoomExitThreshold && !leavingFocus && !focusing)
        {
            BeginLeaveFocus();
            return;
        }

        Quaternion rot = Quaternion.Euler(currentAngleX, currentAngleY, 0f);
        Vector3 offset = rot * (Vector3.back * currentFocusDistance);

        targetPosition = focusTarget.position + offset;

        transform.rotation = rot;
        ApplyCameraBounds();
        ApplySmoothing();
    }

    void HandleFocusTransition()
    {
        if (focusTarget == null)
        {
            focusing = false;
            transitionInputLocked = false;
            return;
        }

        focusTransitionT += Time.deltaTime / focusTransitionDuration;
        float t = Mathf.SmoothStep(0f, 1f, focusTransitionT);

        float dist = Mathf.Lerp(focusStartDistance, currentFocusDistance, t);

        Quaternion targetRot = Quaternion.Euler(currentAngleX, currentAngleY, 0f);
        Vector3 targetPos = focusTarget.position - targetRot * Vector3.forward * dist;

        transform.position = Vector3.Lerp(focusStartPos, targetPos, t);
        transform.rotation = Quaternion.Slerp(focusStartRot, targetRot, t);

        if (t >= 1f)
        {
            focusing = false;
            inFocusMode = true;
            transitionInputLocked = false;
            // ensure lastMousePos is reset so first orbit frame isn't a big jump
            lastMousePos = Input.mousePosition;
        }
    }

    public void EnterFocusMode(Transform target)
    {
        if (target == null) return;

        inFocusMode = false; // stays false until transition completes
        focusing = true;
        leavingFocus = false;
        focusTarget = target;

        // Capture starting state
        focusStartPos = transform.position;
        focusStartRot = transform.rotation;
        

        // Prepare target state
        Vector3 dir = (transform.position - target.position).normalized;
        currentAngleY = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        currentAngleX = tiltAngle;

        float scaledFocusDistance = focusDistance;

        var rend = target.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            float sizeFactor = rend.bounds.extents.magnitude;
            scaledFocusDistance = sizeFactor * 2.0f;
        }

        currentFocusDistance = Mathf.Clamp(scaledFocusDistance, minDistance, maxDistance);

        focusTransitionT = 0f; // reset blend

        transitionInputLocked = true;
        lastMousePos = Input.mousePosition;
    }

    private void BeginLeaveFocus()
    {
        if (focusTarget == null) // defensive
        {
            inFocusMode = false;
            leavingFocus = false;
            return;
        }

        leavingFocus = true;
        focusing = false;
        transitionInputLocked = true;

        leaveStartPos = transform.position;
        leaveStartRot = transform.rotation;

        leaveStartDistance = Vector3.Distance(transform.position, focusTarget.position);

        Vector3 dir = new Vector3(0, 1, -1).normalized;
        float dist = Mathf.Clamp(leaveStartDistance * 2f, minDistance, maxDistance);

        targetPosition = focusTarget.position + dir * dist;

        leaveTransitionT = 0f;
    }

    public void ExitFocusMode()
    {
        inFocusMode = false;
        focusTarget = null;
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
