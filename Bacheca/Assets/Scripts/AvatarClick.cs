//This code checks if the avatarSelection object exists and, if so, opens the avatar's board 
//using the ApriBacheca method. If avatarSelection is not set, it tries to find it and then open 
//the board. In the Update method, it detects touch input on mobile devices, and 
//if the user touches this avatar, it selects the avatar.

using UnityEngine;

public class AvatarClick : MonoBehaviour
{
    public int avatarIndex;
    private AvatarSelection avatarSelection;
    private AvatarManager avatarManager;
    
    void Start()
    {
        avatarSelection = FindObjectOfType<AvatarSelection>();
        avatarManager = FindObjectOfType<AvatarManager>();
    }
    
    void OnMouseDown()
    {
        SelectAvatar();
    }
    
    public void SelectAvatar()
    {
        if (avatarManager != null)
        {
            avatarManager.OnAvatarClicked(avatarIndex);
        }
        else
        {
            OpenAvatarBacheca();
        }
    }

    public void OpenAvatarBacheca()
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