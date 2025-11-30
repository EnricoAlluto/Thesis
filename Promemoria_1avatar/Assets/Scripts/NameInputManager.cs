using TMPro;
using UnityEngine;

public class NameInputManager : MonoBehaviour
{
    public TMP_InputField nameInputField;
    private string saveKey = "SavedUserName";
    
    void Start()
    {
        LoadSavedName();
        nameInputField.onValueChanged.AddListener(OnTextChanged);
    }
    
    public void OnTextChanged(string newText)
    {
        SaveName(newText);
    }
    
    public void SaveName(string name)
    {
        PlayerPrefs.SetString(saveKey, name);
        PlayerPrefs.Save();
        //Debug.Log("Nome salvato: " + name);
    }
    
    private void LoadSavedName()
    {
        if (PlayerPrefs.HasKey(saveKey))
        {
            string savedName = PlayerPrefs.GetString(saveKey);
            nameInputField.text = savedName;
            //Debug.Log("Nome caricato: " + savedName);
        }
    }
    
    public void ClearSavedName()
    {
        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.Save();
        nameInputField.text = "";
        //Debug.Log("Nome cancellato");
    }
}