using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PivotCamera : MonoBehaviour
{
    public Transform pivotPoint;
    public Transform pivotPointOverview;
    public RectTransform leftMonitoringPanel; // Tambahkan referensi ke UI panel
    public RectTransform bottomMonitoringPanel; // Tambahkan referensi ke UI panel
    public RectTransform trendPanel; // Tambahkan referensi ke UI panel
    [Space]
    public float releaseDampTime = 0.2f; // Adjust damping duration after mouse release
    public float smoothTime = 0.1f; // Adjust smoothing duration
    public float transitionSpeed;
    public float targetZoom;
    [Space]
    public float rotationSpeed;
    public float movementSpeed;
    public float moveToTargetSpeed;
    public float resetSpeed;
    public float desktopZoomSpeed;
    public float mobileZoomSpeed;
    [Space]
    public float minZoomDistance;
    public float maxZoomDistance;
    [Space]
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;

    private Vector2 releaseVelocity;
    private Vector2 smoothedDeltaPosition;
    private Vector2 deltaPositionVelocity;
    private Vector2 smoothedMovementDelta;
    private Vector2 movementDeltaVelocity;

    private Vector2 previousPosition;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    
    private float currentVerticalAngle = 0f;
    private bool isResetting = false;
    private bool canZoom = true;

    [HideInInspector]
    public float currentZoomDistance;

    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        currentZoomDistance = Vector3.Distance(transform.position, pivotPoint.position);
        StaticData.pivot_camera = pivotPoint.position;
    }

    private void Update()
    {
        if (IsMouseOverLeftMonitoringPanel() ||
            IsMouseOverBottomMonitoringPanel() ||
            IsMouseOverTrendPanel())
        {
            return;
        }

        if (!isResetting)
        {
            HandleRotation();
            HandleMovement();
            HandleZoom();
        }
    }

    private bool IsMouseOverLeftMonitoringPanel()
    {
        if (leftMonitoringPanel != null)
        {
            Vector2 mousePosition = Input.mousePosition;
            return RectTransformUtility.RectangleContainsScreenPoint(leftMonitoringPanel, mousePosition);
        }

        return false;
    }

    private bool IsMouseOverBottomMonitoringPanel()
    {
        if (bottomMonitoringPanel != null)
        {
            Vector2 mousePosition = Input.mousePosition;
            return RectTransformUtility.RectangleContainsScreenPoint(bottomMonitoringPanel, mousePosition);
        }

        return false;
    }

    private bool IsMouseOverTrendPanel()
    {
        if (trendPanel != null &&
            trendPanel.gameObject.activeSelf)
        {
            Vector2 mousePosition = Input.mousePosition;
            return RectTransformUtility.RectangleContainsScreenPoint(trendPanel, mousePosition);
        }

        return false;
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 currentPosition = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                previousPosition = currentPosition;
            }

            Vector2 rawDeltaPosition = currentPosition - previousPosition;
            previousPosition = currentPosition;

            // Smooth the delta position using Lerp or SmoothDamp
            smoothedDeltaPosition = Vector2.SmoothDamp(smoothedDeltaPosition, rawDeltaPosition, ref deltaPositionVelocity, smoothTime);

            RotateCamera(smoothedDeltaPosition.x, smoothedDeltaPosition.y);

            // Store velocity for release movement
            releaseVelocity = smoothedDeltaPosition;
        }
        else
        {
            // Continue movement after mouse release with damping
            smoothedDeltaPosition = Vector2.SmoothDamp(smoothedDeltaPosition, Vector2.zero, ref deltaPositionVelocity, releaseDampTime);

            if (smoothedDeltaPosition.magnitude > 0.01f) // Prevent tiny movements
            {
                RotateCamera(smoothedDeltaPosition.x, smoothedDeltaPosition.y);
            }
        }
    }

    private void RotateCamera(float deltaX, float deltaY)
    {
        // Horizontal rotation
        transform.RotateAround(pivotPoint.position, Vector3.up, deltaX * rotationSpeed * Time.deltaTime);

        // Vertical rotation
        float newVerticalAngle = currentVerticalAngle - deltaY * rotationSpeed * Time.deltaTime;

        if (newVerticalAngle >= minVerticalAngle && newVerticalAngle <= maxVerticalAngle)
        {
            Vector3 axis = transform.right;
            transform.RotateAround(pivotPoint.position, axis, -deltaY * rotationSpeed * Time.deltaTime);
            currentVerticalAngle = newVerticalAngle;
        }
    }

    private void HandleMovement()
    {
        if (Input.GetMouseButton(1))
        {
            Vector2 currentPosition = Input.mousePosition;
            if (Input.GetMouseButtonDown(1))
            {
                previousPosition = currentPosition;
            }

            Vector2 rawDeltaPosition = currentPosition - previousPosition;
            previousPosition = currentPosition;

            // Smooth the movement delta using SmoothDamp
            smoothedMovementDelta = Vector2.SmoothDamp(smoothedMovementDelta, rawDeltaPosition, ref movementDeltaVelocity, smoothTime);

            MoveCamera(smoothedMovementDelta.x, smoothedMovementDelta.y);
        }
        else if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            if (touchZero.phase == TouchPhase.Moved && touchOne.phase == TouchPhase.Moved)
            {
                Vector2 avgDeltaPosition = (touchZero.deltaPosition + touchOne.deltaPosition) / 2;
                smoothedMovementDelta = Vector2.SmoothDamp(smoothedMovementDelta, avgDeltaPosition, ref movementDeltaVelocity, smoothTime);
                MoveCamera(smoothedMovementDelta.x, smoothedMovementDelta.y);
            }
        }
        else
        {
            // Gradually stop movement when no input
            smoothedMovementDelta = Vector2.SmoothDamp(smoothedMovementDelta, Vector2.zero, ref movementDeltaVelocity, releaseDampTime);

            if (smoothedMovementDelta.magnitude > 0.01f) // Prevent tiny movements
            {
                MoveCamera(smoothedMovementDelta.x, smoothedMovementDelta.y);
            }
        }
    }

    private void MoveCamera(float deltaX, float deltaY)
    {
        Vector3 rightMovement = transform.right * -deltaX * movementSpeed * Time.deltaTime;
        Vector3 upMovement = transform.up * -deltaY * movementSpeed * Time.deltaTime;
        Vector3 movement = rightMovement + upMovement;

        transform.position += movement;
        pivotPoint.position += movement;
    }

    private void HandleZoom()
    {
        if (!canZoom)
            return;

        if (Input.mouseScrollDelta.y != 0)
        {
            ZoomCamera(Input.mouseScrollDelta.y);
        }

        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            float prevTouchDeltaMag = (touchZero.position - touchZero.deltaPosition - (touchOne.position - touchOne.deltaPosition)).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
            float deltaMagnitudeDiff = (prevTouchDeltaMag - touchDeltaMag) * -1; // Kali negatif supaya bisa terbalik

            ZoomCamera(deltaMagnitudeDiff);
        }
    }

    private void ZoomCamera(float increment)
    {
        float zoomSpeed = StaticData.is_mobile ? mobileZoomSpeed : desktopZoomSpeed;
        Vector3 direction = (transform.position - pivotPoint.position).normalized;
        currentZoomDistance = Mathf.Clamp(currentZoomDistance - increment * zoomSpeed * Time.deltaTime, minZoomDistance, maxZoomDistance);

        transform.position = pivotPoint.position + direction * currentZoomDistance;
    }

    public IEnumerator SmoothChangePivot(Transform newPivot, float zoomFactor, Transform cameraPosition = null)
    {
        Vector3 targetPosition;
        Quaternion targetRotation;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 directionToNewPivot = (transform.position - newPivot.position).normalized;
        
        float newZoom = targetZoom + zoomFactor;
        newZoom = Mathf.Clamp(newZoom, minZoomDistance, maxZoomDistance);
        pivotPoint.position = StaticData.pivot_camera;

        if (cameraPosition != null)
        {
            canZoom = false;
            targetPosition = cameraPosition.position;
            targetRotation = cameraPosition.rotation;
        }
        else
        {
            canZoom = true;
            targetPosition = newPivot.position + directionToNewPivot * newZoom;
            targetRotation = Quaternion.LookRotation(newPivot.position - targetPosition);
        }

        float progress = 0f;
        isResetting = true;

        while (progress < 1f)
        {
            progress += Time.deltaTime / transitionSpeed;
            currentZoomDistance = Mathf.Lerp(currentZoomDistance, newZoom, progress);
            transform.SetPositionAndRotation(
                Vector3.Lerp(startPosition, targetPosition, progress),
                Quaternion.Slerp(startRotation, targetRotation, progress));

            yield return null;
        }

        transform.SetPositionAndRotation(targetPosition, targetRotation);
        currentZoomDistance = newZoom;
        isResetting = false;

        pivotPoint = newPivot;
        StaticData.pivot_camera = pivotPoint.position;
    }

    public void StartReset()
    {
        if (!isResetting)
            StartCoroutine(SmoothReset());

        OverviewManager.instance.ResetCurrentMachine();
        OverviewManager.instance.ResetCurrentPOI();
    }

    private IEnumerator SmoothReset()
    {
        canZoom = true;
        float progress = 0f;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float startZoomDistance = currentZoomDistance;
        float targetZoomDistance = Vector3.Distance(initialPosition, pivotPoint.position);

        pivotPoint = pivotPointOverview;
        pivotPoint.position = Vector3.zero;
        isResetting = true;

        while (progress < 1f)
        {
            progress += Time.deltaTime * resetSpeed;

            transform.position = Vector3.Lerp(startPosition, initialPosition, progress);
            transform.rotation = Quaternion.Slerp(startRotation, initialRotation, progress);
            currentZoomDistance = Mathf.Lerp(startZoomDistance, targetZoomDistance, progress);

            yield return null;
        }

        OverviewManager.instance.SetOverviewPanel(true);
        OverviewManager.instance.SetLeftParameterPanel(false);
        OverviewManager.instance.ResetDirectoryButtons();

        transform.position = initialPosition;
        transform.rotation = initialRotation;
        currentZoomDistance = targetZoomDistance;
        currentVerticalAngle = 0f;
        isResetting = false;
    }
}
