//The AvatarInteraction class manages avatar interactions in the game.
// It detects clicks on avatars, displays questionnaires, plays sound and visual effects, 
//and ensures avatars stay in their correct positions. 
//It also prevents unwanted physics movements and provides visual feedback during interactions.

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class AvatarInteraction : MonoBehaviour 
{
    [Header("Room Settings")]
    [Tooltip("Index of the room this avatar belongs to (0-5 for rooms 1-6)")]
    public int roomIndex = -1;
    
    [Header("Interaction Settings")]
    [Tooltip("Cooldown between interactions in seconds")]
    public float interactionCooldown = 0.5f;
    public AudioClip interactionSound;
    public ParticleSystem highlightEffect;
    
    [Header("Visual Feedback")]

    private AudioSource audioSource;
    private float lastInteractionTime = 0f;
    private PersistentQuestionCanvas questionCanvas;
    private Collider interactionCollider;
    private Rigidbody avatarRigidbody;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Camera playerCamera;
    
    private void Start()
    {
        //Debug.Log($"[DEBUG] Avatar per la stanza {roomIndex + 1} inizializzato");
        InitializeComponents();
        FreezeAvatar();
        SetupInteractionCollider();
        FindQuestionCanvas();
    
        if (roomIndex < 0 || roomIndex > 5)
        {
            //Debug.LogError($"[ERRORE] roomIndex {roomIndex} non valido. Deve essere tra 0 e 5.");
        }
    }
    
    private void InitializeComponents()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
            audioSource.playOnAwake = false;
        }
    }
    
    private void FreezeAvatar()
    {
        avatarRigidbody = GetComponent<Rigidbody>();
        if (avatarRigidbody != null)
        {
            avatarRigidbody.isKinematic = true;
            avatarRigidbody.useGravity = false;
            avatarRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
        
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script.GetType().Name.Contains("Movement") || 
                script.GetType().Name.Contains("Controller") ||
                script.GetType().Name.Contains("Navigation"))
            {
                script.enabled = false;
                //Debug.Log($"Disabilitato script di movimento: {script.GetType().Name}");
            }
        }
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }
    }
    
    private void SetupInteractionCollider()
    {
        interactionCollider = GetComponent<Collider>();
        if (interactionCollider == null)
        {
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = 1.5f;
            sphereCollider.isTrigger = true;
            interactionCollider = sphereCollider;
        }
        else
        {
            interactionCollider.isTrigger = true;
        }
    }
    
    private void FindQuestionCanvas()
    {
        questionCanvas = PersistentQuestionCanvas.Instance;
        if (questionCanvas == null)
        {
            questionCanvas = FindObjectOfType<PersistentQuestionCanvas>();
            if (questionCanvas == null)
            {
                //Debug.LogWarning("Nessun PersistentQuestionCanvas trovato nella scena!");
            }
        }
    }
    
    private void Update()
    {
        if (transform.position != originalPosition)
        {
            transform.position = originalPosition;
        }
        
        if (transform.rotation != originalRotation)
        {
            transform.rotation = originalRotation;
        }

    }
    
    private void OnMouseDown()
    {
        //Debug.Log("[DEBUG] OnMouseDown rilevato sul personaggio!");
        Interact();
    }
    
    public void Interact()
    {
        //Debug.Log("[DEBUG] Interact() chiamato");
        
        if (roomIndex == -1)
        {
           // Debug.Log("[DEBUG] Interazione ignorata: roomIndex non valido (-1)");
            return;
        }
        
        if (Time.time - lastInteractionTime < interactionCooldown)
        {
            //Debug.Log($"[DEBUG] Cooldown attivo, aspetta altri {interactionCooldown - (Time.time - lastInteractionTime)} secondi");
            return;
        }
        
        lastInteractionTime = Time.time;
        
        //Debug.Log($"[DEBUG] Interazione con l'avatar nella stanza: {roomIndex + 1}");
        
        if (interactionSound != null && audioSource != null)
        {
          //  Debug.Log("[DEBUG] Riproduco suono di interazione");
            audioSource.PlayOneShot(interactionSound);
        }
        
        if (highlightEffect != null && !highlightEffect.isPlaying)
        {
            highlightEffect.Play();
        }
         
        //Debug.Log($"[DEBUG] Cerco di mostrare il questionario per la stanza {roomIndex + 1} (indice: {roomIndex})");
        if (questionCanvas != null)
        {
           // Debug.Log($"[DEBUG] questionCanvas trovato, chiamo SetCurrentRoom({roomIndex})");
            try 
            {
                questionCanvas.SetCurrentRoom(roomIndex);

                if (questionCanvas != null)
                {
                    questionCanvas.UpdateUI();
                }
                PlayerPrefs.SetInt("LastRoomNumber", roomIndex);
                PlayerPrefs.Save();

                if (roomIndex >= 3 && roomIndex <= 5)
                {

                    RoomCanvasManager roomManager = FindObjectOfType<RoomCanvasManager>();
                    if (roomManager != null)
                    {
                        roomManager.OnRoomSelected(roomIndex);
                        //Debug.Log($"[DEBUG] Impostato canvas per la stanza {roomIndex + 1} (gruppo 3-5)");
                    }
                }
                
                questionCanvas.ShowCanvas(true);
                
                if (questionCanvas.AreAllQuestionsAnswered())
                {
                    //Debug.Log("[DEBUG] Tutte le domande sono già state completate");
                    questionCanvas.ShowCompletionMessage();
                }
                //Debug.Log("[DEBUG] ShowCanvas completato");
            }
            catch (System.Exception e)
            {
                //Debug.LogError($"[ERRORE] Errore durante la visualizzazione del questionario: {e.Message}");
                //Debug.LogError($"Stack trace: {e.StackTrace}");
            }
        }
        else
        {
            //Debug.LogError("[ERRORE] Impossibile mostrare il questionario: questionCanvas è null!");
            FindQuestionCanvas();
            if (questionCanvas != null)
            {
                //Debug.Log("[DEBUG] questionCanvas trovato al secondo tentativo");
                questionCanvas.SetCurrentRoom(roomIndex);
                questionCanvas.ShowCanvas(true);
            }
            else
            {
               // Debug.LogError("[ERRORE] questionCanvas ancora non trovato dopo secondo tentativo");
            }
        }

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(InteractionFeedback());
        }
    }
    
    private System.Collections.IEnumerator InteractionFeedback()
    {
        Vector3 originalScale = transform.localScale;
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float scale = Mathf.Lerp(1f, 1.1f, Mathf.Sin(progress * Mathf.PI));
            transform.localScale = originalScale * scale;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = originalScale;
    }
    
    public void ForceResetPosition()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        
        if (avatarRigidbody != null)
        {
            avatarRigidbody.velocity = Vector3.zero;
            avatarRigidbody.angularVelocity = Vector3.zero;
        }
    }
    
    // Draws a green wireframe in the Unity Editor around the interaction collider
// when the object is selected, helping visualize the interaction area
    private void OnDrawGizmosSelected()
    {
        if (interactionCollider != null)
        {
            Gizmos.color = Color.green;
            if (interactionCollider is SphereCollider)
            {
                SphereCollider sphere = interactionCollider as SphereCollider;
                Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
            }
            else if (interactionCollider is BoxCollider)
            {
                BoxCollider box = interactionCollider as BoxCollider;
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position + box.center, box.size);
            }
        }
    }
}