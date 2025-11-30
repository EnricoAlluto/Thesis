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