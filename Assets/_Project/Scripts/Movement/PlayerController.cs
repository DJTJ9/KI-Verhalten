using UnityEngine;
using UnityEngine.InputSystem;
using BlackboardSystem;

public class PlayerController : MonoBehaviour {
    [Header("Movement")]
    public RigidbodyMovement RigidbodyMovement;
    public CameraRotator CameraRotator;

    [Header("Interact")]
    public Transform MainCameraTransform;
    public Transform ObjectGrabPoint;
    private GrabbableObject grabbableObject;
    public LayerMask PickUpLayer;

    [Header("Input")]
    public PlayerInput PlayerInput;

    [Header("Settings")]
    public float LookSensitivity = 2;
    
    [Header("Blackboard")]
    [SerializeField] BlackboardData blackboardData;
    Blackboard blackboard;
    BlackboardKey BallInHandKey;
    BlackboardKey BallThrownKey;

    private InputAction moveInputAction;
    private InputAction jumpInputAction;
    private InputAction lookInputAction;
    private InputAction grabInputAction;
    private InputAction dropInputAction;
    private InputAction throwInputAction;

    private GrabbableObject objectGrabbable;
    private PlayerInteraction playerInteraction;
    private float pickUpDistance = 2f;
    private bool BallInHand = false;

    private void Awake() {
        MapInputActions();
        blackboard = BlackboardManager.SharedBlackboard;
        BallInHandKey = blackboard.GetOrRegisterKey("BallInHand");
        BallThrownKey = blackboard.GetOrRegisterKey("BallThrown");
        // blackboardData.SetValuesOnBlackboard(blackboard);
        // BallThrownKey = blackboard.GetOrRegisterKey("BallThrown");
    }

    /// <summary>
    /// Sets cursor lock mode on left click to locked and on escape to none.
    /// Gets move direction from input and moves rigidbody into this direction.
    /// Rotates the rigidbody horizontally if cursor lock mode is locked.
    /// </summary>
    private void Update() {
        if (Mouse.current.leftButton.wasPressedThisFrame) Cursor.lockState = CursorLockMode.Locked;
        if (Keyboard.current.escapeKey.wasPressedThisFrame) Cursor.lockState = CursorLockMode.None;


        var moveDirection = GetMoveDirectionFromInput();
        RigidbodyMovement.Move(moveDirection);

        if (Cursor.lockState == CursorLockMode.Locked) {
            var rotation = GetRotationFromInput();
            RigidbodyMovement.RotateHorizontal(rotation.x * LookSensitivity);
        }
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            BlackboardManager.SharedBlackboard.Debug();
        }
    }

    /// <summary>
    /// Rotates camera vertically if cursor lock mode is locked.
    /// </summary>
    private void LateUpdate() {
        if (Cursor.lockState == CursorLockMode.Locked) {
            if (CameraRotator != null)
                UpdateCamera();
        }
    }

    /// <summary>
    /// Gets rotation from input
    /// Rotates camera in the direction of the rotation input
    /// </summary>
    private void UpdateCamera() {
        var rotation = GetRotationFromInput();
        CameraRotator.Rotate(rotation.y);
    }

    /// <summary>
    /// Maps the input actions
    /// Subcribes the OnJumpInput method to the jump started action
    /// </summary>
    private void MapInputActions() {
        moveInputAction = PlayerInput.actions["Move"];

        jumpInputAction = PlayerInput.actions["Jump"];
        jumpInputAction.started += OnJumpInput;

        lookInputAction = PlayerInput.actions["Look"];

        grabInputAction = PlayerInput.actions["Interact"];
        grabInputAction.started += OnGrabInput;

        dropInputAction = PlayerInput.actions["Drop"];
        dropInputAction.started += OnDropInput;
        
        throwInputAction = PlayerInput.actions["Throw"];
        throwInputAction.started += OnThrowInput;
    }

    private void OnJumpInput(InputAction.CallbackContext _context) {
        if (_context.phase == InputActionPhase.Started)
            RigidbodyMovement.Jump();
    }

    /// <summary>
    /// Gets the horizontal move direction from the input
    /// Converts this input into a 3D vector and returns it
    /// </summary>
    private Vector3 GetMoveDirectionFromInput() {
        var moveInput = moveInputAction.ReadValue<Vector2>();
        return new Vector3(moveInput.x, 0f, moveInput.y);

    }

    /// <summary>
    /// Gets the rotation input and returns it
    /// </summary>
    private Vector2 GetRotationFromInput() {
        return lookInputAction.ReadValue<Vector2>();
    }

    private void OnGrabInput(InputAction.CallbackContext _context) {
        if (_context.phase == InputActionPhase.Started)
            if (grabbableObject == null) {
                if (Physics.Raycast(MainCameraTransform.position, MainCameraTransform.forward, out RaycastHit raycastHit, pickUpDistance)) {
                    if (raycastHit.transform.TryGetComponent(out GrabbableObject grabbableObject)) {
                        grabbableObject.Grab(ObjectGrabPoint);
                        this.grabbableObject = grabbableObject;
                        blackboard.SetValue(BallInHandKey, true);
                        blackboard.SetValue(BallThrownKey, false);
                        blackboard.TryGetValue(BallInHandKey, out bool ballInHand);
                        Debug.Log($"BallInHand: {ballInHand}");
                    }
                }
            }
    }

    private void OnDropInput(InputAction.CallbackContext _context) {
        grabbableObject.Drop();
        grabbableObject = null;
        blackboard.SetValue(BallInHandKey, false);
    }
    
    private void OnThrowInput(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            if (blackboard.TryGetValue(BallInHandKey, out bool ballInHand) && ballInHand) {
                grabbableObject.Throw(MainCameraTransform.forward);
                grabbableObject = null;
                blackboard.SetValue(BallInHandKey, false);
                blackboard.SetValue(BallThrownKey, true);
                blackboard.TryGetValue(BallThrownKey, out bool value);
                Debug.Log($"BallThrown: {value}");
            }
            // else {
            //     blackboard.SetValue(BallThrownKey, false);
            // }
        }
    }
}
