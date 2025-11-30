using UnityEngine;

namespace SojaExiles
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float walkSpeed = 30f;
        public float runSpeed = 40f;
        public float jumpHeight = 2f;
        public float gravity = -9.81f;
        public float groundDistance = 0.4f;
        
        [Header("Ground Check")]
        public Transform groundCheck;
        public LayerMask groundMask = 1;
        
        [Header("Mobile Controls")]
        public Joystick movementJoystick;
        public float joystickSensitivity = 1f;
        
        // Private variables
        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        private bool isRunning;
        private bool movementLocked = false;
        
        // Input variables
        private float horizontal;
        private float vertical;
        
        void Start()
        {
            controller = GetComponent<CharacterController>();
            
            // Create ground check if not assigned
            if (groundCheck == null)
            {
                GameObject groundCheckObj = new GameObject("GroundCheck");
                groundCheckObj.transform.SetParent(transform);
                groundCheckObj.transform.localPosition = new Vector3(0, -1f, 0);
                groundCheck = groundCheckObj.transform;
            }
            
            // Auto-assign joystick if not set
            if (movementJoystick == null)
            {
                movementJoystick = FindObjectOfType<Joystick>();
            }
        }    

        void GetInput()
        {
            #if UNITY_WEBGL || UNITY_ANDROID || UNITY_IOS
            if (movementJoystick != null && movementJoystick.gameObject.activeInHierarchy)
            {
                // SOLO movimento - nessuna rotazione qui
                horizontal = movementJoystick.Horizontal * joystickSensitivity;
                vertical = movementJoystick.Vertical * joystickSensitivity;
            }
            else
            #endif
            {
                // Input da tastiera
                horizontal = Input.GetAxis("Horizontal");
                vertical = Input.GetAxis("Vertical");
            }
        }

        void Update()
        {
            if (movementLocked) return;
            
            // Ground check
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            
            // Get input - SOLO per movimento
            GetInput();
            
            // Move player - senza rotazione
            Move();
            
            // Apply gravity
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
        
        void Move()
        {
            // Movimento relativo alla direzione del personaggio
            Vector3 direction = transform.right * horizontal + transform.forward * vertical;
            float speed = isRunning ? runSpeed : walkSpeed;
            
            controller.Move(direction * speed * Time.deltaTime);
        }
        
        public void Jump()
        {
            if (isGrounded && !movementLocked)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        
        public void TryJump()
        {
            Jump();
        }
        
        public void SetRunning(bool running)
        {
            isRunning = running;
        }
        
        public void LockMovement(bool locked)
        {
            movementLocked = locked;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
            }
        }
    }
}