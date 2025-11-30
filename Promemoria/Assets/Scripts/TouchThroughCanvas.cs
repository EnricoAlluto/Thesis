//This script enables click/touch events to pass through a UI element when the user taps on empty space.
//It's useful for creating transparent UI panels that only block interaction with specific elements 
//while allowing clicks to reach objects behind them. If no UI element is clicked, 
//the touch event is forwarded to the parent object.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TouchThroughCanvas : MonoBehaviour, IPointerClickHandler
{
    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;

    private void Start()
    {
        raycaster = GetComponent<GraphicRaycaster>();
        eventSystem = EventSystem.current;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        eventSystem.RaycastAll(eventData, results);
    
        if (results.Count == 0)
        {
            ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, 
                ExecuteEvents.pointerClickHandler);
        }
    }
}