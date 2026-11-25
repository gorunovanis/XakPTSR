using System.Collections.Generic;
using UnityEngine;

public enum Language
{
    Ru,
    En
}

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;

    public Language currentLanguage = Language.Ru;

    private Dictionary<string, string> ru = new Dictionary<string, string>();
    private Dictionary<string, string> en = new Dictionary<string, string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitDictionaries();
    }

    private void InitDictionaries()
    {
        ru["SUD_LABEL"] = "Уровень дистресса (0–10)";
        en["SUD_LABEL"] = "Distress level (0–10)";

        ru["BTN_EMDR"] = "EMDR";
        en["BTN_EMDR"] = "EMDR";

        ru["BTN_SAFEPLACE"] = "Безопасное место";
        en["BTN_SAFEPLACE"] = "Safe Place";

        ru["BTN_EXIT"] = "Выход";
        en["BTN_EXIT"] = "Exit";

        ru["BTN_STOP"] = "СТОП!";
        en["BTN_STOP"] = "STOP!";

        ru["BREATH_IN"] = "Вдохните...";
        en["BREATH_IN"] = "Breathe in...";

        ru["BREATH_HOLD"] = "Удерживайте...";
        en["BREATH_HOLD"] = "Hold...";

        ru["BREATH_OUT"] = "Выдохните...";
        en["BREATH_OUT"] = "Breathe out...";
    }

    public void SetLanguage(Language lang)
    {
        currentLanguage = lang;
    }

    public string Tr(string key)
    {
        var dict = currentLanguage == Language.Ru ? ru : en;
        if (dict.TryGetValue(key, out var val))
            return val;
        return key;
    }
}
