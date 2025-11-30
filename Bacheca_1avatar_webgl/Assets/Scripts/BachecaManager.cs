using UnityEngine;
using UnityEngine.UI;

public class BachecaManager : MonoBehaviour 
{
    public Button[] impegniButtons;
    public int[] roomIndices;
    public Button backToAvatarButton;
    public Color defaultColor = Color.white;
    private AvatarSelection avatarSelection;
    
    void Start()
    {
        avatarSelection = FindObjectOfType<AvatarSelection>();
        
        if (avatarSelection != null)
        {
            for (int i = 0; i < impegniButtons.Length; i++)
            {
                int index = i;
                impegniButtons[i].onClick.AddListener(() => VaiAllaStanza(index));
            }
            
            if (backToAvatarButton != null)
            {
                backToAvatarButton.onClick.AddListener(TornaAllaSceltaAvatar);
            }
            UpdateButtonStates();
        }
    }
    
    public void UpdateButtonStates()
    {
        int avatarIndex = AvatarSelection.GetSelectedAvatarIndex();
        
        for (int i = 0; i < Mathf.Min(impegniButtons.Length, roomIndices.Length); i++)
        {
            Button button = impegniButtons[i];
            ColorBlock colors = button.colors;
            colors.normalColor = defaultColor;
            colors.highlightedColor = new Color(defaultColor.r * 0.9f, defaultColor.g * 0.9f, defaultColor.b * 0.9f);
            colors.pressedColor = new Color(defaultColor.r * 0.8f, defaultColor.g * 0.8f, defaultColor.b * 0.8f);
            colors.selectedColor = defaultColor;
            button.colors = colors;
        }
    }
    
    void VaiAllaStanza(int index)
    {
        if (index < roomIndices.Length && avatarSelection != null)
        {
            avatarSelection.VaiAllaStanza(roomIndices[index]);
        }
    }
    
    void TornaAllaSceltaAvatar()
    {
        if (avatarSelection != null)
        {
            avatarSelection.TornaAllaSceltaAvatar();
        }
    }
}