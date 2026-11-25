using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

/// Простейший UDP-мост для управления сессией по LAN.
/// Формат сообщения (строки):
/// "PAUSE", "STOP", "EMDR", "SAFE", "FINISH",
/// "SUD:7", "NOTE:Текст заметки",
/// "ASK_SUD_PRE", "ASK_SUD_POST".
public class TherapistNetworkLink : MonoBehaviour
{
    [Header("Режим")]
    public bool therapistSide = false; // true = отправитель (панель), false = приёмник (VR)

    [Header("Сеть")]
    public int port = 9876;

    UdpClient udp;
    IPEndPoint remoteEndPoint;

    // Чтобы не трогать Unity API из фонового потока:
    readonly Queue<string> inbox = new Queue<string>();
    readonly object inboxLock = new object();

    void Start()
    {
        if (therapistSide)
        {
            udp = new UdpClient();
            remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
            udp.EnableBroadcast = true;
            Debug.Log("[NET] TherapistNetworkLink: sender mode, port " + port);
        }
        else
        {
            udp = new UdpClient(port);
            udp.BeginReceive(OnReceive, null);
            Debug.Log("[NET] TherapistNetworkLink: receiver mode, port " + port);
        }
    }

    void OnDestroy()
    {
        udp?.Close();
    }

    // ================== ОТПРАВКА (панель) ==================

    public void SendCommand(string cmd)
    {
        if (!therapistSide || udp == null) return;

        byte[] data = Encoding.UTF8.GetBytes(cmd);
        udp.Send(data, data.Length, remoteEndPoint);
        Debug.Log("[Therapist CMD] " + cmd);
    }

    // Можешь вызывать их из WinForms/WPF или из Unity-кнопок,
    // ОТПРАВКА (панель / WinForms)
    public void CmdPause() => SendCommand("PAUSE");
    public void CmdStop() => SendCommand("STOP");
    public void CmdEMDR() => SendCommand("EMDR");
    public void CmdSafePlace() => SendCommand("SAFE");
    public void CmdFinish() => SendCommand("FINISH");

    public void CmdSafeForest() => SendCommand("SAFE_FOREST");
    public void CmdSafeSea() => SendCommand("SAFE_SEA");
    public void CmdSafeMountains() => SendCommand("SAFE_MOUNTAINS");

    public void CmdAskPreSud() => SendCommand("ASK_SUD_PRE");
    public void CmdAskPostSud() => SendCommand("ASK_SUD_POST");
    public void CmdSetSud(int v) => SendCommand($"SUD:{Mathf.Clamp(v, 0, 10)}");
    public void CmdNote(string n) => SendCommand("NOTE:" + n);


    // ================== ПРИЁМ (VR-сторона) ==================

    void OnReceive(System.IAsyncResult ar)
    {
        try
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            byte[] data = udp.EndReceive(ar, ref ep);
            string msg = Encoding.UTF8.GetString(data);

            lock (inboxLock)
                inbox.Enqueue(msg);
        }
        catch { }

        if (udp != null)
            udp.BeginReceive(OnReceive, null);
    }

    void Update()
    {
        // вытаскиваем все сообщения, пришедшие с предыдущего кадра
        while (true)
        {
            string msg = null;
            lock (inboxLock)
            {
                if (inbox.Count > 0)
                    msg = inbox.Dequeue();
            }

            if (msg == null)
                break;

            HandleMessage(msg);
        }
    }

    void HandleMessage(string msg)
    {
        Debug.Log("[Therapist CMD RECEIVED] " + msg);

        var sm = SessionManager.Instance;
        if (sm == null)
        {
            Debug.LogWarning("[TherapistNetworkLink] SessionManager.Instance == null");
            return;
        }

        switch (msg)
        {
            case "PAUSE": sm.RemotePause(); return;
            case "RESUME": sm.ResumeExposure(); return;
            case "STOP": sm.RemoteEmergencyStop(); return;
            case "EMDR": sm.RemoteStartEMDR(); return;
            case "SAFE": sm.RemoteSafePlace(); return;
            case "FINISH": sm.RemoteFinish(); return;

            case "SAFE_FOREST": sm.RemoteSafeForest(); return;
            case "SAFE_SEA": sm.RemoteSafeSea(); return;
            case "SAFE_MOUNTAINS": sm.RemoteSafeMountains(); return;

            case "ASK_SUD": sm.RemoteAskSud("REMOTE_SUD_REQUEST"); return;
            case "ASK_SUD_PRE": sm.RemoteAskSud("REMOTE_PRE_SUD"); return;
            case "ASK_SUD_POST": sm.RemoteAskSud("REMOTE_POST_SUD"); return;
        }


        // остальной код SUD:/NOTE: оставь как был
        if (msg.StartsWith("SUD:"))
        {
            if (int.TryParse(msg.Substring(4), out int v))
                sm.RemoteSetSud(v);
            return;
        }

        if (msg.StartsWith("NOTE:"))
        {
            string note = msg.Substring(5);
            sm.RemoteAddNote(note);
            return;
        }

        if (msg.StartsWith("WORLD:", System.StringComparison.OrdinalIgnoreCase))
        {
            string token = msg.Substring(6).Trim().ToUpperInvariant();
            var worldCtrl = WorldSceneController.Instance;
            if (worldCtrl == null)
            {
                Debug.LogWarning("[TherapistNetworkLink] WorldSceneController.Instance == null, WORLD command ignored.");
                return;
            }

            switch (token)
            {
                case "FOREST":
                    worldCtrl.LoadForestWorld();
                    break;
                case "SEA":
                    worldCtrl.LoadSeaWorld();
                    break;
                case "MOUNTAINS":
                case "MOUNTAIN":
                    worldCtrl.LoadMountainWorld();
                    break;
                case "SAFEPLACE":
                case "SAFE_PLACE":
                    worldCtrl.LoadWorld(WorldType.SafePlace);
                    break;
                case "WARMODE":
                case "WAR_MODE":
                    worldCtrl.LoadWorld(WorldType.WarMode);
                    break;
                default:
                    Debug.LogWarning("[TherapistNetworkLink] Unknown WORLD token: " + token);
                    break;
            }

            return;
        }

        if (msg.StartsWith("VIDEO:", System.StringComparison.OrdinalIgnoreCase))
        {
            string videoPath = msg.Substring(6).Trim();
            Debug.Log($"[TherapistNetworkLink] Received VIDEO command: {videoPath}");
            
            var worldCtrl = WorldSceneController.Instance;
            if (worldCtrl == null)
            {
                Debug.LogWarning("[TherapistNetworkLink] WorldSceneController.Instance == null, VIDEO command ignored.");
                return;
            }

            // Убеждаемся, что WarModeController существует
            if (worldCtrl.warModeController == null)
            {
                Debug.LogWarning("[TherapistNetworkLink] WarModeController is null, creating...");
                worldCtrl.LoadWorld(WorldType.WarMode); // Это создаст контроллер
            }

            if (worldCtrl.warModeController != null)
            {
                worldCtrl.warModeController.LoadVideo(videoPath);
                Debug.Log($"[TherapistNetworkLink] Video loaded: {videoPath}");
            }
            else
            {
                Debug.LogError("[TherapistNetworkLink] Failed to create WarModeController");
            }

            return;
        }
    }

}
