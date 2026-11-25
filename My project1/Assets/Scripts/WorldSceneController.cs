using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls switching between relaxing environments and loading separate scenes.
/// Exposed to UI buttons in pause menu and main menu.
/// </summary>
public class WorldSceneController : MonoBehaviour
{
    public static WorldSceneController Instance { get; private set; }

    [Header("World references")]
    public WorldEnvironmentController environmentController;
    public EnvironmentController atmosphereController;
    public BallTrackingController ballTrackingController;
    public WarModeController warModeController;

    [Header("Scene Names")]
    [Tooltip("Имя сцены для леса (SampleScene)")]
    public string forestSceneName = "SampleScene";
    
    [Tooltip("Имя сцены для моря")]
    public string seaSceneName = "World_Sea";
    
    [Tooltip("Имя стартовой сцены меню")]
    public string startupSceneName = "StartupMenu";

    [Header("Bootstrap")]
    [Tooltip("Если ссылки не назначены, создать простые окружения автоматически.")]
    public bool autoCreateWorlds = true;

    [Header("Defaults")]
    public WorldType defaultWorld = WorldType.Forest;

    WorldType currentWorld;
    public WorldType CurrentWorld => currentWorld;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Подписываемся на событие загрузки сцены
        SceneManager.sceneLoaded += OnSceneLoaded;

        // В стартовой сцене не создаем окружения
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName != "StartupMenu" && currentSceneName != "SampleScene")
        {
            EnsureWorldEnvironment();
            EnsureAtmosphereController();
            ApplyWorld(defaultWorld, false);
        }
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[WorldSceneController] Scene loaded: {scene.name}");
        
        // Если загрузилась сцена леса или моря, активируем соответствующий мир
        if (scene.name == forestSceneName)
        {
            currentWorld = WorldType.Forest;
            ApplyWorld(WorldType.Forest, false);
        }
        else if (scene.name == seaSceneName)
        {
            currentWorld = WorldType.Sea;
            ApplyWorld(WorldType.Sea, false);
        }
    }

    #region Public API for UI buttons

    public void LoadForestWorld() => LoadWorld(WorldType.Forest);
    public void LoadSeaWorld() => LoadWorld(WorldType.Sea);
    public void LoadMountainWorld() => LoadWorld(WorldType.Mountains);

    public void LoadWorld(WorldType type)
    {
        Debug.Log($"[WorldSceneController] LoadWorld called with type: {type}");
        
        // Для Forest и Sea загружаем отдельные сцены
        switch (type)
        {
            case WorldType.Forest:
                LoadWorldScene(forestSceneName);
                return;
                
            case WorldType.Sea:
                LoadWorldScene(seaSceneName);
                return;
                
            case WorldType.SafePlace:
            case WorldType.WarMode:
                // Эти режимы работают в текущей сцене
                ApplyWorld(type, true);
                return;
                
            default:
                // Для остальных используем старую логику
                ApplyWorld(type, true);
                return;
        }
    }
    
    void LoadWorldScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"[WorldSceneController] Scene name is null or empty!");
            return;
        }
        
        Debug.Log($"[WorldSceneController] Loading scene: {sceneName}");
        
        // Проверяем, существует ли сцена в Build Settings
        bool sceneExists = false;
        int sceneBuildIndex = -1;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneNameFromPath == sceneName)
            {
                sceneExists = true;
                sceneBuildIndex = i;
                break;
            }
        }
        
        if (!sceneExists)
        {
            Debug.LogError($"[WorldSceneController] Scene '{sceneName}' NOT FOUND in Build Settings! Please add '{sceneName}.unity' to File → Build Settings → Scenes In Build.");
            return;
        }
        
        // Загружаем сцену
        try
        {
            SceneManager.LoadScene(sceneName);
            Debug.Log($"[WorldSceneController] Scene '{sceneName}' loaded successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WorldSceneController] Error loading scene '{sceneName}': {ex.Message}");
        }
    }

    #endregion

    private void ApplyWorld(WorldType type, bool logEvent)
    {
        Debug.Log($"[WorldSceneController] ApplyWorld called with type: {type}, logEvent: {logEvent}");
        currentWorld = type;

        // Сначала деактивируем все режимы
        DeactivateAllModes();

        // Активируем нужный режим
        switch (type)
        {
            case WorldType.Forest:
            case WorldType.Sea:
            case WorldType.Mountains:
                // Обычные миры
                EnsureWorldEnvironment();
                EnsureAtmosphereController();

                if (environmentController != null)
                {
                    Debug.Log($"[WorldSceneController] Calling ShowWorld({type})");
                    environmentController.ShowWorld(type);
                }
                else
                {
                    Debug.LogError("[WorldSceneController] environmentController is null!");
                }

                if (atmosphereController != null)
                {
                    var preset = type switch
                    {
                        WorldType.Forest => EnvironmentPreset.ForestDay,
                        WorldType.Sea => EnvironmentPreset.SeaSunset,
                        WorldType.Mountains => EnvironmentPreset.MountainDawn,
                        _ => EnvironmentPreset.Neutral
                    };

                    Debug.Log($"[WorldSceneController] Applying preset: {preset}");
                    atmosphereController.ApplyPreset(preset);
                }
                break;

            case WorldType.SafePlace:
                // Безопасное место с шариком
                EnsureBallTrackingController();
                if (ballTrackingController != null)
                {
                    ballTrackingController.StartTracking();
                    Debug.Log("[WorldSceneController] SafePlace mode activated");
                }
                break;

            case WorldType.WarMode:
                // Режим войны с видео
                EnsureWarModeController();
                if (warModeController != null)
                {
                    warModeController.StartWarMode();
                    Debug.Log("[WorldSceneController] WarMode activated");
                }
                break;
        }

        if (logEvent)
            SessionManager.Instance?.AddEvent("WorldSelected", -1, type.ToString());
    }

    void DeactivateAllModes()
    {
        // Деактивируем все окружения
        if (environmentController != null)
            environmentController.HideAll();

        // Деактивируем безопасное место
        if (ballTrackingController != null && ballTrackingController.IsActive)
            ballTrackingController.StopTracking();

        // Деактивируем режим войны
        if (warModeController != null && warModeController.IsActive)
            warModeController.StopWarMode();
    }

    void EnsureBallTrackingController()
    {
        if (ballTrackingController != null)
            return;

        ballTrackingController = FindFirstObjectByType<BallTrackingController>();
        if (ballTrackingController != null)
            return;

        Debug.Log("[WorldSceneController] Creating BallTrackingController at runtime.");
        var go = new GameObject("BallTrackingController");
        ballTrackingController = go.AddComponent<BallTrackingController>();
        ballTrackingController.mainCamera = Camera.main ?? FindFirstObjectByType<Camera>();
    }

    void EnsureWarModeController()
    {
        if (warModeController != null)
            return;

        warModeController = FindFirstObjectByType<WarModeController>();
        if (warModeController != null)
            return;

        Debug.Log("[WorldSceneController] Creating WarModeController at runtime.");
        var go = new GameObject("WarModeController");
        warModeController = go.AddComponent<WarModeController>();
        warModeController.mainCamera = Camera.main ?? FindFirstObjectByType<Camera>();
    }

    void EnsureWorldEnvironment()
    {
        if (environmentController != null && environmentController.forestEnvironment != null)
            return;

        if (environmentController == null)
            environmentController = FindFirstObjectByType<WorldEnvironmentController>();

        if (environmentController != null && environmentController.forestEnvironment != null)
            return;

        if (!autoCreateWorlds)
        {
            Debug.LogWarning("[WorldSceneController] Environment controller not found and autoCreateWorlds disabled.");
            return;
        }

        Debug.Log("[WorldSceneController] Creating default world environments...");
        environmentController = CreateWorldEnvironmentController();
        environmentController.HideAll();
    }

    void EnsureAtmosphereController()
    {
        if (atmosphereController != null)
            return;

        atmosphereController = FindFirstObjectByType<EnvironmentController>();
        if (atmosphereController != null)
            return;

        Debug.Log("[WorldSceneController] Creating EnvironmentController at runtime.");
        var go = new GameObject("EnvironmentController");
        atmosphereController = go.AddComponent<EnvironmentController>();

        Camera cam = Camera.main ?? FindFirstObjectByType<Camera>();
        atmosphereController.mainCamera = cam;
        atmosphereController.mainLight = FindFirstObjectByType<Light>();
        atmosphereController.baseFogDensity = 0.03f;
        atmosphereController.ApplyPreset(EnvironmentPreset.Neutral);
    }

    WorldEnvironmentController CreateWorldEnvironmentController()
    {
        var root = new GameObject("WorldEnvironments");
        
        if (root == null)
        {
            Debug.LogError("[WorldSceneController] CreateWorldEnvironmentController: root is null!");
            return null;
        }
        
        if (root.transform == null)
        {
            Debug.LogError("[WorldSceneController] CreateWorldEnvironmentController: root.transform is null!");
            return null;
        }
        
        DontDestroyOnLoad(root);

        var ctrl = root.AddComponent<WorldEnvironmentController>();
        if (ctrl == null)
        {
            Debug.LogError("[WorldSceneController] CreateWorldEnvironmentController: ctrl is null!");
            return null;
        }
        
        ctrl.forestEnvironment = CreateForestEnvironment(root.transform);
        ctrl.seaEnvironment = CreateSeaEnvironment(root.transform);
        ctrl.mountainEnvironment = CreateMountainEnvironment(root.transform);
        return ctrl;
    }

    GameObject CreateForestEnvironment(Transform parent)
    {
        if (parent == null)
        {
            Debug.LogError("[WorldSceneController] CreateForestEnvironment: parent is null!");
            return null;
        }
        
        var root = new GameObject("ForestEnvironment");
        if (root == null || root.transform == null)
        {
            Debug.LogError("[WorldSceneController] CreateForestEnvironment: root or root.transform is null!");
            return null;
        }
        
        root.transform.SafeSetParent(parent, false);

        var ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (ground != null && ground.transform != null && root != null && root.transform != null)
        {
            ground.transform.SafeSetParent(root.transform);
        }
        else
        {
            Debug.LogError("[WorldSceneController] CreateForestEnvironment: ground or root transform is null!");
            return root;
        }
        ground.transform.localScale = new Vector3(8f, 0.2f, 8f);
        ground.GetComponent<MeshRenderer>().material = URPMaterialFactory.Create(new Color(0.08f, 0.25f, 0.11f), 0.1f);

        for (int i = 0; i < 14; i++)
        {
            float angle = i * Mathf.PI * 2f / 14f;
            float radius = Random.Range(2.5f, 6.5f);
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            CreateTree(root.transform, pos, Random.Range(3.5f, 6f),
                new Color(0.3f, 0.18f, 0.12f),
                new Color(0.15f, 0.55f, 0.25f));
        }

        for (int i = 0; i < 5; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(1.5f, 4.5f);
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 2.8f, Mathf.Sin(angle) * radius);
            CreateGlowingOrb(root.transform, pos, new Color(0.8f, 0.9f, 0.5f));
        }

        root.SetActive(false);
        return root;
    }

    GameObject CreateSeaEnvironment(Transform parent)
    {
        if (parent == null)
        {
            Debug.LogError("[WorldSceneController] CreateSeaEnvironment: parent is null!");
            return null;
        }
        
        var root = new GameObject("SeaEnvironment");
        if (root == null || root.transform == null)
        {
            Debug.LogError("[WorldSceneController] CreateSeaEnvironment: root or root.transform is null!");
            return null;
        }
        
        root.transform.SafeSetParent(parent, false);

        var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (water != null && water.transform != null && root != null && root.transform != null)
        {
            water.transform.SafeSetParent(root.transform);
        }
        else
        {
            Debug.LogError("[WorldSceneController] CreateSeaEnvironment: water or root transform is null!");
            return root;
        }
        water.transform.localScale = new Vector3(10f, 0.15f, 10f);
        var waterMat = URPMaterialFactory.Create(new Color(0.04f, 0.2f, 0.45f), 0.8f);
        waterMat.EnableKeyword("_EMISSION");
        waterMat.SetColor("_EmissionColor", new Color(0.08f, 0.25f, 0.5f));
        water.GetComponent<MeshRenderer>().material = waterMat;

        var island = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (island != null && island.transform != null && root != null && root.transform != null)
        {
            island.transform.SafeSetParent(root.transform);
        }
        else
        {
            Debug.LogError("[WorldSceneController] CreateSeaEnvironment: island or root transform is null!");
        }
        island.transform.position = new Vector3(0, 0.05f, 0);
        island.transform.localScale = new Vector3(4f, 0.2f, 3f);
        island.GetComponent<MeshRenderer>().material = URPMaterialFactory.Create(new Color(0.85f, 0.75f, 0.55f), 0.2f);

        for (int i = 0; i < 5; i++)
        {
            float angle = i * Mathf.PI * 2f / 5f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * 2.3f, 0.2f, Mathf.Sin(angle) * 2.3f);
            CreatePalmTree(root.transform, pos);
        }

        for (int i = 0; i < 3; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-6f, 6f), 0.2f, Random.Range(-6f, 6f));
            CreateFloatingBuoy(root.transform, pos);
        }

        root.SetActive(false);
        return root;
    }

    GameObject CreateMountainEnvironment(Transform parent)
    {
        if (parent == null)
        {
            Debug.LogError("[WorldSceneController] CreateMountainEnvironment: parent is null!");
            return null;
        }
        
        var root = new GameObject("MountainEnvironment");
        if (root == null || root.transform == null)
        {
            Debug.LogError("[WorldSceneController] CreateMountainEnvironment: root or root.transform is null!");
            return null;
        }
        
        root.transform.SafeSetParent(parent, false);

        var plateau = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (plateau != null && plateau.transform != null && root != null && root.transform != null)
        {
            plateau.transform.SafeSetParent(root.transform);
        }
        else
        {
            Debug.LogError("[WorldSceneController] CreateMountainEnvironment: plateau or root transform is null!");
            return root;
        }
        plateau.transform.localScale = new Vector3(9f, 0.2f, 9f);
        plateau.GetComponent<MeshRenderer>().material = URPMaterialFactory.Create(new Color(0.25f, 0.26f, 0.30f), 0.15f);

        for (int i = 0; i < 3; i++)
        {
            float angle = i * Mathf.PI * 2f / 3f + Mathf.PI / 6f;
            float radius = Random.Range(2.5f, 4.5f);
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 1.3f, Mathf.Sin(angle) * radius);
            CreateMountain(root.transform, pos, Random.Range(2.5f, 3.8f));
        }

        CreateMountain(root.transform, new Vector3(0, 1.4f, 0), 4.5f);

        for (int i = 0; i < 5; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-3f, 3f), 0.3f, Random.Range(-3f, 3f));
            var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (rock != null && rock.transform != null && root != null && root.transform != null)
            {
                rock.transform.SafeSetParent(root.transform);
            }
            else
            {
                Debug.LogError("[WorldSceneController] CreateMountainEnvironment: rock or root transform is null!");
                continue;
            }
            rock.transform.position = pos;
            rock.transform.localScale = Vector3.one * Random.Range(0.6f, 1.3f);
            rock.GetComponent<MeshRenderer>().material = URPMaterialFactory.Create(new Color(0.45f, 0.48f, 0.52f), 0.25f);
        }

        root.SetActive(false);
        return root;
    }

    void CreateTree(Transform parent, Vector3 position, float height, Color trunkColor, Color crownColor)
    {
        if (parent == null)
        {
            Debug.LogError("[WorldSceneController] CreateTree: parent is null!");
            return;
        }
        
        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (trunk != null && trunk.transform != null && parent != null)
        {
            trunk.transform.SafeSetParent(parent);
        }
        else
        {
            Debug.LogError("[WorldSceneController] CreateTree: trunk or parent is null!");
            return;
        }
        trunk.transform.position = position + new Vector3(0, height / 2f, 0);
        trunk.transform.localScale = new Vector3(0.4f, height / 2f, 0.4f);
        trunk.GetComponent<MeshRenderer>().material = URPMaterialFactory.Create(trunkColor, 0.1f);

        var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        if (crown != null && crown.transform != null && parent != null)
        {
            crown.transform.SafeSetParent(parent);
        }
        else
        {
            Debug.LogError("[WorldSceneController] CreateTree: crown or parent is null!");
            return;
        }
        crown.transform.position = trunk.transform.position + new Vector3(0, height / 2f, 0);
        crown.transform.localScale = Vector3.one * Random.Range(2f, 3f);
        crown.GetComponent<MeshRenderer>().material = URPMaterialFactory.Create(crownColor, 0.3f);
    }

    void CreateGlowingOrb(Transform parent, Vector3 position, Color color)
    {
        if (parent == null)
        {
            Debug.LogError("[WorldSceneController] CreateGlowingOrb: parent is null!");
            return;
        }
        
        var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        if (orb != null && orb.transform != null && parent != null)
        {
            orb.transform.SafeSetParent(parent);
        }
        else
        {
            Debug.LogError("[WorldSceneController] CreateGlowingOrb: orb or parent is null!");
            return;
        }
        orb.transform.position = position;
        orb.transform.localScale = Vector3.one * Random.Range(0.6f, 1.2f);

        var mat = URPMaterialFactory.Create(color, 0.8f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * 1.5f);
        orb.GetComponent<MeshRenderer>().material = mat;

        var floatAnim = orb.AddComponent<FloatingObject>();
        floatAnim.amplitude = Random.Range(0.3f, 0.7f);
        floatAnim.frequency = Random.Range(0.2f, 0.5f);
        floatAnim.rotationSpeed = new Vector3(0, Random.Range(10f, 40f), 0);
    }

    void CreatePalmTree(Transform parent, Vector3 basePosition)
    {
        if (parent == null) return;
        
        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (trunk == null || trunk.transform == null) return;
        
        trunk.transform.SetParent(parent);
        trunk.transform.position = basePosition + new Vector3(0, 1.2f, 0);
        trunk.transform.localScale = new Vector3(0.35f, 1.2f, 0.35f);
        trunk.transform.rotation = Quaternion.Euler(Random.Range(-5f, 5f), Random.Range(0f, 360f), Random.Range(-5f, 5f));
        trunk.GetComponent<MeshRenderer>().material = URPMaterialFactory.Create(new Color(0.45f, 0.28f, 0.15f), 0.2f);

        for (int i = 0; i < 5; i++)
        {
            var leaf = GameObject.CreatePrimitive(PrimitiveType.Quad);
            if (leaf == null || leaf.transform == null || trunk == null || trunk.transform == null) continue;
            
            leaf.transform.SafeSetParent(trunk.transform);
            leaf.transform.localPosition = new Vector3(0, trunk.transform.localScale.y, 0);
            leaf.transform.localRotation = Quaternion.Euler(Random.Range(5f, 15f), i * 72f, 0);
            leaf.transform.localScale = new Vector3(0.4f, 1.5f, 1f);
            leaf.GetComponent<MeshRenderer>().material = URPMaterialFactory.Create(new Color(0.2f, 0.6f, 0.4f), 0.4f);
        }
    }

    void CreateFloatingBuoy(Transform parent, Vector3 position)
    {
        if (parent == null)
        {
            Debug.LogError("[WorldSceneController] CreateFloatingBuoy: parent is null!");
            return;
        }
        
        var buoy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        if (buoy != null && buoy.transform != null && parent != null)
        {
            buoy.transform.SafeSetParent(parent);
        }
        else
        {
            Debug.LogError("[WorldSceneController] CreateFloatingBuoy: buoy or parent is null!");
            return;
        }
        buoy.transform.position = position;
        buoy.transform.localScale = new Vector3(0.4f, 0.8f, 0.4f);
        buoy.GetComponent<MeshRenderer>().material = URPMaterialFactory.Create(new Color(0.9f, 0.4f, 0.2f), 0.5f);

        var floatAnim = buoy.AddComponent<FloatingObject>();
        floatAnim.amplitude = Random.Range(0.15f, 0.25f);
        floatAnim.frequency = Random.Range(0.3f, 0.5f);
        floatAnim.rotationSpeed = new Vector3(0, Random.Range(-5f, 5f), 0);
    }

    void CreateMountain(Transform parent, Vector3 position, float height)
    {
        if (parent == null)
        {
            Debug.LogError("[WorldSceneController] CreateMountain: parent is null!");
            return;
        }
        
        var mountain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (mountain != null && mountain.transform != null && parent != null)
        {
            mountain.transform.SafeSetParent(parent);
        }
        else
        {
            Debug.LogError("[WorldSceneController] CreateMountain: mountain or parent is null!");
            return;
        }
        mountain.transform.position = position;
        mountain.transform.localScale = new Vector3(height, height, height);
        mountain.GetComponent<MeshRenderer>().material = URPMaterialFactory.Create(new Color(0.55f, 0.58f, 0.65f), 0.2f);

        var snow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        if (snow != null && snow.transform != null && mountain != null && mountain.transform != null)
        {
            snow.transform.SafeSetParent(mountain.transform);
        }
        else
        {
            Debug.LogError("[WorldSceneController] CreateMountain: snow or mountain transform is null!");
            return;
        }
        snow.transform.localPosition = new Vector3(0, height, 0);
        snow.transform.localScale = new Vector3(0.8f, 0.4f, 0.8f);
        snow.GetComponent<MeshRenderer>().material = URPMaterialFactory.Create(new Color(0.95f, 0.97f, 1f), 0.5f);
    }
}

