using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour 
{
    public Camera playerCamera;
    public float interactionDistance = 3f;
    public LayerMask interactionLayer = -1;
    
    public float minTouchTime = 0.1f;
    public float maxTouchTime = 0.5f; 
    
    private float touchStartTime;
    private Vector2 touchStartPosition;
    private bool isValidTouch = false;
    
    private void Update()
    {
        HandleTouchInput();
    }
    
    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnTouchBegan(touch);
                    break;
                    
                case TouchPhase.Ended:
                    OnTouchEnded(touch);
                    break;
                    
                case TouchPhase.Moved:
                    OnTouchMoved(touch);
                    break;
                    
                case TouchPhase.Canceled:
                    ResetTouch();
                    break;
            }
        }
    }
    
    private void OnTouchBegan(Touch touch)
    {
        if (IsTouchOverUI(touch))
        {
            isValidTouch = false;
            return;
        }
        
        touchStartTime = Time.time;
        touchStartPosition = touch.position;
        isValidTouch = true;
    }
    
    private void OnTouchEnded(Touch touch)
    {
        if (!isValidTouch) return;
        
        float touchDuration = Time.time - touchStartTime;
        float touchDistance = Vector2.Distance(touch.position, touchStartPosition);
        
        if (touchDuration >= minTouchTime && 
            touchDuration <= maxTouchTime && 
            touchDistance < 50f)
        {
            PerformInteraction(touch);
        }
        
        ResetTouch();
    }
    
    private void OnTouchMoved(Touch touch)
    {
        if (!isValidTouch) return;
        
        float touchDistance = Vector2.Distance(touch.position, touchStartPosition);

        if (touchDistance > 100f)
        {
            isValidTouch = false;
        }
    }
    
    private void PerformInteraction(Touch touch)
    {
        Ray ray = playerCamera.ScreenPointToRay(touch.position);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionDistance, interactionLayer))
        {
            AvatarInteraction avatar = hit.collider.GetComponent<AvatarInteraction>();
            if (avatar != null)
            {
                avatar.Interact();
            }
        }
    }
    
    private bool IsTouchOverUI(Touch touch)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = touch.position;
        
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<UnityEngine.UI.Button>() != null ||
                result.gameObject.GetComponent<UnityEngine.UI.Scrollbar>() != null ||
                result.gameObject.GetComponent<UnityEngine.UI.Toggle>() != null ||
                result.gameObject.GetComponent<UnityEngine.UI.Slider>() != null ||
                result.gameObject.name.Contains("Joystick") ||
                result.gameObject.tag == "UIControl")
            {
                return true;
            }
        }
        
        return false;
    }
    
    private void ResetTouch()
    {
        isValidTouch = false;
        touchStartTime = 0f;
        touchStartPosition = Vector2.zero;
    }
    
    private void OnDrawGizmos()
    {
        if (playerCamera != null && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = playerCamera.ScreenPointToRay(touch.position);
            
            Gizmos.color = Color.red;
            Gizmos.DrawRay(ray.origin, ray.direction * interactionDistance);
        }
    }
}