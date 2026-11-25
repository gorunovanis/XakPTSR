using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Video;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class VRPTSD_AutoSetup : MonoBehaviour
{

    EnvironmentController CreateEnvironmentController(Camera cam)
    {
        var go = new GameObject("EnvironmentController");
        var env = go.AddComponent<EnvironmentController>();
        env.mainCamera = cam;
        env.mainLight = FindFirstObjectByType<Light>(); // возьмём первый Directional Light в сцене
        env.baseFogDensity = 0.03f;

        // стартуем с нейтральной сцены
        env.ApplyPreset(EnvironmentPreset.Neutral);
        return env;
    }


    void Awake()
    {
        // Чтобы не создавать всё по сто раз
        if (UnityEngine.Object.FindFirstObjectByType<SessionManager>() != null)
        {
            Debug.Log("[AutoSetup] Session already exists, пропускаю создание.");
            return;
        }

        Debug.Log("[AutoSetup] Создаю сцену VR PTSD прототипа...");

        // Проверяем, находимся ли мы в стартовой сцене меню
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isStartupMenuScene = currentSceneName == "StartupMenu" || currentSceneName == "SampleScene";
        Debug.Log($"[AutoSetup] Current scene: {currentSceneName}, isStartupMenuScene: {isStartupMenuScene}");

        // Камера
        var cam = Camera.main;
        if (!cam)
        {
            Debug.Log("[AutoSetup] Camera.main not found, creating new camera...");
            GameObject camGO = new GameObject("Main Camera");
            cam = camGO.AddComponent<Camera>();
            cam.transform.position = new Vector3(0, 1.6f, -3f);
            cam.tag = "MainCamera";
            Debug.Log("[AutoSetup] New camera created");
        }
        else
        {
            Debug.Log($"[AutoSetup] Found Camera.main: {cam.name}, Active: {cam.gameObject.activeInHierarchy}, Enabled: {cam.enabled}");
        }

        if (cam == null)
        {
            Debug.LogError("[AutoSetup] Camera is null! Cannot continue.");
            return;
        }
        
        // Убеждаемся, что камера активна
        if (!cam.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[AutoSetup] Camera {cam.name} is not active! Activating...");
            cam.gameObject.SetActive(true);
        }
        
        if (!cam.enabled)
        {
            Debug.LogWarning($"[AutoSetup] Camera {cam.name} is disabled! Enabling...");
            cam.enabled = true;
        }
        
        // Устанавливаем полностью черный фон для стартового меню
        // ПРИНУДИТЕЛЬНО устанавливаем черный фон, переопределяя настройки сцены
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black; // Полностью черный (R=0, G=0, B=0, A=1)
        
        // Дополнительно: отключаем все эффекты камеры
        cam.cullingMask = -1; // Показываем все слои, но объекты будут скрыты через SetActive
        cam.farClipPlane = 1000f;
        cam.nearClipPlane = 0.1f;
        
        // ---------- ОТКЛЮЧАЕМ ТУМАН И SKYBOX ДЛЯ ЧЕРНОГО ФОНА ----------
        // ПРИНУДИТЕЛЬНО устанавливаем черный фон в Render Settings
        RenderSettings.fog = false;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.skybox = null; // Отключаем Skybox для черного фона
        
        // Принудительно обновляем камеру после всех настроек
        cam.enabled = false;
        cam.enabled = true;
        
        Debug.Log($"[AutoSetup] Camera configured - Clear Flags: {cam.clearFlags}, Background: {cam.backgroundColor}, isStartupMenuScene: {isStartupMenuScene}");
        Debug.Log($"[AutoSetup] RenderSettings - Fog: {RenderSettings.fog}, Skybox: {RenderSettings.skybox}, AmbientLight: {RenderSettings.ambientLight}");
        
        // Отключаем все источники света в стартовой сцене
        if (isStartupMenuScene)
        {
            var allLights = UnityEngine.Object.FindObjectsByType<Light>(UnityEngine.FindObjectsSortMode.None);
            foreach (var light in allLights)
            {
                light.enabled = false;
                Debug.Log($"[AutoSetup] Disabled light: {light.name}");
            }
        }

        // Создаём основные модули (в стартовой сцене создаем, но сразу скрываем все объекты)
        VRExposureController exposureCtrl = null;
        EMDRController emdrCtrl = null;
        SafePlaceController safePlaceCtrl = null;
        
        if (!isStartupMenuScene)
        {
            // Создаем модули только если это НЕ стартовая сцена меню
            exposureCtrl = CreateExposureModule(cam);
            emdrCtrl = CreateEMDRModule();
            safePlaceCtrl = CreateSafePlaceModule(cam);
        }
        else
        {
            // В стартовой сцене создаем пустые контроллеры без объектов
            Debug.Log("[AutoSetup] Startup menu scene - creating empty controllers without objects");
        }
        
        var pauseMenuCtrl = CreatePauseCanvas(cam);
        var worldSceneController = EnsureWorldSceneController();
        pauseMenuCtrl.worldSceneController = worldSceneController;

        var sessionGO = new GameObject("SessionManager");
        var session = sessionGO.AddComponent<SessionManager>();

        if (exposureCtrl != null) session.exposureController = exposureCtrl;
        if (emdrCtrl != null) session.emdrController = emdrCtrl;
        if (safePlaceCtrl != null) session.safePlaceController = safePlaceCtrl;
        session.pauseMenuController = pauseMenuCtrl;


        Debug.Log("[AutoSetup] Готово. Нажми Play — базовая сцена собрана.");

        // Камера создана -> настраиваем окружение
        // В стартовой сцене меню НЕ создаем объекты окружения
        WorldEnvironmentController worldEnvCtrl = null;
        EnvironmentController envCtrl = null;
        
        if (!isStartupMenuScene)
        {
            // Создаем окружение только если это НЕ стартовая сцена меню
            envCtrl = CreateEnvironmentController(cam);
            worldEnvCtrl = CreateWorldEnvironmentController();
            worldSceneController.environmentController = worldEnvCtrl;
            worldSceneController.atmosphereController = envCtrl;
        }
        else
        {
            // В стартовой сцене создаем пустой контроллер, но не создаем объекты
            Debug.Log("[AutoSetup] Startup menu scene detected - skipping environment creation");
            var root = new GameObject("WorldEnvironments");
            worldEnvCtrl = root.AddComponent<WorldEnvironmentController>();
            worldEnvCtrl.forestEnvironment = null;
            worldEnvCtrl.seaEnvironment = null;
            worldEnvCtrl.mountainEnvironment = null;
            worldSceneController.environmentController = worldEnvCtrl;
        }
        
        // Скрываем все созданные модули, чтобы был только черный фон
        // В стартовой сцене это особенно важно
        if (exposureCtrl != null)
        {
            if (exposureCtrl.exposureSphere != null)
            {
                exposureCtrl.exposureSphere.SetActive(false);
                // Скрываем все дочерние объекты
                foreach (Transform child in exposureCtrl.exposureSphere.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
        if (emdrCtrl != null)
        {
            if (emdrCtrl.visualTarget != null)
            {
                emdrCtrl.visualTarget.gameObject.SetActive(false);
            }
            if (emdrCtrl.leftPoint != null) emdrCtrl.leftPoint.gameObject.SetActive(false);
            if (emdrCtrl.rightPoint != null) emdrCtrl.rightPoint.gameObject.SetActive(false);
        }
        if (safePlaceCtrl != null)
        {
            // Скрываем все сцены Safe Place
            if (safePlaceCtrl.forestScene != null)
            {
                safePlaceCtrl.forestScene.SetActive(false);
                foreach (Transform child in safePlaceCtrl.forestScene.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
            if (safePlaceCtrl.seaScene != null)
            {
                safePlaceCtrl.seaScene.SetActive(false);
                foreach (Transform child in safePlaceCtrl.seaScene.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
            if (safePlaceCtrl.mountainScene != null)
            {
                safePlaceCtrl.mountainScene.SetActive(false);
                foreach (Transform child in safePlaceCtrl.mountainScene.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
            if (safePlaceCtrl.neutralScene != null)
            {
                safePlaceCtrl.neutralScene.SetActive(false);
            }
            // Скрываем Canvas для дыхания
            var safePlaceCanvas = GameObject.Find("SafePlaceCanvas");
            if (safePlaceCanvas != null)
            {
                safePlaceCanvas.SetActive(false);
            }
        }
        
        // Дополнительно: скрываем все объекты с определенными именами
        if (isStartupMenuScene)
        {
            HideAllObjectsWithNames(new[] { "ForestEnvironment", "SeaEnvironment", "MountainEnvironment", 
                "ExposureSphere", "EMDR_Target", "EMDR_LeftPoint", "EMDR_RightPoint",
                "ForestScene", "SeaScene", "MountainScene", "SafePlaceCanvas", "WorldEnvironments" });
        }

        // Создаём стример для трансляции изображения в пульт терапевта
        var streamGO = new GameObject("VRStreamController");
        var streamCtrl = streamGO.AddComponent<VRStreamController>();
        streamCtrl.streamCamera = cam;
        streamCtrl.streamPort = 9877;
        streamCtrl.targetFPS = 15;
        streamCtrl.streamWidth = 640;
        streamCtrl.streamHeight = 360;

        // Создаём контроллеры для новых режимов
        var ballTrackingGO = new GameObject("BallTrackingController");
        var ballTrackingCtrl = ballTrackingGO.AddComponent<BallTrackingController>();
        ballTrackingCtrl.mainCamera = cam;
        worldSceneController.ballTrackingController = ballTrackingCtrl;

        var warModeGO = new GameObject("WarModeController");
        var warModeCtrl = warModeGO.AddComponent<WarModeController>();
        warModeCtrl.mainCamera = cam;
        worldSceneController.warModeController = warModeCtrl;

        // Настраиваем XR для взаимодействий ПЕРЕД созданием меню
        Debug.Log("[AutoSetup] Setting up XR interactions...");
        EnsureXRInteractionManager();
        
        // Убеждаемся, что EventSystem правильно настроен для VR
        EnsureVREventSystem();
        
        // Добавляем управление камерой мышью
        EnsureMouseLook(cam);
        
        Debug.Log("[AutoSetup] XR interactions setup complete");
        
        var startupMenu = CreateStartupMenu(cam, worldSceneController, worldEnvCtrl, pauseMenuCtrl);
        
        // Добавляем принудительный контроллер черного фона для стартовой сцены
        if (isStartupMenuScene)
        {
            var blackBgEnforcerGO = new GameObject("BlackBackgroundEnforcer");
            blackBgEnforcerGO.AddComponent<BlackBackgroundEnforcer>();
        }
        
        // В стартовой сцене меню сразу показываем меню
        if (isStartupMenuScene)
        {
            // ФИНАЛЬНАЯ ПРОВЕРКА: еще раз принудительно устанавливаем черный фон
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            RenderSettings.fog = false;
            RenderSettings.ambientLight = Color.black;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.skybox = null;
            
            // Принудительно обновляем камеру
            cam.enabled = false;
            cam.enabled = true;
            
            startupMenu.ShowMenu();
            Debug.Log("[AutoSetup] Startup menu shown on black background - no environment objects created");
            
            // Финальная проверка - скрываем все возможные объекты
            HideAllObjectsWithNames(new[] { "ForestEnvironment", "SeaEnvironment", "MountainEnvironment", 
                "ExposureSphere", "EMDR_Target", "EMDR_LeftPoint", "EMDR_RightPoint",
                "ForestScene", "SeaScene", "MountainScene", "SafePlaceCanvas", "WorldEnvironments",
                "ExposureController", "EMDRController", "SafePlaceController" });
        }
    }
    
    void HideAllObjectsWithNames(string[] names)
    {
        foreach (string name in names)
        {
            var obj = GameObject.Find(name);
            if (obj != null)
            {
                obj.SetActive(false);
                Debug.Log($"[AutoSetup] Hidden object: {name}");
            }
        }
        
        // Также скрываем все объекты с определенными именами в иерархии
        var allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(UnityEngine.FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            // Скрываем все объекты окружения, но не меню
            if (obj.name.Contains("Forest") || obj.name.Contains("Sea") || obj.name.Contains("Mountain") ||
                obj.name.Contains("Environment") || obj.name.Contains("Exposure") || 
                obj.name.Contains("EMDR") || obj.name.Contains("SafePlace"))
            {
                // Не скрываем объекты меню
                bool shouldHide = true;
                
                // Проверяем, является ли объект частью меню
                if (obj.transform != null && obj.transform.parent != null)
                {
                    // Если parent - это меню, не скрываем
                    if (obj.transform.parent.name == "StartupMenuCanvas" || 
                        obj.transform.parent.name == "StartupPanel")
                    {
                        shouldHide = false;
                    }
                }
                
                // Проверяем, является ли объект дочерним элементом StartupMenuCanvas
                if (shouldHide)
                {
                    var menuCanvas = GameObject.Find("StartupMenuCanvas");
                    if (menuCanvas != null && menuCanvas.transform != null && obj.transform != null)
                    {
                        if (obj.transform.IsChildOf(menuCanvas.transform))
                        {
                            shouldHide = false;
                        }
                    }
                }
                
                if (shouldHide)
                {
                    obj.SetActive(false);
                    Debug.Log($"[AutoSetup] Hidden object by name pattern: {obj.name}");
                }
            }
        }
    }

    // ------------------ МОДУЛЬ ЭКСПОЗИЦИИ ------------------

    VRExposureController CreateExposureModule(Camera cam)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "ExposureSphere";
        Object.DestroyImmediate(sphere.GetComponent<Collider>());
        sphere.transform.position = Vector3.zero;
        sphere.transform.localScale = new Vector3(-10, 10, 10); // инвертируем по X

        GameObject exposureGO = new GameObject("ExposureController");
        exposureGO.transform.position = Vector3.zero;

        var videoPlayer = exposureGO.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = true;
        videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
        videoPlayer.targetMaterialRenderer = sphere.GetComponent<MeshRenderer>();
        videoPlayer.targetMaterialProperty = "_MainTex";

        var audioSource = exposureGO.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        var ctrl = exposureGO.AddComponent<VRExposureController>();
        ctrl.videoPlayer = videoPlayer;
        ctrl.audioSource = audioSource;
        ctrl.exposureSphere = sphere;
        ctrl.pauseTimecodes = new float[] { 10f, 20f, 30f };

        return ctrl;
    }

    // ------------------ МОДУЛЬ EMDR ------------------

    EMDRController CreateEMDRModule()
    {
        GameObject emdrGO = new GameObject("EMDRController");
        var emdr = emdrGO.AddComponent<EMDRController>();

        GameObject left = new GameObject("EMDR_LeftPoint");
        GameObject right = new GameObject("EMDR_RightPoint");
        GameObject target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Object.DestroyImmediate(target.GetComponent<Collider>());
        target.name = "EMDR_Target";
        target.transform.localScale = Vector3.one * 0.1f;

        left.transform.position = new Vector3(-1.5f, 1.6f, 0f);
        right.transform.position = new Vector3(1.5f, 1.6f, 0f);
        target.transform.position = new Vector3(0f, 1.6f, 0f);

        emdr.leftPoint = left.transform;
        emdr.rightPoint = right.transform;
        emdr.visualTarget = target.transform;

        GameObject leftAudioGO = new GameObject("EMDR_LeftAudio");
        GameObject rightAudioGO = new GameObject("EMDR_RightAudio");
        var leftAudio = leftAudioGO.AddComponent<AudioSource>();
        var rightAudio = rightAudioGO.AddComponent<AudioSource>();
        leftAudio.playOnAwake = false;
        rightAudio.playOnAwake = false;

        emdr.leftAudio = leftAudio;
        emdr.rightAudio = rightAudio;

        return emdr;
    }

    // ------------------ МОДУЛЬ SAFE PLACE ------------------

    SafePlaceController CreateSafePlaceModule(Camera cam)
    {
        GameObject safeGO = new GameObject("SafePlaceController");
        var safe = safeGO.AddComponent<SafePlaceController>();

        // ----- Простые сцены окружения -----
        safe.forestScene = CreateColoredQuad("ForestScene", new Color(0.20f, 0.35f, 0.20f), new Vector3(-3f, 0f, 5f));
        safe.seaScene = CreateColoredQuad("SeaScene", new Color(0.15f, 0.35f, 0.50f), new Vector3(0f, 0f, 5f));
        safe.mountainScene = CreateColoredQuad("MountainScene", new Color(0.35f, 0.35f, 0.40f), new Vector3(3f, 0f, 5f));
        safe.neutralScene = CreateColoredQuad("NeutralScene", Color.black, new Vector3(0f, -100f, 0f));

        // ====== ОТДЕЛЬНЫЙ WORLD-SPACE CANVAS ДЛЯ ДЫХАНИЯ ======
        GameObject canvasGO = new GameObject("SafePlaceCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = cam;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

        canvasGO.AddComponent<GraphicRaycaster>();

        // крепим к камере
        if (cam != null && cam.transform != null)
        {
            canvas.transform.SafeSetParent(cam.transform, false);
        }
        else
        {
            Debug.LogError("[CreateStartupMenu] Camera or camera transform is null!");
        }
        var rect = canvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(800, 800);     // "логический" размер
        rect.localPosition = new Vector3(0f, 0f, 1.6f); // перед глазами
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one * 0.001f;      // переводим в метры

        // ---------- Заголовок "Дыхание" ----------
        var breathingTitleGO = new GameObject("BreathingTitle");
        if (canvas != null && canvas.transform != null)
        {
            breathingTitleGO.transform.SafeSetParent(canvas.transform, false);
        }
        else
        {
            Debug.LogError("[CreateBreathingUI] Canvas or canvas transform is null!");
            return null;
        }
        var breathingText = breathingTitleGO.AddComponent<Text>();
        breathingText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        breathingText.text = "Дыхание";
        breathingText.alignment = TextAnchor.MiddleCenter;
        breathingText.color = Color.white;
        breathingText.fontSize = 80;

        var titleRect = breathingText.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 0.9f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.sizeDelta = new Vector2(600, 120);

        // ---------- Квадрат дыхания ----------
        var breathingPanelGO = new GameObject("BreathingPanel");
        if (canvas != null && canvas.transform != null)
        {
            breathingPanelGO.transform.SafeSetParent(canvas.transform, false);
        }
        else
        {
            Debug.LogError("[CreateBreathingUI] Canvas or canvas transform is null!");
            return null;
        }
        var breathingImage = breathingPanelGO.AddComponent<Image>();
        breathingImage.color = new Color(0.80f, 0.90f, 1.0f);

        var panelRect = breathingImage.rectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 0.55f);
        panelRect.anchorMax = new Vector2(0.5f, 0.55f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(450, 450);

        // ---------- Текст внутри квадрата ----------
        var labelGO = new GameObject("BreathingLabel");
        if (breathingPanelGO != null && breathingPanelGO.transform != null)
        {
            labelGO.transform.SafeSetParent(breathingPanelGO.transform, false);
        }
        else
        {
            Debug.LogError("[CreateBreathingUI] BreathingPanelGO or transform is null!");
            return null;
        }
        var label = labelGO.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = "Вдохните";
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.black;
        label.fontSize = 90;

        var labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        labelGO.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.5f);

        var breathingPanel = breathingPanelGO.AddComponent<InteractiveBreathingPanel>();
        breathingPanel.label = label;

        // ---------- Grounding-текст ----------
        var groundingGO = new GameObject("GroundingText");
        if (canvas != null && canvas.transform != null)
        {
            groundingGO.transform.SafeSetParent(canvas.transform, false);
        }
        else
        {
            Debug.LogError("[CreateBreathingUI] Canvas or canvas transform is null!");
            return null;
        }
        var groundingText = groundingGO.AddComponent<Text>();
        groundingText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        groundingText.text = "5-4-3-2-1";
        groundingText.alignment = TextAnchor.MiddleCenter;
        groundingText.color = Color.white;
        groundingText.fontSize = 80;

        var groundingRect = groundingText.rectTransform;
        groundingRect.anchorMin = new Vector2(0.5f, 0.12f);
        groundingRect.anchorMax = new Vector2(0.5f, 0.12f);
        groundingRect.sizeDelta = new Vector2(600, 120);

        // пробрасываем в SafePlaceController ссылки на UI
        safe.breathingText = breathingText;
        safe.breathingCircle = breathingImage;
        safe.groundingText = groundingText;

        return safe;
    }



    GameObject CreateColoredQuad(string name, Color color, Vector3 position)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = name;
        quad.transform.position = position;
        var mr = quad.GetComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Standard"));
        mr.material.color = color;
        return quad;
    }

    // ------------------ CANVAS ПАУЗЫ + SUD ------------------

    // ------------------ CANVAS ПАУЗЫ + SUD (VR-дружелюбный) ------------------
    // ------------------ КРАСИВАЯ ПАУЗА В VR ------------------
    PauseMenuController CreatePauseCanvas(Camera cam)
    {
        // Базовый Canvas + EventSystem
        var ui = CreateSimpleUICanvas(cam, "PauseCanvas");
        var canvas = ui.Item1;
        var rect = ui.Item2;

        // World-Space перед камерой
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = cam;

        rect.sizeDelta = new Vector2(1.5f, 0.9f);                 // ~1.5м x 0.9м
        if (cam != null && cam.transform != null)
        {
            canvas.transform.SafeSetParent(cam.transform, false);
        }
        else
        {
            Debug.LogError("[CreateStartupMenu] Camera or camera transform is null!");
        }
        canvas.transform.localPosition = new Vector3(0f, 0f, 1.6f);
        canvas.transform.localRotation = Quaternion.identity;
        canvas.transform.localScale = Vector3.one * 0.001f;

        // ---------- фон панели ----------
        var panelGO = new GameObject("PausePanel");
        
        if (panelGO == null)
        {
            Debug.LogError("[CreatePauseCanvas] PanelGO is null after creation!");
            return null;
        }
        
        if (panelGO.transform == null)
        {
            Debug.LogError("[CreatePauseCanvas] PanelGO transform is null after creation!");
            return null;
        }
        
        if (canvas != null && canvas.transform != null)
        {
            panelGO.transform.SafeSetParent(canvas.transform, false);
        }
        else
        {
            Debug.LogError("[CreatePauseCanvas] Canvas or canvas transform is null!");
            return null;
        }
        
        // Проверяем, что panelGO все еще не null после SetParent
        if (panelGO == null || panelGO.transform == null)
        {
            Debug.LogError("[CreatePauseCanvas] PanelGO or transform became null after SetParent!");
            return null;
        }
        
        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.05f, 0.07f, 0.9f);    // тёмно-серый

        var panelRect = panelImg.rectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(1200, 700);
        panelRect.anchoredPosition = Vector2.zero;

        // ---------- контроллер ----------
        var pauseCtrl = canvas.gameObject.AddComponent<PauseMenuController>();
        pauseCtrl.pauseCanvas = canvas;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // ---------- заголовок "Пауза" ----------
        var titleGO = new GameObject("PauseTitle");
        if (panelGO != null && panelGO.transform != null)
        {
            titleGO.transform.SafeSetParent(panelGO.transform, false);
        }
        else
        {
            Debug.LogError("[CreatePauseCanvas] PanelGO or panelGO transform is null!");
            return null;
        }
        var title = titleGO.AddComponent<Text>();
        title.font = font;
        title.text = "Пауза";
        title.alignment = TextAnchor.MiddleCenter;
        title.color = Color.white;
        title.fontSize = 110;

        var titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 0.9f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = new Vector2(600, 140);
        titleRect.anchoredPosition = Vector2.zero;

        // ---------- строка SUD ----------
        // подпись
        var sudLabelGO = new GameObject("SUDLabel");
        if (panelGO != null && panelGO.transform != null)
        {
            sudLabelGO.transform.SafeSetParent(panelGO.transform, false);
        }
        else
        {
            Debug.LogError("[CreatePauseCanvas] PanelGO or panelGO transform is null!");
            return null;
        }
        var sudLabel = sudLabelGO.AddComponent<Text>();
        sudLabel.font = font;
        sudLabel.text = "SUD";
        sudLabel.alignment = TextAnchor.MiddleLeft;
        sudLabel.color = Color.white;
        sudLabel.fontSize = 90;

        var sudLabelRect = sudLabel.rectTransform;
        sudLabelRect.anchorMin = new Vector2(0.12f, 0.70f);
        sudLabelRect.anchorMax = sudLabelRect.anchorMin;
        sudLabelRect.pivot = new Vector2(0.5f, 0.5f);
        sudLabelRect.sizeDelta = new Vector2(200, 100);
        sudLabelRect.anchoredPosition = Vector2.zero;

        // число SUD справа
        var sudValueGO = new GameObject("SUDValue");
        if (panelGO != null && panelGO.transform != null)
        {
            sudValueGO.transform.SafeSetParent(panelGO.transform, false);
        }
        else
        {
            Debug.LogError("[CreatePauseCanvas] PanelGO or panelGO transform is null!");
            return null;
        }
        var sudValue = sudValueGO.AddComponent<Text>();
        sudValue.font = font;
        sudValue.text = "0";
        sudValue.alignment = TextAnchor.MiddleRight;
        sudValue.color = Color.white;
        sudValue.fontSize = 90;

        var sudValueRect = sudValue.rectTransform;
        sudValueRect.anchorMin = new Vector2(0.88f, 0.70f);
        sudValueRect.anchorMax = sudValueRect.anchorMin;
        sudValueRect.pivot = new Vector2(0.5f, 0.5f);
        sudValueRect.sizeDelta = new Vector2(200, 100);
        sudValueRect.anchoredPosition = Vector2.zero;

        // сам слайдер по центру
        var sliderGO = new GameObject("SUDSlider");
        if (panelGO != null && panelGO.transform != null)
        {
            sliderGO.transform.SafeSetParent(panelGO.transform, false);
        }
        else
        {
            Debug.LogError("[CreatePauseCanvas] PanelGO or panelGO transform is null!");
            return null;
        }
        var sliderBg = sliderGO.AddComponent<Image>();
        sliderBg.color = Color.white;
        var slider = sliderGO.AddComponent<Slider>();

        var sliderRect = sliderGO.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.63f);
        sliderRect.anchorMax = sliderRect.anchorMin;
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.sizeDelta = new Vector2(800, 40);
        sliderRect.anchoredPosition = Vector2.zero;

        pauseCtrl.sudSlider = slider;
        pauseCtrl.sudValueText = sudValue;

        // ---------- ряд кнопок EMDR / Safe Place / Exit ----------
        CreateVRButton(panelGO.transform, "EMDR",
            new Vector2(0.2f, 0.40f), new Vector2(260, 90), font, pauseCtrl.OnEMDRButton);

        CreateVRButton(panelGO.transform, "Safe place",
            new Vector2(0.5f, 0.40f), new Vector2(260, 90), font, pauseCtrl.OnSafePlaceButton);

        CreateVRButton(panelGO.transform, "Exit",
            new Vector2(0.8f, 0.40f), new Vector2(260, 90), font, pauseCtrl.OnExitButton);

        // ---------- большая красная STOP ----------
        CreateVRButton(panelGO.transform, "STOP!",
            new Vector2(0.5f, 0.18f), new Vector2(600, 110), font,
            pauseCtrl.OnEmergencyStopButton,
            new Color(0.9f, 0.1f, 0.1f),    // фон
            Color.white,                    // текст
            100);                           // шрифт

        var worldToggleButton = CreateVRButton(panelGO.transform, "Выбрать мир",
            new Vector2(0.5f, 0.27f), new Vector2(360, 90), font,
            pauseCtrl.OnWorldPanelToggleButton, new Color(0.15f, 0.2f, 0.25f));

        CreateVRButton(panelGO.transform, "Меню",
            new Vector2(0.82f, 0.27f), new Vector2(220, 80), font,
            pauseCtrl.OnStartupMenuButton, new Color(0.25f, 0.25f, 0.33f));

        var worldPanelGO = new GameObject("WorldOptionsPanel");
        
        if (worldPanelGO == null)
        {
            Debug.LogError("[CreatePauseCanvas] WorldPanelGO is null after creation!");
            return null;
        }
        
        if (worldPanelGO.transform == null)
        {
            Debug.LogError("[CreatePauseCanvas] WorldPanelGO transform is null after creation!");
            return null;
        }
        
        if (panelGO != null && panelGO.transform != null)
        {
            worldPanelGO.transform.SafeSetParent(panelGO.transform, false);
        }
        else
        {
            Debug.LogError("[CreatePauseCanvas] PanelGO or panelGO transform is null!");
            return null;
        }
        
        // Проверяем, что worldPanelGO все еще не null после SetParent
        if (worldPanelGO == null || worldPanelGO.transform == null)
        {
            Debug.LogError("[CreatePauseCanvas] WorldPanelGO or transform became null after SetParent!");
            return null;
        }
        
        var worldPanelImg = worldPanelGO.AddComponent<Image>();
        worldPanelImg.color = new Color(0.08f, 0.08f, 0.1f, 0.85f);

        var worldPanelRect = worldPanelImg.rectTransform;
        worldPanelRect.anchorMin = new Vector2(0.5f, 0.10f);
        worldPanelRect.anchorMax = new Vector2(0.5f, 0.10f);
        worldPanelRect.pivot = new Vector2(0.5f, 0.5f);
        worldPanelRect.sizeDelta = new Vector2(700, 180);
        worldPanelRect.anchoredPosition = Vector2.zero;

        // Проверяем worldPanelGO перед созданием кнопок
        if (worldPanelGO == null || worldPanelGO.transform == null)
        {
            Debug.LogError("[CreatePauseMenu] WorldPanelGO or transform is null before creating buttons!");
            return null;
        }

        CreateVRButton(worldPanelGO.transform, "Лес", new Vector2(0.2f, 0.5f),
            new Vector2(200, 80), font, pauseCtrl.OnForestWorldButton, new Color(0.14f, 0.3f, 0.18f));

        CreateVRButton(worldPanelGO.transform, "Море", new Vector2(0.5f, 0.5f),
            new Vector2(200, 80), font, pauseCtrl.OnSeaWorldButton, new Color(0.10f, 0.25f, 0.35f));

        CreateVRButton(worldPanelGO.transform, "Горы", new Vector2(0.8f, 0.5f),
            new Vector2(200, 80), font, pauseCtrl.OnMountainWorldButton, new Color(0.25f, 0.25f, 0.32f));

        worldPanelGO.SetActive(false);
        pauseCtrl.worldOptionsPanel = worldPanelGO;

        // по умолчанию пауза скрыта
        canvas.enabled = false;

        return pauseCtrl;
    }

    StartupMenuController CreateStartupMenu(Camera cam, WorldSceneController worldController, WorldEnvironmentController envController, PauseMenuController pauseMenu)
    {
        Debug.Log("[CreateStartupMenu] Starting to create startup menu...");
        var ui = CreateSimpleUICanvas(cam, "StartupMenuCanvas");
        var canvas = ui.Item1;
        var rect = ui.Item2;
        
        if (canvas == null)
        {
            Debug.LogError("[CreateStartupMenu] Canvas is null after CreateSimpleUICanvas!");
            return null;
        }
        
        Debug.Log($"[CreateStartupMenu] Canvas created: {canvas.gameObject.name}, active: {canvas.gameObject.activeSelf}");

        // Используем WorldSpace для отображения на две линзы (как в VR)
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = cam;
        
        // Крепим Canvas к камере
        if (cam != null && cam.transform != null)
        {
            canvas.transform.SafeSetParent(cam.transform, false);
        }
        
        // Настраиваем размер и позицию Canvas перед камерой
        rect.sizeDelta = new Vector2(1.5f, 0.9f); // Размер в метрах
        rect.localPosition = new Vector3(0f, 0f, 1.6f); // Перед камерой
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one * 0.001f; // Масштаб для WorldSpace
        
        // Убеждаемся, что есть GraphicRaycaster для работы с мышью на WorldSpace Canvas
        var existingRaycaster = canvas.gameObject.GetComponent<GraphicRaycaster>();
        if (existingRaycaster == null)
        {
            var graphicRaycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
            graphicRaycaster.enabled = true;
            Debug.Log("[CreateStartupMenu] Added GraphicRaycaster for WorldSpace Canvas");
        }
        else
        {
            existingRaycaster.enabled = true;
            Debug.Log("[CreateStartupMenu] GraphicRaycaster already exists and is enabled");
        }
        
        // Удаляем TrackedDeviceGraphicRaycaster - он не нужен для мыши
        var existingTrackedRaycaster = canvas.gameObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
        if (existingTrackedRaycaster != null)
        {
            UnityEngine.Object.DestroyImmediate(existingTrackedRaycaster);
            Debug.Log("[CreateStartupMenu] Removed TrackedDeviceGraphicRaycaster");
        }
        
        // Убеждаемся, что Canvas на слое UI для правильной работы с PhysicsRaycaster
        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer == -1)
        {
            Debug.LogWarning("[CreateStartupMenu] UI layer not found! Creating it...");
            // Создаем слой UI если его нет (это должно быть сделано в настройках проекта, но на всякий случай)
        }
        else
        {
            canvas.gameObject.layer = uiLayer;
        }
        
        // Добавляем PhysicsRaycaster к камере для работы с WorldSpace UI мышью
        var physicsRaycaster = cam.GetComponent<PhysicsRaycaster>();
        if (physicsRaycaster == null)
        {
            physicsRaycaster = cam.gameObject.AddComponent<PhysicsRaycaster>();
        }
        // Устанавливаем eventMask на слой UI
        if (uiLayer != -1)
        {
            physicsRaycaster.eventMask = 1 << uiLayer; // Только слой UI
        }
        else
        {
            physicsRaycaster.eventMask = -1; // Все слои, если UI слой не найден
        }
        physicsRaycaster.enabled = true;
        
        // Убеждаемся, что EventSystem правильно настроен для мыши
        var eventSystem = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            // Создаем EventSystem если его нет
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystem = eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
            Debug.Log("[CreateStartupMenu] Created EventSystem for mouse input");
        }
        
        if (eventSystem != null)
        {
            // Отключаем XRUIInputModule - он мешает работе мыши
            var xrUIInput = eventSystem.GetComponent<XRUIInputModule>();
            if (xrUIInput != null)
            {
                xrUIInput.enabled = false;
                Debug.Log("[CreateStartupMenu] Disabled XRUIInputModule");
            }
            
            // Убеждаемся, что StandaloneInputModule активен
            var standaloneInput = eventSystem.GetComponent<StandaloneInputModule>();
            if (standaloneInput == null)
            {
                standaloneInput = eventSystem.gameObject.AddComponent<StandaloneInputModule>();
                Debug.Log("[CreateStartupMenu] Added StandaloneInputModule");
            }
            standaloneInput.enabled = true;
            
            // Убеждаемся, что EventSystem активен
            if (!eventSystem.gameObject.activeSelf)
            {
                eventSystem.gameObject.SetActive(true);
            }
            if (!eventSystem.enabled)
            {
                eventSystem.enabled = true;
            }
            
            Debug.Log($"[CreateStartupMenu] EventSystem configured - StandaloneInputModule enabled: {standaloneInput.enabled}, EventSystem enabled: {eventSystem.enabled}");
        }

        var panelGO = new GameObject("StartupPanel");
        
        if (panelGO == null)
        {
            Debug.LogError("[CreateStartupMenu] PanelGO is null after creation!");
            return null;
        }
        
        if (panelGO.transform == null)
        {
            Debug.LogError("[CreateStartupMenu] PanelGO transform is null after creation!");
            return null;
        }
        
        if (canvas != null && canvas.transform != null)
        {
            panelGO.transform.SafeSetParent(canvas.transform, false);
        }
        else
        {
            Debug.LogError("[CreateStartupMenu] Canvas or canvas transform is null!");
            return null;
        }
        
        // Проверяем, что panelGO все еще не null после SetParent
        if (panelGO == null || panelGO.transform == null)
        {
            Debug.LogError("[CreateStartupMenu] PanelGO or transform became null after SetParent!");
            return null;
        }
        
        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.92f);

        var panelRect = panelImg.rectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(1300, 900);
        panelRect.anchoredPosition = Vector2.zero;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var titleGO = new GameObject("Title");
        if (panelGO != null && panelGO.transform != null)
        {
            titleGO.transform.SafeSetParent(panelGO.transform, false);
        }
        else
        {
            Debug.LogError("[CreateStartupMenu] PanelGO or panelGO transform is null!");
            return null;
        }
        var title = titleGO.AddComponent<Text>();
        title.font = font;
        title.text = "Добро пожаловать";
        title.alignment = TextAnchor.MiddleCenter;
        title.color = Color.white;
        title.fontSize = 80; // Уменьшено с 140

        var titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 0.88f);
        titleRect.anchorMax = new Vector2(0.5f, 0.88f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = new Vector2(800, 120); // Уменьшено
        titleRect.anchoredPosition = Vector2.zero;

        var worldLabelGO = new GameObject("WorldLabel");
        if (panelGO != null && panelGO.transform != null)
        {
            worldLabelGO.transform.SafeSetParent(panelGO.transform, false);
        }
        else
        {
            Debug.LogError("[CreateStartupMenu] PanelGO or panelGO transform is null!");
            return null;
        }
        var worldLabel = worldLabelGO.AddComponent<Text>();
        worldLabel.font = font;
        worldLabel.text = "Выберите мир";
        worldLabel.alignment = TextAnchor.MiddleCenter;
        worldLabel.color = Color.white;
        worldLabel.fontSize = 110;

        var worldLabelRect = worldLabel.rectTransform;
        worldLabelRect.anchorMin = new Vector2(0.5f, 0.72f);
        worldLabelRect.anchorMax = new Vector2(0.5f, 0.72f);
        worldLabelRect.pivot = new Vector2(0.5f, 0.5f);
        worldLabelRect.sizeDelta = new Vector2(900, 140);
        worldLabelRect.anchoredPosition = Vector2.zero;

        var selectionLabelGO = new GameObject("SelectionLabel");
        if (panelGO != null && panelGO.transform != null)
        {
            selectionLabelGO.transform.SafeSetParent(panelGO.transform, false);
        }
        else
        {
            Debug.LogError("[CreateStartupMenu] PanelGO or panelGO transform is null!");
            return null;
        }
        var selectionLabel = selectionLabelGO.AddComponent<Text>();
        selectionLabel.font = font;
        selectionLabel.text = "Выбрано: ЛЕС";
        selectionLabel.alignment = TextAnchor.MiddleCenter;
        selectionLabel.color = Color.white;
        selectionLabel.fontSize = 85;

        var selectionRect = selectionLabel.rectTransform;
        selectionRect.anchorMin = new Vector2(0.5f, 0.50f);
        selectionRect.anchorMax = new Vector2(0.5f, 0.50f);
        selectionRect.pivot = new Vector2(0.5f, 0.5f);
        selectionRect.sizeDelta = new Vector2(750, 130);
        selectionRect.anchoredPosition = Vector2.zero;

        var startupCtrl = canvas.gameObject.AddComponent<StartupMenuController>();
        startupCtrl.menuCanvas = canvas;
        startupCtrl.menuRoot = panelGO;
        startupCtrl.pauseMenuController = pauseMenu;
        startupCtrl.worldSceneController = worldController;
        startupCtrl.environmentController = envController;
        startupCtrl.selectedWorldLabel = selectionLabel;

        // Проверяем panelGO перед созданием кнопок
        if (panelGO == null || panelGO.transform == null)
        {
            Debug.LogError("[CreateStartupMenu] PanelGO or transform is null before creating buttons!");
            return null;
        }

        // ---------- Кнопки верхнего ряда (миры) ----------
        CreateVRButton(panelGO.transform, "Лес",
            new Vector2(0.25f, 0.60f), new Vector2(200, 80),
            font, startupCtrl.OnForestButton, new Color(0.14f, 0.3f, 0.18f),
            Color.white, 60);

        CreateVRButton(panelGO.transform, "Море",
            new Vector2(0.75f, 0.60f), new Vector2(200, 80),
            font, startupCtrl.OnSeaButton, new Color(0.10f, 0.25f, 0.35f),
            Color.white, 60);

        // ---------- Кнопки специальных режимов ----------
        CreateVRButton(panelGO.transform, "Безопасное место",
            new Vector2(0.25f, 0.38f), new Vector2(240, 80),
            font, startupCtrl.OnSafePlaceButton, new Color(0.3f, 0.2f, 0.4f),
            Color.white, 55);

        CreateVRButton(panelGO.transform, "Режим войны",
            new Vector2(0.75f, 0.38f), new Vector2(240, 80),
            font, startupCtrl.OnWarModeButton, new Color(0.6f, 0.1f, 0.1f),
            Color.white, 55);

        // ---------- "Начать" ----------
        CreateVRButton(panelGO.transform, "Начать",
            new Vector2(0.5f, 0.22f), new Vector2(300, 100),
            font, startupCtrl.OnStartButton, new Color(0.2f, 0.6f, 0.35f),
            Color.white, 70);

        // ---------- "Выход" ----------
        CreateVRButton(panelGO.transform, "Выход",
            new Vector2(0.5f, 0.10f), new Vector2(220, 75),
            font, startupCtrl.OnQuitButton, new Color(0.4f, 0.12f, 0.12f),
            Color.white, 55);

        return startupCtrl;
    }



    // ------------------ ВСПОМОГАТЕЛЬНЫЕ UI-ФУНКЦИИ ------------------

    (Canvas, RectTransform) CreateSimpleUICanvas(Camera cam, string name)
    {
        Debug.Log($"[CreateSimpleUICanvas] Creating canvas: {name}");
        GameObject canvasGO = new GameObject(name);
        DontDestroyOnLoad(canvasGO); // Не уничтожать при загрузке сцен
        // Убеждаемся, что Canvas на слое UI для VR взаимодействий
        canvasGO.layer = LayerMask.NameToLayer("UI");
        var canvas = canvasGO.AddComponent<Canvas>();
        Debug.Log($"[CreateSimpleUICanvas] Canvas GameObject created: {canvasGO.name}, active: {canvasGO.activeSelf}");
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.worldCamera = cam;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Создаем или находим EventSystem для VR
        EventSystem eventSystem = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.Log("[CreateSimpleUICanvas] Creating EventSystem...");
            GameObject esGO = new GameObject("EventSystem");
            DontDestroyOnLoad(esGO); // Не уничтожать при загрузке сцен
            eventSystem = esGO.AddComponent<EventSystem>();
            Debug.Log($"[CreateSimpleUICanvas] EventSystem created: {esGO.name}, active: {esGO.activeSelf}");
            
            // Для работы с мышью используем ТОЛЬКО StandaloneInputModule
            var standaloneInput = esGO.AddComponent<StandaloneInputModule>();
            if (standaloneInput != null)
            {
                standaloneInput.enabled = true;
                Debug.Log("[CreateSimpleUICanvas] Added StandaloneInputModule for mouse interaction");
            }
        }
        else
        {
            // Проверяем, есть ли модули ввода
            var existingXRUI = eventSystem.GetComponent<XRUIInputModule>();
            var existingStandalone = eventSystem.GetComponent<StandaloneInputModule>();
            
            // Отключаем XRUIInputModule если он есть
            if (existingXRUI != null)
            {
                existingXRUI.enabled = false;
                Debug.Log("[CreateSimpleUICanvas] Disabled XRUIInputModule - using mouse input");
            }
            
            // Убеждаемся, что StandaloneInputModule есть и активен
            if (existingStandalone == null)
            {
                existingStandalone = eventSystem.gameObject.AddComponent<StandaloneInputModule>();
                Debug.Log("[CreateSimpleUICanvas] Added StandaloneInputModule to existing EventSystem");
            }
            existingStandalone.enabled = true;
            Debug.Log("[CreateSimpleUICanvas] StandaloneInputModule is enabled for mouse clicking");
        }
        
        // НЕ создаем XR Interaction Manager - работаем только с мышью
        // EnsureXRInteractionManager(); // Отключено - используем только мышь

        var rect = canvas.GetComponent<RectTransform>();
        return (canvas, rect);
    }

    void CreateButton(Transform parent, string text, Vector2 normalizedPos,
                      UnityEngine.Events.UnityAction onClick, Color? color = null)
    {
        if (parent == null)
        {
            Debug.LogError($"[CreateButton] Parent is null for button '{text}'!");
            return;
        }
        
        GameObject btnGO = new GameObject(text + "Button");
        if (btnGO == null || btnGO.transform == null)
        {
            Debug.LogError($"[CreateButton] Button GameObject or transform is null for '{text}'!");
            return;
        }
        
        btnGO.transform.SafeSetParent(parent, false);

        var img = btnGO.AddComponent<Image>();
        img.color = color ?? Color.white;

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(normalizedPos.x / 1920f + 0.5f, normalizedPos.y);
        rt.anchorMax = rt.anchorMin;
        rt.sizeDelta = new Vector2(160, 40);

        GameObject txtGO = new GameObject("Text");
        if (txtGO == null || txtGO.transform == null || btnGO == null || btnGO.transform == null)
        {
            Debug.LogError($"[CreateButton] Text GameObject, text transform, button, or button transform is null for '{text}'!");
            return;
        }
        
        txtGO.transform.SafeSetParent(btnGO.transform, false);
        var txt = txtGO.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = text;
        txt.alignment = TextAnchor.MiddleCenter;

        RectTransform trt = txtGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 0);
        trt.anchorMax = new Vector2(1, 1);
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
    }

    WorldSceneController EnsureWorldSceneController()
    {
#if UNITY_2023_1_OR_NEWER
        var existing = Object.FindFirstObjectByType<WorldSceneController>();
#else
        var existing = Object.FindFirstObjectByType<WorldSceneController>();
#endif
        if (existing != null)
            return existing;

        var go = new GameObject("WorldSceneController");
        return go.AddComponent<WorldSceneController>();
    }

    WorldEnvironmentController CreateWorldEnvironmentController()
    {
        Debug.Log("[AutoSetup] Creating WorldEnvironmentController...");
        var root = new GameObject("WorldEnvironments");
        
        if (root == null)
        {
            Debug.LogError("[CreateWorldEnvironmentController] Root GameObject is null!");
            return null;
        }
        
        if (root.transform == null)
        {
            Debug.LogError("[CreateWorldEnvironmentController] Root transform is null!");
            return null;
        }
        
        var controller = root.AddComponent<WorldEnvironmentController>();

        // Создаем объекты окружения, но сразу скрываем их
        controller.forestEnvironment = CreateForestEnvironment(root.transform);
        controller.seaEnvironment = CreateSeaEnvironment(root.transform);
        controller.mountainEnvironment = CreateMountainEnvironment(root.transform);

        Debug.Log($"[AutoSetup] Environments created - Forest: {controller.forestEnvironment != null}, Sea: {controller.seaEnvironment != null}, Mountains: {controller.mountainEnvironment != null}");

        // Не показываем мир сразу - он будет показан при выборе пользователем
        controller.HideAll();
        
        // Дополнительно убеждаемся, что все объекты скрыты
        if (controller.forestEnvironment != null)
        {
            controller.forestEnvironment.SetActive(false);
            // Скрываем все дочерние объекты
            foreach (Transform child in controller.forestEnvironment.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
        if (controller.seaEnvironment != null)
        {
            controller.seaEnvironment.SetActive(false);
            foreach (Transform child in controller.seaEnvironment.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
        if (controller.mountainEnvironment != null)
        {
            controller.mountainEnvironment.SetActive(false);
            foreach (Transform child in controller.mountainEnvironment.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
        
        return controller;
    }

    GameObject CreateForestEnvironment(Transform parent)
    {
        if (parent == null)
        {
            Debug.LogError("[CreateForestEnvironment] Parent is null!");
            return null;
        }
        
        GameObject root = new GameObject("ForestEnvironment");

        if (root == null)
        {
            Debug.LogError("[CreateForestEnvironment] Root GameObject is null after creation!");
            return null;
        }
        
        if (root.transform == null)
        {
            Debug.LogError("[CreateForestEnvironment] Root transform is null after creation!");
            return null;
        }
        
        root.transform.SafeSetParent(parent, false);
        
        if (root == null || root.transform == null)
        {
            Debug.LogError("[CreateForestEnvironment] Root or root transform is null after creation!");
            return null;
        }

        var ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (ground != null && ground.transform != null && root.transform != null)
        {
            ground.transform.SafeSetParent(root.transform);
        }
        else
        {
            Debug.LogError("[CreateForestEnvironment] Ground or root transform is null!");
        }
        ground.transform.localScale = new Vector3(8f, 0.2f, 8f);
        var gm = ground.GetComponent<MeshRenderer>();
        gm.material = URPMaterialFactory.Create(new Color(0.08f, 0.25f, 0.11f), 0.1f);

        for (int i = 0; i < 18; i++)
        {
            float angle = i * Mathf.PI * 2f / 18f;
            float radius = Random.Range(2.5f, 6.5f);
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

            CreateTree(root.transform, pos,
                Random.Range(3.5f, 6f),
                new Color(0.3f, 0.18f, 0.12f),
                new Color(0.15f, 0.55f, 0.25f));
        }

        for (int i = 0; i < 6; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(1.5f, 4.5f);
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 2.5f, Mathf.Sin(angle) * radius);

            CreateGlowingOrb(root.transform, pos, new Color(0.8f, 0.9f, 0.5f));
        }

        root.SetActive(false);
        return root;
    }

    GameObject CreateSeaEnvironment(Transform parent)
    {
        if (parent == null)
        {
            Debug.LogError("[CreateSeaEnvironment] Parent is null!");
            return null;
        }
        
        GameObject root = new GameObject("SeaEnvironment");

        if (root == null)
        {
            Debug.LogError("[CreateSeaEnvironment] Root GameObject is null after creation!");
            return null;
        }
        
        if (root.transform == null)
        {
            Debug.LogError("[CreateSeaEnvironment] Root transform is null after creation!");
            return null;
        }
        
        root.transform.SafeSetParent(parent, false);
        
        if (root == null || root.transform == null)
        {
            Debug.LogError("[CreateSeaEnvironment] Root or root transform is null after creation!");
            return null;
        }

        var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (water != null && water.transform != null && root.transform != null)
        {
            water.transform.SafeSetParent(root.transform);
        }
        else
        {
            Debug.LogError("[CreateSeaEnvironment] Water or root transform is null!");
        }
        water.transform.localScale = new Vector3(10f, 0.15f, 10f);
        var wm = water.GetComponent<MeshRenderer>();
        var waterMat = URPMaterialFactory.Create(new Color(0.04f, 0.2f, 0.45f), 0.8f);
        waterMat.EnableKeyword("_EMISSION");
        waterMat.SetColor("_EmissionColor", new Color(0.1f, 0.2f, 0.4f));
        wm.material = waterMat;

        var island = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (island != null && island.transform != null && root != null && root.transform != null)
        {
            island.transform.SafeSetParent(root.transform);
        }
        else
        {
            Debug.LogError("[CreateSeaEnvironment] Island or root transform is null!");
        }
        island.transform.position = new Vector3(0, 0.05f, 0);
        island.transform.localScale = new Vector3(4f, 0.2f, 3f);
        island.GetComponent<MeshRenderer>().material = URPMaterialFactory.Create(new Color(0.85f, 0.75f, 0.55f), 0.2f);

        for (int i = 0; i < 5; i++)
        {
            float angle = i * Mathf.PI * 2f / 5f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * 2.2f, 0.2f, Mathf.Sin(angle) * 2.2f);
            CreatePalmTree(root.transform, pos);
        }

        for (int i = 0; i < 3; i++)
        {
            Vector3 bouyPos = new Vector3(Random.Range(-6f, 6f), 0.2f, Random.Range(-6f, 6f));
            CreateFloatingBuoy(root.transform, bouyPos);
        }

        root.SetActive(false);
        return root;
    }

    GameObject CreateMountainEnvironment(Transform parent)
    {
        if (parent == null)
        {
            Debug.LogError("[CreateMountainEnvironment] Parent is null!");
            return null;
        }
        
        GameObject root = new GameObject("MountainEnvironment");

        if (root == null)
        {
            Debug.LogError("[CreateMountainEnvironment] Root GameObject is null after creation!");
            return null;
        }
        
        if (root.transform == null)
        {
            Debug.LogError("[CreateMountainEnvironment] Root transform is null after creation!");
            return null;
        }
        
        root.transform.SafeSetParent(parent, false);
        
        if (root == null || root.transform == null)
        {
            Debug.LogError("[CreateMountainEnvironment] Root or root transform is null after creation!");
            return null;
        }

        var plateau = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (plateau != null && plateau.transform != null && root.transform != null)
        {
            plateau.transform.SafeSetParent(root.transform);
        }
        else
        {
            Debug.LogError("[CreateMountainEnvironment] Plateau or root transform is null!");
        }
        plateau.transform.localScale = new Vector3(9f, 0.2f, 9f);
        plateau.GetComponent<MeshRenderer>().material = URPMaterialFactory.Create(new Color(0.25f, 0.26f, 0.30f), 0.15f);

        for (int i = 0; i < 4; i++)
        {
            float angle = i * Mathf.PI * 2f / 4f + Mathf.PI / 8f;
            float radius = Random.Range(2.5f, 4.5f);
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 1.3f, Mathf.Sin(angle) * radius);
            CreateMountain(root.transform, pos, Random.Range(2.5f, 4f));
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
                Debug.LogError("[CreateForestEnvironment] Rock or root transform is null!");
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
            Debug.LogError("[CreateTree] Parent is null!");
            return;
        }
        
        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (trunk != null && trunk.transform != null && parent != null)
        {
            trunk.transform.SafeSetParent(parent);
        }
        else
        {
            Debug.LogError("[CreateTree] Trunk, trunk transform, or parent is null!");
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
            Debug.LogError("[CreateTree] Crown or parent is null!");
        }
        crown.transform.position = trunk.transform.position + new Vector3(0, height / 2f, 0);
        crown.transform.localScale = Vector3.one * Random.Range(2f, 3f);
        crown.GetComponent<MeshRenderer>().material = URPMaterialFactory.Create(crownColor, 0.3f);
    }

    void CreateGlowingOrb(Transform parent, Vector3 position, Color color)
    {
        if (parent == null)
        {
            Debug.LogError("[CreateGlowingOrb] Parent is null!");
            return;
        }
        
        var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        if (orb != null && orb.transform != null && parent != null)
        {
            orb.transform.SafeSetParent(parent);
        }
        else
        {
            Debug.LogError("[CreateGlowingOrb] Orb, orb transform, or parent is null!");
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
        if (parent == null)
        {
            Debug.LogError("[CreatePalmTree] Parent is null!");
            return;
        }
        
        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (trunk != null && trunk.transform != null && parent != null)
        {
            trunk.transform.SafeSetParent(parent);
        }
        else
        {
            Debug.LogError("[CreatePalmTree] Trunk, trunk transform, or parent is null!");
            return;
        }
        trunk.transform.position = basePosition + new Vector3(0, 1.2f, 0);
        trunk.transform.localScale = new Vector3(0.35f, 1.2f, 0.35f);
        trunk.transform.rotation = Quaternion.Euler(Random.Range(-5f, 5f), Random.Range(0f, 360f), Random.Range(-5f, 5f));
        trunk.GetComponent<MeshRenderer>().material = URPMaterialFactory.Create(new Color(0.45f, 0.28f, 0.15f), 0.2f);

        for (int i = 0; i < 5; i++)
        {
            var leaf = GameObject.CreatePrimitive(PrimitiveType.Quad);
            if (leaf != null && leaf.transform != null && trunk != null && trunk.transform != null)
            {
                leaf.transform.SafeSetParent(trunk.transform);
            }
            else
            {
                Debug.LogError("[CreatePalmTree] Leaf or trunk transform is null!");
                continue;
            }
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
            Debug.LogError("[CreateFloatingBuoy] Parent is null!");
            return;
        }
        
        var buoy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        if (buoy != null && buoy.transform != null && parent != null)
        {
            buoy.transform.SafeSetParent(parent);
        }
        else
        {
            Debug.LogError("[CreateFloatingBuoy] Buoy, buoy transform, or parent is null!");
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
            Debug.LogError("[CreateMountain] Parent is null!");
            return;
        }
        
        var mountain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (mountain != null && mountain.transform != null && parent != null)
        {
            mountain.transform.SafeSetParent(parent);
        }
        else
        {
            Debug.LogError("[CreateMountain] Mountain, mountain transform, or parent is null!");
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
            Debug.LogError("[CreateMountain] Snow or mountain transform is null!");
        }
        snow.transform.localPosition = new Vector3(0, height, 0);
        snow.transform.localScale = new Vector3(0.8f, 0.4f, 0.8f);
        snow.GetComponent<MeshRenderer>().material = URPMaterialFactory.Create(new Color(0.95f, 0.97f, 1f), 0.5f);
    }
    Button CreateVRButton(
    Transform parent,
    string text,
    Vector2 anchorPos,          // где на панели (0..1, 0..1)
    Vector2 size,               // размер в пикселях
    Font font,
    UnityEngine.Events.UnityAction onClick,
    Color? bgColor = null,
    Color? textColor = null,
    int fontSize = 80)
    {
        if (parent == null)
        {
            Debug.LogError($"[CreateVRButton] Parent is null for button '{text}'!");
            return null;
        }
        
        GameObject btnGO = new GameObject(text + "Button");
        // Убеждаемся, что кнопка на слое UI для правильной работы с PhysicsRaycaster
        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer != -1)
        {
            btnGO.layer = uiLayer;
        }
        if (btnGO == null || btnGO.transform == null)
        {
            Debug.LogError($"[CreateVRButton] Button GameObject or transform is null for '{text}'!");
            return null;
        }
        
        btnGO.transform.SafeSetParent(parent, false);

        var img = btnGO.AddComponent<Image>();
        img.color = bgColor ?? new Color(0.2f, 0.2f, 0.25f);

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);
        
        // Для WorldSpace Canvas GraphicRaycaster работает напрямую с UI элементами
        // Collider не нужен для UI кнопок - GraphicRaycaster использует RectTransform

        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = anchorPos;
        rt.anchorMax = anchorPos;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;

        GameObject txtGO = new GameObject("Text");
        if (txtGO == null || txtGO.transform == null || btnGO == null || btnGO.transform == null)
        {
            Debug.LogError($"[CreateVRButton] Text GameObject, text transform, button, or button transform is null for '{text}'!");
            return null;
        }
        
        txtGO.transform.SafeSetParent(btnGO.transform, false);
        var txt = txtGO.AddComponent<Text>();
        txt.font = font;
        txt.text = text;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = textColor ?? Color.white;
        txt.fontSize = fontSize;

        var trt = txtGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 0);
        trt.anchorMax = new Vector2(1, 1);
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        return btn;
    }

    void EnsureXRInteractionManager()
    {
        // НЕ создаем XR Interaction Manager - работаем только с мышью
        // Убираем все связанное с XR для упрощения
        Debug.Log("[AutoSetup] Skipping XR setup - using mouse input only");
        
        // Отключаем XR Device Simulator если он есть
        DisableXRDeviceSimulator();
    }
    
    void DisableXRDeviceSimulator()
    {
        // Ищем и отключаем XR Device Simulator
        var simulatorType = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.Samples.DeviceSimulator.XRDeviceSimulator, UnityEngine.XR.Interaction.Toolkit.Samples");
        if (simulatorType != null)
        {
            var simulator = UnityEngine.Object.FindFirstObjectByType(simulatorType);
            if (simulator != null)
            {
                var simulatorMono = simulator as MonoBehaviour;
                if (simulatorMono != null)
                {
                    simulatorMono.gameObject.SetActive(false);
                    Debug.Log("[AutoSetup] Disabled XR Device Simulator - using mouse only");
                }
            }
        }
        
        // Также ищем и отключаем XR Device Simulator через GameObject.Find
        var simulatorGO = GameObject.Find("XR Device Simulator");
        if (simulatorGO != null)
        {
            simulatorGO.SetActive(false);
            Debug.Log("[AutoSetup] Disabled XR Device Simulator GameObject - using mouse only");
        }
    }
    
    void EnsureXRDeviceSimulator()
    {
        // НЕ создаем XR Device Simulator - работаем только с мышью
        Debug.Log("[AutoSetup] Skipping XR Device Simulator - using mouse input only");
        
        // Отключаем XR Device Simulator если он уже есть в сцене
        var simulatorType = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.Samples.DeviceSimulator.XRDeviceSimulator, UnityEngine.XR.Interaction.Toolkit.Samples");
        if (simulatorType != null)
        {
            var simulator = UnityEngine.Object.FindFirstObjectByType(simulatorType);
            if (simulator != null)
            {
                var simulatorMono = simulator as MonoBehaviour;
                if (simulatorMono != null)
                {
                    simulatorMono.gameObject.SetActive(false);
                    Debug.Log("[AutoSetup] Disabled existing XR Device Simulator - using mouse only");
                }
            }
        }
        
        // Также ищем и отключаем через GameObject.Find
        var simulatorGO = GameObject.Find("XR Device Simulator");
        if (simulatorGO != null)
        {
            simulatorGO.SetActive(false);
            Debug.Log("[AutoSetup] Disabled XR Device Simulator GameObject - using mouse only");
        }
    }
    
    void EnsureXROrigin()
    {
        // НЕ создаем XR Origin - работаем только с мышью, камера может быть на корневом уровне
        Debug.Log("[AutoSetup] Skipping XR Origin creation - using mouse input only, camera can be at root level");
        
        // Если XR Origin уже есть, оставляем его, но не создаем новый
        var xrOrigin = UnityEngine.Object.FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin != null)
        {
            Debug.Log("[AutoSetup] XR Origin already exists in scene, leaving it as is");
        }
    }
    
    void EnsureXRRayInteractors(Unity.XR.CoreUtils.XROrigin xrOrigin)
    {
        if (xrOrigin == null) return;
        
        // Ищем существующие Ray Interactors
        var rayInteractors = UnityEngine.Object.FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>(UnityEngine.FindObjectsSortMode.None);
        
        // Если нет Ray Interactors, создаем их
        if (rayInteractors == null || rayInteractors.Length == 0)
        {
            Debug.Log("[AutoSetup] Creating XR Ray Interactors for UI interaction...");
            
            // Получаем XR Interaction Manager
            var xrManager = UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>();
            if (xrManager == null)
            {
                var managerGO = new GameObject("XR Interaction Manager");
                xrManager = managerGO.AddComponent<XRInteractionManager>();
            }
            
            // НЕ создаем XR Ray Interactors - они создают видимые лучи
            // Вместо этого используем мышь для взаимодействия с UI
            Debug.Log("[AutoSetup] Skipping XR Ray Interactors - using mouse input for UI interaction");
        }
    }
    
    void EnsureVREventSystem()
    {
        // Убеждаемся, что EventSystem правильно настроен для VR
        var eventSystem = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem != null)
        {
            var xrUIInput = eventSystem.GetComponent<XRUIInputModule>();
            var standaloneInput = eventSystem.GetComponent<StandaloneInputModule>();
            
            // Если есть XR UI Input Module, убеждаемся что он активен
            if (xrUIInput != null)
            {
                xrUIInput.enabled = true;
                
                // Для работы с XR Device Simulator нужно включить fallback на встроенные действия
                // Это позволит использовать мышь для взаимодействия с UI в режиме симуляции
                try
                {
                    var enableBuiltinProp = typeof(XRUIInputModule).GetProperty("enableBuiltinActionsAsFallback");
                    if (enableBuiltinProp != null)
                    {
                        enableBuiltinProp.SetValue(xrUIInput, true);
                        Debug.Log("[AutoSetup] Enabled Builtin Actions As Fallback for XR Device Simulator");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[AutoSetup] Could not enable Builtin Actions As Fallback: {ex.Message}");
                }
                
                // Устанавливаем Active Input Mode на Input System или Both для работы с симулятором
                try
                {
                    var inputModeProp = typeof(XRUIInputModule).GetProperty("activeInputMode");
                    if (inputModeProp != null)
                    {
                        // Пытаемся установить на Both для поддержки и Input System и старого Input Manager
                        var bothValue = System.Enum.Parse(inputModeProp.PropertyType, "Both");
                        inputModeProp.SetValue(xrUIInput, bothValue);
                        Debug.Log("[AutoSetup] Set Active Input Mode to Both for XR Device Simulator compatibility");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[AutoSetup] Could not set Active Input Mode: {ex.Message}");
                }
            }
            
            // ВСЕГДА включаем StandaloneInputModule для работы с мышью
            if (standaloneInput == null)
            {
                standaloneInput = eventSystem.gameObject.AddComponent<StandaloneInputModule>();
                Debug.Log("[AutoSetup] Added StandaloneInputModule for mouse interaction");
            }
            standaloneInput.enabled = true;
            Debug.Log("[AutoSetup] StandaloneInputModule is enabled for mouse clicking");
            
            // Отключаем XR UI Input Module, чтобы работала мышь
            if (xrUIInput != null)
            {
                xrUIInput.enabled = false;
                Debug.Log("[AutoSetup] XRUIInputModule disabled - using mouse input instead");
            }
        }
        else
        {
            // Если EventSystem не существует, создаем его (это должно было произойти в CreateSimpleUICanvas)
            Debug.LogWarning("[AutoSetup] EventSystem not found in EnsureVREventSystem - it should have been created earlier");
        }
    }
    
    void EnsureMouseLook(Camera cam)
    {
        if (cam == null)
        {
            Debug.LogError("[AutoSetup] Camera is null in EnsureMouseLook!");
            return;
        }
        
        if (cam.gameObject == null)
        {
            Debug.LogError("[AutoSetup] Camera GameObject is null!");
            return;
        }
        
        // Проверяем, есть ли уже компонент MouseLook
        var mouseLook = cam.GetComponent<MouseLookController>();
        if (mouseLook == null)
        {
            Debug.Log("[AutoSetup] Adding MouseLookController for mouse camera control...");
            mouseLook = cam.gameObject.AddComponent<MouseLookController>();
            
            if (mouseLook == null)
            {
                Debug.LogError("[AutoSetup] Failed to add MouseLookController component!");
                return;
            }
            
            // Если камера является дочерним объектом (например, в XR Origin),
            // настраиваем MouseLookController для работы с этой структурой
            if (cam.transform.parent != null)
            {
                Debug.Log($"[AutoSetup] Camera is child of {cam.transform.parent.name}, MouseLook will handle this correctly");
            }
            
            // Убеждаемся, что компонент активен
            mouseLook.enabled = true;
            Debug.Log($"[AutoSetup] MouseLookController added and enabled. GameObject active: {cam.gameObject.activeInHierarchy}, Component enabled: {mouseLook.enabled}");
        }
        else
        {
            Debug.Log("[AutoSetup] MouseLookController already exists");
            // Убеждаемся, что компонент активен
            mouseLook.enabled = true;
            Debug.Log($"[AutoSetup] MouseLookController enabled. GameObject active: {cam.gameObject.activeInHierarchy}, Component enabled: {mouseLook.enabled}");
        }
        
        // Дополнительная проверка через несколько кадров
        StartCoroutine(VerifyMouseLookAfterDelay(cam, mouseLook));
    }
    
    System.Collections.IEnumerator VerifyMouseLookAfterDelay(Camera cam, MouseLookController mouseLook)
    {
        yield return new WaitForSeconds(1f);
        
        if (cam == null || cam.gameObject == null)
        {
            Debug.LogError("[AutoSetup] Camera or GameObject became null after delay!");
            yield break;
        }
        
        var checkMouseLook = cam.GetComponent<MouseLookController>();
        if (checkMouseLook == null)
        {
            Debug.LogError("[AutoSetup] MouseLookController disappeared after delay!");
        }
        else
        {
            Debug.Log($"[AutoSetup] MouseLookController verification: GameObject active: {cam.gameObject.activeInHierarchy}, Component enabled: {checkMouseLook.enabled}, Component active: {checkMouseLook.isActiveAndEnabled}");
        }
    }

}

/// <summary>
/// Простой и надежный контроллер для управления камерой мышью
/// </summary>
public class MouseLookController : MonoBehaviour
{
    [Header("Mouse Look Settings")]
    [Tooltip("Чувствительность мыши для поворота камеры")]
    public float mouseSensitivity = 3f;
    
    [Tooltip("Ограничение вертикального поворота (в градусах)")]
    [Range(0f, 90f)]
    public float verticalRotationLimit = 80f;
    
    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;
    
    void Start()
    {
        // Инициализируем начальное вращение из текущего состояния камеры
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            if (transform.parent != null)
            {
                horizontalRotation = transform.parent.eulerAngles.y;
            }
            else
            {
                horizontalRotation = transform.eulerAngles.y;
            }
            
            float localX = cam.transform.localEulerAngles.x;
            verticalRotation = localX;
            if (verticalRotation > 180f) verticalRotation -= 360f;
        }
        else
        {
            Vector3 euler = transform.eulerAngles;
            horizontalRotation = euler.y;
            verticalRotation = euler.x;
            if (verticalRotation > 180f) verticalRotation -= 360f;
        }
    }
    
    void Update()
    {
        // Вращение камеры при зажатой левой кнопке мыши
        if (Input.GetMouseButton(0))
        {
            // Проверяем, не над UI ли курсор
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            bool isOverUI = eventSystem != null && eventSystem.IsPointerOverGameObject();
            
            if (!isOverUI)
            {
                HandleMouseLook();
            }
        }
        // Также вращаем при зажатой правой кнопке мыши
        else if (Input.GetMouseButton(1))
        {
            HandleMouseLook();
        }
    }
    
    void HandleMouseLook()
    {
        // Получаем движение мыши
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Накопление вращения - НЕ ограничиваем horizontalRotation для 360° вращения
        horizontalRotation += mouseX;
        // НЕ нормализуем horizontalRotation - позволяем ему быть любым значением для непрерывного вращения на 360°
        
        verticalRotation -= mouseY;
        // Ограничиваем только вертикальное вращение
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalRotationLimit, verticalRotationLimit);
        
        // Применяем вращение
        Camera cam = GetComponent<Camera>();
        if (cam != null && transform.parent != null)
        {
            // Если камера в иерархии (XR Origin), вращаем родителя по горизонтали, камеру по вертикали
            // Используем horizontalRotation напрямую без нормализации для 360° вращения
            transform.parent.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);
            transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
        else
        {
            // Если камера на корневом уровне, применяем оба вращения
            transform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
        }
    }
}

