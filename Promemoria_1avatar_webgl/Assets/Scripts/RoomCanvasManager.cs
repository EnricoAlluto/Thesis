using UnityEngine;
using UnityEngine.UI;

public class RoomCanvasManager : MonoBehaviour
{
    public Canvas canvas1;
    public Canvas canvas2;
    public Canvas canvas3;
    public Button backButton;
     
    private int lastRoomNumber = -1;
    private BachecaManager bachecaManager;

    private void Start()
    {       
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
        else
        {
            //Debug.LogError("Back button is not assigned in RoomCanvasManager!");
        }

        bachecaManager = FindObjectOfType<BachecaManager>();
        if (bachecaManager == null)
        {
            //Debug.LogError("BachecaManager not found in the scene!");
        }
    }

    public void OnRoomSelected(int roomNumber)
    {
        //Debug.Log($"[RoomCanvasManager] OnRoomSelected chiamato con roomNumber: {roomNumber}");
        
        if (roomNumber < 0 || roomNumber > 5)
        {
            //Debug.LogError($"[RoomCanvasManager] Numero stanza non valido: {roomNumber}. Deve essere tra 0 e 5.");
            return;
        }
        
        lastRoomNumber = roomNumber;
        PlayerPrefs.SetInt("LastRoomNumber", lastRoomNumber);
        PlayerPrefs.Save();
        
       // Debug.Log($"[RoomCanvasManager] lastRoomNumber aggiornato e salvato: {lastRoomNumber}");
        
        if (canvas1 != null) canvas1.gameObject.SetActive(false);
        if (canvas2 != null) canvas2.gameObject.SetActive(false);
        //if (canvas3 != null) canvas3.gameObject.SetActive(false);
        
        if (roomNumber >= 0 && roomNumber <= 2)
        {
            if (canvas2 != null)
            {
                canvas2.gameObject.SetActive(true);
                //Debug.Log($"[RoomCanvasManager] Mostro canvas2 per la stanza {roomNumber} (gruppo 0-2)");
            }
        }
        else if (roomNumber >= 3 && roomNumber <= 5)
        {
            if (canvas3 != null)
            {
                canvas3.gameObject.SetActive(true);
                //Debug.Log($"[RoomCanvasManager] Mostro canvas3 per la stanza {roomNumber} (gruppo 3-5)");
            }
        }
    }
    
    public int GetLastRoomNumber()
    {
        return lastRoomNumber;
    }
    
    public void SetLastButtonClicked(int buttonIndex)
    {
        lastRoomNumber = buttonIndex;
        //Debug.Log($"SetLastButtonClicked: Updated lastRoomNumber to {lastRoomNumber}");
    }

    private void OnBackButtonClicked()
    {
       // Debug.Log($"[RoomCanvasManager] Back button clicked. lastRoomNumber = {lastRoomNumber}");
        
        if (lastRoomNumber < 0 || lastRoomNumber > 5)
        {
            //Debug.LogError($"[RoomCanvasManager] lastRoomNumber non valido: {lastRoomNumber}. Deve essere tra 0 e 5.");
            return;
        }

        if (canvas1 != null && canvas1.gameObject.activeSelf)
        {
            //Debug.Log("[RoomCanvasManager] Nascondo canvas1 e mostro canvas della stanza");
            canvas1.gameObject.SetActive(false);

            if (lastRoomNumber >= 0 && lastRoomNumber <= 2)
            {
                if (canvas2 != null)
                {
                    canvas2.gameObject.SetActive(true);
                   // Debug.Log($"[RoomCanvasManager] Mostro canvas2 per la stanza {lastRoomNumber} (gruppo 0-2)");
                }
            }
            else if (lastRoomNumber >= 3 && lastRoomNumber <= 5)
            {
                if (canvas3 != null)
                {
                    canvas3.gameObject.SetActive(true);
                    //Debug.Log($"[RoomCanvasManager] Mostro canvas3 per la stanza {lastRoomNumber} (gruppo 3-5)");
                }
            }
            return;
        }

        if ((lastRoomNumber >= 0 && lastRoomNumber <= 2 && canvas2 != null && canvas2.gameObject.activeSelf) ||
            (lastRoomNumber >= 3 && lastRoomNumber <= 5 && canvas3 != null && canvas3.gameObject.activeSelf))
        {
            //Debug.Log("Already in room canvas, doing nothing");
            return;
        }

        if (lastRoomNumber >= 0 && lastRoomNumber <= 2)
        {
            if (canvas2 != null) 
            {
                canvas2.gameObject.SetActive(true);
                //if (canvas3 != null) canvas3.gameObject.SetActive(false);
                //Debug.Log($"Back to room {lastRoomNumber}, showing canvas2");
            }
        }
        else if (lastRoomNumber >= 3 && lastRoomNumber <= 5)
        {
            if (canvas3 != null) 
            {
                canvas3.gameObject.SetActive(true);
                if (canvas2 != null) canvas2.gameObject.SetActive(false);
                //Debug.Log($"Back to room {lastRoomNumber}, showing canvas3");
            }
        }
        else
        {
            Debug.Log("No valid room selected, hiding all canvases");
            if (canvas2 != null) canvas2.gameObject.SetActive(false);
            //if (canvas3 != null) canvas3.gameObject.SetActive(false);
        }
    }

    public void ResetToDefault()
    {
        if (canvas1 != null) canvas1.gameObject.SetActive(false);
        if (canvas2 != null) canvas2.gameObject.SetActive(false);
        //if (canvas3 != null) canvas3.gameObject.SetActive(false);
        lastRoomNumber = -1;
        //Debug.Log("Reset to default state");
    }

    public void ShowCanvas1()
    {
        if (canvas1 != null) canvas1.gameObject.SetActive(true);
        if (canvas2 != null) canvas2.gameObject.SetActive(false);
        //if (canvas3 != null) canvas3.gameObject.SetActive(false);
        //Debug.Log("Showing canvas1");
    }
}