using UnityEngine;
using UnityEngine.UI;

public class PanelButtonManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button nuovoButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button indietroButton;

    [Header("Panels")]
    [SerializeField] private GameObject panel1;
    [SerializeField] private GameObject panel2;
    [SerializeField] private GameObject panel3;

    private void Start()
    {
        if (nuovoButton == null || logoutButton == null || indietroButton == null ||
            panel1 == null || panel2 == null || panel3 == null)
        {
            //Debug.LogError("Please assign all button and panel references in the inspector!");
            return;
        }
        UpdateButtonVisibility();
    }

    public void UpdateButtonVisibility()
    {
        bool isPanel1Active = panel1.activeInHierarchy;
        bool isPanel2Active = panel2.activeInHierarchy;
        bool isPanel3Active = panel3.activeInHierarchy;

        //nuovoButton.gameObject.SetActive(!isPanel1Active && !isPanel2Active && !isPanel3Active);

        logoutButton.gameObject.SetActive(isPanel1Active && !isPanel2Active && !isPanel3Active);

        indietroButton.gameObject.SetActive(isPanel2Active || isPanel3Active);
    }

    public void SwitchToPanel(GameObject panelToActivate)
    {

        panel1.SetActive(false);
        panel2.SetActive(false);
        panel3.SetActive(false);
        
        panelToActivate.SetActive(true);

        UpdateButtonVisibility();
    }

    public void OnNuovoClicked()
    {
        Debug.Log("Nuovo button clicked");
    }

    public void OnLogoutClicked()
    {
        //Debug.Log("Logout button clicked");
    }

    public void OnIndietroClicked()
    {
       // Debug.Log("Indietro button clicked");
        SwitchToPanel(panel1);
    }
}
