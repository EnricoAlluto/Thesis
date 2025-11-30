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