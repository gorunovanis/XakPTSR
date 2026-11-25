using UnityEngine;

/// <summary>
/// Принудительно устанавливает черный фон для стартового меню
/// </summary>
public class BlackBackgroundEnforcer : MonoBehaviour
{
    private Camera mainCamera;
    private bool isStartupScene;
    
    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        // Проверяем, является ли это стартовой сценой
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        isStartupScene = sceneName == "StartupMenu" || sceneName == "SampleScene";
        
        if (isStartupScene)
        {
            EnforceBlackBackground();
        }
    }
    
    void Start()
    {
        if (isStartupScene)
        {
            EnforceBlackBackground();
        }
    }
    
    void LateUpdate()
    {
        // Каждый кадр проверяем и принудительно устанавливаем черный фон
        if (isStartupScene && mainCamera != null)
        {
            if (mainCamera.clearFlags != CameraClearFlags.SolidColor || mainCamera.backgroundColor != Color.black)
            {
                EnforceBlackBackground();
            }
            
            // Также проверяем Render Settings
            if (RenderSettings.fog || RenderSettings.skybox != null || RenderSettings.ambientLight != Color.black)
            {
                RenderSettings.fog = false;
                RenderSettings.skybox = null;
                RenderSettings.ambientLight = Color.black;
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            }
        }
    }
    
    void EnforceBlackBackground()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
        }
        
        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
            
            Debug.Log($"[BlackBackgroundEnforcer] Enforced black background - Clear Flags: {mainCamera.clearFlags}, Background: {mainCamera.backgroundColor}");
        }
        
        // Устанавливаем Render Settings
        RenderSettings.fog = false;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.skybox = null;
        
        // Отключаем все источники света
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light light in allLights)
        {
            light.enabled = false;
        }
    }
}

