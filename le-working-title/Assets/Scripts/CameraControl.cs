using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("CameraControlEditor")]
public class CameraControl : MonoBehaviour
{
    #region Foldouts

    #if UNITY_EDITOR

    public int LastTab;

    public bool MovementSettingsFoldout;
    public bool ZoomingSettingsFoldout;
    public bool RotationSettingsFoldout;
    public bool HeightSettingsFoldout;
    public bool MapLimitSettingsFoldout;
    public bool TargetingSettingsFoldout;
    public bool InputSettingsFoldout;

    #endif

    #endregion

    private Transform cameraTransform;
    public  bool      UseFixedUpdate;

    #region Movement

    public float KeyboardMovementSpeed   = 5f;
    public float ScreenEdgeMovementSpeed = 3f;
    public float FollowingSpeed          = 5f;
    public float RotationSped            = 3f;
    public float PanningSpeed            = 10f;
    public float MouseRotationSpeed      = 10f;

    #endregion

    #region Height

    public bool      AutoHeight = true;
    public LayerMask GroundMask = -1;

    public float MinHeight                     = 10f;
    public float MaxHeight                     = 15f;
    public float HeightDampening               = 5f;
    public float KeyboardZoomingSensitivity    = 2f;
    public float ScrollWheelZoomingSensitivity = 25f;

    private float zoomPosition;

    #endregion

    #region MapLimits

    public bool  LimitMap = true;
    public float LimitX   = 50f;
    public float LimitY   = 50f;

    #endregion

    #region Targeting

    public Transform TargetFollow;
    public Vector3   TargetOffset;

    /// <summary>
    /// Are we following target?
    /// </summary>
    private bool FollowingTarget
    {
        get {return TargetFollow != null;}
    }

    #endregion

    #region Input

    public bool  UseScreenEdgeInput = true;
    public float ScreenEdgeBorder   = 25f;

    public bool   UseKeyboardInput = true;
    public string HorizontalAxis   = "Horizontal";
    public string VerticalAxis     = "Vertical";

    public bool    UsePanning = true;
    public KeyCode PanningKey = KeyCode.Mouse2;

    public bool    UseKeyboardZooming = true;
    public KeyCode ZoomInKey          = KeyCode.E;
    public KeyCode ZoomOutKey         = KeyCode.Q;

    public bool   UseScrollwheelZooming = true;
    public string ZoomingAxis           = "Mouse ScrollWheel";

    public bool    UseKeyboardRotation = true;
    public KeyCode RotateRightKey      = KeyCode.X;
    public KeyCode RotateLeftKey       = KeyCode.Z;

    public bool    UseMouseRotation = true;
    public KeyCode MouseRotationKey = KeyCode.Mouse1;

    private Vector2 KeyboardInput
    {
        get
        {
            return UseKeyboardInput
                       ? new Vector2(Input.GetAxis(HorizontalAxis),
                                     Input.GetAxis(VerticalAxis))
                       : Vector2.zero;
        }
    }

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private Vector2 MouseInput
    {
        get {return Input.mousePosition;}
    }

    private float ScrollWheel
    {
        get {return Input.GetAxis(ZoomingAxis);}
    }

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private Vector2 MouseAxis
    {
        get {return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));}
    }

    private int ZoomDirection
    {
        get
        {
            bool zoomIn  = Input.GetKey(ZoomInKey);
            bool zoomOut = Input.GetKey(ZoomOutKey);

            if(zoomIn && zoomOut)
                return 0;

            if(zoomOut)
                return 1;

            return zoomIn ? -1 : 0;
        }
    }

    private int RotationDirection
    {
        get
        {
            bool rotateRight = Input.GetKey(RotateRightKey);
            bool rotateLeft  = Input.GetKey(RotateLeftKey);

            if(rotateLeft && rotateRight)
                return 0;

            if(rotateLeft)
                return 1;

            return rotateRight ? -1 : 0;
        }
    }

    #endregion

    #region UnityMethods

    private void Start()
    {
        cameraTransform = transform;
    }

    private void Update()
    {
        if(!UseFixedUpdate)
            CameraUpdate();
    }

    private void FixedUpdate()
    {
        if(UseFixedUpdate)
            CameraUpdate();
    }

    #endregion

    #region CameraMovementMethods

    /// <summary>
    /// Update camera movement and rotation
    /// </summary>
    private void CameraUpdate()
    {
        if(FollowingTarget)
            FollowTarget();
        else
            Move();

        HeightCalculation();
        Rotation();
        LimitPosition();
    }

    /// <summary>
    /// Move camera with keyboard or with screen edge
    /// </summary>
    private void Move()
    {
        if(UseKeyboardInput)
        {
            Vector3 desiredMove = new Vector3(KeyboardInput.x, 0, KeyboardInput.y);

            desiredMove *= KeyboardMovementSpeed;
            desiredMove *= Time.deltaTime;
            desiredMove = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f)) *
                          desiredMove;
            desiredMove = cameraTransform.InverseTransformDirection(desiredMove);

            cameraTransform.Translate(desiredMove, Space.Self);
        }

        if(UseScreenEdgeInput)
        {
            Vector3 desiredMove = new Vector3();

            Rect leftRect = new Rect(0, 0, ScreenEdgeBorder, Screen.height);
            Rect rightRect = new Rect(Screen.width - ScreenEdgeBorder, 0,
                                      ScreenEdgeBorder, Screen.height);
            Rect upRect = new Rect(0, Screen.height - ScreenEdgeBorder,
                                   Screen.width, ScreenEdgeBorder);
            Rect downRect = new Rect(0, 0, Screen.width, ScreenEdgeBorder);

            desiredMove.x = leftRect.Contains(MouseInput)  ? -1 :
                            rightRect.Contains(MouseInput) ? 1 : 0;
            desiredMove.z = upRect.Contains(MouseInput)   ? 1 :
                            downRect.Contains(MouseInput) ? -1 : 0;

            desiredMove *= ScreenEdgeMovementSpeed;
            desiredMove *= Time.deltaTime;
            desiredMove = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f)) *
                          desiredMove;
            desiredMove = cameraTransform.InverseTransformDirection(desiredMove);

            cameraTransform.Translate(desiredMove, Space.Self);
        }

        if(!UsePanning || !Input.GetKey(PanningKey) || MouseAxis == Vector2.zero)
        {
            return;
        }

        {
            Vector3 desiredMove = new Vector3(-MouseAxis.x, 0, -MouseAxis.y);

            desiredMove *= PanningSpeed;
            desiredMove *= Time.deltaTime;
            desiredMove = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f)) *
                          desiredMove;
            desiredMove = cameraTransform.InverseTransformDirection(desiredMove);

            cameraTransform.Translate(desiredMove, Space.Self);
        }
    }

    /// <summary>
    /// Calcualte height
    /// </summary>
    private void HeightCalculation()
    {
        float distanceToGround = DistanceToGround();
        if(UseScrollwheelZooming)
            zoomPosition += ScrollWheel * Time.deltaTime * ScrollWheelZoomingSensitivity;
        if(UseKeyboardZooming)
            zoomPosition += ZoomDirection * Time.deltaTime * KeyboardZoomingSensitivity;

        zoomPosition = Mathf.Clamp01(zoomPosition);

        float targetHeight = Mathf.Lerp(MaxHeight, MinHeight, zoomPosition);
        float difference   = 0;

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if(distanceToGround != targetHeight)
            difference = targetHeight - distanceToGround;

        cameraTransform.position = Vector3.Lerp(cameraTransform.position,
                                                new Vector3(cameraTransform.position.x,
                                                            targetHeight + difference,
                                                            cameraTransform.position.z),
                                                Time.deltaTime * HeightDampening);
    }

    /// <summary>
    /// Rotate camera
    /// </summary>
    private void Rotation()
    {
        if(UseKeyboardRotation)
            transform.Rotate(Vector3.up, RotationDirection * Time.deltaTime * RotationSped,
                             Space.World);

        if(UseMouseRotation && Input.GetKey(MouseRotationKey))
            cameraTransform.Rotate(Vector3.up,
                                   -MouseAxis.x * Time.deltaTime * MouseRotationSpeed,
                                   Space.World);
    }

    /// <summary>
    /// Follow targetif target != null
    /// </summary>
    private void FollowTarget()
    {
        Vector3 targetPosition =
            new Vector3(TargetFollow.position.x, cameraTransform.position.y, TargetFollow.position.z) +
            TargetOffset;
        cameraTransform.position =
            Vector3.MoveTowards(cameraTransform.position, targetPosition,
                                Time.deltaTime * FollowingSpeed);
    }

    /// <summary>
    /// Limit camera position
    /// </summary>
    private void LimitPosition()
    {
        if(!LimitMap)
            return;

        float x = Mathf.Clamp(cameraTransform.position.x, -LimitX, LimitX);
        float y = cameraTransform.position.y;
        float z = Mathf.Clamp(cameraTransform.position.z, -LimitY, LimitY);

        cameraTransform.position = new Vector3(x, y, z);
    }

    /// <summary>
    /// Set the target
    /// </summary>
    /// <param name="target"></param>
    public void SetTarget(Transform target)
    {
        TargetFollow = target;
    }

    /// <summary>
    /// Reset the target (target is set to null)
    /// </summary>
    public void ResetTarget()
    {
        TargetFollow = null;
    }

    /// <summary>
    /// Calculate distance to ground
    /// </summary>
    /// <returns></returns>
    private float DistanceToGround()
    {
        Ray        ray = new Ray(cameraTransform.position, Vector3.down);
        RaycastHit hit;

        return Physics.Raycast(ray, out hit, GroundMask.value)
                   ? (hit.point - cameraTransform.position).magnitude
                   : 0f;
    }

    #endregion
}
