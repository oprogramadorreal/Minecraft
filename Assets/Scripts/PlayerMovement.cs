using UnityEngine;

namespace Minecraft
{
    /// <summary>
    /// This is based on this tutorial: https://youtu.be/NEUzB5vPYrE
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField]
        private float walkSpeed = 2.0f;

        [SerializeField]
        private float runSpeed = 4.0f;

        [SerializeField]
        private float playerJumpForce = 2000.0f;

        [SerializeField]
        private ForceMode appliedForceMode = ForceMode.Force;

        [SerializeField]
        private Transform playerHead;

        [SerializeField]
        private Transform playerFeet;

        private bool isJumping;

        private float currentSpeed;

        private float inputAxisX;
        private float inputAxisZ;

        private Rigidbody playerRigidbody;
        private bool isCapslockPressedDown;

        private void Start()
        {
            playerRigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            inputAxisX = Input.GetAxis("Horizontal");
            inputAxisZ = Input.GetAxis("Vertical");

            currentSpeed = isCapslockPressedDown ? runSpeed : walkSpeed;

            isJumping = Input.GetButton("Jump");

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
            {
                var distanceFromTheGround = Vector3.Distance(playerFeet.position, hit.point);

                if (distanceFromTheGround > 0.2f)
                {
                    isJumping = false;
                }
            }
        }

        private void FixedUpdate()
        {
            playerRigidbody.MovePosition(transform.position + Time.deltaTime * currentSpeed * GetMoveDirection());

            if (isJumping)
            {
                Jump();
            }
        }

        private Vector3 GetMoveDirection()
        {
            var moveDirection = playerHead.TransformDirection(inputAxisX, 0f, inputAxisZ);
            moveDirection = Vector3.ProjectOnPlane(moveDirection, Vector3.up);
            return moveDirection;
        }

        private void OnGUI()
        {
            isCapslockPressedDown = Event.current.capsLock;
        }

        private void Jump()
        {
            playerRigidbody.AddForce(playerJumpForce * playerRigidbody.mass * Time.deltaTime * Vector3.up, appliedForceMode);
        }
    }
}