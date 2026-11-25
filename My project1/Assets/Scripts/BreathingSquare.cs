using UnityEngine;

public class BreathingSquare : MonoBehaviour
{
    public float inhaleTime = 4f;
    public float exhaleTime = 4f;
    public float minScale = 0.8f;
    public float maxScale = 1.2f;

    private RectTransform rect;
    private float t;
    private bool inhaling = true;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (rect == null) return;

        float duration = inhaling ? inhaleTime : exhaleTime;
        if (duration <= 0.01f) duration = 0.01f;

        t += Time.deltaTime / duration;
        if (t > 1f)
        {
            t = 0f;
            inhaling = !inhaling;
        }

        float k = inhaling ? Mathf.Lerp(minScale, maxScale, t)
                           : Mathf.Lerp(maxScale, minScale, t);

        rect.localScale = new Vector3(k, k, 1f);
    }
}
