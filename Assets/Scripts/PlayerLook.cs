using UnityEngine;

namespace Minecraft
{
    /// <summary>
    /// This is based on this tutorial: https://youtu.be/_QajrabyTJc
    /// </summary>
    public sealed class PlayerLook : MonoBehaviour
    {
        [SerializeField]
        private float mouseSensitivity = 10.0f;

        private float xRotation = -90.0f;

        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            var rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;

            xRotation += Input.GetAxis("Mouse Y") * mouseSensitivity;
            xRotation = Mathf.Clamp(xRotation, -90, 90);

            transform.localEulerAngles = new Vector3(-xRotation, rotationX, 0);
        }
    }
}
