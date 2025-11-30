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
           // Debug.LogError("AvatarSelection component not found in scene");
        }
        
        if (tornaButton != null)
        {
            tornaButton.onClick.RemoveAllListeners();
            tornaButton.onClick.AddListener(TornaIndietro);
        }
        else
        {
           // Debug.LogError("Button component not found on this GameObject");
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