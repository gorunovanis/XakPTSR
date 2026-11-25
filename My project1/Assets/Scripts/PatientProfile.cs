using System;
using UnityEngine;

[Serializable]
public class PatientProfile
{
    public string patientId;
    public string displayName;
    public string notes;
    public string preferredLanguage = "ru"; // "ru" / "en"

    public PatientProfile(string id, string name)
    {
        patientId = id;
        displayName = name;
    }
}
