//Script was created referencing https://assetstore.unity.com/packages/essentials/starter-assets-thirdperson-updates-in-new-charactercontroller-pa-196526
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;
    // general movement
    private Vector2 move;    
    public bool analogMovement;
	public bool jump;
	public bool climb;   
    public bool climbing;    
    public bool drag;  
    public bool dragForward;    
    public bool dragBackward; 
    [SerializeField] private float MoveSpeed = 2.0f; 
    [SerializeField] private float ClimbSpeed = 3.5f;
    [SerializeField] private float ClimbUpSpeed = 4.5f;
    [SerializeField] private float ClimbSideSpeed = 3.0f;
    [SerializeField] private float DragSpeed = 3.5f;
    private float _speed;
     private float _animationBlend;
    //Acceleration and deceleration
    public float SpeedChangeRate = 10.0f;
    private CharacterController _controller;

    //looking
    public Vector2 look;
	public bool cursorInputForLook = true;
    public bool cursorLocked = true;
    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return _playerInput.currentControlScheme == "KeyboardMouse";
#else
            return false;
#endif
        }
    }

    // camera
    private GameObject _mainCamera;
    [SerializeField] private GameObject CinemachineCameraTarget; 
    private Vector3 _initialCameraPosition;
    private Quaternion _initialCameraRotation;
    private Vector3 cameraOffset; 
    //How far in degrees can you move the camera up
    public float TopClamp = 70.0f;
    //"How far in degrees can you move the camera down
    public float BottomClamp = -30.0f;
    //Additional degress to override the camera. Useful for fine tuning camera position when locked
    public float CameraAngleOverride = 0.0f;
    //For locking the camera position on all axis
    public bool LockCameraPosition = false;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private const float _threshold = 0.01f;
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float controllerSensitivity = 100.0f;

    // direction and turning    
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float RotationSmoothTime = 0.12f; 

    //jump
    public bool Grounded = true;
    public float JumpHeight = 1.2f;
    public float Gravity = -15.0f;
    //Time required to pass before being able to jump again. Set to 0f to instantly jump again
    public float JumpTimeout = 0.50f;
    //Time required to pass before entering the fall state. Useful for walking down stairs
    public float FallTimeout = 0.15f;
    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private float _terminalVelocity = 53.0f;

    //climbing
    public bool inClimbZone;
    private Vector3 wallNormal;
    [SerializeField] private float ledgeCheckDistance = 0.6f;
    [SerializeField] private float ledgeHeight = 1.5f;
    [SerializeField] private LayerMask ledgeLayer;

    private bool isHanging = false;
    private Vector3 ledgePoint;
    private Vector3 ledgeNormal;

    //draging
    public bool inDragZone;
    private Vector3 boxNormal;
    private Transform box;
    private Vector3 boxEdgePoint;
    
    

    void Awake()
    {
        _playerInput = FindFirstObjectByType<PlayerInput>();
        _controller = GetComponent<CharacterController>();

        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        move = ctx.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        if(cursorInputForLook)
        {
            look = ctx.ReadValue<Vector2>();
        }
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        jump = ctx.ReadValueAsButton();
    }

    public void OnClimb(InputAction.CallbackContext ctx)
    {
        climb = ctx.ReadValueAsButton();
    }

    public void OnDrag(InputAction.CallbackContext ctx)
    {
        drag = ctx.ReadValueAsButton();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 1.2f))
        {
            if (hit.collider.CompareTag("Climbable"))
            {
                wallNormal = hit.normal;
                inClimbZone = true;
            }
            else
            {
                inClimbZone = false;
            }
            if (hit.collider.CompareTag("Dragable"))
            {
                box = hit.transform;
                boxNormal = hit.normal;
                inDragZone = true;
                boxEdgePoint = hit.point;
            }
            else
            {
                inDragZone = false;
            }
        }
        else
        {
            inClimbZone = false;
            inDragZone = false;
        }
        Grounded = _controller.isGrounded;
        JumpAndGravity();
        Move();
        
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void Move()
    {
        CheckLedge();

        if (isHanging)
        {
            _verticalVelocity = 0f;

            // stay locked in place
            if (move.y > 0.1f)
            {
                StartCoroutine(ClimbUpLedge());
            }

            return; // IMPORTANT: stop normal movement
        }

        // 0. Determine climbing state
        climbing = climb && inClimbZone; 

        // 1. Target speed
        float targetSpeed = climbing ? ClimbSpeed : MoveSpeed;

        if (move == Vector2.zero && !climbing) targetSpeed = 0f;
        

        // 2. Current horizontal speed
        Vector3 horizontalVelocity = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        float inputMagnitude = analogMovement ? move.magnitude : 1f;
        float speedOffset = 0.1f;

        // 3. Smooth speed change
        if (Mathf.Abs(currentSpeed - targetSpeed) > speedOffset)
        {
            _speed = Mathf.Lerp(currentSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        // 4. Animation blend
        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // 5. Input direction
        Vector3 inputDir = new Vector3(move.x, 0f, move.y).normalized;

        // 6. Rotation (only if moving, not climbing, not dragging)
        if (move != Vector2.zero && Grounded && !climbing && !drag)
        {
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg
                                + _mainCamera.transform.eulerAngles.y;

            float rotation = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetRotation,
                ref _rotationVelocity,
                RotationSmoothTime
            );

            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            _targetRotation = targetRotation;
        }

        // 7. Movement direction
        Vector3 velocity;

        if (climbing)
        {
            _verticalVelocity = 0f;

            Vector3 wallUp = Vector3.up;
            Vector3 wallRight = Vector3.Cross(wallNormal, wallUp).normalized;
            wallUp = Vector3.Cross(wallRight, wallNormal).normalized;

            Vector3 climbMove =
                wallUp * move.y * ClimbUpSpeed +
                wallRight * move.x * ClimbSideSpeed;

            velocity = climbMove;
            velocity += -wallNormal * 2f;

            Quaternion targetRotation = Quaternion.LookRotation(-wallNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
        else if (drag && inDragZone && box != null)
        {
            // While dragging: only allow movement along the push/pull axis
            // Forward input = push box away, Backward input = pull box toward player
            float forwardAmount = move.y; // use raw vertical input only

            if (Mathf.Abs(forwardAmount) > 0.1f)
            {
                Vector3 pushDirection = -boxNormal;
                float moveDir = Mathf.Sign(forwardAmount);
                Vector3 dragMove = pushDirection * moveDir * DragSpeed * Time.deltaTime;

                _controller.Move(dragMove + Vector3.up * _verticalVelocity * Time.deltaTime);
                box.position += dragMove;
            }
            else
            {
                // No input while dragging — stay still
                _controller.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
            }

            // Always face the box while dragging
            Quaternion dragRotation = Quaternion.LookRotation(-boxNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, dragRotation, Time.deltaTime * 10f);

            return; // skip the normal _controller.Move below
        }
        else
        {
            Vector3 moveDirection = Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward;
            velocity = moveDirection.normalized * _speed + Vector3.up * _verticalVelocity;
        }

        // 8. Apply movement (only reached if not dragging)
        _controller.Move(velocity * Time.deltaTime);
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }

            // if we are not grounded, do not jump
            jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            float sensitivity = IsCurrentDeviceMouse ? mouseSensitivity : controllerSensitivity;

            _cinemachineTargetYaw += look.x * sensitivity * deltaTimeMultiplier;
            _cinemachineTargetPitch += look.y * sensitivity * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void CheckLedge()
    {
        if (Grounded || isHanging) return;

        Vector3 origin = transform.position + Vector3.up * ledgeHeight;

        // 1. Forward ray (detect wall)
        if (Physics.Raycast(origin, transform.forward, out RaycastHit wallHit, ledgeCheckDistance, ledgeLayer))
        {
            // 2. Ray from above downwards (check for top surface)
            Vector3 downOrigin = wallHit.point + Vector3.up * 0.5f;

            if (Physics.Raycast(downOrigin, Vector3.down, out RaycastHit topHit, 1.5f, ledgeLayer))
            {
                // Found a ledge!
                ledgePoint = topHit.point;
                ledgeNormal = wallHit.normal;

                StartHang();
            }
        }
    }

    private void StartHang()
    {
        isHanging = true;
        _verticalVelocity = 0f;

        // snap player to ledge
        Vector3 hangPos = ledgePoint - ledgeNormal * 0.5f;
        hangPos.y -= 1.2f; // adjust for character height

        transform.position = hangPos;

        // face the wall
        transform.rotation = Quaternion.LookRotation(-ledgeNormal);
    }

    private IEnumerator ClimbUpLedge()
    {
        isHanging = false;

        Vector3 targetPos = ledgePoint + Vector3.up * 1.0f;

        float time = 0f;
        float duration = 0.3f;

        Vector3 startPos = transform.position;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
    }
    
}
