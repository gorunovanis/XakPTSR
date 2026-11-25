using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;

    public List<PatientProfile> profiles = new List<PatientProfile>();
    public PatientProfile currentProfile;

    private string ProfilesPath => Path.Combine(Application.persistentDataPath, "profiles.json");

    [System.Serializable]
    class ProfileListWrapper
    {
        public List<PatientProfile> profiles;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadProfiles();
    }

    public void SetCurrentProfile(string patientId)
    {
        foreach (var p in profiles)
        {
            if (p.patientId == patientId)
            {
                currentProfile = p;
                Debug.Log($"[ProfileManager] Current patient: {p.displayName} ({p.patientId})");
                if (SessionManager.Instance != null)
                {
                    SessionManager.Instance.patientId = p.patientId;
                    SessionManager.Instance.locale = p.preferredLanguage == "ru" ? "ru-RU" : "en-US";
                }
                return;
            }
        }
        Debug.LogWarning($"[ProfileManager] Profile not found: {patientId}");
    }

    public void AddOrUpdateProfile(PatientProfile profile)
    {
        var existing = profiles.Find(p => p.patientId == profile.patientId);
        if (existing != null)
        {
            existing.displayName = profile.displayName;
            existing.notes = profile.notes;
            existing.preferredLanguage = profile.preferredLanguage;
        }
        else
        {
            profiles.Add(profile);
        }
        SaveProfiles();
    }

    public void SaveProfiles()
    {
        var wrapper = new ProfileListWrapper { profiles = profiles };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(ProfilesPath, json);
        Debug.Log($"[ProfileManager] Profiles saved to {ProfilesPath}");
    }

    public void LoadProfiles()
    {
        if (!File.Exists(ProfilesPath))
        {
            profiles = new List<PatientProfile>();
            return;
        }

        string json = File.ReadAllText(ProfilesPath);
        var wrapper = JsonUtility.FromJson<ProfileListWrapper>(json);
        profiles = wrapper?.profiles ?? new List<PatientProfile>();
        Debug.Log($"[ProfileManager] Profiles loaded: {profiles.Count}");
    }
}
