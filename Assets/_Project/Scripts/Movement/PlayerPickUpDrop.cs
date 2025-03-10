using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickUpDrop : MonoBehaviour
{
    [Header("References")]
    public Transform MainCameraTransform;
    public Transform ObjectGrabPoint;
    public LayerMask PickUpLayer;

    private GrabbableObject grabbableObject;
    private PlayerInteraction playerInteraction;

    private float pickUpDistance = 2f;

    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (grabbableObject == null)
            {
                if (Physics.Raycast(MainCameraTransform.position, MainCameraTransform.forward, out RaycastHit raycastHit, pickUpDistance))
                {
                    if (raycastHit.transform.TryGetComponent(out GrabbableObject grabbableObject))
                    {
                        grabbableObject.Grab(ObjectGrabPoint);
                    }
                }
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
            grabbableObject.Drop();
    }
}
