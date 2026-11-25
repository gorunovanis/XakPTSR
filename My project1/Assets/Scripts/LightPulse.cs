using UnityEngine;

/// <summary>
/// Плавно меняет интенсивность источника света по синусоиде.
/// </summary>
[RequireComponent(typeof(Light))]
public class LightPulse : MonoBehaviour
{
    public float baseIntensity = 1f;
    public float amplitude = 0.3f;
    public float frequency = 0.5f;

    private Light _light;
    private float phaseOffset;

    private void Awake()
    {
        _light = GetComponent<Light>();
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        if (_light == null) return;

        float delta = Mathf.Sin(Time.time * frequency + phaseOffset) * amplitude;
        _light.intensity = baseIntensity + delta;
    }
}
