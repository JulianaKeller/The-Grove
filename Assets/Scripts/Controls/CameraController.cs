using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraController : MonoBehaviour
{
    private RTSCameraMode rtsCameraMode;
    private FocusedCameraMode focusedCameraMode;

    private Camera cam;

    [Header("Depth Of Field")]
    public Volume volume;
    public float dofMinFocus = 3f;
    public float dofMaxFocus = 30f;

    [Header("Smoothing")]
    public float smoothTime = 0.15f;
    private Vector3 velocity = Vector3.zero;

    public DepthOfField dof;
    private float halfSize;

    public Vector3 lastMousePos;
    public Vector3 targetPosition;

    void Start()
    {
        rtsCameraMode = transform.GetComponent<RTSCameraMode>();
        focusedCameraMode = transform.GetComponent<FocusedCameraMode>();

        cam = Camera.main;

        if (volume != null && volume.profile.TryGet(out DepthOfField depthOfField))
            dof = depthOfField;

        float gridSize = EnvironmentGrid.Instance.gridSize * EnvironmentGrid.Instance.cellSize;
        halfSize = gridSize / 2f;
    }


    void Update()
    {
        if(focusedCameraMode.leavingFocus || focusedCameraMode.focusing || focusedCameraMode.inFocusMode)
        {
            focusedCameraMode.updateFocusedCameraMode();
            return;
        }

        rtsCameraMode.updateRTSCameraMode();
    }

    public void ToggleFocusMode(Transform target)
    {
        focusedCameraMode.ToggleFocusMode(target);
    }

    public void ExitFocusModeRequest()
    {
        focusedCameraMode.ExitFocusModeRequest();
    }

    public void ApplyCameraBounds()
    {
        targetPosition.x = Mathf.Clamp(targetPosition.x, -halfSize, halfSize);
        targetPosition.z = Mathf.Clamp(targetPosition.z, -halfSize, halfSize);
    }

    public void ApplySmoothing()
    {
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
