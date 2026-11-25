using UnityEngine;

public class BreathIndicator : MonoBehaviour
{
    public float cycleTime = 5f; // полный цикл вдох-выдох
    public float minScale = 0.7f;
    public float maxScale = 1.3f;

    RectTransform rect;
    float t;

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        t += Time.deltaTime / cycleTime;
        if (t > 1f) t = 0f;

        // плавная синусоида
        float k = (Mathf.Sin(t * Mathf.PI * 2f) + 1f) / 2f; // 0..1

        float scale = Mathf.Lerp(minScale, maxScale, k);
        rect.localScale = new Vector3(scale, scale, 1f);
    }
}
