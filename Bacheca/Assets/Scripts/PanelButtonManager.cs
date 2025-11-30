//The PanelButtonManager manages UI panel interactions and button behaviors in the application. 
//It handles avatar selection, name input fields, and navigation between different UI panels. 
//The class coordinates with NameInputManager for avatar name management and updates the UI to reflect 
//the currently selected avatar. It also manages button states and panel visibility based 
//on user interactions and game state.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PanelButtonManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button nuovoButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button indietroButton;

    [Header("Panels")]
    [SerializeField] private GameObject panel1;
    [SerializeField] private GameObject panel2;
    [SerializeField] private GameObject panel3;

    [Header("Input Fields")]
    [SerializeField] private List<TMP_InputField> avatarInputFields = new List<TMP_InputField>();
    [SerializeField] private NameInputManager nameInputManager;
    
    [Header("Avatar Name Display")]
    [SerializeField] private TextMeshProUGUI avatarNameText; 
    [SerializeField] private GameObject avatarNamePanel;
    
    private int currentAvatarIndex = -1;

    private void Start()
    {
        if (nuovoButton == null || logoutButton == null || indietroButton == null ||
            panel1 == null || panel2 == null || panel3 == null || nameInputManager == null)
        {
            //Debug.LogError("Please assign all button, panel, and NameInputManager references in the inspector!");
            return;
        }

        InitializeInputFields();

        UpdateButtonVisibility();

        UpdateAvatarNameDisplay();
    }

    private void InitializeInputFields()
    {
        while (avatarInputFields.Count < 10)
        {
            avatarInputFields.Add(null);
        }

        foreach (var inputField in avatarInputFields)
        {
            if (inputField != null)
            {
                inputField.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateButtonVisibility()
    {
        bool isPanel1Active = panel1.activeInHierarchy;
        bool isPanel2Active = panel2.activeInHierarchy;
        bool isPanel3Active = panel3.activeInHierarchy;

        nuovoButton.gameObject.SetActive(!isPanel1Active && !isPanel2Active && !isPanel3Active);
        
        logoutButton.gameObject.SetActive(isPanel1Active && !isPanel2Active && !isPanel3Active);
        
        indietroButton.gameObject.SetActive(isPanel2Active || isPanel3Active);

        UpdateInputFieldVisibility();

        UpdateAvatarNameDisplay();
    }

    private void UpdateInputFieldVisibility()
    {
        bool isAnyPanelActive = panel1.activeInHierarchy || panel2.activeInHierarchy || panel3.activeInHierarchy;
        
        if (isAnyPanelActive && currentAvatarIndex >= 0 && currentAvatarIndex < avatarInputFields.Count)
        {
            foreach (var inputField in avatarInputFields)
            {
                if (inputField != null)
                {
                    inputField.gameObject.SetActive(false);
                }
            }
            
            if (avatarInputFields[currentAvatarIndex] != null)
            {
                avatarInputFields[currentAvatarIndex].gameObject.SetActive(true);
            }
        }
        else
        {
            foreach (var inputField in avatarInputFields)
            {
                if (inputField != null)
                {
                    inputField.gameObject.SetActive(false);
                }
            }
        }
    }
    
    private void UpdateAvatarNameDisplay()
    {
        bool isAnyPanelActive = panel1.activeInHierarchy || panel2.activeInHierarchy || panel3.activeInHierarchy;
        
        if (isAnyPanelActive && currentAvatarIndex >= 0 && currentAvatarIndex < 10)
        {
            if (avatarNameText != null)
            {
                string avatarName = nameInputManager.GetAvatarName(currentAvatarIndex);
                avatarNameText.text = avatarName;
                avatarNameText.gameObject.SetActive(true);
            }

            if (avatarNamePanel != null)
            {
                avatarNamePanel.SetActive(true);
            }
        }
        else
        {
            if (avatarNameText != null)
            {
                avatarNameText.gameObject.SetActive(false);
            }

            if (avatarNamePanel != null)
            {
                avatarNamePanel.SetActive(false);
            }
        }
    }

    public void OnAvatarSelected(int avatarIndex)
    {
        if (avatarIndex >= 0 && avatarIndex < 10)
        {
            currentAvatarIndex = avatarIndex;
            UpdateInputFieldVisibility();
            UpdateAvatarNameDisplay();
            
            //Debug.Log($"Avatar {avatarIndex} selezionato, nome: {nameInputManager.GetAvatarName(avatarIndex)}");
        }
    }

    public void SwitchToPanel(GameObject panelToActivate)
    {
        panel1.SetActive(false);
        panel2.SetActive(false);
        panel3.SetActive(false);

        panelToActivate.SetActive(true);

        UpdateButtonVisibility();
    }

    public void OnNuovoClicked()
    {
        //Debug.Log("Nuovo button clicked");
    }

    public void OnLogoutClicked()
    {
        //Debug.Log("Logout button clicked");
    }

    public void OnIndietroClicked()
    {
        //Debug.Log("Indietro button clicked");
        SwitchToPanel(panel1);
    }
}