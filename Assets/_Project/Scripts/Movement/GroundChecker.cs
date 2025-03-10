using UnityEngine;

public class GroundChecker : MonoBehaviour {
    public bool isActive = true;

    [Header("Settings")]
    public LayerMask GroundCheckLayerMask;
    public Vector3 GroundCheckPosition;
    public Vector3 GroundCheckSize;

    [field: SerializeField]
    public bool isGrounded { get; private set; }

    private new Transform transform;

    private void Awake() {
        transform = GetComponent<Transform>();
    }

    private void Update() {
        if (isActive == true)
            CheckForGround();
    }

    private void CheckForGround() {
        isGrounded = Physics.OverlapBox(transform.position + GroundCheckPosition, GroundCheckSize / 2, Quaternion.identity, GroundCheckLayerMask).Length > 0;
    }

    private void OnDrawGizmosSelected() {
        this.transform = GetComponent<Transform>();

        Gizmos.DrawCube(transform.position + GroundCheckPosition, GroundCheckSize);
    }
}
