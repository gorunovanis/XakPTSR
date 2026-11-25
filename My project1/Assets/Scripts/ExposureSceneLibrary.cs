using System.Collections.Generic;
using UnityEngine;

public class ExposureSceneLibrary : MonoBehaviour
{
    public static ExposureSceneLibrary Instance;

    [Header("Список сцен экспозиции")]
    public List<ExposureSceneConfig> scenes = new List<ExposureSceneConfig>();

    public ExposureSceneConfig CurrentScene { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (scenes.Count > 0)
            CurrentScene = scenes[0];
    }

    public void SetSceneById(string sceneId)
    {
        foreach (var s in scenes)
        {
            if (s != null && s.sceneId == sceneId)
            {
                CurrentScene = s;
                Debug.Log($"[ExposureSceneLibrary] Scene set: {sceneId}");
                return;
            }
        }
        Debug.LogWarning($"[ExposureSceneLibrary] Scene id not found: {sceneId}");
    }

    public void SetScene(ExposureSceneConfig config)
    {
        CurrentScene = config;
    }
}
