using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Режим "войны" - воспроизведение видео для экспозиционной терапии.
/// Видео загружается через пульт терапевта и отображается на экране перед камерой.
/// </summary>
public class WarModeController : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    public RenderTexture videoRenderTexture;
    public Material videoMaterial;
    
    [Header("Display")]
    public GameObject videoScreen;
    public float screenDistance = 3f;
    public Vector2 screenSize = new Vector2(4f, 2.25f); // 16:9 aspect ratio
    
    [Header("Camera")]
    public Camera mainCamera;
    
    private GameObject environmentRoot;
    private bool isActive = false;
    private string currentVideoPath = "";

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        CreateEnvironment();
    }

    void CreateEnvironment()
    {
        // Создаем корневой объект для окружения
        environmentRoot = new GameObject("WarModeEnvironment");
        environmentRoot.SetActive(false);
        
        // Черный фон
        if (environmentRoot == null || environmentRoot.transform == null)
        {
            Debug.LogError("[WarModeController] environmentRoot or transform is null!");
            return;
        }
        
        var background = GameObject.CreatePrimitive(PrimitiveType.Quad);
        background.name = "BlackBackground";
        if (background != null && background.transform != null && environmentRoot != null && environmentRoot.transform != null)
        {
            background.transform.SafeSetParent(environmentRoot.transform);
        }
        else
        {
            Debug.LogError("[WarModeController] background or environmentRoot transform is null!");
            return;
        }
        background.transform.position = new Vector3(0, 0, 5f);
        background.transform.localScale = new Vector3(20f, 20f, 1f);
        background.transform.rotation = Quaternion.LookRotation(Vector3.back);
        
        var bgMat = new Material(Shader.Find("Standard"));
        bgMat.color = Color.black;
        bgMat.SetFloat("_Metallic", 0f);
        bgMat.SetFloat("_Glossiness", 0f);
        background.GetComponent<MeshRenderer>().material = bgMat;
        
        // Создаем экран для видео
        videoScreen = GameObject.CreatePrimitive(PrimitiveType.Quad);
        videoScreen.name = "VideoScreen";
        if (videoScreen != null && videoScreen.transform != null && environmentRoot != null && environmentRoot.transform != null)
        {
            videoScreen.transform.SafeSetParent(environmentRoot.transform);
        }
        else
        {
            Debug.LogError("[WarModeController] videoScreen or environmentRoot transform is null!");
            return;
        }
        videoScreen.transform.position = new Vector3(0, 1.6f, screenDistance);
        videoScreen.transform.localScale = new Vector3(screenSize.x, screenSize.y, 1f);
        videoScreen.transform.rotation = Quaternion.LookRotation(Vector3.back);
        
        // Создаем RenderTexture для видео
        videoRenderTexture = new RenderTexture(1920, 1080, 24);
        videoRenderTexture.name = "VideoRenderTexture";
        
        // Создаем материал для экрана
        videoMaterial = new Material(Shader.Find("Standard"));
        videoMaterial.mainTexture = videoRenderTexture;
        videoMaterial.SetFloat("_Metallic", 0f);
        videoMaterial.SetFloat("_Glossiness", 0.1f);
        videoScreen.GetComponent<MeshRenderer>().material = videoMaterial;
        
        // Создаем VideoPlayer
        var videoPlayerGO = new GameObject("VideoPlayer");
        if (videoPlayerGO != null && videoPlayerGO.transform != null && environmentRoot != null && environmentRoot.transform != null)
        {
            videoPlayerGO.transform.SafeSetParent(environmentRoot.transform);
        }
        else
        {
            Debug.LogError("[WarModeController] videoPlayerGO or environmentRoot transform is null!");
            return;
        }
        videoPlayer = videoPlayerGO.AddComponent<VideoPlayer>();
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = videoRenderTexture;
        videoPlayer.isLooping = true;
        videoPlayer.playOnAwake = false;
        
        Debug.Log("[WarMode] Environment created");
    }

    public void LoadVideo(string videoPath)
    {
        if (string.IsNullOrEmpty(videoPath))
        {
            Debug.LogWarning("[WarMode] Video path is empty");
            return;
        }

        Debug.Log($"[WarMode] Loading video from: {videoPath}");
        
        try
        {
            // Проверяем, существует ли файл
            if (!System.IO.File.Exists(videoPath))
            {
                Debug.LogError($"[WarMode] Video file not found: {videoPath}");
                return;
            }

            // Загружаем видео
            videoPlayer.url = "file://" + videoPath;
            videoPlayer.Prepare();
            
            videoPlayer.prepareCompleted += OnVideoPrepared;
            currentVideoPath = videoPath;
            
            Debug.Log($"[WarMode] Video prepared: {videoPath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WarMode] Error loading video: {ex.Message}");
        }
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("[WarMode] Video prepared successfully");
        if (isActive)
        {
            vp.Play();
        }
    }

    public void StartWarMode()
    {
        Debug.Log("[WarMode] Starting war mode");
        isActive = true;
        
        if (environmentRoot != null)
        {
            environmentRoot.SetActive(true);
        }
        
        // Устанавливаем черный фон
        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
        }
        
        RenderSettings.fog = false;
        RenderSettings.ambientLight = Color.black;
        
        // Воспроизводим видео, если оно загружено
        if (videoPlayer != null && videoPlayer.isPrepared)
        {
            videoPlayer.Play();
            Debug.Log("[WarMode] Video playback started");
        }
        else if (!string.IsNullOrEmpty(currentVideoPath))
        {
            // Пытаемся загрузить видео снова
            LoadVideo(currentVideoPath);
        }
    }

    public void StopWarMode()
    {
        Debug.Log("[WarMode] Stopping war mode");
        isActive = false;
        
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
        
        if (environmentRoot != null)
        {
            environmentRoot.SetActive(false);
        }
    }

    public bool IsActive => isActive;
    
    void OnDestroy()
    {
        if (videoRenderTexture != null)
        {
            videoRenderTexture.Release();
            Destroy(videoRenderTexture);
        }
        
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
    }
}

