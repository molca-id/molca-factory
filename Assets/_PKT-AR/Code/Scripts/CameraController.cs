using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public enum InteractionMode
    {
        None = 0,
        Rotate = 1,
        Zoom = 2,
        Move = 3
    }

    [SerializeField]
    private Transform targetObject; // The object the camera should focus on
    public float posSensitivityX = 2f; // Sensitivity for horizontal translation
    public float posSensitivityY = 2f; // Sensitivity for vertical translation
    public float rotSensitivityX = 2f; // Sensitivity for horizontal rotation
    public float rotSensitivityY = 2f; // Sensitivity for vertical rotation
    public float minVertAngle = -45f; // Minimum vertical angle (to prevent flipping)
    public float maxVertAngle = 45f; // Maximum vertical angle (to prevent flipping)
    public float minDistance = 1f; // Minimum distance of the camera
    public float maxDistance = 10f; // Maximum distance of the camera
    public float moveSensitivity = 10f;
    public float moveSpeed = 10f;
    public float rotateSpeed = 10f;
    public float panSpeed = .5f;
    public float panSensitivity = .5f;
    public Bounds targetBounds;

    // New variables for collision detection
    private LayerMask environmentLayer;
    private const float collisionOffset = 0.1f; // Offset to prevent camera from being exactly at collision point

    private Vector3 _lastMousePos;
    private Vector3 _mouseDelta;
    private Vector2 _lastMovePoint;
    private Vector2 _lastMovePosA, _lastMovePosB;
    private Vector3 _moveTarget;
    private float _currentDistance;
    private float _targetDistance;
    private float _startPanDistance;
    private float _startCamDistance;
    private float _xRot = 0.0f;
    private float _yRot = 0.0f;
    private bool _isPanning = false;
    private bool _followMode = true;
    private Quaternion _targetRotation;
    private InteractionMode _mode;
    private Camera _cam;

    private Transform _target => _followMode && _followTransform ? _followTransform : targetObject;
    private Transform _followTransform;
    private CameraPoint _followTarget;

    public bool FollowMode { 
        get => _followMode;
        set
        {
            _followMode = value;
            if (!value && _followTarget)
            {
                _moveTarget = _followTransform.position;
                targetObject.position = _moveTarget;
            }
        }
    }

    public CameraPoint FollowTarget
    {
        get => _followTarget;
        set
        {
            if (_followTarget == value) 
                return;

            if (_followTarget != null)
            {
                _followTarget.ResetTransform();
            }

            _followTarget = value;
            if (_followTarget == null)
            {
                if (FollowMode)
                    targetObject.position = _followTransform.position;
                //_moveTarget = targetObject.position;
                _followTransform = null;
            }
            else
            {
                _followTransform = _followTarget.transform;
            }
        }
    }

    public InteractionMode Mode => _mode;

    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("Please assign a target object to the CameraController script!");
            return;
        }

        _cam = GetComponent<Camera>();
        _moveTarget = targetObject.position;

        // Initialize environment layer
        environmentLayer = LayerMask.GetMask("Environment");
    }

    private void LateUpdate()
    {
        _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, Time.deltaTime * panSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * rotateSpeed);

        // Calculate desired camera position with collision detection
        Vector3 desiredPosition = _target.position - transform.forward * _currentDistance;

        if (!FollowMode || FollowTarget == null)
        {
            transform.position = GetCameraPositionWithCollision(_target.position, desiredPosition);
            _target.position = Vector3.Lerp(_target.position, _moveTarget, Time.deltaTime * moveSpeed);
        }
        else
        {
            transform.position = Vector3.Lerp(
                transform.position,
                GetCameraPositionWithCollision(_target.position, desiredPosition),
                Time.deltaTime * moveSpeed / 2f);
        }
    }

    void Update()
    {
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
            return;

        _mouseDelta = Input.mousePosition - _lastMousePos;
        _lastMousePos = Input.mousePosition;

        if (Input.GetMouseButton(0)) // Only process input when right mouse button is held
        {
            if (Input.GetKey(KeyCode.LeftControl))
                HandleMoving();
            else
                HandleRotation();
        }
        else if (Input.mouseScrollDelta.magnitude > 0)
        {
            HandlePanning(Input.mouseScrollDelta.y);
        }
        else
        {
            _mode = InteractionMode.None;
        }

        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Moved)
        {
            if (Input.touchCount == 1)
                HandleTouchRotation(Input.touches[0].deltaPosition);
            else if (Input.touches[1].phase == TouchPhase.Moved)
                HandleTouchPanning();
        }
        else if ((int)_mode > 0)
        {
            _isPanning = false;
            _mode = InteractionMode.None;
        }
    }

    // Handle collision detection
    private Vector3 GetCameraPositionWithCollision(Vector3 targetPos, Vector3 desiredCamPos)
    {
        Vector3 direction = (desiredCamPos - targetPos).normalized;
        float distance = Vector3.Distance(targetPos, desiredCamPos);

        // Check for collisions
        RaycastHit hit;
        if (Physics.Raycast(targetPos, direction, out hit, distance, environmentLayer))
        {
            // If we hit something, position the camera at the hit point plus a small offset
            return hit.point + direction * collisionOffset;
        }

        // If no collision, first ensure the position is valid (above ground)
        return EnsureValidCameraPosition(desiredCamPos);
    }

    public void MoveToPoint(CameraPoint point)
    {
        if (!gameObject.activeInHierarchy || point == null)
            return;

        if(point.autoAttach)
            point.Attach();

        if (!FollowMode)
            return;

        _moveTarget = point.Position;
        _targetDistance = Mathf.Lerp(maxDistance, minDistance, point.zoomLevel);
        _xRot = point.cameraRotation.y;
        _yRot = point.cameraRotation.x;
        _targetRotation = Quaternion.Euler(point.cameraRotation);
    }

    private void HandleRotation()
    {
        if (_mode != InteractionMode.Rotate)
        {
            _xRot = _targetRotation.eulerAngles.y;
            //_yRot = _targetRotation.eulerAngles.x;
            _mode = InteractionMode.Rotate;
        }

        _xRot += _mouseDelta.x * rotSensitivityX;
        _yRot += -_mouseDelta.y * rotSensitivityY;

        // Clamp vertical rotation within limits to avoid flipping
        _yRot = Mathf.Clamp(_yRot, minVertAngle, maxVertAngle);

        // Apply rotations to the camera transform
        _targetRotation = Quaternion.Euler(_yRot, _xRot, 0.0f);
    }

    private void HandlePanning(float delta)
    {
        if (delta == 0)
            return;
        _mode = InteractionMode.Zoom;

        float lerpValue = Mathf.InverseLerp(minDistance, maxDistance, _currentDistance);
        _targetDistance = Mathf.Clamp(Mathf.Lerp(minDistance, maxDistance, lerpValue - (delta * panSensitivity * _currentDistance * 4f)), minDistance, maxDistance);
    }

    private void HandleMoving()
    {
        _mode = InteractionMode.Move;

        float horizontalInput = _mouseDelta.x * posSensitivityX; 
        float verticalInput = _mouseDelta.y * posSensitivityY;

        Vector3 cameraUp = transform.up;
        cameraUp.Normalize();

        Vector3 cameraRight = transform.right;
        cameraRight.Normalize();

        Vector3 movement = (cameraRight * horizontalInput + cameraUp * verticalInput) * moveSensitivity * _currentDistance;

        _moveTarget = targetBounds.ClosestPoint(_target.position + movement);
    }

    private void HandleTouchRotation(Vector2 touchDelta)
    {
        if(_mode != InteractionMode.Rotate)
        {
            _xRot = _targetRotation.eulerAngles.y;
            //_yRot = _targetRotation.eulerAngles.x;
            _mode = InteractionMode.Rotate;
        }

        //Debug.Log(touchDelta);
        _xRot -= touchDelta.x * rotSensitivityX;
        _yRot -= touchDelta.y * rotSensitivityY;

        // Clamp vertical rotation within limits to avoid flipping
        _yRot = Mathf.Clamp(_yRot, minVertAngle, maxVertAngle);

        // Apply rotations to the camera transform
        _targetRotation = Quaternion.Euler(_yRot, _xRot, 0.0f);
    }

    private void HandleTouchPanning()
    {
        Vector2 touch0 = Input.touches[0].position;
        Vector2 touch1 = Input.touches[1].position;
        Vector2 cPoint = CenterOfPoints(touch0, touch1);

        if (!_isPanning)
        {
            _isPanning = true;
            _lastMovePosA = touch0;
            _lastMovePosB = touch1;
            _targetDistance = _currentDistance; // Stop lerping distance
            return;
        }
        
        if (_mode == InteractionMode.Zoom)
        {
            float deltaDistance = _startPanDistance - Vector2.Distance(touch0, touch1);
            _targetDistance = Mathf.Clamp(_startCamDistance + (deltaDistance * panSensitivity * _currentDistance), minDistance, maxDistance);
            return;
        } else if (_mode == InteractionMode.Move)
        {
            HandleTouchMoving(_lastMovePoint - cPoint);
            _lastMovePoint = cPoint;
            return;
        }

        #region Pan Mode Check
        if (Vector2.Dot(touch0 - _lastMovePosA, touch1 - _lastMovePosB) < 0)
        {
            _startPanDistance = Vector2.Distance(touch0, touch1);
            _startCamDistance = _currentDistance;
            _mode = InteractionMode.Zoom;
        }
        else
        {
            _lastMovePoint = cPoint;
            _mode = InteractionMode.Move;
        }
        #endregion
    }

    private void HandleTouchMoving(Vector2 touchDelta)
    {
        float horizontalInput = touchDelta.x * posSensitivityX;
        float verticalInput = touchDelta.y * posSensitivityY;

        Vector3 cameraUp = transform.up;
        cameraUp.Normalize();

        Vector3 cameraRight = transform.right;
        cameraRight.Normalize();

        Vector3 movement = (cameraRight * horizontalInput + cameraUp * verticalInput) * moveSensitivity * _currentDistance;
        _moveTarget = targetBounds.ClosestPoint(_target.position + movement);
    }

    private Vector3 EnsureValidCameraPosition(Vector3 pos)
    {
        if (pos.y < .1f)
        {
            Vector3 direction = _target.position - pos;
            float verticalDisplacement = _target.position.y - pos.y;
            float verticalMovement = Vector3.Dot(direction.normalized, Vector3.up) * verticalDisplacement;
            verticalMovement = Mathf.Max(0.0f, verticalMovement);
            pos = pos + direction.normalized * Mathf.Sign(verticalMovement) * verticalMovement;
            pos.y = .1f;
        }
        return pos;
    }

    private Vector2 CenterOfPoints(Vector2 pointA, Vector2 pointB)
    {
        return (pointA + pointB) / 2f;
    }

    private float VectorToAngle(Vector2 vector)
    {
        float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;
        return angle;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(targetBounds.center, targetBounds.size);
    }
}
