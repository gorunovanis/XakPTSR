using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Controls the welcome overlay shown at application start. Keeps the screen
/// dark until the user presses "Start" and optionally routes world-selection
/// clicks to the <see cref="WorldSceneController"/>.
/// </summary>
public class StartupMenuController : MonoBehaviour
{
    public static StartupMenuController Instance { get; private set; }

    [Header("References")]
    public Canvas menuCanvas;
    public PauseMenuController pauseMenuController;
    public WorldSceneController worldSceneController;
    public WorldEnvironmentController environmentController;
    public GameObject menuRoot;
    public UnityEngine.UI.Text selectedWorldLabel;

    [Header("Toggle")]
    public bool allowHotkeyToggle = true;
    public KeyCode toggleKey = KeyCode.F1;

    bool isVisible;
    WorldType pendingWorld = WorldType.Forest;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        ShowMenu();
    }

    void Update()
    {
        if (allowHotkeyToggle && Input.GetKeyDown(toggleKey))
            ToggleMenu();
    }

    public static void ShowGlobalMenu() => Instance?.ShowMenu();
    public static void HideGlobalMenu() => Instance?.HideMenu();
    public bool IsVisible => isVisible;

    public void ShowMenu()
    {
        if (menuCanvas != null)
            menuCanvas.enabled = true;

        if (menuRoot != null)
            menuRoot.SetActive(true);

        if (pauseMenuController == null)
            pauseMenuController = FindFirstObjectByType<PauseMenuController>();

        if (worldSceneController == null)
            worldSceneController = WorldSceneController.Instance;

        if (environmentController == null)
            environmentController = worldSceneController != null
                ? worldSceneController.environmentController
                : FindFirstObjectByType<WorldEnvironmentController>();

        pauseMenuController?.HidePauseMenu();

        // Скрываем все объекты окружения для черного фона
        if (environmentController != null)
        {
            environmentController.HideAll();
        }

        if (worldSceneController != null)
            pendingWorld = worldSceneController.CurrentWorld;
        else
            pendingWorld = WorldType.Forest;
        UpdateWorldLabel();
        // НЕ показываем превью - оставляем черный фон
        // PreviewSelectedWorld();
        isVisible = true;
    }

    public void OnStartButton()
    {
        Debug.Log($"[StartupMenu] OnStartButton called, pendingWorld: {pendingWorld}");
        
        if (worldSceneController == null)
        {
            worldSceneController = WorldSceneController.Instance;
            Debug.Log($"[StartupMenu] Retrieved WorldSceneController.Instance: {worldSceneController != null}");
        }

        if (worldSceneController == null)
        {
            Debug.LogError("[StartupMenu] worldSceneController is null! Cannot load world.");
            return;
        }

        Debug.Log($"[StartupMenu] Calling LoadWorld({pendingWorld})");
        worldSceneController.LoadWorld(pendingWorld);
        HideMenu();
        SessionManager.Instance?.AddEvent("StartupMenuClosed");
    }

    public void OnQuitButton()
    {
        SessionManager.Instance?.ExitSession("QuitFromStartupMenu");
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ToggleMenu()
    {
        if (isVisible) HideMenu();
        else ShowMenu();
    }

    public void OnWorldButton(int worldIndex)
    {
        Debug.Log($"[StartupMenu] OnWorldButton called with index: {worldIndex}");
        var values = (WorldType[])Enum.GetValues(typeof(WorldType));
        if (worldIndex < 0 || worldIndex >= values.Length)
        {
            Debug.LogWarning($"[StartupMenu] Invalid world index: {worldIndex}");
            return;
        }

        pendingWorld = values[worldIndex];
        Debug.Log($"[StartupMenu] Pending world set to: {pendingWorld}");
        UpdateWorldLabel();
        // НЕ загружаем сцену сразу - только обновляем метку "Выбрано"
        // PreviewSelectedWorld(); // Убрано - не загружаем сцену при выборе
    }

    public void OnForestButton()
    {
        Debug.Log("[StartupMenu] OnForestButton clicked!");
        OnWorldButton(0);
    }
    
    public void OnSeaButton()
    {
        Debug.Log("[StartupMenu] OnSeaButton clicked!");
        OnWorldButton(1);
    }
    
    public void OnMountainsButton() => OnWorldButton(2);
    
    public void OnSafePlaceButton()
    {
        Debug.Log("[StartupMenu] OnSafePlaceButton clicked!");
        OnWorldButton(3);
    }
    
    public void OnWarModeButton()
    {
        Debug.Log("[StartupMenu] OnWarModeButton clicked!");
        OnWorldButton(4);
    }

    void UpdateWorldLabel()
    {
        if (selectedWorldLabel == null)
        {
            Debug.LogWarning("[StartupMenu] selectedWorldLabel is null!");
            return;
        }

        string text = pendingWorld switch
        {
            WorldType.Forest => "ЛЕС",
            WorldType.Sea => "МОРЕ",
            WorldType.Mountains => "ГОРЫ",
            WorldType.SafePlace => "БЕЗОПАСНОЕ МЕСТО",
            WorldType.WarMode => "РЕЖИМ ВОЙНЫ",
            _ => pendingWorld.ToString().ToUpperInvariant()
        };

        selectedWorldLabel.text = $"Выбрано: {text}";
        Debug.Log($"[StartupMenu] Label updated to: {selectedWorldLabel.text}");
    }

    void PreviewSelectedWorld()
    {
        if (worldSceneController == null)
            worldSceneController = WorldSceneController.Instance;

        if (worldSceneController != null)
            worldSceneController.LoadWorld(pendingWorld);
    }

    void HideMenu()
    {
        if (!isVisible)
            return;

        if (menuRoot != null)
            menuRoot.SetActive(false);

        if (menuCanvas != null)
            menuCanvas.enabled = false;

        isVisible = false;
    }
}

