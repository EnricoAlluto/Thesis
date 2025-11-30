using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    public bool forceShowOnMobile = true;
    
    [Header("Joystick Settings")]
    public float joystickSensitivity = 1f;
    public float lookSensitivity = 2f;

    private bool isMobilePlatform = false;
    private bool controlsInitialized = false;

    private void Awake()
    {
        // Detect platform
        DetectPlatform();
        Debug.Log($"Platform: {Application.platform}, Is Mobile: {isMobilePlatform}");
    }

    private void Start()
    {
        // Initialize controls
        InitializeControls();
    }

    private void InitializeControls()
    {
        Debug.Log("Initializing mobile controls...");
        
        // Find and assign components first
        FindAndAssignComponents();
        
        // Always show controls on mobile or when forced in editor
        bool shouldShow = ShouldShowMobileControls();
        Debug.Log($"Should show controls: {shouldShow}");
        
        // Enable controls
        EnableMobileControls(shouldShow);

        // Set up button listeners
        SetupButtonListeners();
        
        // Validate and configure components
        ValidateReferences();
        
        controlsInitialized = true;
        Debug.Log($"MobileUIController initialized. Controls visible: {shouldShow}");
    }

    private void DetectPlatform()
    {
        isMobilePlatform = Application.platform == RuntimePlatform.Android || 
                          Application.platform == RuntimePlatform.IPhonePlayer;
    }

    private bool ShouldShowMobileControls()
    {
        // Logica semplificata: mostra sempre su mobile, rispetta le impostazioni in editor
        if (isMobilePlatform)
        {
            return true; // Sempre vero su mobile
        }
        else if (Application.isEditor)
        {
            return showControlsInEditor;
        }
        else
        {
            return false; // PC desktop
        }
    }

    private void OnEnable()
    {
        if (!controlsInitialized)
        {
            InitializeControls();
        }
    }

    private void SetupButtonListeners()
    {
        // Set up jump button
        if (jumpButton != null)
        {
            jumpButton.onClick.RemoveAllListeners();
            jumpButton.onClick.AddListener(OnJumpButtonPressed);
            Debug.Log("Jump button listener configured");
        }

        // Set up interact button
        if (interactButton != null)
        {
            interactButton.onClick.RemoveAllListeners();
            interactButton.onClick.AddListener(OnInteractButtonPressed);
            Debug.Log("Interact button listener configured");
        }
    }

    private void FindAndAssignComponents()
    {
        // Find PlayerMovement if not assigned
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
            Debug.Log(playerMovement != null ? "PlayerMovement found" : "PlayerMovement NOT found");
        }
            
        // Find MouseLook if not assigned
        if (mouseLook == null)
        {
            mouseLook = FindObjectOfType<MouseLook>();
            Debug.Log(mouseLook != null ? "MouseLook found" : "MouseLook NOT found");
        }
            
        // Find joysticks if not assigned
        FindJoysticks();
        
        // Find mobile controls panel if not assigned
        FindMobileControlsPanel();
    }

    private void FindJoysticks()
    {
        if (movementJoystick != null && lookJoystick != null)
            return; // Already assigned

        Joystick[] joysticks = FindObjectsOfType<Joystick>();
        Debug.Log($"Found {joysticks.Length} joysticks in scene");
        
        foreach (var joystick in joysticks)
        {
            Debug.Log($"Joystick: {joystick.name} at {joystick.transform.position}");
            
            // Assign based on name or position
            if (movementJoystick == null)
            {
                if (joystick.name.ToLower().Contains("move") || 
                    joystick.name.ToLower().Contains("left") ||
                    joystick.transform.position.x < Screen.width * 0.5f)
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
                    joystick.transform.position.x >= Screen.width * 0.5f)
                {
                    lookJoystick = joystick;
                    Debug.Log($"Look joystick assigned: {joystick.name}");
                }
            }
        }
        
        // Fallback assignment if we have at least 2 joysticks
        if (joysticks.Length >= 2)
        {
            if (movementJoystick == null) movementJoystick = joysticks[0];
            if (lookJoystick == null) lookJoystick = joysticks[1];
        }
    }

    private void FindMobileControlsPanel()
    {
        if (mobileControlsPanel != null)
            return;

        // Try different common names
        string[] panelNames = { "MobileControlsPanel", "MobileControls", "TouchControls", "UI_Mobile" };
        
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
            Debug.LogWarning("Mobile controls panel not found. Make sure to assign it manually.");
        }
    }

    private void ValidateReferences()
    {        
        // Configure movement joystick
        if (movementJoystick != null)
        {
            if (playerMovement != null)
            {
                playerMovement.movementJoystick = movementJoystick;
                playerMovement.joystickSensitivity = joystickSensitivity;
                Debug.Log("Movement joystick linked to PlayerMovement");
            }
            ConfigureJoystickUI(movementJoystick);
        }
        
        // Configure look joystick
        if (lookJoystick != null)
        {
            if (mouseLook != null)
            {
                mouseLook.lookJoystick = lookJoystick;
                mouseLook.lookSensitivity = lookSensitivity;
                Debug.Log("Look joystick linked to MouseLook");
            }
            ConfigureJoystickUI(lookJoystick);
        }
        
        // Configure buttons
        if (interactButton != null) ConfigureButtonUI(interactButton);
        if (jumpButton != null) ConfigureButtonUI(jumpButton);
    }

    private void ConfigureJoystickUI(Joystick joystick)
    {
        if (joystick == null) return;
        
        // Ensure joystick is interactable
        joystick.gameObject.SetActive(true);
        
        var canvasGroup = joystick.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = joystick.gameObject.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = 0.8f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        Debug.Log($"Joystick {joystick.name} configured");
    }

    private void ConfigureButtonUI(Button button)
    {
        if (button == null) return;
        
        button.interactable = true;
        button.gameObject.SetActive(true);
        
        var canvasGroup = button.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = 0.9f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        Debug.Log($"Button {button.name} configured");
    }

    public void EnableMobileControls(bool enable)
    {
        Debug.Log($"EnableMobileControls: {enable}");
        
        // Force enable on mobile platform
        if (isMobilePlatform)
        {
            enable = true;
            Debug.Log("Forced enable on mobile platform");
        }
        
        // Toggle the main controls panel
        if (mobileControlsPanel != null)
        {
            mobileControlsPanel.SetActive(enable);
            Debug.Log($"Mobile controls panel: {enable}");
        }

        // Force enable individual UI elements
        SetJoystickActive(movementJoystick, enable, "Movement");
        SetJoystickActive(lookJoystick, enable, "Look");
        SetButtonActive(jumpButton, enable, "Jump");
        SetButtonActive(interactButton, enable, "Interact");
            
        // Toggle additional UI elements
        foreach (var uiElement in additionalUIElements)
        {
            if (uiElement != null)
            {
                uiElement.SetActive(enable);
            }
        }
        
        // Handle cursor only for non-mobile platforms
        HandleCursorVisibility(enable);
    }

    private void SetJoystickActive(Joystick joystick, bool active, string name)
    {
        if (joystick != null)
        {
            joystick.gameObject.SetActive(active);
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
            Debug.Log($"{name} button: {active}");
        }
        else
        {
            Debug.LogWarning($"{name} button is null");
        }
    }

    private void HandleCursorVisibility(bool mobileControlsEnabled)
    {
        // Don't manage cursor on mobile platforms
        if (isMobilePlatform)
            return;
            
        if (Application.isEditor)
        {
            // In editor, show cursor when mobile controls are enabled for testing
            Cursor.visible = mobileControlsEnabled;
            Cursor.lockState = mobileControlsEnabled ? CursorLockMode.None : CursorLockMode.Locked;
        }
        else
        {
            // On PC, opposite logic
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

    // Public methods for external control
    public void ToggleControls(bool show)
    {
        EnableMobileControls(show);
    }

    public void ForceEnableControls()
    {
        Debug.Log("Force enabling controls");
        EnableMobileControls(true);
    }

    public void SetControlsLocked(bool locked)
    {
        // Lock player movement
        if (playerMovement != null)
        {
            playerMovement.LockMovement(locked);
        }
            
        // Lock mouse look
        if (mouseLook != null)
        {
            mouseLook.SetLookingEnabled(!locked);
        }
            
        // Update touch input
        if (TouchManager.Instance != null)
        {
            TouchManager.Instance.SetTouchInputEnabled(!locked);
        }
            
        // Visual feedback
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

    public void SetJoystickSensitivity(float sensitivity)
    {
        joystickSensitivity = sensitivity;
        if (playerMovement != null)
        {
            playerMovement.joystickSensitivity = sensitivity;
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

    // Debug method to check current state
    [ContextMenu("Debug Current State")]
    public void DebugCurrentState()
    {
        Debug.Log("=== MOBILE UI CONTROLLER DEBUG ===");
        Debug.Log($"Platform: {Application.platform}");
        Debug.Log($"Is Mobile: {isMobilePlatform}");
        Debug.Log($"Should Show Controls: {ShouldShowMobileControls()}");
        Debug.Log($"Movement Joystick: {(movementJoystick != null ? movementJoystick.name : "NULL")}");
        Debug.Log($"Look Joystick: {(lookJoystick != null ? lookJoystick.name : "NULL")}");
        Debug.Log($"Jump Button: {(jumpButton != null ? jumpButton.name : "NULL")}");
        Debug.Log($"Interact Button: {(interactButton != null ? interactButton.name : "NULL")}");
        Debug.Log($"Mobile Panel: {(mobileControlsPanel != null ? mobileControlsPanel.name : "NULL")}");
        Debug.Log($"Controls Panel Active: {(mobileControlsPanel != null ? mobileControlsPanel.activeSelf : false)}");
        Debug.Log("================================");
    }
}