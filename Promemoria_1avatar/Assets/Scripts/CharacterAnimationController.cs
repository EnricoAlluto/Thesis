using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class CharacterAnimationController : MonoBehaviour
{
    public Animator animator;
    public Button yesButton;
    public Button noButton;
    public GameObject mainSceneRoom;
    
    private bool isInMainScene = true;
    private string[] randomAnimations = { "IsWaving", "IsPointing" };
    private float randomAnimationInterval = 10f;
    private float randomAnimationVariance = 5f;
    private Coroutine randomAnimationCoroutine;
    private Coroutine talkingAnimationCoroutine;
    private Coroutine dancingAnimationCoroutine;
    private Coroutine gestureAnimationCoroutine;
    private bool allQuestionsCompleted = false;
    private float animationDuration = 4f;
    
    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                return;
        }
        
        FindMainSceneRoom();
        ResetToIdleAnimation();
        CheckCurrentRoom();
    }
    
    void FindMainSceneRoom()
    {
        if (mainSceneRoom == null)
            mainSceneRoom = GameObject.Find("MainScene");
    }
    
    void CheckCurrentRoom()
    {
        isInMainScene = IsMainSceneActive();
        
        if (isInMainScene)
        {
            UnregisterButtonListeners();
            StopTalkingAnimation();
            StopDancingAnimation();
            StartGestureLoopAnimation();
            StartRandomAnimations();
        }
        else
        {
            StopRandomAnimations();
            StopGestureAnimation();
            FindButtons();
            CheckAllQuestionsAnswered();
            
            if (allQuestionsCompleted)
                StartDancingAnimation();
            else
                StartTalkingAnimation();
        }
    }
    
    bool IsMainSceneActive()
    {
        if (mainSceneRoom != null)
            return mainSceneRoom.activeInHierarchy;
            
        FindMainSceneRoom();
        return mainSceneRoom != null && mainSceneRoom.activeInHierarchy;
    }
    
    void StartGestureLoopAnimation()
    {
        StopGestureAnimation();
        
        if (animator != null && animator.enabled)
        {
            animator.SetBool("IsGesture", true);
        }
    }
    
    void StopGestureAnimation()
    {
        if (animator != null)
            animator.SetBool("IsGesture", false);
            
        if (gestureAnimationCoroutine != null)
        {
            StopCoroutine(gestureAnimationCoroutine);
            gestureAnimationCoroutine = null;
        }
    }
    
    void StartTalkingAnimation()
    {
        StopTalkingAnimation();
        StopDancingAnimation();
        
        if (animator != null && animator.enabled)
        {
            animator.SetBool("IsTalking", true);
        }
    }
    
    void StartDancingAnimation()
    {
        StopTalkingAnimation();
        StopDancingAnimation();
        
        if (animator != null && animator.enabled)
        {
            animator.SetBool("IsDancing", true);
        }
    }
    
    void StopTalkingAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsTalking", false);
            //Debug.Log("Flag IsTalking impostato a false");
        }
        
        if (talkingAnimationCoroutine != null)
        {
            StopCoroutine(talkingAnimationCoroutine);
            talkingAnimationCoroutine = null;
           // Debug.Log("Coroutine dell'animazione di talking fermata");
        }
    }
    
    void StopDancingAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsDancing", false);
            //Debug.Log("Flag IsDancing impostato a false");
        }
        if (dancingAnimationCoroutine != null)
        {
            StopCoroutine(dancingAnimationCoroutine);
            dancingAnimationCoroutine = null;
           // Debug.Log("Coroutine dell'animazione di danza fermata");
        }
    }
    
    void CheckAllQuestionsAnswered()
    {
        PersistentQuestionCanvas questionCanvas = PersistentQuestionCanvas.Instance;
        
        if (questionCanvas != null)
        {
            allQuestionsCompleted = questionCanvas.AreAllQuestionsAnswered();
           // Debug.Log("Controllo domande completate: " + allQuestionsCompleted);

            if (allQuestionsCompleted && !isInMainScene && animator != null)
            {
                StopTalkingAnimation();
                StartDancingAnimation();
            }
        }
        else
        {
           // Debug.LogWarning("Impossibile verificare lo stato delle domande: PersistentQuestionCanvas non trovato");
            allQuestionsCompleted = false;
        }
    }
    
    IEnumerator PlayFirstRandomAnimationWithRandomDelay()
    {
        float randomDelay = Random.Range(5f, 10f);
        //Debug.Log("Attendo " + randomDelay + " secondi prima della prima animazione casuale");
        yield return new WaitForSeconds(randomDelay);
        
        if (!isInMainScene) yield break;
        
        string randomAnim = randomAnimations[Random.Range(0, randomAnimations.Length)];
        //Debug.Log("Avvio prima animazione casuale: " + randomAnim);
        
        PlayAnimationDirectly(randomAnim);
    }
    
    void PlayAnimationDirectly(string animationName)
    {
        if (animator != null)
        {
            ResetAllAnimations();
            
            //Debug.Log("Imposto animazione: " + animationName + " su " + animator.gameObject.name);
           
            animator.SetBool(animationName, true);
            
            StartCoroutine(VerifyAnimationFlag(animationName));
            
            StartCoroutine(ResetAnimationAfterDelay(animationDuration));
        }
        else
        {
           // Debug.LogError("Animator non disponibile per eseguire l'animazione");
        }
    }
    
    IEnumerator VerifyAnimationFlag(string animationName)
    {
        yield return null;
        
        bool paramValue = animator.GetBool(animationName);
        //Debug.Log("Verifica parametro " + animationName + ": " + paramValue);
    }
    
    IEnumerator ResetAnimationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (gameObject.activeInHierarchy)
        {
            ResetAllAnimations();
            //Debug.Log("Animazione completata, ritorno a idle");
            
            if (isInMainScene)
            {
                animator.SetBool("IsGesture", true);
                //Debug.Log("Ripristino animazione di gesture dopo un'altra animazione");
            }
            else if (!isInMainScene)
            {
                if (allQuestionsCompleted)
                {
                    animator.SetBool("IsDancing", true);
                    //Debug.Log("Ripristino animazione di danza dopo un'altra animazione");
                }
                else
                {
                    animator.SetBool("IsTalking", true);
                    //Debug.Log("Ripristino animazione di talking dopo un'altra animazione");
                }
            }
        }
    }
    
    void StartRandomAnimations()
    {
        StopRandomAnimations();
        
        //Debug.Log("Avvio animazioni casuali");

        randomAnimationCoroutine = StartCoroutine(PlayRandomAnimationsCoroutine());
    }
    
    void StopRandomAnimations()
    {
        if (randomAnimationCoroutine != null)
        {
            StopCoroutine(randomAnimationCoroutine);
            randomAnimationCoroutine = null;
            //Debug.Log("Animazioni casuali interrotte");
        }
    }
    
    IEnumerator PlayRandomAnimationsCoroutine()
    {
        //Debug.Log("Coroutine animazioni casuali avviata");
        
        while (true)
        {
            if (!IsMainSceneActive())
            {
                isInMainScene = false;
               // Debug.Log("Non siamo più nella MainScene, interrompo le animazioni casuali");
                break;
            }
            
            float currentInterval = randomAnimationInterval + Random.Range(-randomAnimationVariance, randomAnimationVariance);
            //Debug.Log("In attesa di " + currentInterval + " secondi prima della prossima animazione casuale");
            yield return new WaitForSeconds(currentInterval);

            if (!IsMainSceneActive())
            {
                isInMainScene = false;
                break;
            }

            string randomAnim = randomAnimations[Random.Range(0, randomAnimations.Length)];
            //Debug.Log("Eseguo animazione casuale: " + randomAnim);
            
            PlayAnimationDirectly(randomAnim);
        }
        
        //Debug.Log("Coroutine animazioni casuali terminata");
    }
    
    void ResetAllAnimations()
    {
        if (animator != null)
        {
            animator.SetBool("IsYes", false);
            animator.SetBool("IsNo", false);
            animator.SetBool("IsWaving", false);
            animator.SetBool("IsPointing", false);
            animator.SetBool("IsGesture", false);
            animator.SetBool("IsTalking", false);
            animator.SetBool("IsDancing", false);
            
           // Debug.Log("Animazioni resettate");
        }
        else
        {
           // Debug.LogWarning("Impossibile resettare le animazioni: Animator è null");
        }
    }
    
    void ResetToIdleAnimation()
    {
        if (animator != null)
        {
            ResetAllAnimations();
            //Debug.Log("Animazioni resettate allo stato idle");
        }
        else
        {
           // Debug.LogWarning("Componente Animator non assegnato al CharacterAnimationController");
        }
    }
    
    void FindButtons()
    {
        PersistentQuestionCanvas questionCanvas = PersistentQuestionCanvas.Instance;
        
        if (questionCanvas != null)
        {
            yesButton = questionCanvas.yesButton;
            noButton = questionCanvas.noButton;
            RegisterButtonListeners();
            //Debug.Log("Pulsanti trovati e listener registrati");
        }
        else
        {
           // Debug.LogWarning("PersistentQuestionCanvas non trovato. Potrebbe essere caricato più tardi.");
        }
    }
    
    void RegisterButtonListeners()
    {
        if (yesButton != null)
        {
            yesButton.onClick.AddListener(OnYesClicked);
           // Debug.Log("Listener per il pulsante Yes registrato");
        }
        else
        {
            //Debug.LogWarning("Pulsante Yes non trovato nel PersistentQuestionCanvas");
        }
        
        if (noButton != null)
        {
            noButton.onClick.AddListener(OnNoClicked);
            //Debug.Log("Listener per il pulsante No registrato");
        }
        else
        {
            //Debug.LogWarning("Pulsante No non trovato nel PersistentQuestionCanvas");
        }
    }
    
    void UnregisterButtonListeners()
    {
        if (yesButton != null)
        {
            yesButton.onClick.RemoveListener(OnYesClicked);
           // Debug.Log("Listener per il pulsante Yes rimosso");
        }
        
        if (noButton != null)
        {
            noButton.onClick.RemoveListener(OnNoClicked);
           // Debug.Log("Listener per il pulsante No rimosso");
        }
    }
    
    void OnYesClicked()
    {
        if (!gameObject.activeInHierarchy || isInMainScene) return;
        
        //Debug.Log("Pulsante Yes cliccato");
        StopAllCoroutines();
    
        StartCoroutine(PlayYesAnimationSequence());

        StartCoroutine(CheckQuestionsAfterAnswerDelay());
    }
    
    void OnNoClicked()
    {
        if (!gameObject.activeInHierarchy || isInMainScene) return;
        
       // Debug.Log("Pulsante No cliccato");
        
        StopAllCoroutines();
        
        StartCoroutine(PlayNoAnimationSequence());
        
        StartCoroutine(CheckQuestionsAfterAnswerDelay());
    }
    
    IEnumerator CheckQuestionsAfterAnswerDelay()
    {
        yield return new WaitForSeconds(0.5f);
        
        CheckAllQuestionsAnswered();
    }
    
    IEnumerator PlayYesAnimationSequence()
    {
        if (animator != null)
        {
            ResetAllAnimations();
            
            //Debug.Log("Avvio animazione Yes");
            animator.SetBool("IsYes", true);
            yield return new WaitForSeconds(animationDuration);
            
            ResetAllAnimations();
            if (!isInMainScene)
            {
                if (allQuestionsCompleted)
                {
                    animator.SetBool("IsDancing", true);
                   // Debug.Log("Ritorno all'animazione di danza dopo Yes");
                }
                else
                {
                    animator.SetBool("IsTalking", true);
                   // Debug.Log("Ritorno all'animazione di talking dopo Yes");
                }
            }
            //Debug.Log("Animazione Yes completata");
        }
    }
    
    IEnumerator PlayNoAnimationSequence()
    {
        if (animator != null)
        {
            ResetAllAnimations();

            //Debug.Log("Avvio animazione No");
            animator.SetBool("IsNo", true);
            yield return new WaitForSeconds(animationDuration);

            ResetAllAnimations();
            if (!isInMainScene)
            {
                if (allQuestionsCompleted)
                {
                    animator.SetBool("IsDancing", true);
                    //Debug.Log("Ritorno all'animazione di danza dopo No");
                }
                else
                {
                    animator.SetBool("IsTalking", true);
                    //Debug.Log("Ritorno all'animazione di talking dopo No");
                }
            }
            //Debug.Log("Animazione No completata");
        }
    }
    
    public void OnRoomChanged()
    {
        //Debug.Log("Cambio stanza rilevato");
        CheckCurrentRoom();
        
        if (!isInMainScene)
        {
            FindButtons();
        }
        
        ResetToIdleAnimation();
        
        if (isInMainScene)
        {
            StartGestureLoopAnimation();
            StopTalkingAnimation();
            StopDancingAnimation();
        }
        else
        {
            StopGestureAnimation();
            CheckAllQuestionsAnswered();
            
            if (allQuestionsCompleted)
            {
                StartDancingAnimation();
            }
            else
            {
                StartTalkingAnimation();
            }
        }
    }
    
    void OnEnable()
    {
        //Debug.Log("CharacterAnimationController abilitato");
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        StartCoroutine(DelayedFindButtons());
        
        if (animator != null)
        {
            animator.enabled = true;
        }

        ResetToIdleAnimation();

        StartCoroutine(DelayedRoomCheck());
    }
    
    IEnumerator DelayedRoomCheck()
    {

        yield return new WaitForSeconds(0.5f);
        
        FindMainSceneRoom();
        
        CheckCurrentRoom();
        
        if (isInMainScene)
        {
            if (randomAnimationCoroutine == null)
            {
                //Debug.Log("Avvio animazioni casuali dopo il ritardo");
                StartRandomAnimations();
            }

            StartGestureLoopAnimation();
        }

        else if (!isInMainScene)
        {
            CheckAllQuestionsAnswered();
            
            if (allQuestionsCompleted)
            {
                StartDancingAnimation();
            }
            else
            {
                StartTalkingAnimation();
            }
        }
    }
    
    void OnDisable()
    {
        //Debug.Log("CharacterAnimationController disabilitato");
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StopRandomAnimations();
        StopTalkingAnimation();
        StopDancingAnimation();
        StopGestureAnimation();
        UnregisterButtonListeners();
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("Nuova scena caricata: " + scene.name);
        
        StartCoroutine(DelayedRoomCheck());
    }
    
    IEnumerator DelayedFindButtons()
    {
        yield return null;

        CheckCurrentRoom();

        if (!isInMainScene)
        {
            FindButtons();

            CheckAllQuestionsAnswered();
        }

        if (isInMainScene)
        {
            StartGestureLoopAnimation();
        }
        else if (allQuestionsCompleted)
        {
            StartDancingAnimation();
        }
        else
        {
            StartTalkingAnimation();
        }
    }
    
void StartGestureAnimation()
{
    StopGestureAnimation();
    
    //Debug.Log("Avvio animazione di gesture");
    if (animator != null && animator.enabled)
    {
        animator.SetBool("IsGesture", true);
        //Debug.Log("Flag IsGesture impostato a true");
    }
    else
    {
        //Debug.LogError("Impossibile avviare l'animazione di gesture: Animator non disponibile o disabilitato");
    }
}

public void ForceRefreshAnimationState()
{
    FindMainSceneRoom();
    isInMainScene = IsMainSceneActive();
    
    if (isInMainScene)
    {
        //Debug.Log("Forcing animation refresh in MainScene");
        StopRandomAnimations();
        StartGestureAnimation();
        StartRandomAnimations();
        StartCoroutine(PlayFirstRandomAnimationWithRandomDelay());
    }
    else
    {
        //Debug.Log("Forcing animation refresh in other room");
        StopRandomAnimations();
        StopGestureAnimation();
        CheckAllQuestionsAnswered();
        
        if (allQuestionsCompleted)
        {
            StartDancingAnimation();
        }
        else
        {
            StartTalkingAnimation();
        }
    }
}

void Update()
{
    bool currentMainSceneState = IsMainSceneActive();
    
    if (currentMainSceneState != isInMainScene)
    {
       // Debug.Log("Rilevato cambio di stato della stanza MainScene: " + currentMainSceneState);
        isInMainScene = currentMainSceneState;
        OnRoomChanged();
    }
    
    if (isInMainScene && randomAnimationCoroutine == null)
    {
        //Debug.LogWarning("Rilevata coroutine animazioni inattiva in MainScene, riavvio...");
        StartRandomAnimations();
    }
    
    if (isInMainScene && !animator.GetBool("IsGesture") &&
        !animator.GetBool("IsWaving") && !animator.GetBool("IsPointing"))
    {
        //Debug.LogWarning("Rilevata animazione di gesture inattiva quando dovrebbe essere attiva, riavvio...");
        StartGestureAnimation();
    }

    if (!isInMainScene)
    {
        bool previousQuestionsState = allQuestionsCompleted;
        CheckAllQuestionsAnswered();
        
        if (previousQuestionsState != allQuestionsCompleted)
        {
            //Debug.Log("Rilevato cambio di stato delle domande: " + allQuestionsCompleted);
            
            if (allQuestionsCompleted)
            {
                StartDancingAnimation();
            }
            else
            {
                StartTalkingAnimation();
            }
        }

        if (allQuestionsCompleted && !animator.GetBool("IsDancing") &&
            !animator.GetBool("IsYes") && !animator.GetBool("IsNo"))
        {
            //Debug.LogWarning("Rilevata animazione di danza inattiva quando dovrebbe essere attiva, riavvio...");
            StartDancingAnimation();
        }
        else if (!allQuestionsCompleted && !animator.GetBool("IsTalking") &&
                 !animator.GetBool("IsYes") && !animator.GetBool("IsNo"))
        {
           // Debug.LogWarning("Rilevata animazione di talking inattiva quando dovrebbe essere attiva, riavvio...");
            StartTalkingAnimation();
        }
    }
}
}