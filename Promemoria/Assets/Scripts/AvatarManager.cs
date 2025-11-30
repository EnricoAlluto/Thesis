//The AvatarManager class handles avatar authentication and access control in the game. 
//It manages a collection of avatars, each with optional password protection. 
//When an avatar is clicked, it shows a password prompt for either creating a new password 
//(for first-time access) or entering an existing one. It verifies credentials, 
//saves password data using PlayerPrefs, and grants access to the avatar's board upon successful 
//authentication. The class also loads saved avatar data when the game starts, 
//maintaining user sessions between play sessions.

using System.Collections.Generic;
using UnityEngine;
using TMPro;
 
[System.Serializable]
public class AvatarData
{
    public string avatarName;
    public string password;
    public bool hasPassword;
    public Sprite avatarSprite;
    
    public AvatarData(string name, Sprite sprite)
    {
        avatarName = name;
        avatarSprite = sprite;
        password = "";
        hasPassword = false;
    }
}
public class AvatarManager : MonoBehaviour
{
    [SerializeField] private List<AvatarData> avatars = new List<AvatarData>();
    [SerializeField] private GameObject[] avatarObjects = new GameObject[10];
    [SerializeField] private GameObject passwordPanel;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TextMeshProUGUI passwordPromptText;
    [SerializeField] private UnityEngine.UI.Button confirmButton;
    
    private int currentAvatarIndex = -1;
    private bool isCreatingPassword = false;
    
    void Start()
    {
        InitializeAvatars();
        LoadAvatarData();
        SetupPasswordPanel();
    }
    
    void InitializeAvatars()
    {
        for (int i = 0; i < 10; i++)
        {
            AvatarData newAvatar = new AvatarData("Avatar " + (i + 1), null);
            avatars.Add(newAvatar);
        }
    }
    

    
    void SetupPasswordPanel()
    {
        passwordPanel.SetActive(false);
        confirmButton.onClick.AddListener(OnConfirmPassword);
    }
    
    public void OnAvatarClicked(int avatarIndex)
    {
        currentAvatarIndex = avatarIndex;
        AvatarData selectedAvatar = avatars[avatarIndex];
        
        if (!selectedAvatar.hasPassword)
        {
            isCreatingPassword = true;
            passwordPromptText.text = "Crea una password per " + selectedAvatar.avatarName;
            passwordInput.placeholder.GetComponent<TextMeshProUGUI>().text = "Inserisci nuova password";
        }
        else
        {
            isCreatingPassword = false;
            passwordPromptText.text = "Inserisci la password per " + selectedAvatar.avatarName;
            passwordInput.placeholder.GetComponent<TextMeshProUGUI>().text = "Password";
        }
        
        passwordInput.text = "";
        passwordPanel.SetActive(true);
        passwordInput.Select();
    }
    
    public void OnConfirmPassword()
    {
        string enteredPassword = passwordInput.text;
        
        if (string.IsNullOrEmpty(enteredPassword))
        {
            ShowMessage("Inserisci una password!");
            return;
        }
        
        AvatarData selectedAvatar = avatars[currentAvatarIndex];
        
        if (isCreatingPassword)
        {
            selectedAvatar.password = enteredPassword;
            selectedAvatar.hasPassword = true;
            ShowMessage("Password creata con successo!");
        
            SaveAvatarData();
            passwordPanel.SetActive(false);

            AccessAvatar(selectedAvatar);
        }
        else
        {
            if (enteredPassword == selectedAvatar.password)
            {
                ShowMessage("Accesso riuscito!");
                passwordPanel.SetActive(false);
                AccessAvatar(selectedAvatar);
            }
            else
            {
                ShowMessage("Password errata!");
                passwordInput.text = "";
                passwordInput.Select();
            }
        }
    }
    
    void AccessAvatar(AvatarData avatar)
    {
       // Debug.Log("Accesso all'avatar: " + avatar.avatarName);
    
        AvatarClick[] avatarClicks = FindObjectsOfType<AvatarClick>();
        foreach (AvatarClick avatarClick in avatarClicks)
        {
            if (avatarClick.avatarIndex == currentAvatarIndex)
            {
                avatarClick.OpenAvatarBacheca();
                break;
            }
        }
    }
    
    void ShowMessage(string message)
    {
        //bug.Log(message);
    }
    
    void SaveAvatarData()
    {
        for (int i = 0; i < avatars.Count; i++)
        {
            PlayerPrefs.SetString("Avatar" + i + "_Password", avatars[i].password);
            PlayerPrefs.SetInt("Avatar" + i + "_HasPassword", avatars[i].hasPassword ? 1 : 0);
        }
        PlayerPrefs.Save();
    }
    
    void LoadAvatarData()
    {
        for (int i = 0; i < avatars.Count; i++)
        {
            if (PlayerPrefs.HasKey("Avatar" + i + "_Password"))
            {
                avatars[i].password = PlayerPrefs.GetString("Avatar" + i + "_Password");
                avatars[i].hasPassword = PlayerPrefs.GetInt("Avatar" + i + "_HasPassword") == 1;
            }
        }
    }
}