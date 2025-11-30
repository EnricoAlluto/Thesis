using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;

public class PersistentQuestionCanvas : MonoBehaviour
{
    public static PersistentQuestionCanvas Instance { get; private set; }

    public Canvas questionCanvas;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI scoreText;
    public Button yesButton;
    public Button noButton;

    public GameObject[] avatarModels;

    public string databaseURL = "https://web-m5yj.onrender.com/proxy";

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
        { 2, new Dictionary<string, string[]> {
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

    private string[] currentQuestions;
    private int currentQuestionIndex = 0;
    private bool allYes = true;
    private int score = 0;
    
    private int currentAvatarIndex = -1;
    private string currentRoomName = "";
    
    private string SCORE_KEY => $"PlayerScore_Avatar{currentAvatarIndex}";
    private string LAST_RESET_KEY => $"LastWeeklyReset_Avatar{currentAvatarIndex}";
    private string ANSWERED_QUESTIONS_KEY => $"AnsweredQuestions_Avatar{currentAvatarIndex}_{currentRoomName}";
    private string ALL_YES_KEY => $"AllYesAnswers_Avatar{currentAvatarIndex}_{currentRoomName}";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
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

        ShowCanvas(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateCurrentRoom();
    }
    
    public void UpdateCurrentRoom()
    {
        int newAvatarIndex = AvatarSelection.GetSelectedAvatarIndex();
        int roomIndex = AvatarSelection.GetSelectedRoomIndex();
        string newRoomName = roomIndex >= 0 ? $"Stanza{roomIndex + 1}" : "MainScene";
        
        bool changed = (currentAvatarIndex != newAvatarIndex || currentRoomName != newRoomName);
        
        currentAvatarIndex = newAvatarIndex;
        currentRoomName = newRoomName;
        
        if (changed)
        {
            InitializeQuestionManager();
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
        
        ShowCanvas(true);
    }

    public void ShowCanvas(bool show)
    {
        if (questionCanvas != null)
        {
            questionCanvas.gameObject.SetActive(show);
        }
    }

    void LoadQuestionsForCurrentAvatarAndRoom()
    {
        if (roomAvatarQuestions.ContainsKey(currentAvatarIndex) && 
            roomAvatarQuestions[currentAvatarIndex].ContainsKey(currentRoomName))
        {
            currentQuestions = roomAvatarQuestions[currentAvatarIndex][currentRoomName];
        }
        else
        {
            //Debug.LogWarning($"Nessuna domanda trovata per l'avatar {currentAvatarIndex} nella stanza {currentRoomName}. Uso domande predefinite.");
            currentQuestions = new string[] {"Sei pronto per iniziare?"};
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

    void ShowCompletionMessage()
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

        UpdateUI();

        string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string responseScore = score.ToString();
        StartCoroutine(SendAvatarAccessData(currentQuestion, isYes ? "Sì" : "No", responseScore, currentDateTime));
        
        FindNextAvailableQuestion();
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
        AvatarSelection avatarSelector = FindObjectOfType<AvatarSelection>();
        if (avatarSelector != null)
        {
            avatarSelector.HandleQuestionResponse(true);
        }
        
        Answer(true);
    }

    public void OnNoButtonClicked()
    {
        AvatarSelection avatarSelector = FindObjectOfType<AvatarSelection>();
        if (avatarSelector != null)
        {
            avatarSelector.HandleQuestionResponse(false);
        }
        
        Answer(false);
    }

    private IEnumerator SendAvatarAccessData(string question, string response, string score, string dateTime)
    {
        string playerName = PlayerPrefs.GetString("SavedUserName", $"Avatar_{currentAvatarIndex + 1}");
        
        WWWForm form = new WWWForm();
        form.AddField("player_name", playerName);
        form.AddField("id_domanda", question);
        form.AddField("risposta", response);
        form.AddField("score", score);
        form.AddField("date_and_time", dateTime);
        
        //Debug.Log($"Sending data to database:");
        //Debug.Log($"  player_name: {playerName}");
        //Debug.Log($"  id_domanda: {question}");
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
                //Debug.Log("Data successfully saved to database");
                //Debug.Log("Server response: " + www.downloadHandler?.text);
            }
        }
    }
    
    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"{score}";
        }
    }

    void LoadData()
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
}