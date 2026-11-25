using UnityEngine;

public class FogTester : MonoBehaviour
{
    void OnEnable()
    {
        // ВКЛЮЧАЕМ ТУМАН
        RenderSettings.fog = true;

        // ЛИНЕЙНЫЙ, ОЧЕНЬ ЗАМЕТНЫЙ
        RenderSettings.fogMode = FogMode.Linear;

        // туман начинается сразу от камеры…
        RenderSettings.fogStartDistance = 0f;
        // …и через 8 единиц уже всё практически в дымке
        RenderSettings.fogEndDistance = 8f;

        // ярко-серый, чтобы был заметен
        RenderSettings.fogColor = Color.gray;
    }
}
