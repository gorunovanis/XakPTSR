using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TherapistRemoteController : MonoBehaviour
{
    public int listenPort = 7777;
    private TcpListener listener;
    private Thread listenThread;
    private bool running;

    [Serializable]
    public class Command
    {
        public string type;
        public float value;
        public string payload;
    }

    private void OnEnable()
    {
        StartServer();
    }

    private void OnDisable()
    {
        StopServer();
    }

    public void StartServer()
    {
        if (running) return;

        try
        {
            listener = new TcpListener(IPAddress.Any, listenPort);
            listener.Start();
            running = true;
            listenThread = new Thread(ListenLoop);
            listenThread.IsBackground = true;
            listenThread.Start();
            Debug.Log($"[TherapistRemote] Listening on port {listenPort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[TherapistRemote] Failed to start server: {e}");
        }
    }

    public void StopServer()
    {
        running = false;
        try { listener?.Stop(); } catch { }

        if (listenThread != null && listenThread.IsAlive)
            listenThread.Abort();
    }

    private void ListenLoop()
    {
        while (running)
        {
            try
            {
                var client = listener.AcceptTcpClient();
                var stream = client.GetStream();

                byte[] buffer = new byte[4096];
                int read = stream.Read(buffer, 0, buffer.Length);

                if (read > 0)
                {
                    string json = Encoding.UTF8.GetString(buffer, 0, read);
                    var cmd = JsonUtility.FromJson<Command>(json);

                    if (cmd != null)
                    {
                        UnityMainThreadDispatcher.Enqueue(() =>
                        {
                            ExecuteCommand(cmd);
                        });
                    }
                }

                client.Close();
            }
            catch (SocketException)
            {
                break;
            }
            catch (Exception e)
            {
                Debug.LogError($"[TherapistRemote] Error: {e}");
            }
        }
    }

    private void ExecuteCommand(Command cmd)
    {
        var sm = SessionManager.Instance;
        if (sm == null) return;

        switch (cmd.type)
        {
            case "pause": sm.Pause(); break;
            case "resume": sm.ResumeExposure(); break;
            case "emergency": sm.EmergencyStop(); break;
            case "go_emdr": sm.GoToEMDR(Mathf.RoundToInt(cmd.value)); break;
            case "safe_place": sm.GoToSafePlace(); break;

            case "set_volume":
                sm.exposureController?.SetVolume(cmd.value);
                break;

            case "set_density":
                sm.exposureController?.SetEventDensity(cmd.value);
                break;

            case "set_flash":
                sm.exposureController?.SetFlashIntensity(cmd.value);
                break;

            case "ask_sud":
                sm.AskSud("REMOTE_SUD");
                break;

            default:
                Debug.LogWarning($"[TherapistRemote] Unknown command: {cmd.type}");
                break;
        }
    }
}
