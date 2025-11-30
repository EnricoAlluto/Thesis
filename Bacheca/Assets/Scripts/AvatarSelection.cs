// AvatarSelection class manages avatar interactions in the game, handling avatar selection, 
//room transitions, and UI states. It coordinates avatar display across different scenes, 
//manages bulletin board interactions, and handles avatar positioning between the main scene and rooms. 
//The class also integrates with other managers like AvatarFinishManager for avatar loading and 
//PersistentQuestionCanvas for question handling, ensuring smooth avatar selection and room navigation.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using UnityEngine.Networking;

public class AvatarSelection : MonoBehaviour 
{
    private static string userID = "default_user";
    private static bool isActive = true; 
    public GameObject[] bacheche;
    public GameObject[] avatarModels;
    public Button backButton;
    public Button newMainSceneButton;
    
    [Header("Panel Button Manager")]
    [SerializeField] private PanelButtonManager panelButtonManager;
    
    [Header("Scene References")]
    public GameObject mainScene;
    public GameObject[] stanze;
    
    private static int selectedAvatarIndex = -1; 
    private static int selectedRoomIndex = -1;
    private Transform[] originalParents;
    private Vector3[] originalPositions;
    private Quaternion[] originalRotations;
    private Vector3[] originalScales;
    private Transform[] roomAvatarContainers;
    private Transform defaultParentContainer;
    private GameObject[] avatarClones;

    //This Awake() method initializes the avatar selection system by setting up the environment 
    //and managing avatar models. It creates a container for default avatars, 
    //configures UI elements like back and scene buttons, and ensures a unique user ID is assigned. 
    //The method also organizes room management by deactivating all room objects initially while 
    //keeping the main scene active. It preserves the original transform data (position, rotation, scale)
    //of all avatar models for later restoration and sets up dedicated containers within each room for 
    //avatar placement. The code ensures proper parent-child relationships and initial visibility 
    //states for all game objects in the selection interface.

    void Awake()
    {
        //Debug.Log("AvatarSelection Awake chiamato");

        if (panelButtonManager == null)
        {
            panelButtonManager = FindObjectOfType<PanelButtonManager>();
        }
        
        GameObject containerObj = new GameObject("DefaultAvatarContainer");
        containerObj.transform.SetParent(transform);
        defaultParentContainer = containerObj.transform;
        
        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
            backButton.onClick.AddListener(TornaAllaSceltaAvatar);
        }

        if (newMainSceneButton != null)
        {
            newMainSceneButton.gameObject.SetActive(true);
        }
        
        userID = PlayerPrefs.GetString("UserID", "");
        if (string.IsNullOrEmpty(userID))
        {
            userID = "user_" + SystemInfo.deviceUniqueIdentifier;
            PlayerPrefs.SetString("UserID", userID);
        }
        
        if (stanze != null)
        {
            foreach (GameObject stanza in stanze)
            {
                if (stanza != null)
                {
                    stanza.SetActive(false);
                }
            }
        }
        
        if (mainScene != null)
        {
            mainScene.SetActive(true);
        }
        
        originalParents = new Transform[avatarModels.Length];
        originalPositions = new Vector3[avatarModels.Length];
        originalRotations = new Quaternion[avatarModels.Length];
        originalScales = new Vector3[avatarModels.Length];
        
        for (int i = 0; i < avatarModels.Length; i++)
        {
            if (avatarModels[i] != null)
            {
                originalParents[i] = avatarModels[i].transform.parent;
                originalPositions[i] = avatarModels[i].transform.localPosition;
                originalRotations[i] = avatarModels[i].transform.localRotation;
                originalScales[i] = avatarModels[i].transform.localScale;
                avatarModels[i].SetActive(false);
                
                if (originalParents[i] == null)
                {
                    avatarModels[i].transform.SetParent(defaultParentContainer);
                    originalParents[i] = defaultParentContainer;
                }
            }
        }
        
        roomAvatarContainers = new Transform[stanze.Length];
        for (int i = 0; i < stanze.Length; i++)
        {
            if (stanze[i] != null)
            {
                Transform container = stanze[i].transform.Find("AvatarContainer");
                if (container == null)
                {
                    GameObject newContainer = new GameObject("AvatarContainer");
                    newContainer.transform.SetParent(stanze[i].transform);
                    container = newContainer.transform;
                }
                roomAvatarContainers[i] = container;
            }
        }
        avatarClones = new GameObject[stanze.Length];
    }

    private AvatarFinishManager _avatarFinishManager;
    
    private IEnumerator Start()
    {
        _avatarFinishManager = FindObjectOfType<AvatarFinishManager>();
        
        if (_avatarFinishManager != null)
        {
            //Debug.Log("AvatarFinishManager trovato nella scena, procedo con l'inizializzazione...");
            if (_avatarFinishManager.caricaAutomaticamenteAllAvvio)
            {
                _avatarFinishManager.CaricaUltimiAvatarSalvatiAllAvvio();
            }
        }
        else
        {
           // Debug.LogWarning("AvatarFinishManager non trovato nella scena. La gestione degli avatar potrebbe non funzionare correttamente.");
        }

        yield return null;

        foreach (GameObject bacheca in bacheche)
        {
            if (bacheca != null)
                bacheca.SetActive(false);
        }

        for (int i = 0; i < avatarModels.Length; i++)
        {
            if (avatarModels[i] != null)
            {
                if (avatarModels[i].transform.parent == null)
                {
                    avatarModels[i].transform.SetParent(defaultParentContainer);
                    originalParents[i] = defaultParentContainer;
                }
                avatarModels[i].SetActive(false);
            }
        }
        
        selectedAvatarIndex = -1;
        selectedRoomIndex = -1;
        
        for (int i = 0; i < avatarModels.Length; i++)
        {
            if (avatarModels[i] != null)
            {
                AvatarClick clickScript = avatarModels[i].GetComponent<AvatarClick>();
                if (clickScript != null)
                {
                    clickScript.avatarIndex = i;
                }
            }
        }
        
        //Debug.Log("Inizializzazione AvatarSelection completata");
    }

    private void UpdateNewButtonVisibility()
    {
        if (newMainSceneButton == null) return;
        
        int avatarCount = 0;
        foreach (GameObject avatar in avatarModels)
        {
            if (avatar != null && HasLoadedAvatarStructure(avatar))
            {
                avatarCount++;
            }
        }
        
        newMainSceneButton.gameObject.SetActive(avatarCount < 10);
        
        //Debug.Log($"[AvatarSelection] Total avatars: {avatarCount}, New button active: {newMainSceneButton.gameObject.activeSelf}");
    }

    public void AttivaSelettore()
    {
        isActive = true;
        
        if (mainScene != null)
        {
            mainScene.SetActive(true);
        }
        
        foreach (GameObject stanza in stanze)
        {
            if (stanza != null)
            {
                stanza.SetActive(false);
            }
        }
        
        CleanupAllAvatarClones();
        
        selectedAvatarIndex = -1;
        selectedRoomIndex = -1;
        
        for (int i = 0; i < avatarModels.Length; i++)
        {
            if (avatarModels[i] != null)
            {
                avatarModels[i].SetActive(true);
            }
        }

        foreach (GameObject bacheca in bacheche)
        {
            bacheca.SetActive(false);
        }

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }

        if (newMainSceneButton != null)
        {
            newMainSceneButton.gameObject.SetActive(true);
        }
        
        UpdateNewButtonVisibility();
    }

    public void ApriBacheca(int index)
    {
        if (!isActive || index < 0 || index >= bacheche.Length) return;
        
        selectedAvatarIndex = index;

        foreach (GameObject bacheca in bacheche)
        {
            bacheca.SetActive(false);
        }

        bacheche[index].SetActive(true);

        for (int i = 0; i < avatarModels.Length; i++)
        {
            if (avatarModels[i] != null)
            {
                avatarModels[i].SetActive(false);
            }
        }
        
        if (backButton != null)
        {
            backButton.gameObject.SetActive(true);
        }

        if (newMainSceneButton != null)
        {
            newMainSceneButton.gameObject.SetActive(false);
        }

        if (panelButtonManager != null)
        {
            panelButtonManager.OnAvatarSelected(index);
        }
        
        PersistentQuestionCanvas questionCanvas = FindObjectOfType<PersistentQuestionCanvas>();
        if (questionCanvas != null)
        {
            questionCanvas.AvatarChanged();
        }
    }

    public static int GetSelectedAvatarIndex()
    {
        return selectedAvatarIndex;
    }
    
    public static int GetSelectedRoomIndex()
    {
        return selectedRoomIndex;
    }

    public void TornaAllaSceltaAvatar()
    {
        if (!isActive) return;
        
        selectedAvatarIndex = -1;
        
        if (panelButtonManager != null)
        {
            panelButtonManager.OnAvatarSelected(-1);
        }
        
        CleanupAllAvatarClones();
        
        if (mainScene != null)
        {
            mainScene.SetActive(true);
        }

        foreach (GameObject stanza in stanze)
        {
            if (stanza != null)
            {
                stanza.SetActive(false);
            }
        }

        foreach (GameObject bacheca in bacheche)
        {
            bacheca.SetActive(false);
        }

        for (int i = 0; i < avatarModels.Length; i++)
        {
            if (avatarModels[i] != null)
            {
                bool hasLoadedAvatar = HasLoadedAvatarStructure(avatarModels[i]);
                avatarModels[i].SetActive(hasLoadedAvatar);
            }
        }

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }
        
        UpdateNewButtonVisibility();
 
        PersistentQuestionCanvas questionCanvas = PersistentQuestionCanvas.Instance;
        if (questionCanvas != null)
        {
            questionCanvas.UpdateCurrentRoom();
        }
    }

    private bool HasLoadedAvatarStructure(GameObject avatar)
    {
        for (int i = 0; i < avatar.transform.childCount; i++)
        {
            Transform child = avatar.transform.GetChild(i);
            
            for (int j = 0; j < child.childCount; j++)
            {
                Transform grandChild = child.GetChild(j);
                if (grandChild.name.StartsWith("LoadedAvatar_"))
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    //The VaiAllaStanza method manages room transitions by deactivating the main scene and all
    //other rooms, then activating the selected room. It moves the chosen avatar into the room, 
    //updates the UI by hiding the new scene button, and notifies both the room's canvas manager and 
    //question system about the room change.The MoveAvatarToRoom method handles the actual movement of 
    //an avatar into a specific room. It validates the avatar and room indices, retrieves the correct 
    //avatar, and ensures a container exists in the target room. The avatar is then parented to the 
    //room's container, positioned at the center with default rotation and scaled to 50% size, 
    //and made visible. This method also keeps track of the currently active avatar in the room.
    
    public void VaiAllaStanza(int roomIndex)
    {
        if (!isActive || roomIndex < 0 || roomIndex >= stanze.Length) return;
        
        selectedRoomIndex = roomIndex;
        
        if (mainScene != null)
        {
            mainScene.SetActive(false);
        }
        
        foreach (GameObject stanza in stanze)
        {
            if (stanza != null)
            {
                stanza.SetActive(false);
            }
        }
        
        stanze[roomIndex].SetActive(true);
        
        CreaAvatarCloneNellaStanza(selectedAvatarIndex, roomIndex);
        
        if (newMainSceneButton != null)
        {
            newMainSceneButton.gameObject.SetActive(false);
        }
        
        PersistentQuestionCanvas questionCanvas = PersistentQuestionCanvas.Instance;
        if (questionCanvas != null)
        {
            questionCanvas.RoomChanged();
        }
    }
    
    private void CreaAvatarCloneNellaStanza(int avatarIndex, int roomIndex)
    {
        if (avatarIndex < 0 || avatarIndex >= avatarModels.Length || avatarModels[avatarIndex] == null)
        {
            //Debug.LogError($"Indice avatar non valido: {avatarIndex}");
            return;
        }
        
        if (roomIndex >= 0 && roomIndex < avatarClones.Length && avatarClones[roomIndex] != null)
        {
            Destroy(avatarClones[roomIndex]);
            avatarClones[roomIndex] = null;
        }
        
        if (roomIndex < 0 || roomIndex >= stanze.Length || stanze[roomIndex] == null)
        {
           // Debug.LogError($"Stanza non valida all'indice: {roomIndex}");
            return;
        }
        
        Transform avatarContainer = roomAvatarContainers[roomIndex];
        
        if (avatarContainer == null)
        {
           // Debug.Log($"Creazione di un nuovo container per la stanza {roomIndex}");
            GameObject container = new GameObject($"AvatarContainer_Room{roomIndex}");
            container.transform.SetParent(stanze[roomIndex].transform);
            container.transform.localPosition = Vector3.zero;
            container.transform.localRotation = Quaternion.identity;
            container.transform.localScale = Vector3.one;
            avatarContainer = container.transform;
            roomAvatarContainers[roomIndex] = avatarContainer;
        }
        
        try
        {
            GameObject avatarClone = Instantiate(avatarModels[avatarIndex], avatarContainer);

            avatarClone.transform.localPosition = Vector3.zero;
            avatarClone.transform.localRotation = Quaternion.identity;
            avatarClone.transform.localScale = Vector3.one * 0.5f;

            avatarClone.name = $"AvatarClone_{avatarIndex}_{roomIndex}_{Time.time}";

            avatarClone.SetActive(true);

            if (roomIndex >= 0 && roomIndex < avatarClones.Length)
            {
                avatarClones[roomIndex] = avatarClone;
            }
            
            //Debug.Log($"Creato clone dell'avatar {avatarIndex} nella stanza {roomIndex}");
        }
        catch (Exception e)
        {
            //Debug.LogError($"Errore durante la creazione del clone dell'avatar: {e.Message}");
        }
    }

    private void CleanupAllAvatarClones()
    {
        for (int i = 0; i < avatarClones.Length; i++)
        {
            if (avatarClones[i] != null)
            {
                Destroy(avatarClones[i]);
                avatarClones[i] = null;
            }
        }
    }
    
    public void TornaDallaStanzaAllaBacheca()
    {
        //Debug.Log("TornaDallaStanzaAllaBacheca called");
        
        int currentRoomIndex = selectedRoomIndex;
        
        if (currentRoomIndex >= 0 && currentRoomIndex < avatarClones.Length && avatarClones[currentRoomIndex] != null)
        {
            Destroy(avatarClones[currentRoomIndex]);
            avatarClones[currentRoomIndex] = null;
        }
        
        foreach (GameObject stanza in stanze)
        {
            if (stanza != null)
            {
                stanza.SetActive(false);
            }
        }

        if (mainScene != null)
        {
            mainScene.SetActive(true);
        }
        
        if (selectedAvatarIndex >= 0 && selectedAvatarIndex < bacheche.Length)
        {
            foreach (GameObject bacheca in bacheche)
            {
                bacheca.SetActive(false);
            }
            
            bacheche[selectedAvatarIndex].SetActive(true);
            
            if (backButton != null)
            {
                backButton.gameObject.SetActive(true);
            }
            
            if (newMainSceneButton != null)
            {
                newMainSceneButton.gameObject.SetActive(false);
            }
        }
        
        int tempIndex = selectedAvatarIndex;

        selectedRoomIndex = -1;
        selectedAvatarIndex = tempIndex;

        PersistentQuestionCanvas questionCanvas = PersistentQuestionCanvas.Instance;
        if (questionCanvas != null)
        {
            questionCanvas.UpdateCurrentRoom();
        }
    }
    
    public void DisattivaSelettore()
    {
        isActive = false;
        
        if (panelButtonManager != null)
        {
            panelButtonManager.OnAvatarSelected(-1);
        }
        
        CleanupAllAvatarClones();

        foreach (GameObject bacheca in bacheche)
        {
            bacheca.SetActive(false);
        }

        for (int i = 0; i < avatarModels.Length; i++)
        {
            if (avatarModels[i] != null)
            {
                avatarModels[i].SetActive(false);
            }
        }
        
        foreach (GameObject stanza in stanze)
        {
            if (stanza != null)
            {
                stanza.SetActive(false);
            }
        }
        
        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }

        if (newMainSceneButton != null)
        {
            newMainSceneButton.gameObject.SetActive(false);
        }
        
        PersistentQuestionCanvas questionCanvas = PersistentQuestionCanvas.Instance;
        if (questionCanvas != null)
        {
            questionCanvas.ShowCanvas(false);
        }
    }

    public void HandleQuestionResponse(bool isYesResponse)
    {
    }
    
    void OnDestroy()
    {
        CleanupAllAvatarClones();
    }
}