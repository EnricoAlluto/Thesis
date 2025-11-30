//The TornaAllaSceltaAvatar class manages the functionality for returning to the avatar selection screen.
//It provides a button that, when clicked, triggers the avatar selection process through the 
//AvatarSelection component. The class ensures proper initialization of button click handlers and 
//manages the transition back to the avatar selection interface.

using UnityEngine;
using UnityEngine.UI;
 
public class TornaAllaSceltaAvatar : MonoBehaviour 
{
    private Button tornaButton;
    private AvatarSelection avatarSelection;
    
    void Start()
    {
        tornaButton = GetComponent<Button>();
        
        avatarSelection = FindObjectOfType<AvatarSelection>();
        
        if (avatarSelection == null)
        {
            //Debug.LogError("AvatarSelection component not found in scene");
        }
        
        if (tornaButton != null)
        {
            tornaButton.onClick.RemoveAllListeners();
            tornaButton.onClick.AddListener(TornaIndietro);
        }
        else
        {
            //Debug.LogError("Button component not found on this GameObject");
        }
    }
    
    void TornaIndietro()
    {
        if (avatarSelection != null)
        {
            avatarSelection.TornaAllaSceltaAvatar();
        }
        else
        {
            avatarSelection = FindObjectOfType<AvatarSelection>();
            
            if (avatarSelection != null)
            {
                avatarSelection.TornaAllaSceltaAvatar();
            }
            else
            {
                //Debug.LogError("AvatarSelection component not found in scene");
            }
        }
    }
}