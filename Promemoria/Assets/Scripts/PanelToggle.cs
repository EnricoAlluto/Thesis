//The PanelToggle manages the visibility of UI panels and controls button states in the application. 
//It initializes the UI elements, sets up button listeners, and handles the toggling of panel visibility.
//The class ensures proper UI state management during scene transitions and user interactions, 
//coordinating between different UI elements for a consistent user experience.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
 
public class PanelToggle : MonoBehaviour
{
    public Button panel1Button;
    public Button logoutButton;
    public Button nuovoButton;
    public GameObject panel1; 
    private bool isInitialized = false;

    private void Start()
    {
        if (panel1Button == null || panel1 == null ||
            logoutButton == null || nuovoButton == null)
        {
           // Debug.LogError("Mancano componenti necessari nel PanelToggle!");
            return;
        }

        if (panel1 != null)
            panel1.SetActive(false);

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
            if (panel1Button != null)
                panel1Button.onClick.AddListener(ShowPanel1);
        }
        catch (System.Exception e)
        {
            //Debug.LogError("Errore nella configurazione dei pulsanti: " + e.Message);
        }
    }

    private void ShowPanel1()
    {
        if (!isInitialized) return;

        try
        {
            if (panel1 != null)
                panel1.SetActive(true);

            panel1Button.gameObject.SetActive(false); 
            logoutButton.gameObject.SetActive(true); 
            nuovoButton.gameObject.SetActive(true);  
        }
        catch (System.Exception e)
        {
           // Debug.LogError("Errore nel cambio di pannello: " + e.Message);
        }
    }
}