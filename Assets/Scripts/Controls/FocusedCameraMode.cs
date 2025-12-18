using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class FocusedCameraMode : MonoBehaviour
{
    private CameraController cameraController;
    public RTSCameraMode rtsCameraMode;

    [Header("Focus Mode")]
    public bool leavingFocus = false;
    public bool focusing = false;
    public bool inFocusMode = false;

    public Transform focusTarget;
    public float focusDistance = 6f;
    public float orbitSensitivity = 3f;
    public float zoomExitThreshold = 25f;

    private float currentAngleY = 0f;
    private float currentAngleX = 20f;
    public float currentFocusDistance = 6f;

    private Quaternion smoothOrbitRot;
    public float orbitSmoothSpeed = 10f;

    [Header("Focus Transition")]
    public float focusTransitionDuration = 0.7f;
    private float focusTransitionT = 0f;

    private Vector3 focusStartPos;
    private Quaternion focusStartRot;
    private float focusStartDistance;

    private float leaveTransitionT = 0f;

    public float leaveTransitionDuration = 0.6f;

    private Vector3 leaveStartPos;
    private Quaternion leaveStartRot;
    private float leaveStartDistance;

    private bool transitionInputLocked = false;
    private Vector3 focusTransitionAnchor;


    private void Start()
    {
        cameraController = transform.GetComponent<CameraController>();
        rtsCameraMode = transform.GetComponent<RTSCameraMode>();

        smoothOrbitRot = transform.rotation;
    }

    public void updateFocusedCameraMode()
    {
        if (focusing)
        {
            HandleFocusTransition();
        }
        else if (leavingFocus)
        {
            HandleLeaveFocusTransition();
        }
        else if (inFocusMode)
        {
            HandleFocusMode();
        }

        UpdateDepthOfField();
    }

    private void UpdateDepthOfField()
    {
        if (cameraController.dof == null) return;

        if (inFocusMode || focusing)
        {
            float dist = Vector3.Distance(transform.position, focusTarget.position);

            // Stronger DOF when close, weaker when far
            float t = Mathf.InverseLerp(rtsCameraMode.maxDistance, rtsCameraMode.minDistance, dist);

            float focus = Mathf.Lerp(cameraController.dofMaxFocus * 0.3f, cameraController.dofMaxFocus, t);
            cameraController.dof.focusDistance.Override(focus);
            return;
        }
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

        transform.position = Vector3.Lerp(leaveStartPos, cameraController.targetPosition, t);
        transform.rotation = Quaternion.Slerp(leaveStartRot, Quaternion.Euler(rtsCameraMode.tiltAngle, transform.rotation.eulerAngles.y, 0f), t);

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
                Vector3 delta = Input.mousePosition - cameraController.lastMousePos;
                currentAngleY += delta.x * orbitSensitivity;
                currentAngleX = Mathf.Clamp(currentAngleX - delta.y * orbitSensitivity, 5f, 80f);
            }
            cameraController.lastMousePos = Input.mousePosition;

            // Zoom in/out
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0005f)
                currentFocusDistance = Mathf.Clamp(currentFocusDistance - scroll * rtsCameraMode.zoomSpeed * 0.5f, rtsCameraMode.minDistance, rtsCameraMode.maxDistance);
        }

        // Exit when zoomed too far (only when actually in focus mode and not transitioning)
        if (!transitionInputLocked && currentFocusDistance > zoomExitThreshold && !leavingFocus && !focusing)
        {
            BeginLeaveFocus();
            return;
        }

        Quaternion rot = Quaternion.Euler(currentAngleX, currentAngleY, 0f);
        Vector3 offset = rot * (Vector3.back * currentFocusDistance);

        cameraController.targetPosition = focusTarget.position + offset;

        smoothOrbitRot = Quaternion.Slerp(smoothOrbitRot, rot, Time.deltaTime * orbitSmoothSpeed);
        transform.rotation = smoothOrbitRot;

        cameraController.ApplyCameraBounds();
        cameraController.ApplySmoothing();
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
        smoothOrbitRot = Quaternion.Slerp(focusStartRot, targetRot, t);
        transform.rotation = smoothOrbitRot;

        if (t >= 1f)
        {
            focusing = false;
            inFocusMode = true;
            transitionInputLocked = false;
            // ensure lastMousePos is reset so first orbit frame isn't a big jump
            cameraController.lastMousePos = Input.mousePosition;
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
        currentAngleX = rtsCameraMode.tiltAngle;

        float scaledFocusDistance = focusDistance;

        var rend = target.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            float sizeFactor = rend.bounds.extents.magnitude;
            scaledFocusDistance = sizeFactor * 2.0f;
        }

        currentFocusDistance = Mathf.Clamp(scaledFocusDistance, rtsCameraMode.minDistance, rtsCameraMode.maxDistance);

        focusTransitionT = 0f; // reset blend

        transitionInputLocked = true;
        cameraController.lastMousePos = Input.mousePosition;
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
        float dist = Mathf.Clamp(leaveStartDistance * 2f, rtsCameraMode.minDistance, rtsCameraMode.maxDistance);

        cameraController.targetPosition = focusTarget.position + dir * dist;

        leaveTransitionT = 0f;
    }

    public void ExitFocusMode()
    {
        inFocusMode = false;
        focusTarget = null;
    }
}
