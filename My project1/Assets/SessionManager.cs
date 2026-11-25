using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum TherapyState
{
    Idle,
    Exposure,
    Pause,
    EMDR,
    SafePlace,
    Finished
}

[Serializable]
public class SessionEvent
{
    public string timestamp;
    public float timeFromStart;
    public string state;
    public string action;
    public int sud;
    public string details;
}

[Serializable]
public class SessionLog
{
    public string sessionId;
    public string patientId;
    public string therapistId;
    public string locale;
    public List<SessionEvent> events = new List<SessionEvent>();
}

public class SessionManager : MonoBehaviour
{
    

    public static SessionManager Instance;

    [Header("Profile")]
    public string patientId = "P001";
    public string therapistId = "T001";
    public string locale = "ru-RU";

    [Header("State")]
    public TherapyState currentState = TherapyState.Idle;
    float sessionStartTime;
    SessionLog log;

    // ===== ссылки, о которых мы говорили =====
    [Header("References")]
    public VRExposureController exposureController;
    public EMDRController emdrController;
    public SafePlaceController safePlaceController;
    public PauseMenuController pauseMenuController;

    [Header("UI")]
    public SudPromptController sudPrompt;   // ← ВОТ ЭТО ПОЛЕ ОБЯЗАТЕЛЬНО

    public Canvas pauseCanvas;

    bool isVisible;

    public void Show(string label, Action<int> onResult)
    {
        onResult?.Invoke(0);
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        sessionStartTime = Time.time;
        InitLog();
    }

    void InitLog()
    {
        log = new SessionLog
        {
            sessionId = Guid.NewGuid().ToString(),
            patientId = patientId,
            therapistId = therapistId,
            locale = locale,
            events = new List<SessionEvent>()
        };
    }

    float TimeFromStart() => Time.time - sessionStartTime;

    public void AddEvent(string action, int sud = -1, string details = "")
    {
        if (log == null) InitLog();

        var ev = new SessionEvent
        {
            timestamp = DateTime.UtcNow.ToString("o"),
            timeFromStart = TimeFromStart(),
            state = currentState.ToString(),
            action = action,
            sud = sud,
            details = details
        };

        log.events.Add(ev);
        Debug.Log($"[LOG] {ev.state} | {ev.action} | SUD={ev.sud} | {ev.details}");
    }

    // ========= Основные переходы =========

    public void StartExposure()
    {
        currentState = TherapyState.Exposure;
        exposureController?.StartExposure();
        pauseMenuController?.HidePauseMenu();
        AddEvent("StartExposure");
    }

    public void Pause()
    {
        if (currentState == TherapyState.Exposure ||
            currentState == TherapyState.EMDR ||
            currentState == TherapyState.SafePlace)
        {
            currentState = TherapyState.Pause;
            exposureController?.PauseExposure();
            emdrController?.StopEMDR();
            pauseMenuController?.ShowPauseMenu();
            AddEvent("Pause");
        }
    }

    public void ResumeExposure()
    {
        currentState = TherapyState.Exposure;
        exposureController?.ResumeExposure();
        pauseMenuController?.HidePauseMenu();
        AddEvent("ResumeExposure");
    }

    public void GoToEMDR(int preSud)
    {
        AddEvent("SUD_PRE", preSud, "Before EMDR");

        currentState = TherapyState.EMDR;
        pauseMenuController?.HidePauseMenu();
        exposureController?.HideExposureVisual();
        emdrController?.StartEMDR();

        AddEvent("StartEMDR");
    }

    // обёртка для вызовов без SUD
    public void GoToEMDR()
    {
        GoToEMDR(-1);
    }

    public void CompleteEMDR(int postSud)
    {
        AddEvent("SUD_POST", postSud, "After EMDR");
        AddEvent("EndEMDR");
    }

    public void GoToSafePlace()
    {
        currentState = TherapyState.SafePlace;

        pauseMenuController?.HidePauseMenu();
        exposureController?.HideExposureVisual();
        emdrController?.StopEMDR();
        safePlaceController?.StartSafePlace();

        AddEvent("StartSafePlace");
    }

    public void ExitSession(string reason = "UserExit")
    {
        currentState = TherapyState.Finished;

        exposureController?.StopExposure();
        emdrController?.StopEMDR();
        safePlaceController?.StopSafePlace();
        pauseMenuController?.HidePauseMenu();

        AddEvent("ExitSession", -1, reason);
        SaveLogs();
    }

    public void FinishSession()
    {
        ExitSession("FinishByUser");
    }

    public void EmergencyStop()
    {
        Debug.Log("[Session] EmergencyStop() ENTER");

        currentState = TherapyState.Finished;

        // просто используем уже назначенные ссылки
        if (exposureController != null)
        {
            exposureController.StopExposure();
            Debug.Log("[Session] Exposure stopped");
        }

        if (emdrController != null)
        {
            emdrController.StopEMDR();
            Debug.Log("[Session] EMDR stopped");
        }

        if (safePlaceController != null)
        {
            safePlaceController.GoToNeutral();
            Debug.Log("[Session] SafePlace -> Neutral");
        }

        if (pauseMenuController != null)
        {
            pauseMenuController.HidePauseMenu();
            Debug.Log("[Session] Pause menu hidden");
        }

        AddEvent("EmergencyStop");
        SaveLogs();

        Debug.Log("[Session] EmergencyStop() EXIT");
    }





    // ========= SUD из VR UI =========

    public void RecordSudFromUI(int value)
    {
        AddEvent("SUD_UI", value, "From VR slider");
    }

    // ========= Удалённое управление (TherapistNetworkLink) =========

    // ======== REMOTE ОБВЁРТКИ ДЛЯ ПАНЕЛИ ТЕРАПЕВТА ========

    public void RemotePause()
    {
        Debug.Log("[REMOTE] RemotePause() -> Pause()");
        Pause();
    }

    public void RemoteEmergencyStop()
    {
        Debug.Log("[REMOTE] RemoteEmergencyStop() -> EmergencyStop()");
        EmergencyStop();
    }

    public void RemoteFinish()
    {
        Debug.Log("[REMOTE] RemoteFinish() -> ExitSession(\"RemoteFinish\")");
        ExitSession("RemoteFinish");
    }

    public void RemoteSafePlace()
    {
        Debug.Log("[REMOTE] RemoteSafePlace() -> GoToSafePlace()");
        GoToSafePlace();
    }

    public void RemoteStartEMDR()
    {
        Debug.Log("[REMOTE] RemoteStartEMDR() -> GoToEMDR( preSud: -1 )");
        // если нужен обязательный Pre-SUD — сначала вызовем AskSud,
        // а потом EMDR. Для простоты пока сразу стартуем.
        GoToEMDR(-1);
    }

    /// <summary>
    /// Показать окно ввода SUD (с Pre/Post/кастомным тегом).
    /// </summary>
    public void RemoteAskSud(string tag)
    {
        Debug.Log("[REMOTE] RemoteAskSud(" + tag + ")");
        AskSud(tag);
    }

    /// <summary>
    /// Установить SUD из панели (без всплывающего окна).
    /// </summary>
    public void RemoteSetSud(int value)
    {
        Debug.Log("[REMOTE] RemoteSetSud(" + value + ")");
        AddEvent("REMOTE_SUD", value);
;
    }

    /// <summary>
    /// Добавить текстовую заметку в лог.
    /// </summary>
    public void RemoteAddNote(string text)
    {
        Debug.Log("[REMOTE] RemoteAddNote: " + text);
        AddEvent("REMOTE_NOTE", -1, text);
    }


    // ========= Сохранение логов =========

    void SaveLogs()
    {
        if (log == null) return;

        string dir = Application.persistentDataPath + "/session_logs";
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string baseName = $"{log.sessionId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
        string jsonPath = Path.Combine(dir, baseName + ".json");
        string csvPath = Path.Combine(dir, baseName + ".csv");

        string json = JsonUtility.ToJson(log, true);
        File.WriteAllText(jsonPath, json);

        using (var sw = new StreamWriter(csvPath))
        {
            sw.WriteLine("timestamp,timeFromStart,state,action,sud,details");
            foreach (var e in log.events)
            {
                string cleanDetails = e.details?.Replace(",", ";");
                sw.WriteLine($"{e.timestamp},{e.timeFromStart},{e.state},{e.action},{e.sud},{cleanDetails}");
            }
        }

        Debug.Log($"Logs saved to: {jsonPath} and {csvPath}");
    }

    // ======================== SUD ========================

    public void AskSud(string actionTag)
    {
        if (sudPrompt == null)
        {
            Debug.LogWarning("[SessionManager] SudPromptController not assigned");
            return;
        }

        // показываем окно ввода SUD
        sudPrompt.Show("SUD", (value) =>
        {
            // лог
            AddEvent(actionTag, value);

        });
    }

    // ======================================
    // ====== REMOTE COMMANDS (WinForms) =====
    // ======================================


 

    public void RemoteAskSud()
    {
        Debug.Log("[REMOTE] REQUEST SUD");
        AskSud("REMOTE_SUD_REQUEST");
    }

    // ======== REMOTE SAFE PLACE PRESETS ========

    public void RemoteSafeForest()
    {
        Debug.Log("[REMOTE] RemoteSafeForest()");

        // если мы ещё не в SafePlace – переходим туда
        if (currentState != TherapyState.SafePlace)
            GoToSafePlace();

        safePlaceController?.ShowForest();
        AddEvent("REMOTE_SAFE_FOREST");
    }

    public void RemoteSafeSea()
    {
        Debug.Log("[REMOTE] RemoteSafeSea()");

        if (currentState != TherapyState.SafePlace)
            GoToSafePlace();

        safePlaceController?.ShowSea();
        AddEvent("REMOTE_SAFE_SEA");
    }

    public void RemoteSafeMountains()
    {
        Debug.Log("[REMOTE] RemoteSafeMountains()");

        if (currentState != TherapyState.SafePlace)
            GoToSafePlace();

        safePlaceController?.ShowMountains();
        AddEvent("REMOTE_SAFE_MOUNTAINS");
    }

}
