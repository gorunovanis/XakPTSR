using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public Canvas pauseCanvas;
    public Slider sudSlider;
    public Text sudValueText;
    [Header("World switching")]
    public WorldSceneController worldSceneController;
    public GameObject worldOptionsPanel;
    bool worldPanelVisible;

    private void Start()
    {
        HidePauseMenu();
        if (sudSlider)
        {
            sudSlider.minValue = 0;
            sudSlider.maxValue = 10;
            sudSlider.value = 0;
            sudSlider.onValueChanged.AddListener(OnSudChanged);
        }
    }

    private void OnSudChanged(float value)
    {
        if (sudValueText)
            sudValueText.text = Mathf.RoundToInt(value).ToString();
    }

    public void ShowPauseMenu()
    {
        pauseCanvas.enabled = true;
    }

    public void HidePauseMenu()
    {
        pauseCanvas.enabled = false;
    }

    public int GetCurrentSUD()
    {
        return Mathf.RoundToInt(sudSlider.value);
    }

    public void OnEMDRButton()
    {
        int preSud = GetCurrentSUD();
        SessionManager.Instance.GoToEMDR(preSud);
    }

    public void OnSafePlaceButton()
    {
        int preSud = GetCurrentSUD();
        SessionManager.Instance.AddEvent("SUD_BEFORE_SAFE_PLACE", preSud);
        SessionManager.Instance.GoToSafePlace();
    }

    public void OnSettingsButton()
    {
        SessionManager.Instance.AddEvent("OpenSettings");
    }

    public void OnExitButton()
    {
        SessionManager.Instance.ExitSession("ExitFromPauseMenu");
    }

    public void OnEmergencyStopButton()
    {
        SessionManager.Instance.EmergencyStop();
    }

    public void OnWorldPanelToggleButton()
    {
        worldPanelVisible = !worldPanelVisible;
        if (worldOptionsPanel != null)
            worldOptionsPanel.SetActive(worldPanelVisible);
    }

    public void OnForestWorldButton() => LoadWorld(WorldType.Forest);
    public void OnSeaWorldButton() => LoadWorld(WorldType.Sea);
    public void OnMountainWorldButton() => LoadWorld(WorldType.Mountains);

    public void OnWorldButton(int worldTypeIndex)
    {
        var values = (WorldType[])System.Enum.GetValues(typeof(WorldType));
        if (worldTypeIndex < 0 || worldTypeIndex >= values.Length)
        {
            Debug.LogWarning($"[PauseMenu] Invalid world index: {worldTypeIndex}");
            return;
        }

        LoadWorld(values[worldTypeIndex]);
    }

    private void LoadWorld(WorldType type)
    {
        var controller = worldSceneController != null
            ? worldSceneController
            : WorldSceneController.Instance;

        if (controller == null)
        {
            Debug.LogWarning("[PauseMenu] WorldSceneController is not assigned");
            return;
        }

        controller.LoadWorld(type);
    }

    public void OnStartupMenuButton()
    {
        StartupMenuController.ShowGlobalMenu();
        HidePauseMenu();
    }
}
