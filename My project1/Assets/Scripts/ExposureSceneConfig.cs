using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "ExposureSceneConfig", menuName = "VRPTSD/Exposure Scene Config")]
public class ExposureSceneConfig : ScriptableObject
{
    [Header("ID / Название")]
    public string sceneId = "bombardment_shelter";
    public string displayNameRu = "Бомбардировка (убежище)";
    public string displayNameEn = "Bombardment (Shelter)";

    [Header("Контент 360°")]
    public VideoClip videoClip;
    public AudioClip ambisonicClip;

    [Header("Интенсивность по умолчанию (0–1)")]
    [Range(0f, 1f)] public float defaultVolume = 0.8f;
    [Range(0f, 1f)] public float defaultFlashIntensity = 0.7f;
    [Range(0.5f, 2f)] public float defaultEventDensity = 1.0f;

    [Header("Триггеры / Рекомендации паузы (с)")]
    public float[] pauseTimecodes;

    [Header("Описание/метаданные")]
    [TextArea] public string descriptionRu;
    [TextArea] public string descriptionEn;
}
