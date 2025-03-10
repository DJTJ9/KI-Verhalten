using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrabbableObject : MonoBehaviour
{
    public float ThrowSpeed = 50f;

    private Rigidbody objectRigidbody;
    private Transform objectGrabPoint;

    private float lerpSpeed = 10f;

    private void Awake() {
        objectRigidbody = GetComponent<Rigidbody>();
    }

    public void Grab(Transform _objectGrabPoint) {
        objectGrabPoint = _objectGrabPoint;
        objectRigidbody.useGravity = false;
        objectRigidbody.linearVelocity = Vector3.zero;
    }

    public void Drop() {
        if (objectGrabPoint != null) {
            objectGrabPoint = null;
            objectRigidbody.useGravity = true;
        }
    }

    public void Throw(Vector3 _direction) {
        objectGrabPoint = null;
        objectRigidbody.useGravity = true;
        objectRigidbody.AddForce(_direction * ThrowSpeed, ForceMode.Impulse);
    }

    private void FixedUpdate() {
        if (objectGrabPoint != null) {
            Vector3 newPosition = Vector3.Lerp(transform.position, objectGrabPoint.position, lerpSpeed * Time.deltaTime);
            objectRigidbody.MovePosition(newPosition);
        }
    }
}