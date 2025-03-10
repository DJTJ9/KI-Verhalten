using UnityEngine;

[RequireComponent (typeof(Rigidbody), typeof(GroundChecker))]
public class RigidbodyMovement : MonoBehaviour
{
    [Header("Settings")]
    public float Speed;
    public float MaxSpeed;
    public float JumpPower;
    public float JumpSpeedModifier = 1;
    public float FallSpeedModifier = 1;

    private new Transform transform;
    private new Rigidbody rigidbody;
    private GroundChecker groundChecker;

    private Vector3 moveDirection;

    private void Awake()
    {
        transform = GetComponent<Transform>();
        rigidbody = GetComponent<Rigidbody>();
        groundChecker = GetComponent<GroundChecker>();
    }

    private void FixedUpdate()
    {
        UpdateHorizontalMovement();
        UpdateVerticalMovement();
    }

    /// <summary>
    /// Recieves a move direction
    /// </summary>
    public void Move(Vector3 _direction)
    {
        moveDirection = _direction;
    }

    public void Jump()
    {
        if (groundChecker.isGrounded == true)
            rigidbody.AddForce(Vector3.up * JumpPower, ForceMode.Impulse);
    }

    /// <summary>
    /// Collects the current Velocity of the rigidbody and sets the speed
    /// Transforms moving direction from local space to world space
    /// Collects the speed difference to target velocity and clamps the max velocity
    /// Sets force mode to VelocityChange
    /// </summary>
    public void UpdateHorizontalMovement()
    {
        Vector3 currentVelocity = rigidbody.linearVelocity;
        Vector3 targetVelocity = new Vector3(moveDirection.x, 0f , moveDirection.z);
        targetVelocity *= Speed;

        targetVelocity = transform.TransformDirection(targetVelocity);

        Vector3 velocityChange = targetVelocity - currentVelocity;
        velocityChange = new Vector3(velocityChange.x, 0f, velocityChange.z);
        velocityChange = Vector3.ClampMagnitude(velocityChange, MaxSpeed);

        rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    /// <summary>
    /// Recieves the current rotation
    /// Sets the rotation to a target rotation
    /// </summary>
    public void RotateHorizontal(float _rotation)
    {
        var currentRotation = rigidbody.rotation.eulerAngles;
        var targetRotation = currentRotation + new Vector3(0f, _rotation, 0f);
        rigidbody.rotation = Quaternion.Euler(targetRotation);
    }

    /// <summary>
    /// Modifies jump and fall speed
    /// </summary>
    private void UpdateVerticalMovement()
    {
        if (rigidbody.linearVelocity.y < 0)
            rigidbody.linearVelocity += Vector3.up * Physics.gravity.y * (FallSpeedModifier - 1) * Time.fixedDeltaTime;

        if (rigidbody.linearVelocity.y > 0)
            rigidbody.linearVelocity += Vector3.up * Physics.gravity.y * JumpSpeedModifier * Time.fixedDeltaTime;
    }
}
