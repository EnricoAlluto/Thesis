using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SojaExiles
{
    public class TouchManager : MonoBehaviour
    {
        public static TouchManager Instance;
        
        [Header("Touch Settings")]
        public float touchRaycastDistance = 3f;
        public LayerMask interactableLayer = -1; // Default to all layers
        public bool showDebugRay = true;
        public Color debugRayColor = Color.red;

        [Header("Touch Controls")]
        public bool enableTouchInput = true;
        public bool requireDoubleTapForInteraction = false;
        public float doubleTapTimeThreshold = 0.3f;
         
        // Private variables
        private Camera mainCamera;
        private float lastTapTime;
        private int tapCount = 0;
        private const string INTERACTABLE_LAYER = "Interactable";

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
            
            // Set up interactable layer if not set
            if (interactableLayer == 0)
            {
                int layerIndex = LayerMask.NameToLayer(INTERACTABLE_LAYER);
                if (layerIndex != -1)
                {
                    interactableLayer = 1 << layerIndex;
                }
                else
                {
                    interactableLayer = -1; // All layers
                    Debug.LogWarning($"Layer '{INTERACTABLE_LAYER}' does not exist. Using all layers for interaction.");
                }
            }
        }

        private void Update()
        {
            if (!enableTouchInput || mainCamera == null)
                return;

            // Handle input for both editor and mobile
            HandleEditorInput();
            HandleMobileInput();
        }

        private void HandleMobileInput()
        {
            // Process touches - sempre attivo, non solo su mobile
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0); // Use first touch
                
                // Check if touch is over UI element
                if (IsTouchOverUI(touch.fingerId))
                    return;

                // Handle touch phases
                if (touch.phase == TouchPhase.Ended)
                {
                    if (requireDoubleTapForInteraction)
                    {
                        HandleDoubleTap(touch.position);
                    }
                    else
                    {
                        ProcessTouchInteraction(touch.position);
                    }
                }
                
                // Draw debug ray
                if (showDebugRay)
                {
                    Ray ray = mainCamera.ScreenPointToRay(touch.position);
                    Debug.DrawRay(ray.origin, ray.direction * touchRaycastDistance, debugRayColor, 0.1f);
                }
            }
        }

        private void HandleEditorInput()
        {
            // Handle mouse input in the editor - sempre attivo
            if (Input.GetMouseButtonUp(0))
            {
                // Ignore clicks on UI elements
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;
                    
                if (requireDoubleTapForInteraction)
                {
                    HandleDoubleTap(Input.mousePosition);
                }
                else
                {
                    ProcessTouchInteraction(Input.mousePosition);
                }
            }
            
            // Draw debug ray in the editor
            if (showDebugRay && Input.GetMouseButton(0))
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                Debug.DrawRay(ray.origin, ray.direction * touchRaycastDistance, debugRayColor, 0.1f);
            }
        }

        private bool IsTouchOverUI(int fingerId)
        {
            if (EventSystem.current == null)
                return false;

            if (Application.isEditor)
            {
                return EventSystem.current.IsPointerOverGameObject();
            }
            else
            {
                return EventSystem.current.IsPointerOverGameObject(fingerId);
            }
        }

        private void HandleDoubleTap(Vector2 touchPosition)
        {
            float currentTime = Time.time;
            
            if (currentTime - lastTapTime < doubleTapTimeThreshold)
            {
                tapCount++;
                if (tapCount >= 2)
                {
                    ProcessTouchInteraction(touchPosition);
                    tapCount = 0;
                }
            }
            else
            {
                tapCount = 1;
            }
            
            lastTapTime = currentTime;
        }

        private void ProcessTouchInteraction(Vector2 screenPosition)
        {
            if (mainCamera == null)
                return;
                
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, touchRaycastDistance, interactableLayer))
            {
                // Try IInteractable interface first
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.OnInteract();
                    Debug.Log($"Interacted with: {hit.collider.name}");
                    return;
                }
                
                // Fallback to BaseInteractable
                BaseInteractable baseInteractable = hit.collider.GetComponent<BaseInteractable>();
                if (baseInteractable != null)
                {
                    baseInteractable.OnInteract();
                    Debug.Log($"Interacted with: {hit.collider.name}");
                    return;
                }
                
                // Final fallback to OnMouseDown
                MonoBehaviour[] components = hit.collider.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour component in components)
                {
                    var method = component.GetType().GetMethod("OnMouseDown");
                    if (method != null && method.DeclaringType != typeof(MonoBehaviour))
                    {
                        component.SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver);
                        Debug.Log($"Sent OnMouseDown to: {hit.collider.name}");
                        break;
                    }
                }
            }
        }
          
        // Public method to interact with object at screen center (for interact button)
        public void InteractAtScreenCenter()
        {
            Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
            ProcessTouchInteraction(screenCenter);
        }
        
        // Call this to temporarily disable touch input
        public void SetTouchInputEnabled(bool enabled)
        {
            enableTouchInput = enabled;
        }
    }

    // Interface for interactable objects
    public interface IInteractable
    {
        void OnInteract();
    }
}