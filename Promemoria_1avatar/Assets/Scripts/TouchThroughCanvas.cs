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