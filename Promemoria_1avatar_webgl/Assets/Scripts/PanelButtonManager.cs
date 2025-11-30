using UnityEngine;
using UnityEngine.UI;

public class PanelButtonManager : MonoBehaviour
{
    [SerializeField] private Button nuovoButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button indietroButton;

    [SerializeField] private GameObject panel1;

    private void Awake()
    {
        if (nuovoButton == null || logoutButton == null || indietroButton == null ||
            panel1 == null)
        {
            //Debug.LogError("Please assign all button and panel references in the inspector!");
            return;
        }

        panel1.SetActive(false);

        logoutButton.gameObject.SetActive(false);
    }

    private void Start()
    {
        UpdateButtonVisibility();
    }

    public void UpdateButtonVisibility()
    {
        bool isPanel1Active = panel1.activeInHierarchy;

        //nuovoButton.gameObject.SetActive(!isPanel1Active);
        
        logoutButton.gameObject.SetActive(isPanel1Active);
        
        indietroButton.gameObject.SetActive(false);
    }

    public void SwitchToPanel(GameObject panelToActivate)
    {
        panel1.SetActive(false);

        if (panelToActivate == panel1)
        {
            panel1.SetActive(true);
        }

        UpdateButtonVisibility();
    }

    public void OnNuovoClicked()
    {
        //Debug.Log("Nuovo button clicked");
    }

    public void OnLogoutClicked()
    {
        //Debug.Log("Logout button clicked");
    }

    public void OnIndietroClicked()
    {
        //Debug.Log("Indietro button clicked - No action available as there's only one panel");
    }
}
