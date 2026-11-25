using UnityEngine;

/// <summary>
/// Holds simple handcrafted/procedural environments for three relaxing worlds.
/// Enables only the requested environment at runtime.
/// </summary>
public class WorldEnvironmentController : MonoBehaviour
{
    public GameObject forestEnvironment;
    public GameObject seaEnvironment;
    public GameObject mountainEnvironment;

    GameObject current;

    void Awake()
    {
        HideAll();
    }

    public void ShowWorld(WorldType type)
    {
        Debug.Log($"[WorldEnvironment] ShowWorld called with type: {type}");
        Debug.Log($"[WorldEnvironment] forestEnvironment: {forestEnvironment != null}, seaEnvironment: {seaEnvironment != null}, mountainEnvironment: {mountainEnvironment != null}");
        
        HideAll();

        // Обрабатываем только обычные миры, SafePlace и WarMode обрабатываются отдельно
        if (type == WorldType.SafePlace || type == WorldType.WarMode)
        {
            Debug.Log($"[WorldEnvironment] {type} is handled by separate controller, hiding all environments");
            return;
        }

        current = type switch
        {
            WorldType.Forest => forestEnvironment,
            WorldType.Sea => seaEnvironment,
            WorldType.Mountains => mountainEnvironment,
            _ => null
        };

        if (current == null)
        {
            Debug.LogError($"[WorldEnvironment] No environment assigned for {type}! forest={forestEnvironment != null}, sea={seaEnvironment != null}, mountains={mountainEnvironment != null}");
            return;
        }

        Debug.Log($"[WorldEnvironment] Activating environment: {current.name}");
        current.SetActive(true);
        Debug.Log($"[WorldEnvironment] Environment active: {current.activeSelf}");
    }

    public void HideAll()
    {
        if (forestEnvironment != null) forestEnvironment.SetActive(false);
        if (seaEnvironment != null) seaEnvironment.SetActive(false);
        if (mountainEnvironment != null) mountainEnvironment.SetActive(false);
    }
}

