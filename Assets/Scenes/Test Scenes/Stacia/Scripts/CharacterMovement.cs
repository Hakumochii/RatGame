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
    [SerializeField] private float MoveSpeed = 2.0f; 
    [SerializeField] private float ClimbSpeed = 3.5f;
    [SerializeField] private float ClimbUpSpeed = 4.5f;
    [SerializeField] private float ClimbSideSpeed = 3.0f;
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
    public bool onEdge = false;
    
    

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
        }
        else
        {
            inClimbZone = false;
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

        // 6. Rotation (only if moving and not climbing)
        if (move != Vector2.zero && Grounded && !climbing)
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

            // Apply different speeds
            Vector3 climbMove =
                wallUp * move.y * ClimbUpSpeed +
                wallRight * move.x * ClimbSideSpeed;

            velocity = climbMove;

            // stick to wall
            velocity += -wallNormal * 2f;
            //rotate 
            Quaternion targetRotation = Quaternion.LookRotation(-wallNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
        else
        {
            Vector3 moveDirection = Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward;
            velocity = moveDirection.normalized * _speed + Vector3.up * _verticalVelocity;
        }

        // 8. Apply movement
        _controller.Move(velocity * Time.deltaTime);

        /*if (onEdge)
        {
            if (jump)
            {
                
            }
        }*/
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

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ClimbZone"))
        {
            inClimbZone = true;
        }
        if (other.CompareTag("Edge"))
        {
            onEdge = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ClimbZone"))
        {
            inClimbZone = false;
        }
    }
}

