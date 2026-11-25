using UnityEngine;
using UnityEngine.UI;

public enum SafePlacePreset
{
    Forest,
    Sea,
    Mountains
}

[System.Serializable]
public class EnvironmentSettings
{
    public Color fogColor = new Color(0.02f, 0.04f, 0.05f);
    public float fogDensity = 0.02f;
    public Material skybox;
    public Color ambientLight = new Color(0.2f, 0.2f, 0.25f);
}

public class SafePlaceController : MonoBehaviour
{
    [Header("Environment roots")]
    public GameObject forestScene;
    public GameObject seaScene;
    public GameObject mountainScene;
    public GameObject neutralScene;

    [Header("UI (Canvas с дыханием)")]
    public Text breathingText;
    public Image breathingCircle;
    public Text groundingText;

    [Header("Environment settings")]
    public EnvironmentSettings forestSettings;
    public EnvironmentSettings seaSettings;
    public EnvironmentSettings mountainSettings;

    SafePlacePreset currentPreset = SafePlacePreset.Forest;

    void Awake()
    {
        // при старте показываем нейтральную сцену
        ShowEnvironment(neutralScene);
    }

    // --- Вход в Safe Place ---
    public void StartSafePlace()
    {
        currentPreset = SafePlacePreset.Forest;
        ShowPreset(currentPreset);

        if (breathingText != null)
            breathingText.text = "Дыхание";

        if (groundingText != null)
            groundingText.text = "5-4-3-2-1";
    }

    public void StopSafePlace()
    {
        ShowEnvironment(neutralScene);
    }

    public void GoToNeutral()
    {
        ShowEnvironment(neutralScene);
    }

    // Методы, которые дергаем из SessionManager / сети
    public void ShowForest() => ShowPreset(SafePlacePreset.Forest);
    public void ShowSea() => ShowPreset(SafePlacePreset.Sea);
    public void ShowMountains() => ShowPreset(SafePlacePreset.Mountains);

    void ShowPreset(SafePlacePreset p)
    {
        currentPreset = p;

        switch (p)
        {
            case SafePlacePreset.Forest:
                ShowEnvironment(forestScene);
                ApplySettings(forestSettings);
                break;

            case SafePlacePreset.Sea:
                ShowEnvironment(seaScene);
                ApplySettings(seaSettings);
                break;

            case SafePlacePreset.Mountains:
                ShowEnvironment(mountainScene);
                ApplySettings(mountainSettings);
                break;
        }
    }

    void ShowEnvironment(GameObject env)
    {
        if (forestScene != null) forestScene.SetActive(env == forestScene);
        if (seaScene != null) seaScene.SetActive(env == seaScene);
        if (mountainScene != null) mountainScene.SetActive(env == mountainScene);
        if (neutralScene != null) neutralScene.SetActive(env == neutralScene);
    }

    void ApplySettings(EnvironmentSettings s)
    {
        if (s == null) return;

        RenderSettings.fog = true;
        RenderSettings.fogColor = s.fogColor;
        RenderSettings.fogDensity = s.fogDensity;
        RenderSettings.ambientLight = s.ambientLight;

        if (s.skybox != null)
            RenderSettings.skybox = s.skybox;
    }
}
