using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Procedurally assembles a light-weight world scene (forest/sea/mountains)
/// without needing handcrafted geometry in the .unity file. The component
/// ensures camera, lighting and environment controller exist and then
/// configures <see cref="PrettyWorldController"/> so that each scene has
/// distinct mood/visuals.
/// </summary>
public class WorldSceneBootstrap : MonoBehaviour
{
    [Header("World setup")]
    public WorldType worldType = WorldType.Forest;
    public Vector3 cameraPosition = new Vector3(0f, 1.7f, -6f);
    public Vector3 cameraLookAt = new Vector3(0f, 1.4f, 0f);
    public Vector3 lightEulerAngles = new Vector3(50f, -30f, 0f);

    private void Awake()
    {
        BuildWorld();
    }

    private void BuildWorld()
    {
        var cam = EnsureCamera();
        var light = EnsureDirectionalLight();

        ConfigureEnvironment(cam, light);
        ConfigurePrettyWorld(worldType);
    }

    private Camera EnsureCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            cam = cameraGO.AddComponent<Camera>();
            cameraGO.AddComponent<AudioListener>();
        }

        var additional = cam.GetComponent<UniversalAdditionalCameraData>();
        if (additional == null)
        {
            cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
        }

        cam.transform.position = cameraPosition;
        cam.transform.LookAt(cameraLookAt);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 200f;

        return cam;
    }

    private Light EnsureDirectionalLight()
    {
        var light = FindFirstObjectByType<Light>();
        if (light == null)
        {
            var go = new GameObject("Directional Light");
            light = go.AddComponent<Light>();
        }

        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(lightEulerAngles);
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.9f;
        light.intensity = 1.2f;

        return light;
    }

    private void ConfigureEnvironment(Camera cam, Light light)
    {
        var env = FindFirstObjectByType<EnvironmentController>();
        if (env == null)
        {
            var go = new GameObject("EnvironmentController");
            env = go.AddComponent<EnvironmentController>();
        }

        env.mainCamera = cam;
        env.mainLight = light;
        env.baseFogDensity = 0.025f;

        var preset = worldType switch
        {
            WorldType.Forest => EnvironmentPreset.ForestDay,
            WorldType.Sea => EnvironmentPreset.SeaSunset,
            WorldType.Mountains => EnvironmentPreset.MountainDawn,
            _ => EnvironmentPreset.Neutral
        };

        env.ApplyPreset(preset);
    }

    private void ConfigurePrettyWorld(WorldType type)
    {
        var pretty = FindFirstObjectByType<PrettyWorldController>();
        if (pretty == null)
        {
            var go = new GameObject("ProceduralWorld");
            pretty = go.AddComponent<PrettyWorldController>();
        }

        switch (type)
        {
            case WorldType.Forest:
                ApplyForestPreset(pretty);
                break;
            case WorldType.Sea:
                ApplySeaPreset(pretty);
                break;
            case WorldType.Mountains:
                ApplyMountainPreset(pretty);
                break;
        }
    }

    private void ApplyForestPreset(PrettyWorldController pretty)
    {
        pretty.treeCount = 28;
        pretty.worldRadius = 24f;
        pretty.groundColor = new Color(0.10f, 0.23f, 0.14f);
        pretty.trunkColor = new Color(0.28f, 0.17f, 0.11f);
        pretty.crownColor = new Color(0.20f, 0.55f, 0.32f);
        pretty.orbColor = new Color(1.0f, 0.95f, 0.65f);
        pretty.orbCount = 8;
        pretty.orbMinHeight = 3f;
        pretty.orbMaxHeight = 6.5f;
    }

    private void ApplySeaPreset(PrettyWorldController pretty)
    {
        pretty.treeCount = 18;
        pretty.worldRadius = 18f;
        pretty.groundColor = new Color(0.05f, 0.17f, 0.27f);
        pretty.trunkColor = new Color(0.12f, 0.25f, 0.35f);
        pretty.crownColor = new Color(0.30f, 0.70f, 0.85f);
        pretty.orbColor = new Color(0.85f, 0.65f, 1.0f);
        pretty.orbCount = 10;
        pretty.orbMinHeight = 1.5f;
        pretty.orbMaxHeight = 4f;
    }

    private void ApplyMountainPreset(PrettyWorldController pretty)
    {
        pretty.treeCount = 16;
        pretty.worldRadius = 26f;
        pretty.groundColor = new Color(0.25f, 0.27f, 0.31f);
        pretty.trunkColor = new Color(0.35f, 0.30f, 0.27f);
        pretty.crownColor = new Color(0.65f, 0.70f, 0.75f);
        pretty.orbColor = new Color(0.95f, 0.95f, 1.0f);
        pretty.orbCount = 5;
        pretty.orbMinHeight = 4f;
        pretty.orbMaxHeight = 7f;
    }
}

