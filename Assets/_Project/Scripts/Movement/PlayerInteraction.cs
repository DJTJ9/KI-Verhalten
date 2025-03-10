using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")] public float rayLength = 1.5f;

    public RaycastHit hit;

    public Renderer crosshairRenderer;

    Transform mainCameraTransform;

    private void Start() {
        mainCameraTransform = Camera.main.transform;
    }

    /// <summary>
    /// Creates a raycast and collects hit info in hit.
    /// Changes crosshair color to red when raycast hits a target.
    /// Changes color of object hit to green on left mouse botton click.
    /// </summary>
    private void Update() {
        Physics.Raycast(mainCameraTransform.position, mainCameraTransform.forward, out hit, rayLength);

        if (hit.transform) {
            crosshairRenderer.material.color = Color.red;

            if (Mouse.current.leftButton.wasPressedThisFrame)
                hit.transform.GetComponent<Renderer>().material.color = Color.green;
        }
        else
            crosshairRenderer.material.color = Color.black;

        Debug.DrawRay(mainCameraTransform.position, transform.forward * rayLength, Color.red);
    }
}