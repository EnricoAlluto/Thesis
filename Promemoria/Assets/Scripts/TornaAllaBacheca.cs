//The TornaAllaBacheca class manages the functionality for returning to the main bulletin board screen 
//from other parts of the application. It handles both UI button clicks and the Android 
//back button press, triggering the appropriate navigation through the AvatarSelection component. 
//If no navigation target is available, it provides a fallback to exit the application.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TornaAllaBacheca : MonoBehaviour
{
    private AvatarSelection avatarSelection;
    [SerializeField] private Button backButton;
    
    void Start()
    {
        if (avatarSelection == null)
        {
            avatarSelection = FindObjectOfType<AvatarSelection>();
            if (avatarSelection == null)
            {
                //Debug.LogWarning("AvatarSelection not found in scene!");
            }
        }

        if (backButton == null)
        {
            backButton = GetComponent<Button>();
        }
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(TornaIndietro);
        }
        else
        {
            //Debug.LogError("Button component not found!");
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TornaIndietro();
        }
    }
     
    public void TornaIndietro()
    {
        if (avatarSelection != null)
        {
            avatarSelection.TornaDallaStanzaAllaBacheca();
        }
        else
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}