using UnityEngine;

namespace SojaExiles
{
    public abstract class BaseInteractable : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        public bool isInteractable = true;
        public float interactionDistance = 3f;
        public string interactionPrompt = "Press to interact";
        
        [Header("Visual Feedback")]
        public bool showHighlight = true;
        public Color highlightColor = Color.yellow;
        public Material highlightMaterial;
        
        // Protected variables
        protected Renderer objectRenderer;
        protected Material originalMaterial;
        protected bool isHighlighted = false;
        
        protected virtual void Start()
        {
            // Get renderer for highlighting
            objectRenderer = GetComponent<Renderer>();
            if (objectRenderer != null)
            {
                originalMaterial = objectRenderer.material;
            }
            
            // Set the object to the Interactable layer if it exists
            int interactableLayerIndex = LayerMask.NameToLayer("Interactable");
            if (interactableLayerIndex != -1)
            {
                gameObject.layer = interactableLayerIndex;
            }
        }
        
        // IInteractable implementation
        public virtual void OnInteract()
        {
            if (!isInteractable)
                return;
                
            HandleInteraction();
        }
        
        // Abstract method that child classes must implement
        protected abstract void HandleInteraction();
        
        // Highlight the object
        public virtual void Highlight()
        {
            if (!showHighlight || isHighlighted || objectRenderer == null)
                return;
                
            isHighlighted = true;
            
            if (highlightMaterial != null)
            {
                objectRenderer.material = highlightMaterial;
            }
            else
            {
                // Create a simple highlight effect by changing color
                Material highlightMat = new Material(originalMaterial);
                highlightMat.color = highlightColor;
                objectRenderer.material = highlightMat;
            }
        }
        
        // Remove highlight
        public virtual void RemoveHighlight()
        {
            if (!isHighlighted || objectRenderer == null)
                return;
                
            isHighlighted = false;
            objectRenderer.material = originalMaterial;
        }
        
        // Enable/disable interaction
        public virtual void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
            
            if (!interactable)
            {
                RemoveHighlight();
            }
        }
        
        // Get interaction prompt for UI
        public virtual string GetInteractionPrompt()
        {
            return isInteractable ? interactionPrompt : "";
        }
        
        // Check if player is within interaction distance
        public virtual bool IsPlayerInRange(Transform playerTransform)
        {
            if (playerTransform == null)
                return false;
                
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            return distance <= interactionDistance;
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw interaction range in the editor
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}