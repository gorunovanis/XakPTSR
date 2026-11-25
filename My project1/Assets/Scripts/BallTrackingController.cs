using UnityEngine;

/// <summary>
/// Безопасное место с движущимся шариком, за которым нужно следить взглядом.
/// Черный фон, шарик движется по плавной траектории.
/// </summary>
public class BallTrackingController : MonoBehaviour
{
    [Header("Ball Settings")]
    public GameObject ballPrefab;
    public float ballSpeed = 2f;
    public float ballRadius = 0.15f;
    public Color ballColor = new Color(0.8f, 0.9f, 1f);
    
    [Header("Movement")]
    public float movementRadius = 3f;
    public float movementHeight = 1.5f;
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Environment")]
    public Camera mainCamera;
    
    private GameObject ball;
    private GameObject environmentRoot;
    private bool isActive = false;
    private float timeElapsed = 0f;
    private Vector3 centerPosition = Vector3.zero;

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        CreateEnvironment();
    }

    void Update()
    {
        if (!isActive || ball == null)
            return;

        // Обновляем позицию шарика по круговой траектории
        timeElapsed += Time.deltaTime * ballSpeed;
        
        float angle = timeElapsed;
        float x = Mathf.Cos(angle) * movementRadius;
        float z = Mathf.Sin(angle) * movementRadius;
        float y = movementHeight + Mathf.Sin(angle * 2f) * 0.3f; // Небольшое вертикальное движение
        
        ball.transform.position = centerPosition + new Vector3(x, y, z);
        
        // Вращение шарика
        ball.transform.Rotate(Vector3.up, 90f * Time.deltaTime);
    }

    void CreateEnvironment()
    {
        // Создаем корневой объект для окружения
        environmentRoot = new GameObject("SafePlaceEnvironment");
        environmentRoot.SetActive(false);
        
        // Черный фон (большая плоскость)
        if (environmentRoot == null || environmentRoot.transform == null)
        {
            Debug.LogError("[BallTrackingController] environmentRoot or transform is null!");
            return;
        }
        
        var background = GameObject.CreatePrimitive(PrimitiveType.Quad);
        background.name = "BlackBackground";
        if (background != null && background.transform != null && environmentRoot != null && environmentRoot.transform != null)
        {
            background.transform.SafeSetParent(environmentRoot.transform);
        }
        else
        {
            Debug.LogError("[BallTrackingController] background or environmentRoot transform is null!");
            return;
        }
        background.transform.position = centerPosition + new Vector3(0, 0, 5f);
        background.transform.localScale = new Vector3(20f, 20f, 1f);
        background.transform.rotation = Quaternion.LookRotation(Vector3.back);
        
        var bgMat = new Material(Shader.Find("Standard"));
        bgMat.color = Color.black;
        bgMat.SetFloat("_Metallic", 0f);
        bgMat.SetFloat("_Glossiness", 0f);
        background.GetComponent<MeshRenderer>().material = bgMat;
        
        // Создаем шарик
        ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "TrackingBall";
        if (ball != null && ball.transform != null && environmentRoot != null && environmentRoot.transform != null)
        {
            ball.transform.SafeSetParent(environmentRoot.transform);
        }
        else
        {
            Debug.LogError("[BallTrackingController] ball or environmentRoot transform is null!");
            return;
        }
        ball.transform.position = centerPosition + new Vector3(0, movementHeight, 0);
        ball.transform.localScale = Vector3.one * ballRadius * 2f;
        
        var ballMat = new Material(Shader.Find("Standard"));
        ballMat.color = ballColor;
        ballMat.EnableKeyword("_EMISSION");
        ballMat.SetColor("_EmissionColor", ballColor * 2f);
        ballMat.SetFloat("_Metallic", 0.2f);
        ballMat.SetFloat("_Glossiness", 0.8f);
        ball.GetComponent<MeshRenderer>().material = ballMat;
        
        // Добавляем легкое свечение
        var light = new GameObject("BallLight");
        if (light != null && light.transform != null && ball != null && ball.transform != null)
        {
            light.transform.SafeSetParent(ball.transform);
        }
        else
        {
            Debug.LogError("[BallTrackingController] light or ball transform is null!");
            return;
        }
        light.transform.localPosition = Vector3.zero;
        var lightComp = light.AddComponent<Light>();
        lightComp.type = LightType.Point;
        lightComp.color = ballColor;
        lightComp.intensity = 1.5f;
        lightComp.range = 3f;
    }

    public void StartTracking()
    {
        Debug.Log("[BallTracking] Starting ball tracking mode");
        isActive = true;
        
        if (environmentRoot != null)
        {
            environmentRoot.SetActive(true);
            Debug.Log("[BallTracking] Environment activated");
        }
        
        // Устанавливаем черный фон
        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
        }
        
        RenderSettings.fog = false;
        RenderSettings.ambientLight = Color.black;
    }

    public void StopTracking()
    {
        Debug.Log("[BallTracking] Stopping ball tracking mode");
        isActive = false;
        
        if (environmentRoot != null)
        {
            environmentRoot.SetActive(false);
        }
    }

    public bool IsActive => isActive;
}

