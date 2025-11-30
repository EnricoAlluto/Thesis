//The NameInputManager class manages avatar name inputs, handling the saving and loading of names 
//using PlayerPrefs. It initializes up to 10 input fields, loads saved names on start, and saves 
//hanges automatically as users type. The class ensures persistent storage of avatar names across
//game sessions.

using TMPro;
using UnityEngine;
using System.Collections.Generic;
 
public class NameInputManager : MonoBehaviour
{
    public List<TMP_InputField> avatarNameInputs = new List<TMP_InputField>();
    private const string SAVE_KEY_PREFIX = "AvatarName_";
    
    void Start()
    {
        if (avatarNameInputs.Count != 10)
        {
           // Debug.LogWarning("Dovrebbero esserci 10 input fields per i nomi degli avatar!");
            while (avatarNameInputs.Count < 10)
            {
                avatarNameInputs.Add(null);
            }
        }
        
        for (int i = 0; i < avatarNameInputs.Count; i++)
        {
            int index = i;
            if (avatarNameInputs[i] != null)
            {
                LoadSavedName(index);
                avatarNameInputs[i].onValueChanged.AddListener((value) => OnTextChanged(index, value));
            }
        }
    }
    
    public void OnTextChanged(int avatarIndex, string newText)
    {
        SaveName(avatarIndex, newText);
    }
    
    public void SaveName(int avatarIndex, string name)
    {
        if (avatarIndex < 0 || avatarIndex >= 10) return;
        
        string saveKey = SAVE_KEY_PREFIX + avatarIndex;
        PlayerPrefs.SetString(saveKey, name);
        PlayerPrefs.Save();
        //Debug.Log($"Nome avatar {avatarIndex} salvato: {name}");
    }
    
    private void LoadSavedName(int avatarIndex)
    {
        if (avatarIndex < 0 || avatarIndex >= 10 || avatarNameInputs[avatarIndex] == null) return;
        
        string saveKey = SAVE_KEY_PREFIX + avatarIndex;
        if (PlayerPrefs.HasKey(saveKey))
        {
            string savedName = PlayerPrefs.GetString(saveKey);
            avatarNameInputs[avatarIndex].text = savedName;
           // Debug.Log($"Nome avatar {avatarIndex} caricato: {savedName}");
        }
    }
    
    public string GetAvatarName(int avatarIndex)
    {
        if (avatarIndex < 0 || avatarIndex >= 10) return string.Empty;
        
        string saveKey = SAVE_KEY_PREFIX + avatarIndex;
        return PlayerPrefs.GetString(saveKey, $"Avatar {avatarIndex + 1}");
    }
    
    public void ClearAllSavedNames()
    {
        for (int i = 0; i < 10; i++)
        {
            string saveKey = SAVE_KEY_PREFIX + i;
            PlayerPrefs.DeleteKey(saveKey);
            if (i < avatarNameInputs.Count && avatarNameInputs[i] != null)
            {
                avatarNameInputs[i].text = "";
            }
        }
        PlayerPrefs.Save();
       // Debug.Log("Tutti i nomi degli avatar sono stati cancellati");
    }
}