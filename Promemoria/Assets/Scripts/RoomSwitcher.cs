//The RoomSwitcher class manages scene transitions between different rooms in the application. 
//It handles the activation and deactivation of room GameObjects and provides an optional animation 
//system for smooth transitions. The class supports both immediate room changes and timed transitions, 
//with configurable duration through the Unity Inspector.

using UnityEngine;
using UnityEngine.UI;
 
public class RoomSwitcher : MonoBehaviour
{
    [Header("Rooms")]
    public GameObject stanzaCorrente;
    public GameObject stanzaNuova;
    
    [Header("Button")]
    public Button pulsanteCambioStanza;
    
    [Header("Options")]
    public bool usaAnimazione = false;
    public float durataCambio = 0;
    
    private void Start()
    {
        if (pulsanteCambioStanza != null)
        {
            pulsanteCambioStanza.onClick.AddListener(CambiaStanza);
        }
        
        if (stanzaCorrente != null)
            stanzaCorrente.SetActive(true);
            
        if (stanzaNuova != null)
            stanzaNuova.SetActive(false);
    }
    
    public void CambiaStanza()
    {
        if (stanzaCorrente == null || stanzaNuova == null)
            return;
        
        if (usaAnimazione)
        {
            StartCoroutine(CambiaStanzaConAnimazione());
        }
        else
        {
            CambiaStanzaIstantaneo();
        }
    }
    
    private void CambiaStanzaIstantaneo()
    {
        stanzaCorrente.SetActive(false);
        stanzaNuova.SetActive(true);
        
        GameObject temp = stanzaCorrente;
        stanzaCorrente = stanzaNuova;
        stanzaNuova = temp;
    }
    
    private System.Collections.IEnumerator CambiaStanzaConAnimazione()
    {
        if (pulsanteCambioStanza != null)
            pulsanteCambioStanza.interactable = false;
        
        CanvasGroup canvasGroupCorrente = stanzaCorrente.GetComponent<CanvasGroup>() ?? stanzaCorrente.AddComponent<CanvasGroup>();
        stanzaNuova.SetActive(true);
        CanvasGroup canvasGroupNuova = stanzaNuova.GetComponent<CanvasGroup>() ?? stanzaNuova.AddComponent<CanvasGroup>();
        canvasGroupNuova.alpha = 0f;
        
        float tempoTrascorso = 0f;
        while (tempoTrascorso < durataCambio)
        {
            float t = tempoTrascorso / durataCambio;
            canvasGroupCorrente.alpha = 1f - t;
            canvasGroupNuova.alpha = t;
            
            tempoTrascorso += Time.deltaTime;
            yield return null;
        }
        
        canvasGroupCorrente.alpha = 0f;
        canvasGroupNuova.alpha = 1f;
        
        stanzaCorrente.SetActive(false);
        canvasGroupCorrente.alpha = 1f;
        
        GameObject temp = stanzaCorrente;
        stanzaCorrente = stanzaNuova;
        stanzaNuova = temp;
        
        if (pulsanteCambioStanza != null)
            pulsanteCambioStanza.interactable = true;
    }
    
    
    private void OnDestroy()
    {
        if (pulsanteCambioStanza != null)
        {
            pulsanteCambioStanza.onClick.RemoveListener(CambiaStanza);
        }
    }
}