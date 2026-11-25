using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InteractiveBreathingPanel : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    public Text label;

    [Header("Длительности фаз (секунды)")]
    public float inhaleTime = 4f;
    public float holdTime = 2f;
    public float exhaleTime = 4f;

    [Header("Масштаб дыхания")]
    public float minBreathScale = 0.9f;
    public float maxBreathScale = 1.15f;

    [Header("Подсветка при наведении")]
    public float hoverExtraScale = 1.05f;

    [Header("Цвета")]
    public Color inhaleColor = new Color(0.7f, 0.9f, 1f);
    public Color exhaleColor = new Color(0.9f, 0.7f, 0.7f);
    public Color pausedColor = new Color(0.7f, 0.7f, 0.7f);

    RectTransform rect;
    Image image;

    float timer;
    Phase phase;
    bool isPaused;
    bool isHover;

    enum Phase
    {
        Inhale,
        Hold,
        Exhale
    }

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        SetPhase(Phase.Inhale);
    }

    void SetPhase(Phase p)
    {
        phase = p;
        timer = 0f;

        if (label == null) return;

        switch (phase)
        {
            case Phase.Inhale:
                label.text = "Вдохните";
                break;
            case Phase.Hold:
                label.text = "Задержите";
                break;
            case Phase.Exhale:
                label.text = "Выдохните";
                break;
        }
    }

    void Update()
    {
        if (isPaused)
        {
            ApplyVisual(pausedColor, (minBreathScale + maxBreathScale) * 0.5f);
            return;
        }

        float duration =
            phase == Phase.Inhale ? inhaleTime :
            phase == Phase.Hold ? holdTime :
                                   exhaleTime;

        if (duration <= 0.001f) duration = 0.001f;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);

        float baseScale;
        Color baseColor;

        if (phase == Phase.Inhale)
        {
            baseScale = Mathf.Lerp(minBreathScale, maxBreathScale, t);
            baseColor = Color.Lerp(exhaleColor, inhaleColor, t);
        }
        else if (phase == Phase.Exhale)
        {
            baseScale = Mathf.Lerp(maxBreathScale, minBreathScale, t);
            baseColor = Color.Lerp(inhaleColor, exhaleColor, t);
        }
        else // Hold
        {
            baseScale = maxBreathScale;
            baseColor = inhaleColor;
        }

        ApplyVisual(baseColor, baseScale);

        if (timer >= duration)
        {
            if (phase == Phase.Inhale)
                SetPhase(Phase.Hold);
            else if (phase == Phase.Hold)
                SetPhase(Phase.Exhale);
            else if (phase == Phase.Exhale)
                SetPhase(Phase.Inhale);
        }
    }

    void ApplyVisual(Color baseColor, float baseScale)
    {
        if (image != null)
            image.color = baseColor;

        float finalScale = baseScale;
        if (isHover)
            finalScale *= hoverExtraScale;

        if (rect != null)
            rect.localScale = new Vector3(finalScale, finalScale, 1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isPaused = !isPaused;

        if (label != null)
        {
            if (isPaused)
                label.text = "Пауза";
            else
                SetPhase(Phase.Inhale);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHover = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHover = false;
    }
}
