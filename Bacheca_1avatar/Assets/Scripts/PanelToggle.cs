using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PanelToggle : MonoBehaviour
{
    public Button giornoButton;
    public Button notteButton;
    public Button panel1Button;
    public Button logoutButton;
    public Button nuovoButton;
    public GameObject panel2;
    public GameObject panel3;
    public GameObject panel1; 
    private bool isInitialized = false;

    private void Start()
    {
        if (giornoButton == null || notteButton == null || panel1Button == null ||
            panel2 == null || panel3 == null || panel1 == null ||
            logoutButton == null || nuovoButton == null)
        {
            //Debug.LogError("Mancano componenti necessari nel PanelToggle!");
            return;
        }

        if (panel1 != null)
            panel1.SetActive(false);
        if (panel2 != null)
            panel2.SetActive(false);
        if (panel3 != null)
            panel3.SetActive(false);

        panel1Button.gameObject.SetActive(false); 
        logoutButton.gameObject.SetActive(false);
        nuovoButton.gameObject.SetActive(true);   

        SetupButtonListeners();
        isInitialized = true;
    }

    private void SetupButtonListeners()
    {
        try
        {
            if (giornoButton != null)
                giornoButton.onClick.AddListener(ShowPanel2);
            if (notteButton != null)
                notteButton.onClick.AddListener(ShowPanel3);
            if (panel1Button != null)
                panel1Button.onClick.AddListener(ShowPanel1);
        }
        catch (System.Exception e)
        {
           // Debug.LogError("Errore nella configurazione dei pulsanti: " + e.Message);
        }
    }

    private void ShowPanel2()
    {
        if (!isInitialized) return;

        try
        {
            if (panel3 != null)
                panel3.SetActive(false);
            if (panel2 != null)
                panel2.SetActive(true);

            panel1Button.gameObject.SetActive(true); 
            logoutButton.gameObject.SetActive(false); 
            nuovoButton.gameObject.SetActive(false); 
        }
        catch (System.Exception e)
        {
            //Debug.LogError("Errore nel cambio di pannello: " + e.Message);
        }
    }

    private void ShowPanel3()
    {
        if (!isInitialized) return;

        try
        {
            if (panel2 != null)
                panel2.SetActive(false);
            if (panel3 != null)
                panel3.SetActive(true);

            panel1Button.gameObject.SetActive(true);
            logoutButton.gameObject.SetActive(false);
            nuovoButton.gameObject.SetActive(false); 
        }
        catch (System.Exception e)
        {
           // Debug.LogError("Errore nel cambio di pannello: " + e.Message);
        }
    }

    private void ShowPanel1()
    {
        if (!isInitialized) return;

        try
        {
            if (panel2 != null)
                panel2.SetActive(false);
            if (panel3 != null)
                panel3.SetActive(false);
            if (panel1 != null)
                panel1.SetActive(true);

            panel1Button.gameObject.SetActive(false);
            logoutButton.gameObject.SetActive(true); 
            nuovoButton.gameObject.SetActive(false); 
        }
        catch (System.Exception e)
        {
            //Debug.LogError("Errore nel cambio di pannello: " + e.Message);
        }
    }
}