using UnityEngine;

public class PrettyWorldController : MonoBehaviour
{
    public int treeCount = 20;
    public float worldRadius = 20f;

    [Header("Colors")]
    public Color groundColor = new Color(0.08f, 0.09f, 0.12f);
    public Color trunkColor = new Color(0.25f, 0.18f, 0.12f);
    public Color crownColor = new Color(0.25f, 0.55f, 0.32f);
    public Color orbColor = new Color(0.9f, 0.85f, 0.5f);

    [Header("Floating orbs")]
    public int orbCount = 6;
    public float orbMinHeight = 2f;
    public float orbMaxHeight = 6f;

    private void Start()
    {
        CreateGround();
        CreateTrees();
        CreateOrbs();
        SetupSkyAndFog();
    }

    private void SetupSkyAndFog()
    {
        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = new Color(0.02f, 0.03f, 0.05f);
        }

        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.03f, 0.035f, 0.05f);
        RenderSettings.fogDensity = 0.01f;
    }

    private void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = Vector3.one * 5f;

        var mr = ground.GetComponent<MeshRenderer>();
        mr.material = URPMaterialFactory.Create(groundColor, 0.1f);
    }

    private void CreateTrees()
    {
        for (int i = 0; i < treeCount; i++)
        {
            float angle = i * Mathf.PI * 2f / treeCount;
            float r = worldRadius;

            Vector3 pos = new Vector3(Mathf.Cos(angle) * r, 0, Mathf.Sin(angle) * r);

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Tree_Trunk_" + i;
            float height = Random.Range(4f, 7f);
            trunk.transform.position = pos + new Vector3(0, height / 2f, 0);
            trunk.transform.localScale = new Vector3(0.5f, height / 2f, 0.5f);
            trunk.GetComponent<MeshRenderer>().material =
                URPMaterialFactory.Create(trunkColor, 0.05f);

            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = "Tree_Crown_" + i;
            crown.transform.position = trunk.transform.position + new Vector3(0, height / 2f, 0);
            crown.transform.localScale = Vector3.one * Random.Range(2.5f, 3.5f);

            var crownMat = URPMaterialFactory.Create(crownColor, 0.25f);
            crown.GetComponent<MeshRenderer>().material = crownMat;

            // ПОКАЧИВАНИЕ КРОНЫ – делаем заметнее
            var floatAnim = crown.AddComponent<FloatingObject>();
            floatAnim.amplitude = 0.35f;
            floatAnim.frequency = Random.Range(0.25f, 0.45f);
            floatAnim.rotationSpeed = new Vector3(0, Random.Range(-15f, 15f), 0);
        }
    }

    private void CreateOrbs()
    {
        for (int i = 0; i < orbCount; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float r = worldRadius * Random.Range(0.3f, 0.8f);
            float h = Random.Range(orbMinHeight, orbMaxHeight);

            Vector3 pos = new Vector3(Mathf.Cos(angle) * r, h, Mathf.Sin(angle) * r);

            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "Floating_Orb_" + i;
            orb.transform.position = pos;
            orb.transform.localScale = Vector3.one * Random.Range(0.6f, 1.2f);

            var mr = orb.GetComponent<MeshRenderer>();
            var mat = URPMaterialFactory.Create(orbColor, 0.8f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", orbColor * 1.7f);
            mr.material = mat;

            var floatAnim = orb.AddComponent<FloatingObject>();
            floatAnim.amplitude = Random.Range(0.4f, 0.9f);
            floatAnim.frequency = Random.Range(0.3f, 0.8f);
            floatAnim.rotationSpeed = new Vector3(0, Random.Range(20f, 50f), 0);
        }
    }
}
