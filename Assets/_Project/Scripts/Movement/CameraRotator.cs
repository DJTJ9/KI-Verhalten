using UnityEngine;

public class CameraRotator : MonoBehaviour {
    [Header("Settings")]
    public Transform CameraTransform;
    public float LookSensitivity = 0.3f;
    public Angles AngleSettings;

    private float cameraRotationY;

    private void Start() {
        cameraRotationY = CameraTransform.rotation.eulerAngles.x;
    }

    /// <summary>
    /// Rotates the camera in opposite direction of the input (because input is inverted)
    /// Clamps the rotation around the x-axis
    /// </summary>
    public void Rotate(float _rotation) {
        cameraRotationY += -_rotation * LookSensitivity;
        cameraRotationY = Mathf.Clamp(cameraRotationY, AngleSettings.Min, AngleSettings.Max);
        CameraTransform.eulerAngles = new Vector3(cameraRotationY, CameraTransform.eulerAngles.y, CameraTransform.eulerAngles.z);

    }

    [System.Serializable]
    public class Angles {
        public float Min = -90;
        public float Max = 90;
    }
}
