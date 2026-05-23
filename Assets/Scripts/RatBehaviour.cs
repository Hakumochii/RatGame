//Script was created referencing https://assetstore.unity.com/packages/essentials/starter-assets-thirdperson-updates-in-new-charactercontroller-pa-196526
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RatBehaviour : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;

    // general movement
    private Vector2 move;
    public bool analogMovement;
    public bool interact;
    public bool jump;
    public bool climb;
    public bool climbing;
    public bool drag;
    public bool dragStopped;
    public bool dragForward;
    public bool dragBackward;
    [SerializeField] private float MoveSpeed = 2.0f;
    [SerializeField] private float ClimbSpeed = 3.5f;
    [SerializeField] private float ClimbUpSpeed = 4.5f;
    [SerializeField] private float ClimbSideSpeed = 3.0f;
    [SerializeField] private float DragSpeed = 3.5f;
    private float _speed;
    private float _animationBlend;
    // Acceleration and deceleration
    public float SpeedChangeRate = 10.0f;
    private CharacterController _controller;

    // ── Animator ──────────────────────────────────────────────────────────────
    private Animator _animator;

    // ── Drag latch ────────────────────────────────────────────────────────────
    // Stays true while drag is held after making initial contact with a
    // draggable object. Prevents a single-frame raycast miss from dropping
    // all grab bools to false and blipping through Locomotion.
    // Releases the moment the drag button is released.
    private bool _dragLatch;

    // looking
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
    // How far in degrees can you move the camera up
    public float TopClamp = 70.0f;
    // How far in degrees can you move the camera down
    public float BottomClamp = -30.0f;
    // Additional degrees to override the camera. Useful for fine tuning camera position when locked
    public float CameraAngleOverride = 0.0f;
    // For locking the camera position on all axis
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

    // jump
    public bool Grounded = true;
    public float JumpHeight = 1.2f;
    public float Gravity = -15.0f;
    // Time required to pass before being able to jump again. Set to 0f to instantly jump again
    public float JumpTimeout = 0.50f;
    // Time required to pass before entering the fall state. Useful for walking down stairs
    public float FallTimeout = 0.15f;
    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private float _terminalVelocity = 53.0f;

    // ── Freefall ──────────────────────────────────────────────────────────────
    // How long the rat must be falling continuously before the freefall
    // animation triggers. Prevents it firing on small hops or steps.
    [SerializeField] private float fallAnimationDelay = 0.3f;
    private float _fallAnimationTimer = 0f;
    private bool _fallAnimationActive = false;

    // climbing / ledge
    public bool inClimbZone;
    private Vector3 wallNormal;
    [SerializeField] private float ledgeCheckDistance = 0.6f;
    [SerializeField] private float ledgeHeight = 1.5f;
    [SerializeField] private LayerMask ledgeLayer;

    private bool isHanging = false;
    private Vector3 ledgePoint;
    private Vector3 ledgeNormal;

    // dragging
    public bool canDrag = false;
    public bool dragging = false;
    public bool inDragZone;
    private Vector3 boxNormal;
    public Transform box;
    private Vector3 boxEdgePoint;
    public float _dragDirectionMultiplier = 1f;
    public bool inSwingZone; // set this from your trigger script
    private Vector3 _originalBoxPosition; // store on grab, not a Transform reference

    //swinnging
     public bool isSwinging = false;


    private GameManager _gameManager;


    void Awake()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        _playerInput = FindFirstObjectByType<PlayerInput>();
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

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
        if (cursorInputForLook)
        {
            look = ctx.ReadValue<Vector2>();
        }
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            interact = true;
        }
        else if (ctx.canceled)
        {
            interact = false;
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
        if (canDrag)
        {
            drag = ctx.ReadValueAsButton();

            if (ctx.canceled)
            {
                dragStopped = true;
                if (inSwingZone && box != null)
                {
                    box.position = _originalBoxPosition;
                }
            }
                
        }
        
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
        if(!_gameManager.usingComputer)
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
            UpdateAnimator();
        }
        
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    // ── Animator Update ───────────────────────────────────────────────────────
    private void UpdateAnimator()
    {
        if (isSwinging) return; // KnockBack owns the animator entirely

        // Locomotion blend (0 = idle, 1 = walk)
        _animator.SetFloat("Speed", _animationBlend);

        _animator.SetBool("IsGrounded", Grounded);
        _animator.SetBool("IsJumping", !Grounded && _verticalVelocity > 0f);
        _animator.SetBool("IsFalling", !Grounded && _verticalVelocity <= 0f);

        bool isFallingPhysically = !Grounded && !climbing && !isHanging
                                && _verticalVelocity <= 0f;

        if (isFallingPhysically)
        {
            _fallAnimationTimer += Time.deltaTime;
        }
        else
        {
            _fallAnimationTimer = 0f;
            _fallAnimationActive = false;
        }

        if (_fallAnimationTimer >= fallAnimationDelay)
            _fallAnimationActive = true;

        _animator.SetBool("IsFreefall", _fallAnimationActive);


        // ── Drag latch ────────────────────────────────────────────────────────
        // Set latch when drag button is held and we are in contact with a
        // draggable object. Keep it set while drag is held even if the
        // raycast briefly misses. Release only when drag button is released.
        // In OnDrag, when released:
        if (drag && inDragZone && !_dragLatch)
        {
            _originalBoxPosition = box.position;
            _dragLatch = true;
        }
        if (!drag) _dragLatch = false;

        bool isNearDraggable = drag && _dragLatch;

        // ── Push / pull / grab idle ───────────────────────────────────────────
        bool isPushing = isNearDraggable && move.y > 0.1f;
        bool isPulling = isNearDraggable && move.y < -0.1f;
        // GrabIdle: holding object but not actively pushing or pulling
        bool isGrabIdle = isNearDraggable && !isPushing && !isPulling;

        _animator.SetBool("IsPushing", isPushing);
        _animator.SetBool("IsPulling", isPulling);
        _animator.SetBool("IsGrabIdle", isGrabIdle);

        // ── Climbing ──────────────────────────────────────────────────────────
        _animator.SetBool("IsClimbing", climbing);
    }

    private void Move()
    {
        // ── Ledge hang — vault animation still detached ───────────────────────
        CheckLedge();
        if (isSwinging) return;

        if (isHanging)
        {
            _verticalVelocity = 0f;

            if (move.y > 0.1f)
            {
                StartCoroutine(ClimbUpLedge());
            }

            return;
        }

        // 0. Determine climbing state — requires directional input
        climbing = climb && inClimbZone && move != Vector2.zero;
        dragging = drag && _dragLatch; // was: drag && inDragZone

        // 1. Reset blend immediately when drag engages
        if (drag && _dragLatch) // was: drag && inDragZone
        {
            _animationBlend = 0f;
            _speed = 0f;
        }

        // 2. Target speed
        float targetSpeed = climbing ? ClimbSpeed : MoveSpeed;
        if (move == Vector2.zero && !climbing) targetSpeed = 0f;

        // 3. Current horizontal speed
        Vector3 horizontalVelocity = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        float inputMagnitude = analogMovement ? move.magnitude : 1f;
        float speedOffset = 0.1f;

        // 4. Smooth speed change
        if (Mathf.Abs(currentSpeed - targetSpeed) > speedOffset)
        {
            _speed = Mathf.Lerp(currentSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        // 5. Animation blend
        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // 6. Input direction
        Vector3 inputDir = new Vector3(move.x, 0f, move.y).normalized;

        // 7. Rotation (only if moving, not climbing, not dragging)
        if (move != Vector2.zero && !climbing && !drag)
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

        // 8. Movement direction
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
        else if (dragging && box != null)
        {
            float forwardAmount = move.y;

            if (!inDragZone){return;}

            if (Mathf.Abs(forwardAmount) > 0.1f)
            {
                Vector3 pushDirection = -boxNormal;
                float moveDir = Mathf.Sign(forwardAmount);
                Vector3 dragMove = pushDirection * moveDir * DragSpeed * Time.deltaTime;
                Vector3 boxMove = dragMove * _dragDirectionMultiplier;

                _controller.Move(dragMove + Vector3.up * _verticalVelocity * Time.deltaTime);
                box.position += boxMove;
            }
            else
            {
                _controller.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
            }

            Quaternion dragRotation = Quaternion.LookRotation(-boxNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, dragRotation, Time.deltaTime * 10f);

            return;
        }
        else
        {
            Vector3 moveDirection = Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward;
            velocity = moveDirection.normalized * _speed + Vector3.up * _verticalVelocity;
        }

        // 9. Apply movement (only reached if not dragging)
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

        // apply gravity over time if under terminal velocity
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
            // Don't multiply mouse input by Time.deltaTime
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
            float sensitivity = IsCurrentDeviceMouse ? mouseSensitivity : controllerSensitivity;

            _cinemachineTargetYaw += look.x * sensitivity * deltaTimeMultiplier;
            _cinemachineTargetPitch += look.y * sensitivity * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
            _cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw,
            0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    // ── Ledge detection ───────────────────────────────────────────────────────
    // Detects when the rat is airborne and approaching a ledge it can grab.
    // Vault animation is still detached — the rat will simply pull up
    // to the ledge position without playing a vault clip for now.
    private void CheckLedge()
    {
        if (Grounded || isHanging) return;

        Vector3 origin = transform.position + Vector3.up * ledgeHeight;
        Debug.DrawRay(origin, transform.forward * ledgeCheckDistance, Color.red);

        if (Physics.Raycast(origin, transform.forward, out RaycastHit wallHit, ledgeCheckDistance, ledgeLayer))
        {
            Vector3 downOrigin = wallHit.point + Vector3.up * 0.5f;
            Debug.DrawRay(downOrigin, Vector3.down * 1.5f, Color.green);

            if (Physics.Raycast(downOrigin, Vector3.down, out RaycastHit topHit, 1.5f, ledgeLayer))
            {
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

        Vector3 hangPos = ledgePoint - ledgeNormal * 0.5f;
        hangPos.y -= 1.2f;

        transform.position = hangPos;
        transform.rotation = Quaternion.LookRotation(-ledgeNormal);
    }

    // ── Ledge climb-up — no vault animation yet ───────────────────────────────
    // Uncomment the SetBool lines once you are ready to re-enable vault.
    private IEnumerator ClimbUpLedge()
    {
        isHanging = false;

        // _animator.SetBool("IsVaulting", true);  // re-enable with vault

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

        // _animator.SetBool("IsVaulting", false);  // re-enable with vault
    }

    public IEnumerator SwingToPosition(Vector3 targetPos, float duration = 0.8f)
    {
        isSwinging = true;
        _animator.SetBool("IsFreefall", true);
        
        Vector3 startPos = transform.position;
        
        // Arc midpoint — halfway between start and target, lifted upward
        Vector3 midPoint = (startPos + targetPos) / 2f + Vector3.up * 8f;
        
        float time = 0f;
        while (time < duration)
        {
            float t = time / duration;
            
            // Quadratic bezier curve for the arc
            Vector3 a = Vector3.Lerp(startPos, midPoint, t);
            Vector3 b = Vector3.Lerp(midPoint, targetPos, t);
            transform.position = Vector3.Lerp(a, b, t);

            
            
            time += Time.deltaTime;
            yield return null;
        }
        
        _animator.SetBool("IsFreefall", false);
        transform.position = targetPos;
        isSwinging = false;
    }


    public IEnumerator KnockBack(float duration = 1.5f)
    {
        isSwinging = true;

        // Direction AWAY from where the character is facing, in local space
        Vector3 knockDirection = transform.TransformDirection(Vector3.back);
        knockDirection.y = 0f; // Zero out Y so we control vertical separately
        knockDirection.Normalize();

        float horizontalSpeed = 6f;  // Tweak knockback distance feel
        float verticalSpeed   = 15f;  // Initial upward launch
        float gravity         = 20f; // How fast they fall back down

        float velocityY = verticalSpeed;
        float time = 0f;

        drag = false;        // Release drag state
        _dragLatch = false;  // Release latch

        _animator.SetBool("IsFreefall", true);
        _animator.SetBool("IsPushing", false);
        _animator.SetBool("IsPulling", false);
        _animator.SetBool("IsGrabIdle", false);

        while (time < duration)
        {
            velocityY -= gravity * Time.deltaTime; // Apply gravity each frame

            Vector3 moveVelocity = (knockDirection * horizontalSpeed + Vector3.up * velocityY) 
                                * Time.deltaTime;

            _controller.Move(moveVelocity); // Respects colliders

            time += Time.deltaTime;
            yield return null;
        }

        _animator.SetBool("IsFreefall", false);
        _animator.SetBool("IsGrounded", Grounded);
        _fallAnimationTimer = 0f;
        _fallAnimationActive = false;
        isSwinging = false;

        yield return new WaitForSeconds(2f);
    }
}