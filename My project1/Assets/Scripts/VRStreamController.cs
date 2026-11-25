using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// Captures the main camera view and streams it to the therapist panel via UDP.
/// Sends JPEG-compressed frames at a configurable framerate.
/// </summary>
public class VRStreamController : MonoBehaviour
{
    [Header("Streaming")]
    public Camera streamCamera;
    public int streamPort = 9877; // Different port from commands
    public int targetFPS = 15; // Lower FPS to reduce bandwidth
    public int jpegQuality = 75; // 0-100, lower = smaller files
    public bool useBroadcast = true; // Use broadcast instead of specific IP
    
    [Header("Resolution")]
    public int streamWidth = 640;
    public int streamHeight = 360;
    
    private RenderTexture renderTexture;
    private Texture2D texture2D;
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;
    private float frameInterval;
    private float lastFrameTime;
    private bool isStreaming;

    void Start()
    {
        if (streamCamera == null)
        {
            // Try to find main camera or XR camera
            streamCamera = Camera.main;
            if (streamCamera == null)
            {
                streamCamera = FindFirstObjectByType<Camera>();
            }
        }
        
        if (streamCamera == null)
        {
            Debug.LogError("[VRStream] No camera found!");
            enabled = false;
            return;
        }

        frameInterval = 1f / targetFPS;
        
        // Create render texture (we'll render to it manually, not set as targetTexture)
        renderTexture = new RenderTexture(streamWidth, streamHeight, 24);
        renderTexture.Create();
        
        // Create texture for reading
        texture2D = new Texture2D(streamWidth, streamHeight, TextureFormat.RGB24, false);
        
        // Setup UDP (will send to broadcast or specific IP)
        try
        {
            udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            
            // Setup endpoint based on useBroadcast setting
            if (useBroadcast)
            {
                remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, streamPort);
                Debug.Log($"[VRStream] UDP client created, sending to broadcast: {remoteEndPoint}");
            }
            else
            {
                // Try to find local IP address, fallback to localhost
                try
                {
                    string localIP = GetLocalIPAddress();
                    if (!string.IsNullOrEmpty(localIP))
                    {
                        remoteEndPoint = new IPEndPoint(IPAddress.Parse(localIP), streamPort);
                        Debug.Log($"[VRStream] UDP client created, sending to local IP: {remoteEndPoint}");
                    }
                    else
                    {
                        // Fallback to localhost
                        remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), streamPort);
                        Debug.Log($"[VRStream] UDP client created, sending to localhost: {remoteEndPoint}");
                    }
                }
                catch
                {
                    // Fallback to localhost
                    remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), streamPort);
                    Debug.Log($"[VRStream] UDP client created, sending to localhost: {remoteEndPoint}");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[VRStream] Failed to create UDP client: {ex.Message}");
            enabled = false;
            return;
        }
        
        isStreaming = true;
        Debug.Log($"[VRStream] Started streaming from {streamCamera.name} at {streamWidth}x{streamHeight} @ {targetFPS} FPS");
        Debug.Log($"[VRStream] Camera enabled: {streamCamera.enabled}, gameObject active: {streamCamera.gameObject.activeInHierarchy}");
    }

    void OnDisable()
    {
        StopStreaming();
    }
    
    void OnDestroy()
    {
        StopStreaming();
    }
    
    void StopStreaming()
    {
        isStreaming = false;
        
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
            renderTexture = null;
        }
        
        if (texture2D != null)
        {
            Destroy(texture2D);
            texture2D = null;
        }
        
        if (udpClient != null)
        {
            try
            {
                udpClient.Close();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[VRStream] Error closing UDP client: {ex.Message}");
            }
            udpClient = null;
        }
        
        remoteEndPoint = null;
    }

    void Update()
    {
        if (!isStreaming || Time.time - lastFrameTime < frameInterval)
            return;
        
        // Проверяем, что все необходимые компоненты инициализированы
        if (udpClient == null || remoteEndPoint == null || streamCamera == null || renderTexture == null || texture2D == null)
        {
            // Пропускаем этот кадр, если что-то не готово
            return;
        }

        lastFrameTime = Time.time;
        StartCoroutine(CaptureAndSendFrame());
    }

    IEnumerator CaptureAndSendFrame()
    {
        yield return new WaitForEndOfFrame();
        
        if (streamCamera == null || !streamCamera.enabled)
        {
            Debug.LogWarning("[VRStream] Camera is null or disabled!");
            yield break;
        }
        
        try
        {
            // Always use render texture approach - render camera to texture
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            
            // Clear render texture
            GL.Clear(true, true, Color.black);
            
            // Render camera to texture
            streamCamera.Render();
            
            // Read pixels from render texture
            texture2D.ReadPixels(new Rect(0, 0, streamWidth, streamHeight), 0, 0);
            texture2D.Apply();
            
            RenderTexture.active = previous;
            
            // Convert to JPEG
            byte[] jpegData = texture2D.EncodeToJPG(jpegQuality);
            
            if (jpegData == null || jpegData.Length == 0)
            {
                Debug.LogWarning("[VRStream] JPEG encoding failed or returned empty data");
                yield break;
            }
            
            // UDP max packet size is typically 65507, but safer to use smaller chunks
            // For now, send as single packet if small enough, otherwise split
            int maxPacketSize = 60000; // Safe size for UDP
            
            if (jpegData.Length > maxPacketSize)
            {
                Debug.LogWarning($"[VRStream] Frame too large ({jpegData.Length} bytes), skipping");
                yield break;
            }
            
            // Send frame header + data
            // Format: "FRAME:" + length (4 bytes) + jpeg data
            byte[] header = System.Text.Encoding.UTF8.GetBytes("FRAME:");
            byte[] lengthBytes = System.BitConverter.GetBytes(jpegData.Length);
            
            byte[] packet = new byte[header.Length + lengthBytes.Length + jpegData.Length];
            System.Buffer.BlockCopy(header, 0, packet, 0, header.Length);
            System.Buffer.BlockCopy(lengthBytes, 0, packet, header.Length, lengthBytes.Length);
            System.Buffer.BlockCopy(jpegData, 0, packet, header.Length + lengthBytes.Length, jpegData.Length);
            
            // Проверяем, что UDP клиент и endpoint не null
            if (udpClient == null || remoteEndPoint == null)
            {
                Debug.LogWarning("[VRStream] UDP client or endpoint is null, cannot send frame");
                yield break;
            }
            
            try
            {
                udpClient.Send(packet, packet.Length, remoteEndPoint);
            }
            catch (System.Exception sendEx)
            {
                Debug.LogWarning($"[VRStream] Failed to send UDP packet: {sendEx.Message}");
                yield break;
            }
            
            // Log every 30 frames (2 seconds at 15 FPS)
            if (Time.frameCount % 30 == 0)
            {
                Debug.Log($"[VRStream] Sent frame {Time.frameCount}, size: {jpegData.Length} bytes");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[VRStream] Error capturing/sending frame: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public void SetStreamingIP(string ip)
    {
        if (IPAddress.TryParse(ip, out IPAddress address))
        {
            remoteEndPoint = new IPEndPoint(address, streamPort);
            Debug.Log($"[VRStream] Streaming target set to {ip}:{streamPort}");
        }
        else
        {
            Debug.LogWarning($"[VRStream] Invalid IP address: {ip}");
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        isStreaming = !pauseStatus;
        Debug.Log($"[VRStream] Streaming {(isStreaming ? "resumed" : "paused")}");
    }
    
    private string GetLocalIPAddress()
    {
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    // Skip loopback and link-local addresses
                    if (!IPAddress.IsLoopback(ip) && !ip.ToString().StartsWith("169.254"))
                    {
                        return ip.ToString();
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[VRStream] Failed to get local IP: {ex.Message}");
        }
        return null;
    }
}

