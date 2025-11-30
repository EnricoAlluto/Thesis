//The TornaAllaBacheca class manages the functionality for returning to the main bulletin board screen 
//from other parts of the application. It handles both UI button clicks and the Android 
//back button press, triggering the appropriate navigation through the AvatarSelection component. 
//If no navigation target is available, it provides a fallback to exit the application.

using UnityEngine;
using UnityEngine.UI;

public class TornaAllaBacheca : MonoBehaviour
{
    private AvatarSelection avatarSelection;
    
    void Start()
    {
        avatarSelection = FindObjectOfType<AvatarSelection>();
        
        if (avatarSelection == null)
        {
            //Debug.LogError("AvatarSelection not found in scene!");
        }

        Button backButton = GetComponent<Button>();
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(TornaIndietro);
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
            avatarSelection.TornaDallaStanzaAllaBacheca();
        }
        else
        {
            avatarSelection = FindObjectOfType<AvatarSelection>();
            
            if (avatarSelection != null)
            {
                avatarSelection.TornaDallaStanzaAllaBacheca();
            }
            else
            {
                //Debug.LogError("AvatarSelection not found in scene!");
            }
        }
    }
}