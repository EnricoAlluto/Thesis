using UnityEngine;

namespace SojaExiles
{
    public class MouseLook : MonoBehaviour
    {
        [Header("Look Settings")]
        public float mouseSensitivity = 100f;
        public float lookSensitivity = 2f;
        public bool invertY = false;
        
        [Header("Look Limits")]
        public float maxLookAngle = 80f;
        public float minLookAngle = -80f;
        
        [Header("Mobile Controls")]
        public Joystick lookJoystick;
        
        // Private variables
        private float verticalRotation = 0;
        private bool lookingEnabled = true;
        private Transform playerTransform;
        
        // Mouse input
        private float mouseX;
        private float mouseY;
        
        void Start()
        {
            // Get player transform
            PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
            if (playerMovement != null)
            {
                playerTransform = playerMovement.transform;
            }
            else
            {
                // Fallback - assume this is attached to camera and player is parent
                if (transform.parent != null)
                {
                    playerTransform = transform.parent;
                }
            }
            
            // Auto-assign joystick if not set
            if (lookJoystick == null)
            {
                Joystick[] joysticks = FindObjectsOfType<Joystick>();
                foreach (var joystick in joysticks)
                {
                    // Try to find the look joystick (usually on the right side)
                    if (joystick.name.ToLower().Contains("look") || 
                        joystick.name.ToLower().Contains("right") ||
                        joystick.transform.position.x > Screen.width * 0.5f)
                    {
                        lookJoystick = joystick;
                        break;
                    }
                }
            }
            
            // Lock cursor on desktop
            #if UNITY_STANDALONE || UNITY_EDITOR
            Cursor.lockState = CursorLockMode.Locked;
            #endif
        }
        
        void Update()
        {
            if (!lookingEnabled) return;
            
            GetInput();
            ApplyRotation();
        }
        
        void GetInput()
        {
            #if UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
            // Mobile input using joystick
            if (lookJoystick != null)
            {
                mouseX = lookJoystick.Horizontal * lookSensitivity * 20f; // Multiplied for better sensitivity
                mouseY = lookJoystick.Vertical * lookSensitivity * 20f;
            }
            else
            {
                mouseX = 0;
                mouseY = 0;
            }
            #else
            // Desktop mouse input
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            #endif
            
            if (invertY)
            {
                mouseY = -mouseY;
            }
        }
        
        void ApplyRotation()
        {
            // Rotate player horizontally
            if (playerTransform != null)
            {
                playerTransform.Rotate(Vector3.up * mouseX);
            }
            
            // Rotate camera vertically
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, minLookAngle, maxLookAngle);
            transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
        
        public void SetLookingEnabled(bool enabled)
        {
            lookingEnabled = enabled;
        }
        
        public void SetSensitivity(float sensitivity)
        {
            mouseSensitivity = sensitivity;
            lookSensitivity = sensitivity / 50f; // Adjust mobile sensitivity accordingly
        }
        
        public void SetInvertY(bool invert)
        {
            invertY = invert;
        }
    }
}