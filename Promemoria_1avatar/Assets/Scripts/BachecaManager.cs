using UnityEngine;
using UnityEngine.UI;

public class BachecaManager : MonoBehaviour 
{
    public Button[] impegniButtons;
    public int[] roomIndices;
    public Button backToAvatarButton;
    public Color defaultColor = Color.white;
    private AvatarSelection avatarSelection;
    public RoomCanvasManager roomCanvasManager;
    
    void Start()
    {
        avatarSelection = FindObjectOfType<AvatarSelection>();

        if (roomCanvasManager == null)
        {
            roomCanvasManager = FindObjectOfType<RoomCanvasManager>();
        }
        
        if (avatarSelection != null && roomCanvasManager != null)
        {
            for (int i = 0; i < impegniButtons.Length; i++)
            {
                int index = i;
                if (i < roomIndices.Length)
                {
                    impegniButtons[i].onClick.AddListener(() => VaiAllaStanza(roomIndices[index]));
                }
            }
            
            if (backToAvatarButton != null)
            {
                backToAvatarButton.onClick.AddListener(TornaAllaSceltaAvatar);
            }
            UpdateButtonStates();
        }
        else
        {
            //if (avatarSelection == null) Debug.LogError("AvatarSelection not found!");
            //if (roomCanvasManager == null) Debug.LogError("RoomCanvasManager not found!");
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
    
    void VaiAllaStanza(int roomNumber)
    {
        //Debug.Log($"[BachecaManager] VaiAllaStanza chiamato con roomNumber: {roomNumber}");
        
        var roomCanvasManager = FindObjectOfType<RoomCanvasManager>();
        if (roomCanvasManager != null)
        {
            roomCanvasManager.OnRoomSelected(roomNumber);
        }
        
        if (avatarSelection != null)
        {
            avatarSelection.VaiAllaStanza(roomNumber);
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