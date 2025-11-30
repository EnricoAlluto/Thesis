//The code shows the beginning of the AvatarFinishManager class, which manages 3D avatars in Unity. 
//It includes essential imports for 3D model handling (GLTF), file operations, and Android compatibility.
//The class defines constants for saving avatars and uses events to notify when avatars are loaded.
//It maintains a list of loaded avatars and tracks the currently selected avatar through an index.
//The class is designed to work across different platforms, with specific handling for Android devices.
 
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using GLTFast.Export; 
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine.Events;
using UnityEditor;
#if SICCITY_GLTF
using GLTFast;
using GLTFast.Loading;
using GLTFast.Logging;
using GLTFast.Materials;
#endif
 
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class AvatarFinishManager : MonoBehaviour
{
    private const string SAVE_FOLDER_NAME = "SavedAvatars";
    private const string GLB_FILE_NAME = "avatar.glb";

    public UnityEvent<List<AvatarSaveData>> OnAvatarsLoaded = new UnityEvent<List<AvatarSaveData>>();
    private List<AvatarSaveData> loadedAvatars = new List<AvatarSaveData>();
    public List<AvatarSaveData> LoadedAvatars => loadedAvatars;
    
    public int GetCurrentAvatarIndex()
    {
        if (avatarCreato != null && currentAvatarIndex >= 0)
        {
            return currentAvatarIndex;
        }
        return -1;
    }

    [Header("Settings Avatar")]
    public Vector3 rotazioneAvatar = Vector3.zero;
    public Vector3 scalaAvatar = Vector3.one;
    public bool mantieniFisicaAvatar = false;

    [Header("Settings")]
    public bool caricaAutomaticamenteAllAvvio = true;
    public int maxAvatarsDaCaricare = 10;

    [Header("References")]
    public GameObject avatarCreato;
    public GameObject[] containerAvatars = new GameObject[10];
    public GameObject[] avatarObjects = new GameObject[10];
    public string tagAvatarCreato = "FinishedAvatar";
    public GameObject stanzaCreazione;
    public GameObject stanzaFinale;
    public AudioSource audioSource;
    public AudioClip suonoFinish;
    public Button pulsanteFinish;

    [Header("Debug")]
    public bool enableDebugLogs = true;
    public bool showDebugInfo = false;

    private int currentAvatarIndex = 0;
    
    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (SystemInfo.operatingSystem.Contains("Android 13") || 
            SystemInfo.operatingSystem.Contains("Android 14") || 
            SystemInfo.operatingSystem.Contains("Android 15"))
        {
            string[] permissions = {
                "android.permission.READ_MEDIA_IMAGES",
                "android.permission.READ_EXTERNAL_STORAGE",
                "android.permission.WRITE_EXTERNAL_STORAGE"
            };
            
            foreach (string permission in permissions)
            {
                if (!Permission.HasUserAuthorizedPermission(permission))
                {
                    Permission.RequestUserPermission(permission);
                }
            }
        }
#endif

        CaricaTuttiGliAvatar();
        if (pulsanteFinish != null)
        {
            pulsanteFinish.onClick.AddListener(() => StartCoroutine(FinishAvatar()));
        }
        //Debug.Log($"[AvatarFinishManager] Percorso di salvataggio: {Application.persistentDataPath}");
        string saveDir = GetSaveDirectory();
        if (string.IsNullOrEmpty(saveDir))
        {
            //Debug.LogError("Impossibile accedere alla directory di salvataggio");
            return;
        }
        if (caricaAutomaticamenteAllAvvio)
        {
            StartCoroutine(CaricaTuttiGliAvatarNeiContainer());
        }
    }

    //The FinishAvatar function handles avatar saving in four steps:
    //it initializes by checking for an avatar, saves it as a GLB file, verifies the file's integrity, 
    //and cleans up by re-enabling the finish button and handling any errors.
    private IEnumerator FinishAvatar()
    {
       // Debug.Log("=== INIZIO PROCESSO FINISH AVATAR ===");
        float startTime = Time.realtimeSinceStartup;
        string errorMessage = "";
        bool success = false;
    
        if (pulsanteFinish != null)
            pulsanteFinish.interactable = false;

        if (avatarCreato == null)
        {
            //Debug.Log("[FinishAvatar] Avatar non trovato, cerco nella scena...");
            TrovaAvatarCreato();
            if (avatarCreato == null)
            {
                errorMessage = "Nessun avatar trovato da salvare!";
                //Debug.LogError($"[FinishAvatar] ERRORE: {errorMessage}");
                goto Cleanup;
            }
        }

        //Debug.Log($"[FinishAvatar] Avatar da salvare: {avatarCreato.name}, attivo: {avatarCreato.activeInHierarchy}");
        //Debug.Log("[FinishAvatar] Inizio salvataggio GLB...");
        bool salvataggiokEffettuato = false;
        string percorsoGLBSalvato = "";
        string erroreSalvataggio = "";

        //Debug.Log("[FinishAvatar] Inizio salvataggio avatar...");
        
        bool saveCompleted = false;
        
        var saveCoroutine = SalvaAvatarComeGLBCoroutine((success, glbPath) =>
        {
            salvataggiokEffettuato = success;
            if (success) {
                percorsoGLBSalvato = glbPath;
                //Debug.Log($"[FinishAvatar] Callback salvataggio - Successo! Path: {glbPath}");
            } else {
                erroreSalvataggio = glbPath ?? "Errore sconosciuto durante il salvataggio";
                //Debug.LogError($"[FinishAvatar] Callback salvataggio - Fallito: {erroreSalvataggio}");
            }
            saveCompleted = true;
        });
        StartCoroutine(saveCoroutine);
        float saveStartTime = Time.time;
        float saveTimeout = 30f;
        
        while (!saveCompleted && (Time.time - saveStartTime) < saveTimeout)
        {
            yield return null;
        }
        
        if (!saveCompleted)
        {
            errorMessage = "Timeout durante il salvataggio dell'avatar";
            //Debug.LogError($"[FinishAvatar] {errorMessage}");
            goto Cleanup;
        }
        
        if (!salvataggiokEffettuato)
        {
            errorMessage = $"Salvataggio GLB fallito: {erroreSalvataggio}";
            //Debug.LogError($"[FinishAvatar] {errorMessage}");
            goto Cleanup;
        }

        if (!salvataggiokEffettuato || string.IsNullOrEmpty(percorsoGLBSalvato))
        {
            errorMessage = $"Salvataggio GLB fallito: {erroreSalvataggio}";
           // Debug.LogError($"[FinishAvatar] {errorMessage}");
            goto Cleanup;
        }

        //Debug.Log($"[FinishAvatar] Avatar salvato con successo in: {percorsoGLBSalvato}");
        //Debug.Log("[FinishAvatar] Verifica integrità file...");
        bool fileReady = false;
        string fileReadyPath = "";
        
        var waitForFileCoroutine = WaitForFileToBeReady(percorsoGLBSalvato, (readyPath) => {
            fileReady = !string.IsNullOrEmpty(readyPath);
            fileReadyPath = readyPath;
            //Debug.Log($"[FinishAvatar] File ready callback - Ready: {fileReady}, Path: {fileReadyPath}");
        }, 30f); 
        
        while (waitForFileCoroutine.MoveNext())
        {
            yield return waitForFileCoroutine.Current;
        }

        if (!fileReady || string.IsNullOrEmpty(fileReadyPath))
        {
            errorMessage = $"Timeout: il file GLB non è pronto per la lettura o è vuoto. Path: {percorsoGLBSalvato}";
            //Debug.LogError($"[FinishAvatar] {errorMessage}");
            goto Cleanup;
        }
        FileInfo fileInfo = new FileInfo(fileReadyPath);
        if (!fileInfo.Exists || fileInfo.Length == 0)
        {
            errorMessage = $"Il file GLB è vuoto o inaccessibile. Dimensione: {fileInfo.Length} bytes";
            //Debug.LogError($"[FinishAvatar] {errorMessage}");
            goto Cleanup;
        }
        
        //Debug.Log($"[FinishAvatar] File GLB pronto: {fileReadyPath} ({fileInfo.Length} bytes)");

        int containerLibero = TrovaPrimoContainerLibero();
        if (containerLibero == -1)
        {
            errorMessage = "Tutti i container sono pieni. Elimina un avatar esistente per crearne uno nuovo.";
            //Debug.LogError($"[FinishAvatar] {errorMessage}");
            goto Cleanup;
        }

        //Debug.Log($"[FinishAvatar] Caricamento avatar nel container {containerLibero}...");
        GameObject avatarCaricato = null;
        string erroreCaricamento = "";
        yield return StartCoroutine(CaricaEPosizionaAvatarDaGLB(fileReadyPath, containerLibero, (loadedAvatar) => {
            avatarCaricato = loadedAvatar;
            if (avatarCaricato != null) {
                //Debug.Log($"[FinishAvatar] Avatar caricato con successo: {avatarCaricato.name} nel container {containerLibero}");
            } else {
                erroreCaricamento = "Il caricamento ha restituito null";
                //Debug.LogError($"[FinishAvatar] Errore nel caricamento nel container {containerLibero}");
            }
        }));

        if (avatarCaricato == null)
        {
            errorMessage = $"Caricamento dell'avatar salvato fallito: {erroreCaricamento}";
            //Debug.LogError($"[FinishAvatar] {errorMessage}");
            goto Cleanup;
        }

        //Debug.Log($"[FinishAvatar] Avatar caricato correttamente: {avatarCaricato.name}");
        //Debug.Log("[FinishAvatar] Cambio stanza...");
        CambiaStanza();
        //Debug.Log("[FinishAvatar] Riproduzione suono...");
        RiproduciSuono();

        float elapsedTime = Time.realtimeSinceStartup - startTime;
       // Debug.Log($"[FinishAvatar] Processo completato con successo in {elapsedTime:F2} secondi!");
        success = true;

        Cleanup:
        if (pulsanteFinish != null)
        {
            pulsanteFinish.interactable = true;
            //Debug.Log("[FinishAvatar] Pulsante riabilitato");
        }
        
        float totalTime = Time.realtimeSinceStartup - startTime;
        //Debug.Log($"[FinishAvatar] Tempo totale impiegato: {totalTime:F2} secondi");
        
        if (!success && !string.IsNullOrEmpty(errorMessage))
        {
            //Debug.LogError($"[FinishAvatar] ERRORE CRITICO: {errorMessage}");
        }
        if (pulsanteFinish != null)
            pulsanteFinish.interactable = true;

        //Debug.Log("=== FINE PROCESSO FINISH AVATAR ===");
    }

    //The CaricaTuttiGliAvatarNeiContainer function loads saved avatars into scene containers, 
    //sorting them by creation date (oldest first). It handles up to the container limit or 
    //maxAvatarsDaCaricare, verifies GLB files, and loads avatars asynchronously. 
    //The function updates currentAvatarIndex to track the next available container, 
    //using a circular buffer approach for efficient container management.

    public IEnumerator CaricaTuttiGliAvatarNeiContainer()
    {
        //Debug.Log("[CaricaTuttiGliAvatarNeiContainer] Inizio caricamento di tutti gli avatar...");
        CaricaTuttiGliAvatar();
        yield return null;
        
        if (loadedAvatars.Count == 0)
        {
            //Debug.Log("Nessun avatar salvato trovato");
            yield break;
        }
        loadedAvatars.Sort((a, b) => 
        {
            if (DateTime.TryParse(a.creationDate, out DateTime dateA) && DateTime.TryParse(b.creationDate, out DateTime dateB))
            {
                return dateA.CompareTo(dateB);
            }
            return 0;
        });
        int avatarsDaCaricare = Mathf.Min(loadedAvatars.Count, containerAvatars.Length, maxAvatarsDaCaricare);
        //Debug.Log($"Caricamento di {avatarsDaCaricare} avatar in {containerAvatars.Length} container");
        //Debug.Log($"Avatar più vecchio: {loadedAvatars[0].avatarName} ({loadedAvatars[0].creationDate}) -> Container 0");
        currentAvatarIndex = 0;
        
        for (int containerIndex = 0; containerIndex < avatarsDaCaricare; containerIndex++)
        {
            AvatarSaveData avatarData = loadedAvatars[containerIndex];
            string glbPath = avatarData.glbPath;
            
            //Debug.Log($"Caricamento nel Container {containerIndex}: {avatarData.avatarName} creato il {avatarData.creationDate}");
            
            if (string.IsNullOrEmpty(glbPath) || !File.Exists(glbPath))
            {
                //Debug.LogWarning($"File GLB non trovato per l'avatar {avatarData.avatarName}: {glbPath}");
                continue;
            }
            FileInfo fileInfo = new FileInfo(glbPath);
            if (fileInfo.Length == 0)
            {
                //Debug.LogWarning($"File GLB vuoto per l'avatar {avatarData.avatarName}: {glbPath}");
                continue;
            }
            
            //Debug.Log($"Caricamento avatar {containerIndex + 1}/{avatarsDaCaricare}: {avatarData.avatarName} nel Container {containerIndex}");
            
            currentAvatarIndex = containerIndex;
            bool caricamentoCompletato = false;
            GameObject avatarCaricato = null;
            
            yield return StartCoroutine(CaricaEPosizionaAvatarDaGLB(glbPath, containerIndex, (avatar) => {
                avatarCaricato = avatar;
                caricamentoCompletato = true;
            }));
            yield return new WaitUntil(() => caricamentoCompletato);
            
            if (avatarCaricato != null)
            {
                //Debug.Log($"✓ Avatar {avatarData.avatarName} caricato con successo nel Container {containerIndex}");
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
               // Debug.LogError($"✗ Impossibile caricare l'avatar {avatarData.avatarName} nel Container {containerIndex}");
            }
        }
        currentAvatarIndex = avatarsDaCaricare % containerAvatars.Length;
        
        //Debug.Log($"[CaricaTuttiGliAvatarNeiContainer] Caricamento completato.");
        //Debug.Log($"Mapping finale: Container 0 = Avatar più vecchio, Container 1 = Secondo più vecchio, ecc.");
        //Debug.Log($"Prossimo indice per nuovi avatar: {currentAvatarIndex}");
    }

    //This function loads a 3D avatar from a GLB file and positions it in a specified container. 
    //It first validates the GLB file path and container index, then loads the avatar 
    //asynchronously using the GLTF library. Once loaded, it positions, scales, and orients 
    //the avatar in the target container, updates the avatarObjects array, and activates the container.
    //The function uses a callback to signal completion and handles errors gracefully.

    private IEnumerator CaricaEPosizionaAvatarDaGLB(string glbPath, int containerIndex, System.Action<GameObject> onComplete)
    {
        //Debug.Log($"[CaricaEPosizionaAvatarDaGLB] Caricamento da: {glbPath} nel container {containerIndex}");

        if (string.IsNullOrEmpty(glbPath) || !File.Exists(glbPath))
        {
            //Debug.LogError($"File GLB non trovato: {glbPath}");
            onComplete?.Invoke(null);
            yield break;
        }
        if (containerIndex < 0 || containerIndex >= containerAvatars.Length)
        {
            //Debug.LogError($"Indice container non valido: {containerIndex}");
            onComplete?.Invoke(null);
            yield break;
        }

    #if SICCITY_GLTF
        GameObject avatarCaricato = null;
        bool caricamentoCompletato = false;

        StartCoroutine(CaricaAvatarDaGLBCoroutine(glbPath, (loadedAvatar) => {
            avatarCaricato = loadedAvatar;
            caricamentoCompletato = true;
        }));
                
        yield return new WaitUntil(() => caricamentoCompletato);
                
        if (avatarCaricato == null)
        {
           // Debug.LogError("Caricamento dell'avatar fallito");
            onComplete?.Invoke(null);
            yield break;
        }
        GameObject targetContainer = containerAvatars[containerIndex];
        if (targetContainer == null)
        {
            //Debug.LogError($"Container {containerIndex} non disponibile!");
            if (avatarCaricato != null)
                Destroy(avatarCaricato);
            onComplete?.Invoke(null);
            yield break;
        }
        ClearContainer(targetContainer);
        avatarCaricato.transform.SetParent(targetContainer.transform, false);
        avatarCaricato.transform.localPosition = Vector3.zero;
        avatarCaricato.transform.localRotation = Quaternion.Euler(rotazioneAvatar);
        avatarCaricato.transform.localScale = scalaAvatar;
        ApplyFinalTransformations(avatarCaricato);
        HidePlaceholder(targetContainer);
                
        if (containerIndex >= 0 && containerIndex < avatarObjects.Length)
        {
            avatarObjects[containerIndex] = avatarCaricato;
            //Debug.Log($"[CaricaEPosizionaAvatarDaGLB] Avatar salvato in avatarObjects[{containerIndex}]");
        }

        avatarCaricato.SetActive(true);
        if (targetContainer != null)
        {
            targetContainer.SetActive(true);
            if (targetContainer.transform.parent != null)
            {
                targetContainer.transform.parent.gameObject.SetActive(true);
            }
        }
                
        //Debug.Log($"[CaricaEPosizionaAvatarDaGLB] Avatar posizionato con successo nel container {targetContainer.name} (indice: {containerIndex})");
                
        onComplete?.Invoke(avatarCaricato);
    #else
        //Debug.LogError("GLTFast non è installato!");
        onComplete?.Invoke(null);
        yield break;
    #endif
    }

    private void ApplyFinalTransformations(GameObject avatar)
    {
        if (avatar == null) return;
        if (rotazioneAvatar != Vector3.zero)
        {
            avatar.transform.localRotation = Quaternion.Euler(rotazioneAvatar);
        }
        if (scalaAvatar != Vector3.one)
        {
            avatar.transform.localScale = scalaAvatar;
        }
        if (mantieniFisicaAvatar)
        {
            if (avatar.GetComponent<Rigidbody>() == null)
            {
                Rigidbody rb = avatar.AddComponent<Rigidbody>();
                rb.isKinematic = true;
            }
        }
        else
        {
            var colliders = avatar.GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }
        }

        //Debug.Log($"Trasformazioni finali applicate a {avatar.name}");
    }

    private void HidePlaceholder(GameObject container)
    {
        if (container == null) return;

        Transform placeholder = container.transform.Find("Placeholder");
        if (placeholder != null)
        {
           // Debug.Log($"[HidePlaceholder] Nascondo il placeholder in: {container.name}");
            placeholder.gameObject.SetActive(false);
        }
    }

    private void CambiaStanza()
    {
        if (stanzaCreazione != null && stanzaFinale != null)
        {
            if (stanzaFinale.activeInHierarchy)
            {
                //Debug.Log("Già nella stanza finale");
                return;
            }

            //Debug.Log("Cambio dalla stanza di creazione alla stanza finale");
            stanzaCreazione.SetActive(false);
            stanzaFinale.SetActive(true);
        }
    }

    private void RiproduciSuono()
    {
        if (audioSource != null && suonoFinish != null)
        {
            audioSource.PlayOneShot(suonoFinish);
           // Debug.Log("Suono finish riprodotto");
        }
    }

    private void TrovaAvatarCreato()
    {
        GameObject[] taggedAvatars = GameObject.FindGameObjectsWithTag(tagAvatarCreato);
        if (taggedAvatars != null && taggedAvatars.Length > 0)
        {
            avatarCreato = taggedAvatars[0];
            //Debug.Log($"[TrovaAvatarCreato] Trovato avatar per tag '{tagAvatarCreato}': {avatarCreato.name}");
            return;
        }

        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Length >= 20 && System.Text.RegularExpressions.Regex.IsMatch(obj.name, "^[0-9a-fA-F]+$"))
            {
                if (obj.GetComponentInChildren<Renderer>() != null)
                {
                    avatarCreato = obj;
                    //Debug.Log($"[TrovaAvatarCreato] Trovato avatar per nome hash: {obj.name}");
                    return;
                }
            }
        }
        GameObject defaultAvatar = GameObject.Find("Avatar");
        if (defaultAvatar != null)
        {
            avatarCreato = defaultAvatar;
            //Debug.Log($"[TrovaAvatarCreato] Trovato oggetto con nome 'Avatar': {defaultAvatar.name}");
            return;
        }

       // Debug.LogWarning($"[TrovaAvatarCreato] Nessun avatar trovato");
    }

    public string GetSaveDirectory()
    {
        string saveDir;
        
        #if UNITY_EDITOR || UNITY_STANDALONE_WIN
        saveDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", Application.productName, SAVE_FOLDER_NAME);
        #else
        saveDir = Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME);
        #endif
        
        try
        {
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
                #if UNITY_ANDROID && !UNITY_EDITOR
                File.Create(Path.Combine(saveDir, ".nomedia")).Dispose();
                #endif
            }
            return saveDir;
        }
        catch (Exception e)
        {
           // Debug.LogError($"Errore nell'accesso alla directory: {e.Message}");
            return null;
        }
    }

    public List<string> GetSavedAvatarPaths()
    {
        List<string> avatarPaths = new List<string>();
        string saveDir = GetSaveDirectory();

        if (string.IsNullOrEmpty(saveDir) || !Directory.Exists(saveDir))
            return avatarPaths;

        try
        {
            string[] avatarDirs = Directory.GetDirectories(saveDir, "avatar_*");

            foreach (string dir in avatarDirs)
            {
                string dataFile = Path.Combine(dir, "avatar_data.json");
                string glbFile = Path.Combine(dir, GLB_FILE_NAME);
                if (File.Exists(dataFile) && File.Exists(glbFile))
                {
                    avatarPaths.Add(dir);
                }
            }
            avatarPaths.Sort((a, b) =>
                Directory.GetLastWriteTime(b).CompareTo(Directory.GetLastWriteTime(a)));

            return avatarPaths;
        }
        catch (Exception e)
        {
           // Debug.LogError($"Errore nel recupero degli avatar salvati: {e.Message}");
            return new List<string>();
        }
    }

    public void CaricaTuttiGliAvatar()
    {
        loadedAvatars.Clear();
        List<string> avatarDirs = GetSavedAvatarPaths();

        foreach (string dir in avatarDirs)
        {
            try
            {
                string dataFile = Path.Combine(dir, "avatar_data.json");
                if (File.Exists(dataFile))
                {
                    string jsonData = File.ReadAllText(dataFile);
                    AvatarSaveData avatarData = JsonUtility.FromJson<AvatarSaveData>(jsonData);

                    if (avatarData != null)
                    {
                        avatarData.saveDirectory = dir;
                        loadedAvatars.Add(avatarData);
                    }
                }
            }
            catch (Exception e)
            {
                //Debug.LogError($"Errore nel caricamento dell'avatar da {dir}: {e.Message}");
            }
        }
        OnAvatarsLoaded?.Invoke(loadedAvatars);
    }
    private class ExportState
    {
        public bool isExporting = false;
        public bool isComplete = false;
        public bool success = false;
        public string error = "";
        public string glbPath = "";
        public GameObject exportObject = null;
        public System.Action<bool, string> onComplete = null;
    }
    
    private ExportState currentExportState = null;

    //The code saves a 3D avatar to a GLB file using Unity's GLTFast library. 
    //It creates a timestamped directory, makes a clean copy of the avatar without unnecessary 
    //components, and exports it as a GLB file. The process runs asynchronously with error handling 
    //and cleanup, ensuring the export doesn't freeze the application. The result is returned through
    //a callback indicating success or failure.
    
    public void SalvaAvatarComeGLB(System.Action<bool> onComplete = null)
    {
        if (avatarCreato == null)
        {
           // Debug.LogError("[SalvaAvatarComeGLB] Nessun avatar da salvare");
            onComplete?.Invoke(false);
            return;
        }

        string saveDir = GetSaveDirectory();
        if (string.IsNullOrEmpty(saveDir))
        {
           // Debug.LogError("[SalvaAvatarComeGLB] Directory di salvataggio non valida");
            onComplete?.Invoke(false);
            return;
        }
        if (!Directory.Exists(saveDir))
        {
            Directory.CreateDirectory(saveDir);
        }

        string glbPath = Path.Combine(saveDir, GLB_FILE_NAME);
        //Debug.Log($"[SalvaAvatarComeGLB] Salvataggio in corso su: {glbPath}");

        StartCoroutine(SalvaAvatarComeGLBCoroutine((success, path) =>
        {
            onComplete?.Invoke(success);
        }));
    }

    private IEnumerator SalvaAvatarComeGLBCoroutine(System.Action<bool, string> onComplete)
    {
        //Debug.Log("[DEBUG] ===== INIZIO SalvaAvatarComeGLBCoroutine =====");
        
        bool operationSuccess = false;
        string savedPath = "";
        string errorMessage = "";
        
        if (avatarCreato == null)
        {
            errorMessage = "Nessun avatar da salvare - avatarCreato è null";
           // Debug.LogError($"[SalvaAvatarComeGLBCoroutine] ERRORE: {errorMessage}");
            onComplete?.Invoke(false, errorMessage);
            yield break;
        }
        
        System.Action<bool, string> wrappedCallback = (success, path) => {
            operationSuccess = success;
            savedPath = path;
            if (!success) {
                errorMessage = path; 
            }
            //Debug.Log($"[wrappedCallback] Chiamata di callback - Success: {success}, Path: {path}");
            onComplete?.Invoke(success, success ? path : errorMessage);
        };
        
        //Debug.Log($"[DEBUG] Avatar da salvare: {avatarCreato.name}, attivo: {avatarCreato.activeInHierarchy}");
        
        var renderer = avatarCreato.GetComponentInChildren<Renderer>();
        if (renderer == null)
        {
           // Debug.LogError("[SalvaAvatarComeGLBCoroutine] ERRORE: Nessun componente Renderer trovato nell'avatar");
            onComplete?.Invoke(false, "");
            yield break;
        }

        string saveDir = GetSaveDirectory();
        //Debug.Log($"[DEBUG] Directory di salvataggio: {saveDir}");
        
        if (string.IsNullOrEmpty(saveDir))
        {
           // Debug.LogError("[SalvaAvatarComeGLBCoroutine] ERRORE: Directory di salvataggio non valida o vuota");
            onComplete?.Invoke(false, "");
            yield break;
        }
        
        try
        {
            if (!Directory.Exists(saveDir))
            {
                //Debug.Log($"[DEBUG] La directory di salvataggio non esiste, tentativo di creazione...");
                Directory.CreateDirectory(saveDir);
            }
            string testFile = Path.Combine(saveDir, "test_permissions.tmp");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            //Debug.Log("[DEBUG] Verifica permessi di scrittura riuscita");
        }
        catch (Exception ex)
        {
            //Debug.LogError($"[SalvaAvatarComeGLBCoroutine] ERRORE: Impossibile scrivere nella directory {saveDir}: {ex.Message}");
            onComplete?.Invoke(false, "");
            yield break;
        }

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string avatarSaveDir = Path.Combine(saveDir, $"avatar_{timestamp}");
        bool directoryCreated = false;
        
        //Debug.Log($"[DEBUG] Inizio salvataggio GLB in: {avatarSaveDir}");
        try
        {
            //Debug.Log($"[DEBUG] Tentativo di creare la directory: {avatarSaveDir}");
            Directory.CreateDirectory(avatarSaveDir);
            directoryCreated = true;
            //Debug.Log("[DEBUG] Directory creata con successo");
        }
        catch (Exception e)
        {
            //Debug.LogError($"[SalvaAvatarComeGLBCoroutine] Errore nella creazione directory: {e.Message}");
            //Debug.LogError($"[DEBUG] Stack trace: {e.StackTrace}");
            onComplete?.Invoke(false, "");
            yield break;
        }

        if (!directoryCreated)
        {
            onComplete?.Invoke(false, "");
            yield break;
        }
        //Debug.Log("[DEBUG] Attesa per la sincronizzazione della directory...");
        yield return new WaitForEndOfFrame();

        //Debug.Log("[DEBUG] Verifica accessibilità directory...");
        if (!Directory.Exists(avatarSaveDir))
        {
            //Debug.LogError($"[SalvaAvatarComeGLBCoroutine] Impossibile accedere alla directory creata: {avatarSaveDir}");
            onComplete?.Invoke(false, "");
            yield break;
        }

        string glbPath = Path.Combine(avatarSaveDir, GLB_FILE_NAME);
        //Debug.Log($"[SalvaAvatarComeGLBCoroutineFixed] Salvataggio in: {glbPath}");
        GameObject exportObject = new GameObject("TempExport");

        currentExportState = new ExportState
        {
            isExporting = true,
            glbPath = glbPath,
            exportObject = exportObject,
            onComplete = onComplete
        };

        StartCoroutine(RunExportProcess(exportObject, glbPath, onComplete));
    }
    
    private IEnumerator RunExportProcess(GameObject exportObject, string glbPath, System.Action<bool, string> onComplete)
    {
        //Debug.Log($"[RunExportProcess] Inizio esportazione su percorso: {glbPath}");
        if (avatarCreato == null)
        {
            string error = "[RunExportProcess] ERRORE: avatarCreato è null";
            //Debug.LogError(error);
            onComplete?.Invoke(false, error);
            yield break;
        }

        GameObject avatarCopy = Instantiate(avatarCreato, exportObject.transform, false);
        if (avatarCopy == null)
        {
            string error = "[RunExportProcess] ERRORE: Impossibile creare la copia dell'avatar";
            Debug.LogError(error);
            onComplete?.Invoke(false, error);
            yield break;
        }
        
       // Debug.Log("[RunExportProcess] Copia avatar creata, rimozione componenti non necessari...");

        var componentsToRemove = avatarCopy.GetComponentsInChildren<Component>();
        int removedCount = 0;
        
        foreach (var comp in componentsToRemove)
        {
            if (comp == null) continue;
            if (comp is Transform) continue;
            if (comp is Renderer || comp is MeshFilter || comp is SkinnedMeshRenderer) continue;
            if (comp is MeshRenderer) continue;
            if (comp is Rigidbody || comp is Collider || comp is Animation || comp is Animator)
            {
                Destroy(comp);
                removedCount++;
            }
        }
        //Debug.Log($"[RunExportProcess] Rimossi {removedCount} componenti non necessari");

       // Debug.Log("[RunExportProcess] Inizio esportazione GLB...");
        
        var export = new GameObjectExport();
        export.AddScene(new[] { exportObject.transform.GetChild(0).gameObject }, "AvatarScene");
        
        Task exportTask = export.SaveToFileAndDispose(glbPath);
        float timeout = 30f;
        float startTime = Time.time;
        bool exportSuccess = false;
        string errorMessage = string.Empty;
        
        while (!exportTask.IsCompleted && (Time.time - startTime) < timeout)
        {
            yield return null;
        }
        
        if (exportTask.IsFaulted)
        {
            errorMessage = $"Errore durante l'esportazione: {exportTask.Exception?.Message ?? "Errore sconosciuto"}";
            //Debug.LogError($"[RunExportProcess] {errorMessage}");
        }
        else if (exportTask.IsCanceled)
        {
            errorMessage = "Esportazione annullata";
           // Debug.LogWarning($"[RunExportProcess] {errorMessage}");
        }
        else if ((Time.time - startTime) >= timeout)
        {
            errorMessage = "Timeout durante l'esportazione";
            //Debug.LogError($"[RunExportProcess] {errorMessage}");
        }
        else if (!File.Exists(glbPath))
        {
            errorMessage = $"Il file {glbPath} non è stato creato";
            //Debug.LogError($"[RunExportProcess] {errorMessage}");
        }
        else
        {
            FileInfo fileInfo = new FileInfo(glbPath);
            if (fileInfo.Length == 0)
            {
                errorMessage = $"Il file {glbPath} è vuoto";
               // Debug.LogError($"[RunExportProcess] {errorMessage}");
            }
            else
            {
                exportSuccess = true;
                //Debug.Log($"[RunExportProcess] File GLB creato con successo: {glbPath} ({fileInfo.Length} bytes)");
            }
        }
        
        exportTask?.Dispose();
        if (exportSuccess)
        {
            onComplete?.Invoke(true, glbPath);
        }
        else
        {
            onComplete?.Invoke(false, errorMessage);
        }
        if (exportObject != null)
        {
            GameObject.Destroy(exportObject);
        }
    }

    private IEnumerator ExportGLBAndVerify(GameObject avatar, string outputPath, Action<bool, string> onComplete)
    {
        GameObjectExport export = null;
        try
        {
            export = new GameObjectExport();
            export.AddScene(new[] { avatar }, "AvatarScene");
        }
        catch (Exception ex)
        {
            //Debug.LogError($"[ExportGLBAndVerify] Errore durante la creazione dell'export: {ex.Message}");
            onComplete?.Invoke(false, ex.Message);
            yield break;
        }
        Task exportTask;
        try
        {
            exportTask = export.SaveToFileAndDispose(outputPath);
        }
        catch (Exception ex)
        {
           // Debug.LogError($"[ExportGLBAndVerify] Errore durante l'avvio dell'export: {ex.Message}");
            onComplete?.Invoke(false, ex.Message);
            yield break;
        }
        bool taskCompleted = false;
        bool taskSuccess = false;
        string taskError = null;

        yield return WaitForTaskCompletion(exportTask, (success, error) => {
            taskCompleted = true;
            taskSuccess = success;
            taskError = error;
        });

        if (!taskCompleted)
        {
            onComplete?.Invoke(false, "Errore durante l'attesa del completamento dell'export");
            yield break;
        }

        if (!taskSuccess)
        {
            onComplete?.Invoke(false, taskError ?? "Errore sconosciuto durante l'export");
            yield break;
        }

        yield return WaitForSecondsHelper(0.5f);
        if (!File.Exists(outputPath))
        {
            string errMsg = "File GLB non creato dopo l'esportazione";
            //Debug.LogError($"[ExportGLBAndVerify] {errMsg}");
            onComplete?.Invoke(false, errMsg);
            yield break;
        }

        FileInfo fileInfo = new FileInfo(outputPath);
        if (fileInfo.Length == 0)
        {
            string errMsg = "File GLB vuoto dopo l'esportazione";
            //Debug.LogError($"[ExportGLBAndVerify] {errMsg}");
            onComplete?.Invoke(false, errMsg);
            yield break;
        }

        onComplete?.Invoke(true, null);
    }

    private IEnumerator WaitForTaskCompletion(Task task, Action<bool, string> onComplete)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }

        bool success = !task.IsFaulted && !task.IsCanceled && task.IsCompletedSuccessfully;
        string error = task.Exception?.GetBaseException()?.Message;
        
        onComplete?.Invoke(success, error);
    }

    private IEnumerator WaitForSecondsHelper(float seconds)
    {
        float startTime = Time.time;
        while (Time.time - startTime < seconds)
        {
            yield return null;
        }
    }

    private IEnumerator WaitForFileToBeReady(string filePath, Action<string> onComplete, float timeout = 15f)
    {
        float startTime = Time.time;
        bool fileIsReady = false;
        bool shouldRetry = true;
        
        //Debug.Log($"[WaitForFileToBeReady] Attesa file: {filePath}");
        
        while (!fileIsReady && (Time.time - startTime) < timeout && shouldRetry)
        {
            bool fileCheckCompleted = false;
            bool fileExistsAndValid = false;
            try
            {
                if (File.Exists(filePath))
                {
                    using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        fileExistsAndValid = fileStream.Length > 0;
                       // Debug.Log($"[WaitForFileToBeReady] File trovato con {fileStream.Length} bytes");
                    }
                }
                fileCheckCompleted = true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                //Debug.Log($"[WaitForFileToBeReady] File non ancora pronto: {e.GetType().Name}");
                fileCheckCompleted = true;
                fileExistsAndValid = false;
            }
            catch (Exception e)
            {
                //Debug.LogError($"[WaitForFileToBeReady] Errore imprevisto: {e.Message}");
                fileCheckCompleted = true;
                fileExistsAndValid = false;
            }
            
            if (fileCheckCompleted)
            {
                if (fileExistsAndValid)
                {
                    fileIsReady = true;
                    //Debug.Log($"[WaitForFileToBeReady] File pronto dopo {Time.time - startTime:F2} secondi");
                    break;
                }
                else
                {
                    yield return new WaitForSeconds(0.2f);
                }
            }
            else
            {
                shouldRetry = false;
            }
        }
        
        if (fileIsReady)
        {
            onComplete?.Invoke(filePath);
        }
        else
        {
            //Debug.LogError($"[WaitForFileToBeReady] Timeout raggiunto per il file: {filePath}");
            onComplete?.Invoke(null);
        }
    }

    //The system loads and manages 3D avatars by first scanning the save directory for GLB files, 
    //sorting them by modification time, and then asynchronously loading up to 10 avatars using the 
    //GLTFast library. It handles each avatar through a pipeline that verifies file integrity, 
    //creates a clean copy, removes unnecessary components like scripts and physics, 
    //and places them in designated container slots. The process includes robust error handling, 
    //timeout management, and resource cleanup to ensure smooth operation. When saving, 
    //it creates a timestamped directory, exports the 3D model to a GLB file, 
    //and maintains a clean state by removing temporary objects and managing memory efficiently.


    public void CaricaUltimiAvatarSalvatiAllAvvio()
    {
        try
        {
            string saveDir = GetSaveDirectory();
            if (string.IsNullOrEmpty(saveDir) || !Directory.Exists(saveDir))
            {
                //Debug.LogWarning("Nessuna directory di salvataggio trovata per il caricamento automatico");
                return;
            }

            var savedAvatars = Directory.GetFiles(saveDir, "*.glb", SearchOption.AllDirectories)
                .OrderBy(f => new FileInfo(f).LastWriteTime)
                .Take(10)
                .ToList();
                            
            if (savedAvatars.Count == 0)
            {
                //Debug.Log("Nessun avatar salvato trovato per il caricamento automatico");
                return;
            }

            //Debug.Log($"[CaricaUltimiAvatarSalvatiAllAvvio] Trovati {savedAvatars.Count} avatar da caricare");
                        
            for (int i = 0; i < savedAvatars.Count; i++)
            {
                string avatarPath = savedAvatars[i];
                //Debug.Log($"[CaricaUltimiAvatarSalvatiAllAvvio] Caricamento avatar {i + 1}/{savedAvatars.Count}: {Path.GetFileName(avatarPath)}");
                int targetContainerIndex = i % containerAvatars.Length;
                StartCoroutine(WaitForFileToBeReady(avatarPath, (readyFilePath) => {
                    if (string.IsNullOrEmpty(readyFilePath))
                    {
                        //Debug.LogError($"[CaricaUltimiAvatarSalvatiAllAvvio] Timeout attesa file: {Path.GetFileName(avatarPath)}");
                        return;
                    }
                                
                    StartCoroutine(CaricaEPosizionaAvatarDaGLB(readyFilePath, targetContainerIndex, (avatar) => {
                        if (avatar != null)
                        {
                            //Debug.Log($"Avatar {Path.GetFileName(readyFilePath)} caricato con successo nel container {targetContainerIndex}");
                        }
                        else
                        {
                            //Debug.LogError($"Impossibile caricare l'avatar: {Path.GetFileName(readyFilePath)}");
                        }
                    }));
                }));
            }
        }
        catch (Exception e)
        {
            //Debug.LogError($"Errore durante il caricamento automatico dell'avatar: {e.Message}");
           // Debug.LogException(e);
        }
    }


    private IEnumerator CaricaAvatarDaGLBCoroutine(string filePath, Action<GameObject> onComplete = null)
    {
        //Debug.Log($"[CaricaAvatarDaGLBCoroutine] Inizio caricamento da: {filePath}");

        if (string.IsNullOrEmpty(filePath))
        {
            //Debug.LogError("[CaricaAvatarDaGLBCoroutine] Il percorso del file non può essere vuoto");
            onComplete?.Invoke(null);
            yield break;
        }

        if (!File.Exists(filePath))
        {
           // Debug.LogError($"[CaricaAvatarDaGLBCoroutine] File non trovato: {filePath}");
            onComplete?.Invoke(null);
            yield break;
        }

        FileInfo fileInfo = new FileInfo(filePath);
        if (fileInfo.Length == 0)
        {
           // Debug.LogError($"[CaricaAvatarDaGLBCoroutine] Il file è vuoto: {filePath}");
            onComplete?.Invoke(null);
            yield break;
        }

       // Debug.Log($"[CaricaAvatarDaGLBCoroutine] File verificato: {fileInfo.Length} bytes");

    #if SICCITY_GLTF
        GameObject avatarContainer = new GameObject($"LoadedAvatar_{System.DateTime.Now.Ticks}");
        GLTFast.GltfImport gltf = null;
        System.Threading.Tasks.Task<bool> loadTask = null;
        System.Threading.Tasks.Task<bool> instantiateTask = null;
        Exception loadException = null;
        Exception instantiateException = null;
        
        try
        {
            gltf = new GLTFast.GltfImport();
        }
        catch (Exception e)
        {
            //Debug.LogError($"[CaricaAvatarDaGLBCoroutine] Errore durante l'inizializzazione di GLTFast: {e.Message}");
            if (avatarContainer != null)
                Destroy(avatarContainer);
            onComplete?.Invoke(null);
            yield break;
        }
        
        try
        {
            //Debug.Log($"[CaricaAvatarDaGLBCoroutine] Inizio caricamento del file GLB: {filePath}");
            loadTask = gltf.Load(filePath);
            
            if (loadTask == null)
            {
                throw new Exception("Il task di caricamento non è stato creato correttamente");
            }
        }
        catch (Exception e)
        {
            loadException = e;
            //Debug.LogError($"[CaricaAvatarDaGLBCoroutine] Eccezione durante l'avvio del caricamento: {e.Message}");
        }
        
        if (loadTask != null)
        {
            float timeout = 30f;
            float startTime = Time.time;
            
            while (!loadTask.IsCompleted)
            {
                if (Time.time - startTime > timeout)
                {
                   // Debug.LogError("[CaricaAvatarDaGLBCoroutine] Timeout durante il caricamento del file");
                    onComplete?.Invoke(null);
                    yield break;
                }
                yield return null;
            }
        }
        
        if (loadException != null || loadTask == null || loadTask.IsFaulted || loadTask.Exception != null || !loadTask.IsCompletedSuccessfully)
        {
            var exception = loadException ?? (loadTask?.Exception?.GetBaseException());
            string errorMessage = exception != null ? 
                $"Errore durante il caricamento del file: {exception.Message}" : 
                "Errore sconosciuto durante il caricamento del file";
                
            //Debug.LogError($"[CaricaAvatarDaGLBCoroutine] {errorMessage}");
            
            if (exception != null)
            {
                //Debug.LogException(exception);
            }
            
            if (avatarContainer != null)
            {
                Destroy(avatarContainer);
            }
            
            gltf?.Dispose();
            
            onComplete?.Invoke(null);
            yield break;
        }
        
        if (!loadTask.Result)
        {
            //Debug.LogError($"[CaricaAvatarDaGLBCoroutine] Caricamento del file GLB fallito");
            if (avatarContainer != null)
                Destroy(avatarContainer);
            onComplete?.Invoke(null);
            yield break;
        }

        try
        {
            //Debug.Log("[CaricaAvatarDaGLBCoroutine] Inizio istanziazione del modello...");
            instantiateTask = gltf.InstantiateMainSceneAsync(avatarContainer.transform);
            
            if (instantiateTask == null)
            {
                throw new Exception("Il task di istanziazione non è stato creato correttamente");
            }
        }
        catch (Exception e)
        {
            instantiateException = e;
            //Debug.LogError($"[CaricaAvatarDaGLBCoroutine] Eccezione durante l'avvio dell'istanziazione: {e.Message}");
        }
        
        if (instantiateTask != null)
        {
            float timeout = 30f;
            float startTime = Time.time;
            
            while (!instantiateTask.IsCompleted)
            {
                if (Time.time - startTime > timeout)
                {
                    //Debug.LogError("[CaricaAvatarDaGLBCoroutine] Timeout durante l'istanziazione del modello");
                    gltf?.Dispose();
                    if (avatarContainer != null)
                        Destroy(avatarContainer);
                    onComplete?.Invoke(null);
                    yield break;
                }
                yield return null;
            }
        }
        
        if (instantiateException != null || instantiateTask == null || instantiateTask.IsFaulted || 
            instantiateTask.Exception != null || !instantiateTask.IsCompletedSuccessfully)
        {
            var exception = instantiateException ?? (instantiateTask?.Exception?.GetBaseException());
            string errorMessage = exception != null ?
                $"Errore durante l'istanziazione del modello: {exception.Message}" :
                "Errore sconosciuto durante l'istanziazione del modello";
                
           // Debug.LogError($"[CaricaAvatarDaGLBCoroutine] {errorMessage}");
            
            if (exception != null)
            {
                //Debug.LogException(exception);
            }
            
            gltf?.Dispose();
            
            if (avatarContainer != null)
            {
                Destroy(avatarContainer);
            }
            
            onComplete?.Invoke(null);
            yield break;
        }
        
        if (!instantiateTask.Result)
        {
            //Debug.LogError("[CaricaAvatarDaGLBCoroutine] Istanziazione del modello fallita: il risultato è false");
            gltf?.Dispose();
            if (avatarContainer != null)
                Destroy(avatarContainer);
            onComplete?.Invoke(null);
            yield break;
        }
        
        //Debug.Log($"[CaricaAvatarDaGLBCoroutine] Avatar caricato con successo: {avatarContainer.name}");
        onComplete?.Invoke(avatarContainer);
    #else
        //Debug.LogError("[CaricaAvatarDaGLBCoroutine] GLTFast non è installato");
        onComplete?.Invoke(null);
        yield break;
    #endif
    }

    private GameObject GetCurrentContainer()
    {
        if (containerAvatars == null || containerAvatars.Length == 0)
        {
           // Debug.LogError("Nessun container disponibile!");
            return null;
        }
        if (currentAvatarIndex < 0 || currentAvatarIndex >= containerAvatars.Length)
        {
            //Debug.LogWarning($"Indice container non valido ({currentAvatarIndex}), reset a 0");
            currentAvatarIndex = 0;
        }

        GameObject container = containerAvatars[currentAvatarIndex];
        if (container == null)
        {
            //Debug.LogError($"Container all'indice {currentAvatarIndex} è null!");
            
            for (int i = 0; i < containerAvatars.Length; i++)
            {
                if (containerAvatars[i] != null)
                {
                    currentAvatarIndex = i;
                    container = containerAvatars[i];
                    //Debug.Log($"Usato container alternativo all'indice {i}");
                    break;
                }
            }
            
            if (container == null)
            {
                //Debug.LogError("Nessun container valido trovato!");
                return null;
            }
        }

        //Debug.Log($"Container corrente: {container.name} (indice: {currentAvatarIndex})");
        return container;
    }

    private void ClearContainer(GameObject container)
    {
        if (container == null) 
        {
            //Debug.LogError("[ClearContainer] Container è null!");
            return;
        }

        //Debug.Log($"[ClearContainer] Pulizia container: {container.name}");

        List<Transform> childrenToDestroy = new List<Transform>();
        
        for (int i = 0; i < container.transform.childCount; i++)
        {
            Transform child = container.transform.GetChild(i);
            if (child.name.ToLower().Contains("placeholder")) 
            {
                //Debug.Log($"[ClearContainer] Mantengo placeholder: {child.name}");
                continue;
            }
            childrenToDestroy.Add(child);
        }

        foreach (Transform child in childrenToDestroy)
        {
            //Debug.Log($"[ClearContainer] Distruzione di: {child.name}");
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
        
        //Debug.Log($"[ClearContainer] Container pulito, rimossi {childrenToDestroy.Count} oggetti");
    }

    private int TrovaPrimoContainerLibero()
    {
        if (containerAvatars == null || containerAvatars.Length == 0)
        {
            //Debug.LogError("Nessun container disponibile!");
            return -1;
        }

        for (int i = 0; i < containerAvatars.Length; i++)
        {
            if (containerAvatars[i] == null)
            {
                //Debug.LogError($"Container {i} è null!");
                continue;
            }
            bool hasOnlyCapsule = true;
            foreach (Transform child in containerAvatars[i].transform)
            {
                if (!child.name.ToLower().Contains("capsule"))
                {
                    hasOnlyCapsule = false;
                    break;
                }
            }

            if (hasOnlyCapsule)
            {
                //Debug.Log($"Trovato container con solo la capsula all'indice {i}, considerato come libero");
                return i;
            }

            if (avatarObjects != null && i < avatarObjects.Length && avatarObjects[i] == null)
            {
                //Debug.Log($"Trovato container con avatar null all'indice {i}");
                return i;
            }
        }

        //Debug.Log("Nessun container libero trovato");
        return -1;
    }

    public event System.Action<GameObject> OnAvatarFinalized;


    [System.Serializable]
    public class AvatarSaveData
    {
        public string avatarName;
        public string saveDirectory;
        public string creationDate;
        public SerializableVector3 position;
        public SerializableVector3 rotation;
        public SerializableVector3 scale;
        public string glbPath;
        public bool isActive;
        public int saveVersion = 2;

        public AvatarSaveData()
        {
            avatarName = "Nuovo Avatar";
            creationDate = System.DateTime.UtcNow.ToString("o"); 
            position = Vector3.zero;
            rotation = Vector3.zero;
            scale = Vector3.one;
            isActive = true;
        }
        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }
        public static AvatarSaveData FromJson(string json)
        {
            try
            {
                return JsonUtility.FromJson<AvatarSaveData>(json);
            }
            catch (System.Exception e)
            {
                //Debug.LogError($"Errore nel parsing dei dati dell'avatar: {e.Message}");
                return null;
            }
        }
    }
    [System.Serializable]
    public class SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3() { }

        public SerializableVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public static implicit operator Vector3(SerializableVector3 rValue)
        {
            return new Vector3(rValue.x, rValue.y, rValue.z);
        }

        public static implicit operator SerializableVector3(Vector3 rValue)
        {
            return new SerializableVector3(rValue);
        }
    }
}