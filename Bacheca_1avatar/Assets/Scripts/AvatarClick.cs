using UnityEngine;

public class AvatarClick : MonoBehaviour 
{
    public int avatarIndex;
    private AvatarSelection avatarSelection;
    
    void Start()
    {
        avatarSelection = FindObjectOfType<AvatarSelection>();
    }
    
    void OnMouseDown()
    {
        SelectAvatar();
    }
    
    public void SelectAvatar()
    {
        if (avatarSelection != null)
        {
            avatarSelection.ApriBacheca(avatarIndex);
        }
        else
        {
            avatarSelection = FindObjectOfType<AvatarSelection>();
            if (avatarSelection != null)
            {
                avatarSelection.ApriBacheca(avatarIndex);
            }
        }
    }
    
    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit) && hit.transform == transform)
            {
                SelectAvatar();
            }
        }
    }
}