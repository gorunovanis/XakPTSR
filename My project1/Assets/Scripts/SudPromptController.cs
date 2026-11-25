using UnityEngine;
using UnityEngine.UI;

public class SudPromptController : MonoBehaviour
{
    public Canvas sudCanvas;
    public Slider sudSlider;
    public Text sudValueText;
    public Text titleText;

    private System.Action<int> onSudEntered;

    private void Start()
    {
        Hide();
        if (sudSlider)
        {
            sudSlider.minValue = 0;
            sudSlider.maxValue = 10;
            sudSlider.value = 0;
            sudSlider.onValueChanged.AddListener(OnSliderChanged);
        }
    }

    private void OnSliderChanged(float v)
    {
        if (sudValueText)
            sudValueText.text = Mathf.RoundToInt(v).ToString();
    }

    public void Show(string titleKey, System.Action<int> callback)
    {
        onSudEntered = callback;
        if (sudCanvas) sudCanvas.enabled = true;

        if (titleText)
        {
            titleText.text = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.Tr(titleKey)
                : titleKey;
        }
    }

    public void Hide()
    {
        if (sudCanvas) sudCanvas.enabled = false;
        onSudEntered = null;
    }

    public void OnConfirmButton()
    {
        int v = Mathf.RoundToInt(sudSlider.value);
        onSudEntered?.Invoke(v);
        Hide();
    }

    public void OnCancelButton()
    {
        Hide();
    }
}
