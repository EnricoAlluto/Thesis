using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using SojaExiles;

public class MobileUIController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Riferimento allo script PlayerMovement sul giocatore")]
    public PlayerMovement playerMovement;
    
    [Tooltip("Riferimento allo script MouseLook sulla telecamera del giocatore")]
    public MouseLook mouseLook;
    public Joystick movementJoystick;
    public Joystick lookJoystick;
    public Button jumpButton;
    public Button interactButton;
    public GameObject mobileControlsPanel;
    public GameObject[] additionalUIElements;
 
    [Header("Settings")]
    public bool enableTouchControls = true;
    public bool showControlsInEditor = true;
    public bool forceShowOnWebGL = true;
    
    [Header("Joystick Settings")]
    [Tooltip("Sensibilità movimento - aumentata per WebGL")]
    public float joystickSensitivity = 2.5f; // *** MODIFICATO: era 1f ***
    [Tooltip("Sensibilità visuale")]
    public float lookSensitivity = 2f;

    private bool isWebGLPlatform = false;
    private bool controlsInitialized = false;
    private bool isWebGLReady = false;

    private void Awake()
    {
        DetectPlatform();
        Debug.Log($"Platform: {Application.platform}, Is WebGL: {isWebGLPlatform}");
        
        if (isWebGLPlatform)
        {
            StartCoroutine(InitializeWebGLControls());
        }
    }

    private void Start()
    {
        if (!isWebGLPlatform)
        {
            InitializeControls();
        }
    }

    private IEnumerator InitializeWebGLControls()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        Debug.Log("Initializing WebGL controls...");
        
        SetupWebGLEnvironment();
        InitializeControls();
        
        if (forceShowOnWebGL)
        {
            yield return new WaitForSeconds(0.1f);
            ForceShowControlsWebGL();
        }
        
        isWebGLReady = true;
    }

    private void SetupWebGLEnvironment()
    {
        SetupWebGLEventSystem();
        SetupWebGLCanvas();
        
        if (isWebGLPlatform)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void SetupWebGLCanvas()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (var canvas in canvases)
        {
            if (canvas.GetComponent<GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
            
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                Debug.Log($"Changed canvas {canvas.name} to Screen Space Overlay for WebGL");
            }
        }
    }

    private void ForceShowControlsWebGL()
    {
        Debug.Log("Force showing controls for WebGL");
        
        FindAndAssignComponents();
        EnableMobileControls(true);
        ConfigureAllControlsForWebGL();
    }

    private void ConfigureAllControlsForWebGL()
    {
        if (movementJoystick != null)
        {
            ConfigureJoystickForWebGL(movementJoystick, "Movement");
        }
        
        if (lookJoystick != null)
        {
            ConfigureJoystickForWebGL(lookJoystick, "Look");
        }
        
        if (jumpButton != null)
        {
            ConfigureButtonForWebGL(jumpButton, "Jump");
        }
        
        if (interactButton != null)
        {
            ConfigureButtonForWebGL(interactButton, "Interact");
        }
    }

    private void ConfigureJoystickForWebGL(Joystick joystick, string name)
    {
        if (joystick == null) return;
        
        joystick.gameObject.SetActive(true);
        
        if (joystick.GetComponent<Image>() == null)
        {
            joystick.gameObject.AddComponent<Image>();
        }
        
        CanvasGroup canvasGroup = joystick.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = joystick.gameObject.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = 0.7f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        RectTransform rectTransform = joystick.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
        }
        
        Debug.Log($"WebGL: {name} joystick configured");
    }

    private void ConfigureButtonForWebGL(Button button, string name)
    {
        if (button == null) return;
        
        button.gameObject.SetActive(true);
        button.interactable = true;
        
        CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = 0.8f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        if (button.GetComponent<Image>() == null)
        {
            button.gameObject.AddComponent<Image>();
        }
        
        Debug.Log($"WebGL: {name} button configured");
    }

    private void InitializeControls()
    {
        Debug.Log("Initializing mobile controls...");
        
        FindAndAssignComponents();
        
        bool shouldShow = ShouldShowMobileControls();
        Debug.Log($"Should show controls: {shouldShow}");
        
        EnableMobileControls(shouldShow);
        SetupButtonListeners();
        ValidateReferences();
        
        controlsInitialized = true;
        Debug.Log($"MobileUIController initialized. Controls visible: {shouldShow}");
    }

    private void DetectPlatform()
    {
        isWebGLPlatform = Application.platform == RuntimePlatform.WebGLPlayer;
        
        if (!isWebGLPlatform)
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            isWebGLPlatform = true;
            #endif
        }
    }

    private bool ShouldShowMobileControls()
    {
        if (isWebGLPlatform)
        {
            return forceShowOnWebGL;
        }
        else if (Application.isEditor)
        {
            return showControlsInEditor;
        }
        else
        {
            return Application.platform == RuntimePlatform.Android || 
                   Application.platform == RuntimePlatform.IPhonePlayer;
        }
    }

    private void OnEnable()
    {
        if (!controlsInitialized && !isWebGLPlatform)
        {
            InitializeControls();
        }
        else if (isWebGLPlatform && isWebGLReady)
        {
            if (forceShowOnWebGL)
            {
                EnableMobileControls(true);
            }
        }
    }

    private void SetupButtonListeners()
    {
        if (jumpButton != null)
        {
            jumpButton.onClick.RemoveAllListeners();
            jumpButton.onClick.AddListener(OnJumpButtonPressed);
            Debug.Log("Jump button listener configured");
        }

        if (interactButton != null)
        {
            interactButton.onClick.RemoveAllListeners();
            interactButton.onClick.AddListener(OnInteractButtonPressed);
            Debug.Log("Interact button listener configured");
        }
    }

    private void FindAndAssignComponents()
    {
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
            Debug.Log(playerMovement != null ? "PlayerMovement found" : "PlayerMovement NOT found");
        }
            
        if (mouseLook == null)
        {
            mouseLook = FindObjectOfType<MouseLook>();
            Debug.Log(mouseLook != null ? "MouseLook found" : "MouseLook NOT found");
        }
            
        FindJoysticks();
        FindMobileControlsPanel();
        FindButtons();
    }

    private void FindButtons()
    {
        if (jumpButton == null)
        {
            string[] jumpButtonNames = { "JumpButton", "Jump", "ButtonJump", "jumpBtn" };
            foreach (string name in jumpButtonNames)
            {
                GameObject buttonObj = GameObject.Find(name);
                if (buttonObj != null)
                {
                    jumpButton = buttonObj.GetComponent<Button>();
                    if (jumpButton != null)
                    {
                        Debug.Log($"Jump button found: {name}");
                        break;
                    }
                }
            }
        }
        
        if (interactButton == null)
        {
            string[] interactButtonNames = { "InteractButton", "Interact", "ButtonInteract", "interactBtn" };
            foreach (string name in interactButtonNames)
            {
                GameObject buttonObj = GameObject.Find(name);
                if (buttonObj != null)
                {
                    interactButton = buttonObj.GetComponent<Button>();
                    if (interactButton != null)
                    {
                        Debug.Log($"Interact button found: {name}");
                        break;
                    }
                }
            }
        }
    }

    private void FindJoysticks()
    {
        if (movementJoystick != null && lookJoystick != null)
            return;

        Joystick[] joysticks = FindObjectsOfType<Joystick>();
        Debug.Log($"Found {joysticks.Length} joysticks in scene");
        
        foreach (var joystick in joysticks)
        {
            Debug.Log($"Joystick: {joystick.name} at {joystick.transform.position}");
            
            if (movementJoystick == null)
            {
                if (joystick.name.ToLower().Contains("move") || 
                    joystick.name.ToLower().Contains("left") ||
                    IsLeftSideJoystick(joystick))
                {
                    movementJoystick = joystick;
                    Debug.Log($"Movement joystick assigned: {joystick.name}");
                    continue;
                }
            }
            
            if (lookJoystick == null)
            {
                if (joystick.name.ToLower().Contains("look") || 
                    joystick.name.ToLower().Contains("right") ||
                    IsRightSideJoystick(joystick))
                {
                    lookJoystick = joystick;
                    Debug.Log($"Look joystick assigned: {joystick.name}");
                }
            }
        }
        
        if (joysticks.Length >= 2)
        {
            if (movementJoystick == null) movementJoystick = joysticks[0];
            if (lookJoystick == null) lookJoystick = joysticks[1];
        }
    }

    private bool IsLeftSideJoystick(Joystick joystick)
    {
        RectTransform rectTransform = joystick.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            float centerX = (corners[0].x + corners[2].x) / 2f;
            return centerX < Screen.width * 0.5f;
        }
        
        return joystick.transform.position.x < Screen.width * 0.5f;
    }

    private bool IsRightSideJoystick(Joystick joystick)
    {
        RectTransform rectTransform = joystick.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            float centerX = (corners[0].x + corners[2].x) / 2f;
            return centerX >= Screen.width * 0.5f;
        }
        
        return joystick.transform.position.x >= Screen.width * 0.5f;
    }

    private void FindMobileControlsPanel()
    {
        if (mobileControlsPanel != null)
            return;

        string[] panelNames = { "MobileControlsPanel", "MobileControls", "TouchControls", "UI_Mobile", "WebGLControls", "Canvas" };
        
        foreach (string name in panelNames)
        {
            GameObject panel = GameObject.Find(name);
            if (panel != null)
            {
                mobileControlsPanel = panel;
                Debug.Log($"Mobile controls panel found: {name}");
                break;
            }
        }
        
        if (mobileControlsPanel == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                mobileControlsPanel = canvas.gameObject;
                Debug.Log($"Using canvas as mobile controls panel: {canvas.name}");
            }
        }
    }

    // *** SEZIONE MODIFICATA: Configurazione separata dei joystick ***
    private void ValidateReferences()
    {        
        // IMPORTANTE: Configura solo il joystick di MOVIMENTO
        if (movementJoystick != null && playerMovement != null)
        {
            playerMovement.movementJoystick = movementJoystick;
            // *** MODIFICATO: Sensibilità aumentata per WebGL ***
            playerMovement.joystickSensitivity = isWebGLPlatform ? joystickSensitivity * 1.5f : joystickSensitivity;
            Debug.Log($"Movement joystick linked to PlayerMovement with sensitivity: {playerMovement.joystickSensitivity}");
        }
        
        // IMPORTANTE: Configura solo il joystick di VISUALE
        if (lookJoystick != null && mouseLook != null)
        {
            mouseLook.lookJoystick = lookJoystick;
            mouseLook.lookSensitivity = lookSensitivity;
            Debug.Log("Look joystick linked to MouseLook");
        }
        
        if (isWebGLPlatform)
        {
            SetupWebGLEventSystem();
        }
    }

    private void SetupWebGLEventSystem()
    {
        if (EventSystem.current == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
            Debug.Log("Created EventSystem for WebGL");
        }
        else
        {
            StandaloneInputModule inputModule = EventSystem.current.GetComponent<StandaloneInputModule>();
            if (inputModule == null)
            {
                inputModule = EventSystem.current.gameObject.AddComponent<StandaloneInputModule>();
            }
        }
    }

    public void EnableMobileControls(bool enable)
    {
        Debug.Log($"EnableMobileControls: {enable}");
        
        if (isWebGLPlatform && forceShowOnWebGL)
        {
            enable = true;
            Debug.Log("Forced enable on WebGL platform");
        }
        
        if (mobileControlsPanel != null)
        {
            mobileControlsPanel.SetActive(enable);
            Debug.Log($"Mobile controls panel: {enable}");
        }

        SetJoystickActive(movementJoystick, enable, "Movement");
        SetJoystickActive(lookJoystick, enable, "Look");
        SetButtonActive(jumpButton, enable, "Jump");
        SetButtonActive(interactButton, enable, "Interact");
            
        foreach (var uiElement in additionalUIElements)
        {
            if (uiElement != null)
            {
                uiElement.SetActive(enable);
            }
        }
        
        HandleCursorVisibility(enable);
    }

    private void SetJoystickActive(Joystick joystick, bool active, string name)
    {
        if (joystick != null)
        {
            joystick.gameObject.SetActive(active);
            
            if (active && isWebGLPlatform)
            {
                ConfigureJoystickForWebGL(joystick, name);
            }
            
            Debug.Log($"{name} joystick: {active}");
        }
        else
        {
            Debug.LogWarning($"{name} joystick is null");
        }
    }

    private void SetButtonActive(Button button, bool active, string name)
    {
        if (button != null)
        {
            button.gameObject.SetActive(active);
            
            if (active && isWebGLPlatform)
            {
                ConfigureButtonForWebGL(button, name);
            }
            
            Debug.Log($"{name} button: {active}");
        }
        else
        {
            Debug.LogWarning($"{name} button is null");
        }
    }

    private void HandleCursorVisibility(bool mobileControlsEnabled)
    {
        if (isWebGLPlatform)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            return;
        }
            
        if (Application.isEditor)
        {
            Cursor.visible = mobileControlsEnabled;
            Cursor.lockState = mobileControlsEnabled ? CursorLockMode.None : CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = !mobileControlsEnabled;
            Cursor.lockState = mobileControlsEnabled ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    private void OnJumpButtonPressed()
    {
        Debug.Log("Jump button pressed");
        if (playerMovement != null)
        {
            playerMovement.Jump();
        }
        else
        {
            Debug.LogWarning("PlayerMovement reference is missing");
        }
    }

    private void OnInteractButtonPressed()
    {
        Debug.Log("Interact button pressed");
        if (TouchManager.Instance != null)
        {
            TouchManager.Instance.InteractAtScreenCenter();
        }
        else
        {
            Debug.LogWarning("TouchManager instance not found");
        }
    }

    public void ToggleControls(bool show)
    {
        EnableMobileControls(show);
    }

    public void ForceEnableControls()
    {
        Debug.Log("Force enabling controls");
        EnableMobileControls(true);
        
        if (isWebGLPlatform)
        {
            ConfigureAllControlsForWebGL();
        }
    }

    public void SetControlsLocked(bool locked)
    {
        if (playerMovement != null)
        {
            playerMovement.LockMovement(locked);
        }
            
        if (mouseLook != null)
        {
            mouseLook.SetLookingEnabled(!locked);
        }
            
        if (TouchManager.Instance != null)
        {
            TouchManager.Instance.SetTouchInputEnabled(!locked);
        }
            
        UpdateControlsVisualFeedback(locked);
    }

    private void UpdateControlsVisualFeedback(bool locked)
    {
        if (mobileControlsPanel != null)
        {
            CanvasGroup canvasGroup = mobileControlsPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = mobileControlsPanel.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = locked ? 0.3f : 1f;
            canvasGroup.interactable = !locked;
        }
    }

    // *** MODIFICATO: Applica moltiplicatore WebGL ***
    public void SetJoystickSensitivity(float sensitivity)
    {
        joystickSensitivity = sensitivity;
        if (playerMovement != null)
        {
            // Applica moltiplicatore per WebGL
            playerMovement.joystickSensitivity = isWebGLPlatform ? sensitivity * 1.5f : sensitivity;
        }
    }
 
    public void SetLookSensitivity(float sensitivity)
    {
        lookSensitivity = sensitivity;
        if (mouseLook != null)
        {
            mouseLook.lookSensitivity = sensitivity;
        }
    }

    public bool IsRunningOnWebGL()
    {
        return isWebGLPlatform;
    }

    [ContextMenu("Debug Current State")]
    public void DebugCurrentState()
    {
        Debug.Log("=== MOBILE UI CONTROLLER DEBUG (WebGL Optimized) ===");
        Debug.Log($"Platform: {Application.platform}");
        Debug.Log($"Is WebGL: {isWebGLPlatform}");
        Debug.Log($"WebGL Ready: {isWebGLReady}");
        Debug.Log($"Should Show Controls: {ShouldShowMobileControls()}");
        Debug.Log($"Force Show on WebGL: {forceShowOnWebGL}");
        Debug.Log($"Joystick Sensitivity: {joystickSensitivity}");
        Debug.Log($"Movement Joystick: {(movementJoystick != null ? movementJoystick.name + " (Active: " + movementJoystick.gameObject.activeSelf + ")" : "NULL")}");
        Debug.Log($"Look Joystick: {(lookJoystick != null ? lookJoystick.name + " (Active: " + lookJoystick.gameObject.activeSelf + ")" : "NULL")}");
        Debug.Log($"Jump Button: {(jumpButton != null ? jumpButton.name + " (Active: " + jumpButton.gameObject.activeSelf + ")" : "NULL")}");
        Debug.Log($"Interact Button: {(interactButton != null ? interactButton.name + " (Active: " + interactButton.gameObject.activeSelf + ")" : "NULL")}");
        Debug.Log($"Mobile Panel: {(mobileControlsPanel != null ? mobileControlsPanel.name + " (Active: " + mobileControlsPanel.activeSelf + ")" : "NULL")}");
        Debug.Log($"EventSystem Present: {EventSystem.current != null}");
        Debug.Log($"Cursor Visible: {Cursor.visible}");
        Debug.Log($"Cursor Lock State: {Cursor.lockState}");
        
        if (playerMovement != null)
        {
            Debug.Log($"PlayerMovement Joystick Sensitivity: {playerMovement.joystickSensitivity}");
        }
        
        Debug.Log("==========================================");
    }
}