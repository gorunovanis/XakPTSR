using UnityEngine;

public enum EnvironmentPreset
{
    Neutral,
    ForestDay,
    SeaSunset,
    MountainDawn
}

public class EnvironmentController : MonoBehaviour
{
    [Header("Основные ссылки")]
    public Camera mainCamera;
    public Light mainLight;

    [Tooltip("Насколько плотным может быть туман в самых густых сценах")]
    public float baseFogDensity = 0.03f;

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainLight == null)
            mainLight = FindFirstObjectByType<Light>();

        // по умолчанию – нейтральная сцена
        ApplyPreset(EnvironmentPreset.Neutral);
    }

    // Главный метод – применить пресет
    public void ApplyPreset(EnvironmentPreset preset)
    {
        switch (preset)
        {
            case EnvironmentPreset.Neutral:
                ApplyNeutral();
                break;
            case EnvironmentPreset.ForestDay:
                ApplyForestDay();
                break;
            case EnvironmentPreset.SeaSunset:
                ApplySeaSunset();
                break;
            case EnvironmentPreset.MountainDawn:
                ApplyMountainDawn();
                break;
        }
    }

    void ApplyCommon(float fogK)
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = baseFogDensity * fogK;
    }

    void ApplyNeutral()
    {
        ApplyCommon(0.6f);

        RenderSettings.fogColor = new Color(0.10f, 0.12f, 0.16f); // холодный серо-синий
        RenderSettings.ambientLight = new Color(0.08f, 0.09f, 0.11f);

        if (mainCamera != null)
            mainCamera.backgroundColor = new Color(0.03f, 0.04f, 0.06f);

        if (mainLight != null)
        {
            mainLight.color = new Color(0.95f, 0.95f, 1.0f);
            mainLight.intensity = 1.0f;
        }
    }

    void ApplyForestDay()
    {
        ApplyCommon(0.8f);

        // немного туманный солнечный лес
        RenderSettings.fogColor = new Color(0.48f, 0.60f, 0.52f); // зеленовато-серый
        RenderSettings.ambientLight = new Color(0.35f, 0.45f, 0.35f);

        if (mainCamera != null)
            mainCamera.backgroundColor = new Color(0.55f, 0.75f, 0.90f); // голубое небо

        if (mainLight != null)
        {
            mainLight.color = new Color(1.0f, 0.96f, 0.88f); // тёплое солнце
            mainLight.intensity = 1.4f;
        }
    }

    void ApplySeaSunset()
    {
        ApplyCommon(0.5f);

        // мягкий туман над морем, закат
        RenderSettings.fogColor = new Color(0.75f, 0.55f, 0.60f);
        RenderSettings.ambientLight = new Color(0.55f, 0.40f, 0.45f);

        if (mainCamera != null)
            mainCamera.backgroundColor = new Color(0.85f, 0.55f, 0.45f); // оранжево-розовый закат

        if (mainLight != null)
        {
            mainLight.color = new Color(1.0f, 0.80f, 0.65f);
            mainLight.intensity = 1.2f;
        }
    }

    void ApplyMountainDawn()
    {
        ApplyCommon(1.0f);

        // прохладное горное утро
        RenderSettings.fogColor = new Color(0.55f, 0.60f, 0.70f);
        RenderSettings.ambientLight = new Color(0.40f, 0.45f, 0.55f);

        if (mainCamera != null)
            mainCamera.backgroundColor = new Color(0.35f, 0.40f, 0.55f);

        if (mainLight != null)
        {
            mainLight.color = new Color(0.90f, 0.95f, 1.0f); // холодный свет
            mainLight.intensity = 1.3f;
        }
    }
}
