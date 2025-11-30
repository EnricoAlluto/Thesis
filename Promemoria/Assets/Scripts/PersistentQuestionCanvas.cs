//The PersistentQuestionCanvas manages the question and answer system across different rooms and 
//avatars in the application. It handles the display of questions, tracks user responses, 
//manages scoring, and saves progress both locally and to a database. 
//The class coordinates with UI elements, manages room-specific questions, 
//and provides data export and backup functionality for user progress. 
//It ensures persistent data storage using PlayerPrefs and file system operations, 
//supporting features like weekly score resets, detailed logging, and data restoration from backups.
 
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.Networking;
 
public class PersistentQuestionCanvas : MonoBehaviour
{
    public static PersistentQuestionCanvas Instance { get; private set; }

    [Header("UI References")]
    public Canvas questionCanvas;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI scoreText;
    public Button yesButton;
    public Button noButton;
    
    [Header("Additional Canvases to Hide")]
    public Canvas canvas1;
    public Canvas canvas2;

    [Header("Avatar References")]
    public GameObject[] avatarModels;
    
    [Header("Button References")]
    public GameObject[] roomButtons; 
    
    private int currentRoomIndex = -1;
    
    [Header("Avatar Objects")]
    public GameObject[] roomAvatars;
    
    [Header("Database Settings")]
    public string databaseURL = "http://fontanegli.spallared.it:8080/manudb/store.php";

    [Header("Question Configuration")]
    private Dictionary<int, Dictionary<string, string[]>> roomAvatarQuestions = new Dictionary<int, Dictionary<string, string[]>>
    {
        { 0, new Dictionary<string, string[]> {
            { "MainScene", new string[] {
                "Sei pronto per iniziare la tua routine quotidiana?",
                "Hai pianificato le attività di oggi?"
            }},
            { "Stanza1", new string[] {
                "Hai preso le medicine?",
            }},
            { "Stanza2", new string[] {
                "Ti sei cambiato la biancheria intima (calze e mutande)?",
                "Hai preparato tutte le cose che ti servono per la giornata?"
            }},
            { "Stanza3", new string[] {
                "Ti sei lavato i denti e la faccia?"
            }},
            { "Stanza4", new string[] {
                "Hai preso le medicine?"
            }},
            { "Stanza5", new string[] {
                "Hai messo la roba sporca nel sacchetto?",
                "Hai telefonato a casa?"
            }},
            { "Stanza6", new string[] {
                "Hai fatto la doccia e ti sei lavato i denti?"
            }}
        }}, 
        { 1, new Dictionary<string, string[]> {
            { "MainScene", new string[] {
                "Sei pronto per iniziare la tua routine quotidiana?",
                "Hai pianificato le attività di oggi?"
            }},
            { "Stanza1", new string[] {
                "Hai preso le medicine?",
            }},
            { "Stanza2", new string[] {
                "Ti sei cambiato la biancheria intima (calze e mutande)?",
                "Hai preparato tutte le cose che ti servono per la giornata?"
            }},
            { "Stanza3", new string[] {
                "Ti sei lavato i denti e la faccia?"
            }},
            { "Stanza4", new string[] {
                "Hai preso le medicine?"
            }},
            { "Stanza5", new string[] {
                "Hai messo la roba sporca nel sacchetto?",
                "Hai telefonato a casa?"
            }},
            { "Stanza6", new string[] {
                "Hai fatto la doccia e ti sei lavato i denti?"
            }}
        }},
        { 2, new Dictionary<string, string[]> {
            { "MainScene", new string[] {
                "Sei pronto per iniziare la tua routine quotidiana?",
                "Hai pianificato le attività di oggi?"
            }},
            { "Stanza1", new string[] {
                "Hai preso le medicine?",
            }},
            { "Stanza2", new string[] {
                "Ti sei cambiato la biancheria intima (calze e mutande)?",
                "Hai preparato tutte le cose che ti servono per la giornata?"
            }},
            { "Stanza3", new string[] {
                "Ti sei lavato i denti e la faccia?"
            }},
            { "Stanza4", new string[] {
                "Hai preso le medicine?"
            }},
            { "Stanza5", new string[] {
                "Hai messo la roba sporca nel sacchetto?",
                "Hai telefonato a casa?"
            }},
            { "Stanza6", new string[] {
                "Hai fatto la doccia e ti sei lavato i denti?"
            }}
        }},
        { 3, new Dictionary<string, string[]> {
            { "MainScene", new string[] {
                "Sei pronto per iniziare la tua routine quotidiana?",
                "Hai pianificato le attività di oggi?"
            }},
            { "Stanza1", new string[] {
                "Hai preso o preparato le medicine per del mattino o del pomeriggio?",
                "Hai riordinato la cucina?",
                "Hai controllato di avere il necessario per il pranzo e per la cena?"
            }},
            { "Stanza2", new string[] {
                "Hai rifatto il letto?",
                "Hai riordinato la camera?"
            }},
            { "Stanza3", new string[] {
                "Ti sei lavato il viso?",
                "Ti sei lavato i denti?"
            }},
            { "Stanza4", new string[] {
                "Hai preso le medicine della sera?",
                "Hai riordinato la cucina?"
            }},
            { "Stanza5", new string[] {
                "Hai preparato i vestiti per domani?",
                "Hai preparato lo zaino con il necessario per domani?",
                "Hai messo la sveglia?"
            }},
            { "Stanza6", new string[] {
                "Ti sei lavato le mani, il viso o ti sei fatto la doccia?",
                "Ti sei lavato i denti?"
            }}
        }},
        { 4, new Dictionary<string, string[]> {
            { "MainScene", new string[] {
                "Sei pronto per iniziare la tua routine quotidiana?",
                "Hai pianificato le attività di oggi?"
            }},
            { "Stanza1", new string[] {
                "Hai preso o preparato le medicine per del mattino o del pomeriggio?",
                "Hai riordinato la cucina?",
                "Hai controllato di avere il necessario per il pranzo e per la cena?"
            }},
            { "Stanza2", new string[] {
                "Hai rifatto il letto?",
                "Hai riordinato la camera?"
            }},
            { "Stanza3", new string[] {
                "Ti sei lavato il viso?",
                "Ti sei lavato i denti?"
            }},
            { "Stanza4", new string[] {
                "Hai preso le medicine della sera?",
                "Hai riordinato la cucina?"
            }},
            { "Stanza5", new string[] {
                "Hai preparato i vestiti per domani?",
                "Hai preparato lo zaino con il necessario per domani?",
                "Hai messo la sveglia?"
            }},
            { "Stanza6", new string[] {
                "Ti sei lavato le mani, il viso o ti sei fatto la doccia?",
                "Ti sei lavato i denti?"
            }}
        }},
        { 5, new Dictionary<string, string[]> {
            { "MainScene", new string[] {
                "Sei pronto per iniziare la tua routine quotidiana?",
                "Hai pianificato le attività di oggi?"
            }},
            { "Stanza1", new string[] {
                "Hai preso o preparato le medicine per del mattino o del pomeriggio?",
                "Hai riordinato la cucina?",
                "Hai controllato di avere il necessario per il pranzo e per la cena?"
            }},
            { "Stanza2", new string[] {
                "Hai rifatto il letto?",
                "Hai riordinato la camera?"
            }},
            { "Stanza3", new string[] {
                "Ti sei lavato il viso?",
                "Ti sei lavato i denti?"
            }},
            { "Stanza4", new string[] {
                "Hai preso le medicine della sera?",
                "Hai riordinato la cucina?"
            }},
            { "Stanza5", new string[] {
                "Hai preparato i vestiti per domani?",
                "Hai preparato lo zaino con il necessario per domani?",
                "Hai messo la sveglia?"
            }},
            { "Stanza6", new string[] {
                "Ti sei lavato le mani, il viso o ti sei fatto la doccia?",
                "Ti sei lavato i denti?"
            }}
        }},
        { 6, new Dictionary<string, string[]> {
            { "MainScene", new string[] {
                "Sei pronto per iniziare la tua routine quotidiana?",
                "Hai pianificato le attività di oggi?"
            }},
            { "Stanza1", new string[] {
                "Hai preso o preparato le medicine per del mattino o del pomeriggio?",
                "Hai riordinato la cucina?",
                "Hai controllato di avere il necessario per il pranzo e per la cena?"
            }},
            { "Stanza2", new string[] {
                "Hai rifatto il letto?",
                "Hai riordinato la camera?"
            }},
            { "Stanza3", new string[] {
                "Ti sei lavato il viso?",
                "Ti sei lavato i denti?"
            }},
            { "Stanza4", new string[] {
                "Hai preso le medicine della sera?",
                "Hai riordinato la cucina?"
            }},
            { "Stanza5", new string[] {
                "Hai preparato i vestiti per domani?",
                "Hai preparato lo zaino con il necessario per domani?",
                "Hai messo la sveglia?"
            }},
            { "Stanza6", new string[] {
                "Ti sei lavato le mani, il viso o ti sei fatto la doccia?",
                "Ti sei lavato i denti?"
            }}
        }},
        { 7, new Dictionary<string, string[]> {
            { "MainScene", new string[] {
                "Sei pronto per iniziare la tua routine quotidiana?",
                "Hai pianificato le attività di oggi?"
            }},
            { "Stanza1", new string[] {
                "Hai preso o preparato le medicine per del mattino o del pomeriggio?",
                "Hai riordinato la cucina?",
                "Hai controllato di avere il necessario per il pranzo e per la cena?"
            }},
            { "Stanza2", new string[] {
                "Hai rifatto il letto?",
                "Hai riordinato la camera?"
            }},
            { "Stanza3", new string[] {
                "Ti sei lavato il viso?",
                "Ti sei lavato i denti?"
            }},
            { "Stanza4", new string[] {
                "Hai preso le medicine della sera?",
                "Hai riordinato la cucina?"
            }},
            { "Stanza5", new string[] {
                "Hai preparato i vestiti per domani?",
                "Hai preparato lo zaino con il necessario per domani?",
                "Hai messo la sveglia?"
            }},
            { "Stanza6", new string[] {
                "Ti sei lavato le mani, il viso o ti sei fatto la doccia?",
                "Ti sei lavato i denti?"
            }}
        }},
        { 8, new Dictionary<string, string[]> {
            { "MainScene", new string[] {
                "Sei pronto per iniziare la tua routine quotidiana?",
                "Hai pianificato le attività di oggi?"
            }},
            { "Stanza1", new string[] {
                "Hai preso o preparato le medicine per del mattino o del pomeriggio?",
                "Hai riordinato la cucina?",
                "Hai controllato di avere il necessario per il pranzo e per la cena?"
            }},
            { "Stanza2", new string[] {
                "Hai rifatto il letto?",
                "Hai riordinato la camera?"
            }},
            { "Stanza3", new string[] {
                "Ti sei lavato il viso?",
                "Ti sei lavato i denti?"
            }},
            { "Stanza4", new string[] {
                "Hai preso le medicine della sera?",
                "Hai riordinato la cucina?"
            }},
            { "Stanza5", new string[] {
                "Hai preparato i vestiti per domani?",
                "Hai preparato lo zaino con il necessario per domani?",
                "Hai messo la sveglia?"
            }},
            { "Stanza6", new string[] {
                "Ti sei lavato le mani, il viso o ti sei fatto la doccia?",
                "Ti sei lavato i denti?"
            }}
        }}, 
        { 9, new Dictionary<string, string[]> {
            { "MainScene", new string[] {
                "Sei pronto per iniziare la tua routine quotidiana?",
                "Hai pianificato le attività di oggi?"
            }},
            { "Stanza1", new string[] {
                "Hai preso o preparato le medicine per del mattino o del pomeriggio?",
                "Hai riordinato la cucina?",
                "Hai controllato di avere il necessario per il pranzo e per la cena?"
            }},
            { "Stanza2", new string[] {
                "Hai rifatto il letto?",
                "Hai riordinato la camera?"
            }},
            { "Stanza3", new string[] {
                "Ti sei lavato il viso?",
                "Ti sei lavato i denti?"
            }},
            { "Stanza4", new string[] {
                "Hai preso le medicine della sera?",
                "Hai riordinato la cucina?"
            }},
            { "Stanza5", new string[] {
                "Hai preparato i vestiti per domani?",
                "Hai preparato lo zaino con il necessario per domani?",
                "Hai messo la sveglia?"
            }},
            { "Stanza6", new string[] {
                "Ti sei lavato le mani, il viso o ti sei fatto la doccia?",
                "Ti sei lavato i denti?"
            }}
        }}
    };

    [Header("State Variables")]
    private string[] currentQuestions;
    private int currentQuestionIndex = 0;
    private bool allYes = true;
    private int score = 0;
    private bool isCanvasActive = false; 
    
    private int currentAvatarIndex = -1;
    private string currentRoomName = "";
    
    private string SCORE_KEY => $"PlayerScore_Avatar{currentAvatarIndex}";
    private string LAST_RESET_KEY => $"LastWeeklyReset_Avatar{currentAvatarIndex}";
    private string ANSWERED_QUESTIONS_KEY => $"AnsweredQuestions_Avatar{currentAvatarIndex}_{currentRoomName}";
    private string ALL_YES_KEY => $"AllYesAnswers_Avatar{currentAvatarIndex}_{currentRoomName}";

    [Header("File Saving Configuration")]
    private string baseSavePath;
    private bool saveToFile = true;
    private string currentAvatarFolderPath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            InitializeSavePaths();
            if (questionCanvas != null)
            {
                questionCanvas.gameObject.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void InitializeSavePaths()
    {
        baseSavePath = Path.Combine(Application.persistentDataPath, "AvatarResponses");
        
        if (!Directory.Exists(baseSavePath))
        {
            try
            {
                Directory.CreateDirectory(baseSavePath);
                //Debug.Log($"Creata cartella base per le risposte: {baseSavePath}");
            }
            catch (Exception e)
            {
                //Debug.LogError($"Errore nella creazione della cartella base: {e.Message}");
                saveToFile = false;
            }
        }
    }

    void CreateAvatarFolders()
    {
        int groupIndex = currentAvatarIndex / 5;
        string groupFolderName = $"Gruppo_{groupIndex + 1}";
        string groupFolderPath = Path.Combine(baseSavePath, groupFolderName);
        
        if (!Directory.Exists(groupFolderPath))
        {
            try
            {
                Directory.CreateDirectory(groupFolderPath);
                //Debug.Log($"Creata cartella gruppo: {groupFolderPath}");
            }
            catch (Exception e)
            {
                //Debug.LogError($"Errore nella creazione della cartella gruppo: {e.Message}");
                saveToFile = false;
                return;
            }
        }
        string avatarFolderName = $"Avatar_{currentAvatarIndex + 1}";
        currentAvatarFolderPath = Path.Combine(groupFolderPath, avatarFolderName);
        
        if (!Directory.Exists(currentAvatarFolderPath))
        {
            try
            {
                Directory.CreateDirectory(currentAvatarFolderPath);
                //Debug.Log($"Creata cartella avatar: {currentAvatarFolderPath}");
            }
            catch (Exception e)
            {
                //Debug.LogError($"Errore nella creazione della cartella avatar: {e.Message}");
                saveToFile = false;
            }
        }
    }

    void Start()
    {
        if (yesButton != null && noButton != null)
        {
            yesButton.onClick.RemoveAllListeners();
            noButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(() => OnYesButtonClicked());
            noButton.onClick.AddListener(() => OnNoButtonClicked());
        }
        else
        {
            //Debug.LogError("Yes/No buttons are not assigned in PersistentQuestionCanvas!");
        }
        SetupRoomButtons();

        SetupAvatars();

        ShowCanvas(false);
    }
    
    void SetupRoomButtons()
    {
        if (roomButtons == null || roomButtons.Length == 0)
        {
            //Debug.LogError("Room buttons array is not assigned or empty in PersistentQuestionCanvas!");
            return;
        }
        
        for (int i = 0; i < roomButtons.Length; i++)
        {
            int index = i;
            Button button = roomButtons[i]?.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnRoomButtonClicked(index));
                //Debug.Log($"Set up listener for room button {index}");
            }
            else
            {
                //Debug.LogError($"Button component not found on room button {i}");
            }
        }
    }
    
    void SetupAvatars()
    {
        if (roomAvatars == null || roomAvatars.Length == 0)
        {
            //Debug.LogError("Room avatars array is not assigned or empty in PersistentQuestionCanvas!");
            return;
        }
        
        for (int i = 0; i < roomAvatars.Length; i++)
        {
            if (roomAvatars[i] != null)
            {
                var interaction = roomAvatars[i].GetComponent<AvatarInteraction>() ?? 
                                roomAvatars[i].AddComponent<AvatarInteraction>();
                interaction.roomIndex = i;

                if (roomAvatars[i].GetComponent<Collider>() == null)
                {
                    roomAvatars[i].AddComponent<BoxCollider>();
                }
                
                //Debug.Log($"Set up avatar {i} with room index {i}");
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateCurrentRoom();
    }
    
    public void OnRoomButtonClicked(int roomIndex)
    {
        currentRoomIndex = roomIndex;
    currentRoomName = $"Stanza{roomIndex + 1}";
        
        var roomCanvasManager = FindObjectOfType<RoomCanvasManager>();
        if (roomCanvasManager != null)
        {
            roomCanvasManager.SetLastButtonClicked(roomIndex);
        }
        UpdateAvatarVisibility(roomIndex);
        if (currentAvatarIndex >= 0)
        {
            CreateAvatarFolders();
            InitializeQuestionManager();
            ShowCanvas(true);
        }
    }
    
    private void UpdateAvatarVisibility(int roomIndex)
    {
        if (roomAvatars == null || roomAvatars.Length == 0) return;
        //Debug.Log($"Selected room: {roomIndex}");
    }
     
    public void UpdateCurrentRoom()
    {
        int newAvatarIndex = AvatarSelection.GetSelectedAvatarIndex();
        
        if (currentAvatarIndex != newAvatarIndex)
        {
            currentAvatarIndex = newAvatarIndex;
            if (currentAvatarIndex >= 0)
            {
                CreateAvatarFolders();
                InitializeQuestionManager();
            }
        }
    }
 
    void InitializeQuestionManager()
    {
        if (currentAvatarIndex < 0)
        {
            ShowCanvas(false);
            return;
        }
        
        if (currentRoomName == "MainScene")
        {
            ShowCanvas(false);
            return;
        }
        
        LoadQuestionsForCurrentAvatarAndRoom();
        LoadData();
        CheckWeeklyReset();
        FilterQuestions();
        UpdateUI();
    }

    public void ShowCanvas(bool show)
    {
        if (questionCanvas != null)
        {
            questionCanvas.gameObject.SetActive(show);

            isCanvasActive = show;

            if (show)
            {
                if (canvas1 != null) canvas1.gameObject.SetActive(false);
                if (canvas2 != null && currentRoomIndex >= 3) canvas2.gameObject.SetActive(false); // Only hide/show canvas2 for rooms 4-6 (indices 3-5)
                

                UpdateUI();

                PlayerPrefs.SetInt("IsShowingQuestions", 1);
            }
            else
            {
                int lastRoom = PlayerPrefs.GetInt("LastRoomNumber", -1);
                if (lastRoom >= 0 && lastRoom <= 2 && canvas2 != null)
                {
                    canvas2.gameObject.SetActive(true);
                }
                else if (lastRoom >= 3 && lastRoom <= 5 && canvas2 != null)
                {
                    canvas2.gameObject.SetActive(true);
                }
                PlayerPrefs.SetInt("IsShowingQuestions", 0);
            }
            
            PlayerPrefs.Save();
        }
    }
    public void SetCurrentRoom(int roomIndex)
    {
        //Debug.Log($"[DEBUG] SetCurrentRoom chiamato con roomIndex: {roomIndex}");
        
        if (roomIndex < 0 || roomIndex > 5)
        {
           // Debug.LogError($"[ERRORE] roomIndex {roomIndex} non valido. Deve essere tra 0 e 5.");
            return;
        }
        
        currentRoomIndex = roomIndex;
        currentRoomName = "Stanza" + (roomIndex + 1);
        
        //Debug.Log($"[DEBUG] Impostata stanza corrente: {currentRoomName} (indice: {currentRoomIndex})");
        
        try 
        {
            LoadQuestionsForCurrentAvatarAndRoom();
            //Debug.Log("[DEBUG] Domande caricate con successo");
        }
        catch (System.Exception e)
        {
            //Debug.LogError($"[ERRORE] Errore nel caricamento delle domande: {e.Message}");
            //Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }

    private void DisplayCurrentQuestion()
    {
        if (currentQuestions == null || currentQuestions.Length == 0)
        {
            //Debug.LogWarning("[AVVISO] Nessuna domanda disponibile da mostrare");
            return;
        }

        if (currentQuestionIndex < 0 || currentQuestionIndex >= currentQuestions.Length)
        {
            //Debug.LogError($"[ERRORE] Indice domanda non valido: {currentQuestionIndex}");
            return;
        }

        if (yesButton != null)
        {
            yesButton.gameObject.SetActive(true);
        }
        
        if (noButton != null)
        {
            noButton.gameObject.SetActive(true);
        }

        if (questionText != null)
        {
            questionText.text = currentQuestions[currentQuestionIndex];
            //Debug.Log($"[DEBUG] Mostrata domanda {currentQuestionIndex + 1}/{currentQuestions.Length}: {questionText.text}");
        }
        else
        {
            //Debug.LogError("[ERRORE] Riferimento a questionText mancante");
        }
    }

    private void LoadQuestionsForCurrentAvatarAndRoom()
    {
        //Debug.Log($"[DEBUG] LoadQuestionsForCurrentAvatarAndRoom - Avatar: {currentAvatarIndex}, Stanza: {currentRoomName}");
        
        if (currentAvatarIndex < 0 || currentAvatarIndex >= roomAvatarQuestions.Count)
        {
            //Debug.LogError($"[ERRORE] Indice avatar non valido: {currentAvatarIndex}");
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        //Debug.Log($"[DEBUG] Nome scena corrente: {sceneName}");

        var avatarQuestions = roomAvatarQuestions[currentAvatarIndex];
        //Debug.Log($"[DEBUG] Avatar {currentAvatarIndex} ha domande per {avatarQuestions.Keys.Count} scene");

        if (avatarQuestions.TryGetValue(currentRoomName, out string[] questions))
        {
            //Debug.Log($"[DEBUG] Trovate {questions.Length} domande specifiche per {currentRoomName}");
            currentQuestions = questions;
            currentQuestionIndex = 0;
            DisplayCurrentQuestion();
            return;
        }
        
        if (avatarQuestions.TryGetValue(sceneName, out questions))
        {
            //Debug.Log($"[DEBUG] Trovate {questions.Length} domande per la scena {sceneName}");
            currentQuestions = questions;
            currentQuestionIndex = 0;
            DisplayCurrentQuestion();
        }
        else
        {
            //Debug.LogWarning($"[AVVISO] Nessuna domanda trovata per la scena: {sceneName} o per la stanza: {currentRoomName}");
            currentQuestions = null;
        }
    }

    void FilterQuestions()
    {
        DateTime today = DateTime.Today;
        bool allQuestionsAnsweredToday = true;
        allYes = true;
        
        for (int i = 0; i < currentQuestions.Length; i++)
        {
            string questionKey = $"{ANSWERED_QUESTIONS_KEY}_{i}";
            
            if (PlayerPrefs.HasKey(questionKey))
            {
                DateTime lastAnsweredDate = DateTime.Parse(PlayerPrefs.GetString(questionKey));
                if (lastAnsweredDate.Date != today)
                {
                    allQuestionsAnsweredToday = false;
                    currentQuestionIndex = i;
                    break;
                }
                string answerKey = $"{ANSWERED_QUESTIONS_KEY}_{i}_Answer";
                if (PlayerPrefs.GetInt(answerKey, 0) == 0)
                {
                    allYes = false;
                }
            }
            else
            {
                allQuestionsAnsweredToday = false;
                currentQuestionIndex = i;
                break;
            }
        }
        
        if (allQuestionsAnsweredToday)
        {
            ShowCompletionMessage();
            return;
        }
        
        UpdateQuestion();
    }

    void UpdateQuestion()
    {
        if (currentQuestionIndex < currentQuestions.Length)
        {
            if (questionText != null)
            {
                questionText.text = currentQuestions[currentQuestionIndex];
            }
            
            if (yesButton != null)
            {
                yesButton.gameObject.SetActive(true);
            }
            
            if (noButton != null)
            {
                noButton.gameObject.SetActive(true);
            }
        }
        else
        {
            ShowCompletionMessage();
        }
    }

    public void ShowCompletionMessage()
    {
        if (questionText != null)
        {
            if (allYes)
            {
                questionText.text = "Bravo! Hai completato tutte le attività per questa stanza!";
            }
            else
            {
                questionText.text = "Ricorda di completare le attività che hai dimenticato!";
            }
        }
        
        if (yesButton != null)
        {
            yesButton.gameObject.SetActive(false);
        }
        
        if (noButton != null)
        {
            noButton.gameObject.SetActive(false);
        }
        PlayerPrefs.SetInt(ALL_YES_KEY, allYes ? 1 : 0);
        PlayerPrefs.Save();

        NotifyBachecaManagers();

        SaveRoomCompletionStatusToFile();
    }

    void NotifyBachecaManagers()
    {
        BachecaManager[] bachecaManagers = FindObjectsOfType<BachecaManager>(true);
        foreach (var bachecaManager in bachecaManagers)
        {
            bachecaManager.UpdateButtonStates();
        }
    }

    void Answer(bool isYes)
    {
        DateTime today = DateTime.Today;
        string currentQuestion = currentQuestions[currentQuestionIndex];
        
        string questionKey = $"{ANSWERED_QUESTIONS_KEY}_{currentQuestionIndex}";
        string answerKey = $"{ANSWERED_QUESTIONS_KEY}_{currentQuestionIndex}_Answer";
        
        PlayerPrefs.SetString(questionKey, today.ToString());
        PlayerPrefs.SetInt(answerKey, isYes ? 1 : 0);
        
        if (isYes)
        {
            score += 100;
            PlayerPrefs.SetInt(SCORE_KEY, score);
        }
        else
        {
            allYes = false;
        }
        
        PlayerPrefs.Save();

        SaveQuestionAnswerToFile(currentQuestion, isYes);

        UpdateUI();

        string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string responseScore = score.ToString(); 
        
        StartCoroutine(SendAvatarAccessData(currentQuestion, isYes ? "Sì" : "No", responseScore, currentDateTime));
        
        FindNextAvailableQuestion();
    }

    void SaveQuestionAnswerToFile(string question, bool answer)
    {
        if (!saveToFile || string.IsNullOrEmpty(currentAvatarFolderPath))
            return;
            
        try
        {
            string roomLogFile = Path.Combine(currentAvatarFolderPath, $"{currentRoomName}_Risposte.txt");

            string answerText = answer ? "Sì" : "No";
            string entry = $"[{DateTime.Now}] Domanda: {question} - Risposta: {answerText}\n";
            
            File.AppendAllText(roomLogFile, entry);
            
            //Debug.Log($"Risposta salvata nel file: {roomLogFile}");
        }
        catch (Exception e)
        {
            //Debug.LogError($"Errore nel salvataggio della risposta: {e.Message}");
        }
    }
    
    void SaveRoomCompletionStatusToFile()
    {
        if (!saveToFile || string.IsNullOrEmpty(currentAvatarFolderPath))
            return;
            
        try
        {
            string completionLogFile = Path.Combine(currentAvatarFolderPath, $"{currentRoomName}_Completamento.txt");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"[{DateTime.Now}] Stato completamento stanza: {currentRoomName}");
            sb.AppendLine($"Tutte le attività completate: {(allYes ? "Sì" : "No")}");
            sb.AppendLine("Dettaglio risposte:");
            
            for (int i = 0; i < currentQuestions.Length; i++)
            {
                string questionKey = $"{ANSWERED_QUESTIONS_KEY}_{i}";
                string answerKey = $"{ANSWERED_QUESTIONS_KEY}_{i}_Answer";
                
                if (PlayerPrefs.HasKey(questionKey))
                {
                    DateTime answerDate = DateTime.Parse(PlayerPrefs.GetString(questionKey));
                    bool wasYes = PlayerPrefs.GetInt(answerKey, 0) == 1;
                    
                    sb.AppendLine($"- Domanda: {currentQuestions[i]}");
                    sb.AppendLine($"  Risposta: {(wasYes ? "Sì" : "No")} ({answerDate})");
                }
                else
                {
                    sb.AppendLine($"- Domanda: {currentQuestions[i]} (Non risposta)");
                }
            }
            
            sb.AppendLine("----------------------------------------------------");

            File.AppendAllText(completionLogFile, sb.ToString());
            
            //Debug.Log($"Stato di completamento salvato nel file: {completionLogFile}");
        }
        catch (Exception e)
        {
            //Debug.LogError($"Errore nel salvataggio dello stato di completamento: {e.Message}");
        }
    }

    void FindNextAvailableQuestion()
    {
        DateTime today = DateTime.Today;
        bool foundNext = false;
        
        for (int i = currentQuestionIndex + 1; i < currentQuestions.Length; i++)
        {
            string questionKey = $"{ANSWERED_QUESTIONS_KEY}_{i}";
            
            if (!PlayerPrefs.HasKey(questionKey) || 
                DateTime.Parse(PlayerPrefs.GetString(questionKey)).Date < today)
            {
                currentQuestionIndex = i;
                foundNext = true;
                break;
            }
        }
        
        if (!foundNext)
        {
            currentQuestionIndex = currentQuestions.Length;
        }
        
        UpdateQuestion();
        UpdateUI();
    }
    
    public void OnYesButtonClicked()
    {        
        Answer(true);
    }

    public void OnNoButtonClicked()
    {      
        Answer(false);
    }
     
    private IEnumerator SendAvatarAccessData(string question, string response, string score, string dateTime)
    {
       string playerName = PlayerPrefs.GetString($"AvatarName_{currentAvatarIndex}", $"Avatar_{currentAvatarIndex + 1}");
        
        WWWForm form = new WWWForm();
        form.AddField("player_name", playerName);
        form.AddField("id_domanda", question);
        form.AddField("risposta", response);
        form.AddField("score", score.ToString());
        form.AddField("date_and_time", dateTime);
        
        //Debug.Log($"Sending data to database:");
        //Debug.Log($"  player_name: {playerName}");
        //Debug.Log($"  domanda: {question}");
        //Debug.Log($"  risposta: {response}");
        //Debug.Log($"  score: {score}");
        //Debug.Log($"  date_and_time: {dateTime}");
        
        using (UnityWebRequest www = UnityWebRequest.Post(databaseURL, form))
        {
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                //Debug.LogError("Error saving data to database: " + www.error);
                //Debug.LogError("Response code: " + www.responseCode);
                //Debug.LogError("Response text: " + www.downloadHandler?.text);
            }
            else
            {
               // Debug.Log("Data successfully saved to database");
                //Debug.Log("Server response: " + www.downloadHandler?.text);
            }
        }
    }

    public void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"{score}";
        }
    }

    public void LoadData()
    {
        score = PlayerPrefs.GetInt(SCORE_KEY, 0);
    }

    void CheckWeeklyReset()
    {
        if (PlayerPrefs.HasKey(LAST_RESET_KEY))
        {
            DateTime lastReset = DateTime.Parse(PlayerPrefs.GetString(LAST_RESET_KEY));
            DateTime now = DateTime.Now;
            
            if ((now - lastReset).TotalDays >= 7)
            {
                ResetWeeklyScore();
            }
        }
        else
        {
            PlayerPrefs.SetString(LAST_RESET_KEY, DateTime.Now.ToString());
            PlayerPrefs.Save();
        }
    }

    void ResetWeeklyScore()
    {
        int previousScore = score;
        
        score = 0;
        PlayerPrefs.SetInt(SCORE_KEY, score);
        PlayerPrefs.SetString(LAST_RESET_KEY, DateTime.Now.ToString());
        PlayerPrefs.Save();
        
        SaveWeekScoreToHistory(previousScore, DateTime.Now.AddDays(-7), DateTime.Now);
        SaveWeeklyResetToFile(previousScore, DateTime.Now.AddDays(-7), DateTime.Now);
    }

    void SaveWeekScoreToHistory(int weeklyScore, DateTime startDate, DateTime endDate)
    {
        string weekHistoryKey = $"WeeklyScoreHistory_Avatar{currentAvatarIndex}";
        string weekHistory = PlayerPrefs.GetString(weekHistoryKey, "");
        
        string newEntry = $"{weeklyScore},{startDate:yyyy-MM-dd},{endDate:yyyy-MM-dd};";
        
        weekHistory += newEntry;
        PlayerPrefs.SetString(weekHistoryKey, weekHistory);
        PlayerPrefs.Save();
    }
    
    void SaveWeeklyResetToFile(int previousScore, DateTime startDate, DateTime endDate)
    {
        if (!saveToFile || string.IsNullOrEmpty(currentAvatarFolderPath))
            return;
            
        try
        {
            string weekSummaryFile = Path.Combine(currentAvatarFolderPath, "RiepilogoSettimanale.txt");
            
            string entry = $"[{DateTime.Now}] Riepilogo settimanale ({startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd}):\n";
            entry += $"- Punteggio totale: {previousScore}\n";
            entry += $"- Punteggio azzerato per la nuova settimana\n";
            entry += "----------------------------------------------------\n";
            
            File.AppendAllText(weekSummaryFile, entry);
            
            //Debug.Log($"Riepilogo settimanale salvato nel file: {weekSummaryFile}");
        }
        catch (Exception e)
        {
            //Debug.LogError($"Errore nel salvataggio del riepilogo settimanale: {e.Message}");
        }
    }

    public void AvatarChanged()
    {
        UpdateCurrentRoom();
    }

    public void RoomChanged()
    {
        UpdateCurrentRoom();
    }
    public bool AreAllQuestionsAnswered()
    {
        DateTime today = DateTime.Today;
        
        if (currentQuestions == null || currentQuestions.Length == 0)
            return false;
            
        for (int i = 0; i < currentQuestions.Length; i++)
        {
            string questionKey = $"{ANSWERED_QUESTIONS_KEY}_{i}";
            
            if (!PlayerPrefs.HasKey(questionKey) || 
                DateTime.Parse(PlayerPrefs.GetString(questionKey)).Date < today)
            {
                return false;
            }
        }
        
        return true;
    }

    public bool AreAllAnswersYes()
    {
        if (currentQuestions == null || currentQuestions.Length == 0)
            return false;
            
        for (int i = 0; i < currentQuestions.Length; i++)
        {
            string answerKey = $"{ANSWERED_QUESTIONS_KEY}_{i}_Answer";
            if (PlayerPrefs.GetInt(answerKey, 0) == 0)
            {
                return false;
            }
        }
        return true;
    }
    
    public void ExportAllAvatarData()
    {
        if (!saveToFile || currentAvatarIndex < 0)
            return;
            
        try
        {
            string summaryFile = Path.Combine(currentAvatarFolderPath, "RiepilogoCompletoDati.txt");
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"RIEPILOGO COMPLETO DATI AVATAR {currentAvatarIndex + 1}");
            sb.AppendLine($"Data di esportazione: {DateTime.Now}");
            sb.AppendLine($"Punteggio attuale: {score}");
            sb.AppendLine();
            sb.AppendLine("STORICO PUNTEGGI SETTIMANALI:");
            
            string weekHistoryKey = $"WeeklyScoreHistory_Avatar{currentAvatarIndex}";
            string weekHistory = PlayerPrefs.GetString(weekHistoryKey, "");
            
            if (!string.IsNullOrEmpty(weekHistory))
            {
                string[] entries = weekHistory.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string entry in entries)
                {
                    string[] parts = entry.Split(',');
                    if (parts.Length >= 3)
                    {
                        sb.AppendLine($"- Dal {parts[1]} al {parts[2]}: {parts[0]} punti");
                    }
                }
            }
            else
            {
                sb.AppendLine("Nessuno storico disponibile");
            }
            
            sb.AppendLine();
            sb.AppendLine("STATO COMPLETAMENTO STANZE:");
            
            foreach (string roomName in new[] { "Stanza1", "Stanza2", "Stanza3", "Stanza4", "Stanza5", "Stanza6" })
            {
                string roomKey = $"AllYesAnswers_Avatar{currentAvatarIndex}_{roomName}";
                if (PlayerPrefs.HasKey(roomKey))
                {
                    bool allYes = PlayerPrefs.GetInt(roomKey) == 1;
                    sb.AppendLine($"- {roomName}: {(allYes ? "Completata correttamente" : "Completata con alcune attività mancanti")}");
                }
                else {
                    sb.AppendLine($"- {roomName}: Non completata");
                }
            }
            
            sb.AppendLine();
            sb.AppendLine("DETTAGLIO RISPOSTE PER STANZA:");
            
            foreach (string roomName in new[] { "Stanza1", "Stanza2", "Stanza3", "Stanza4", "Stanza5", "Stanza6" })
            {
                sb.AppendLine($"\n{roomName}:");
                
                string[] roomQuestions = null;
                if (roomAvatarQuestions.ContainsKey(currentAvatarIndex) && 
                    roomAvatarQuestions[currentAvatarIndex].ContainsKey(roomName))
                {
                    roomQuestions = roomAvatarQuestions[currentAvatarIndex][roomName];
                    
                    for (int i = 0; i < roomQuestions.Length; i++)
                    {
                        string questionKey = $"AnsweredQuestions_Avatar{currentAvatarIndex}_{roomName}_{i}";
                        string answerKey = $"AnsweredQuestions_Avatar{currentAvatarIndex}_{roomName}_{i}_Answer";
                        
                        if (PlayerPrefs.HasKey(questionKey))
                        {
                            DateTime answerDate = DateTime.Parse(PlayerPrefs.GetString(questionKey));
                            bool wasYes = PlayerPrefs.GetInt(answerKey, 0) == 1;
                            
                            sb.AppendLine($"  - Domanda: {roomQuestions[i]}");
                            sb.AppendLine($"    Risposta: {(wasYes ? "Sì" : "No")} ({answerDate})");
                        }
                        else
                        {
                            sb.AppendLine($"  - Domanda: {roomQuestions[i]} (Non risposta)");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("  Nessuna domanda configurata per questa stanza");
                }
            }
            
            File.WriteAllText(summaryFile, sb.ToString());
            
            Debug.Log($"Riepilogo completo dei dati salvato nel file: {summaryFile}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Errore nell'esportazione dei dati dell'avatar: {e.Message}");
        }
    }
    
    public void ExportCSVData()
    {
        if (!saveToFile || currentAvatarIndex < 0)
            return;
            
        try
        {
            string csvFile = Path.Combine(currentAvatarFolderPath, "DatiRisposte.csv");
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Data,Stanza,Domanda,Risposta");
            
            foreach (string roomName in new[] { "Stanza1", "Stanza2", "Stanza3", "Stanza4", "Stanza5", "Stanza6" })
            {
                string[] roomQuestions = null;
                if (roomAvatarQuestions.ContainsKey(currentAvatarIndex) && 
                    roomAvatarQuestions[currentAvatarIndex].ContainsKey(roomName))
                {
                    roomQuestions = roomAvatarQuestions[currentAvatarIndex][roomName];
                    
                    for (int i = 0; i < roomQuestions.Length; i++)
                    {
                        string questionKey = $"AnsweredQuestions_Avatar{currentAvatarIndex}_{roomName}_{i}";
                        string answerKey = $"AnsweredQuestions_Avatar{currentAvatarIndex}_{roomName}_{i}_Answer";
                        
                        if (PlayerPrefs.HasKey(questionKey))
                        {
                            DateTime answerDate = DateTime.Parse(PlayerPrefs.GetString(questionKey));
                            bool wasYes = PlayerPrefs.GetInt(answerKey, 0) == 1;
                        
                            sb.AppendLine($"{answerDate:yyyy-MM-dd HH:mm:ss},{roomName},\"{roomQuestions[i]}\",{(wasYes ? "Sì" : "No")}");
                        }
                    }
                }
            }
            
            File.WriteAllText(csvFile, sb.ToString());
            
            //Debug.Log($"Dati CSV salvati nel file: {csvFile}");
        }
        catch (Exception e)
        {
            //Debug.LogError($"Errore nell'esportazione dei dati CSV: {e.Message}");
        }
    }
    
    public void CreateBackup()
    {
        if (!saveToFile || currentAvatarIndex < 0)
            return;
        
        try
        {
            string backupFolderPath = Path.Combine(baseSavePath, "Backup");
            if (!Directory.Exists(backupFolderPath))
            {
                Directory.CreateDirectory(backupFolderPath);
            }
            
            string backupFile = Path.Combine(backupFolderPath, $"Backup_Avatar{currentAvatarIndex}_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"BACKUP DATI AVATAR {currentAvatarIndex + 1}");
            sb.AppendLine($"Data backup: {DateTime.Now}");
            sb.AppendLine($"Punteggio: {score}");
            sb.AppendLine();
            
            foreach (string key in GetAllPlayerPrefsKeysForAvatar())
            {
                if (PlayerPrefs.HasKey(key))
                {
                    string valueType = "string";
                    string value = "";
                    
                    if (key.EndsWith("_Answer") || key.Contains("Score") || key.Contains("AllYes"))
                    {
                        valueType = "int";
                        value = PlayerPrefs.GetInt(key).ToString();
                    }
                    else
                    {
                        value = PlayerPrefs.GetString(key);
                    }
                    
                    sb.AppendLine($"{key}|{valueType}|{value}");
                }
            }
            
            File.WriteAllText(backupFile, sb.ToString());
            
            //Debug.Log($"Backup creato nel file: {backupFile}");
        }
        catch (Exception e)
        {
            //Debug.LogError($"Errore nella creazione del backup: {e.Message}");
        }
    }
    
    private List<string> GetAllPlayerPrefsKeysForAvatar()
    {
        List<string> keys = new List<string>();
        
        keys.Add(SCORE_KEY);
        keys.Add(LAST_RESET_KEY);
        keys.Add($"WeeklyScoreHistory_Avatar{currentAvatarIndex}");

        foreach (string roomName in new[] { "Stanza1", "Stanza2", "Stanza3", "Stanza4", "Stanza5", "Stanza6" })
        {
            keys.Add($"AllYesAnswers_Avatar{currentAvatarIndex}_{roomName}");

            if (roomAvatarQuestions.ContainsKey(currentAvatarIndex) && 
                roomAvatarQuestions[currentAvatarIndex].ContainsKey(roomName))
            {
                string[] roomQuestions = roomAvatarQuestions[currentAvatarIndex][roomName];
                
                for (int i = 0; i < roomQuestions.Length; i++)
                {
                    keys.Add($"AnsweredQuestions_Avatar{currentAvatarIndex}_{roomName}_{i}");
                    keys.Add($"AnsweredQuestions_Avatar{currentAvatarIndex}_{roomName}_{i}_Answer");
                }
            }
        }
        
        return keys;
    }
    
    public bool RestoreFromBackup(string backupFilePath)
    {
        if (!File.Exists(backupFilePath))
        {
            //Debug.LogError($"File di backup non trovato: {backupFilePath}");
            return false;
        }
        
        try
        {
            string[] lines = File.ReadAllLines(backupFilePath);
            bool isValidBackup = false;
            
            foreach (string line in lines)
            {
                if (line.StartsWith("BACKUP DATI AVATAR"))
                {
                    isValidBackup = true;
                    continue;
                }
                
                string[] parts = line.Split('|');
                if (parts.Length == 3)
                {
                    string key = parts[0];
                    string type = parts[1];
                    string value = parts[2];
                    
                    if (type == "int")
                    {
                        int intValue;
                        if (int.TryParse(value, out intValue))
                        {
                            PlayerPrefs.SetInt(key, intValue);
                        }
                    }
                    else if (type == "string")
                    {
                        PlayerPrefs.SetString(key, value);
                    }
                }
            }
            
            if (isValidBackup)
            {
                PlayerPrefs.Save();

                LoadData();
                UpdateUI();
                
                //Debug.Log("Backup ripristinato con successo");
                return true;
            }
            else
            {
                //Debug.LogError("Il file di backup non è valido");
                return false;
            }
        }
        catch (Exception e)
        {
           // Debug.LogError($"Errore nel ripristino del backup: {e.Message}");
            return false;
        }
    }
}