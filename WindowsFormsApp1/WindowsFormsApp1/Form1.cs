using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private UdpClient udp;
        private IPEndPoint remoteEndPoint;
        private const int MaxLogEntries = 200;
        
        // Video streaming
        private UdpClient streamUdp;
        private IPEndPoint streamEndPoint;
        private const int streamPort = 9877;
        private Thread streamThread;
        private bool isStreaming = false;

        public Form1()
        {
            InitializeComponent();

            trackSud.Minimum = 0;
            trackSud.Maximum = 10;
            trackSud.Value = 0;
            lblSud.Text = "Уровень SUD: 0";
            txtIp.Text = "127.0.0.1";
            numPort.Value = 9876;

            ConfigureConnection();
            StartVideoStream();
            AppendLog("Пульт готов. Настройте соединение и отправляйте команды.");
        }
        
        private void StartVideoStream()
        {
            try
            {
                streamUdp = new UdpClient(streamPort);
                streamUdp.Client.ReceiveBufferSize = 1024 * 1024; // 1MB buffer
                streamEndPoint = new IPEndPoint(IPAddress.Any, streamPort);
                isStreaming = true;
                streamThread = new Thread(ReceiveVideoStream) { IsBackground = true };
                streamThread.Start();
                AppendLog($"Видеопоток запущен на порту {streamPort}");
                if (InvokeRequired)
                {
                    Invoke(new Action(() => picVRView.BackColor = Color.Black));
                }
                else
                {
                    picVRView.BackColor = Color.Black;
                }
            }
            catch (Exception ex)
            {
                AppendLog($"Ошибка запуска видеопотока: {ex.Message}");
                AppendLog($"Детали: {ex.StackTrace}");
            }
        }
        
        private int framesReceived = 0;
        private DateTime lastFrameTime = DateTime.MinValue;
        
        private void ReceiveVideoStream()
        {
            AppendLog("Поток приема видео запущен, ожидание данных...");
            
            while (isStreaming)
            {
                try
                {
                    byte[] data = streamUdp.Receive(ref streamEndPoint);
                    
                    // Log first packet received
                    if (framesReceived == 0)
                    {
                        if (InvokeRequired)
                        {
                            Invoke(new Action(() => AppendLog($"Получен первый пакет: {data.Length} байт от {streamEndPoint}")));
                        }
                        else
                        {
                            AppendLog($"Получен первый пакет: {data.Length} байт от {streamEndPoint}");
                        }
                    }
                    
                    // Check for frame header "FRAME:"
                    if (data.Length < 6) 
                    {
                        if (framesReceived == 0 && InvokeRequired)
                        {
                            Invoke(new Action(() => AppendLog($"Пакет слишком мал: {data.Length} байт")));
                        }
                        continue;
                    }
                    
                    string header = Encoding.UTF8.GetString(data, 0, 6);
                    if (header == "FRAME:")
                    {
                        // Read frame length (4 bytes after header)
                        if (data.Length >= 10)
                        {
                            int frameLength = BitConverter.ToInt32(data, 6);
                            
                            // Check if we have complete frame
                            if (data.Length >= 10 + frameLength)
                            {
                                // Extract JPEG data
                                byte[] jpegData = new byte[frameLength];
                                Array.Copy(data, 10, jpegData, 0, frameLength);
                                
                                framesReceived++;
                                lastFrameTime = DateTime.Now;
                                
                                // Update picture box on UI thread
                                if (picVRView.InvokeRequired)
                                {
                                    picVRView.Invoke(new Action<byte[]>(UpdateVideoFrame), jpegData);
                                }
                                else
                                {
                                    UpdateVideoFrame(jpegData);
                                }
                                
                                // Log every 30 frames
                                if (framesReceived % 30 == 0 && InvokeRequired)
                                {
                                    Invoke(new Action(() => AppendLog($"Получено кадров: {framesReceived}, размер: {frameLength} байт")));
                                }
                            }
                            else
                            {
                                if (framesReceived == 0 && InvokeRequired)
                                {
                                    Invoke(new Action(() => AppendLog($"Неполный пакет: получено {data.Length}, ожидается {10 + frameLength}")));
                                }
                            }
                        }
                        else
                        {
                            if (framesReceived == 0 && InvokeRequired)
                            {
                                Invoke(new Action(() => AppendLog($"Пакет слишком мал для заголовка: {data.Length} байт")));
                            }
                        }
                    }
                    else
                    {
                        if (framesReceived == 0 && InvokeRequired)
                        {
                            Invoke(new Action(() => AppendLog($"Неверный заголовок: '{header}' (ожидается 'FRAME:')")));
                        }
                    }
                }
                catch (SocketException ex)
                {
                    // Socket closed or error
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => AppendLog($"SocketException в видеопотоке: {ex.Message}")));
                    }
                    break;
                }
                catch (Exception ex)
                {
                    // Log error but continue
                    if (InvokeRequired)
                    {
                        Invoke(new Action<string>(s => AppendLog($"Ошибка видеопотока: {s}")), ex.Message);
                    }
                }
            }
            
            if (InvokeRequired)
            {
                Invoke(new Action(() => AppendLog("Поток приема видео остановлен")));
            }
        }
        
        private void UpdateVideoFrame(byte[] jpegData)
        {
            try
            {
                if (jpegData == null || jpegData.Length == 0)
                {
                    return;
                }
                
                using (MemoryStream ms = new MemoryStream(jpegData))
                {
                    Image img = Image.FromStream(ms);
                    if (img != null)
                    {
                        Image oldImg = picVRView.Image;
                        picVRView.Image = new Bitmap(img);
                        oldImg?.Dispose();
                        img.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log decode errors occasionally
                if (framesReceived % 60 == 0)
                {
                    AppendLog($"Ошибка декодирования JPEG: {ex.Message}");
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void ConfigureConnection()
        {
            try
            {
                var ipText = txtIp.Text.Trim();
                if (!IPAddress.TryParse(ipText, out var ipAddress))
                {
                    UpdateConnectionStatus("Некорректный IP", Color.Firebrick);
                    return;
                }

                int port = (int)numPort.Value;
                udp?.Close();
                udp = new UdpClient();
                remoteEndPoint = new IPEndPoint(ipAddress, port);

                UpdateConnectionStatus($"Подключено: {ipText}:{port}", Color.FromArgb(0, 120, 215));
                AppendLog($"Соединение настроено на {ipText}:{port}");
            }
            catch (Exception ex)
            {
                UpdateConnectionStatus("Ошибка подключения", Color.Firebrick);
                MessageBox.Show("Не удалось настроить соединение: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateConnectionStatus(string text, Color color)
        {
            lblConnectionStatus.Text = text;
            lblConnectionStatus.ForeColor = color;
        }

        private void AppendLog(string message)
        {
            string entry = $"{DateTime.Now:HH:mm:ss}  {message}";
            lstLog.BeginUpdate();
            lstLog.Items.Insert(0, entry);

            while (lstLog.Items.Count > MaxLogEntries)
            {
                lstLog.Items.RemoveAt(lstLog.Items.Count - 1);
            }

            lstLog.EndUpdate();
        }

        private void Send(string message)
        {
            if (remoteEndPoint == null)
            {
                AppendLog("Соединение не настроено.");
                return;
            }

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                udp.Send(data, data.Length, remoteEndPoint);
                AppendLog($"→ {message}");
            }
            catch (Exception ex)
            {
                AppendLog("Ошибка отправки: " + ex.Message);
                MessageBox.Show("Ошибка отправки: " + ex.Message);
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            Send("PAUSE");
        }

        private void btnEmdr_Click(object sender, EventArgs e)
        {
            // Если в Unity ожидается просто "EMDR"
            Send("EMDR");

            // если ты хочешь сразу запросить Pre-SUD:
            // Send("ASK_SUD");
        }

        private void btnSafe_Click(object sender, EventArgs e)
        {
            Send("SAFE");
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Send("STOP");
        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            Send("FINISH");
        }

        private void btnResume_Click(object sender, EventArgs e)
        {
            Send("RESUME");
        }

        private void btnAskSud_Click(object sender, EventArgs e)
        {
            Send("ASK_SUD");
        }

        private void trackSud_Scroll(object sender, EventArgs e)
        {
            int v = trackSud.Value;
            lblSud.Text = $"Уровень SUD: {v}";
            Send($"SUD:{v}");
        }

        private void btnSendNote_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNote.Text))
                return;

            Send("NOTE:" + txtNote.Text.Trim());
            txtNote.Clear();
        }

        private void btnWorldForest_Click(object sender, EventArgs e)
        {
            AppendLog("Нажата кнопка 'Лес'");
            Send("WORLD:FOREST");
            AppendLog("Отправлена команда: WORLD:FOREST");
        }

        private void btnWorldSea_Click(object sender, EventArgs e)
        {
            AppendLog("Нажата кнопка 'Море'");
            Send("WORLD:SEA");
            AppendLog("Отправлена команда: WORLD:SEA");
        }

        private void btnWorldMountains_Click(object sender, EventArgs e)
        {
            Send("WORLD:MOUNTAINS");
        }

        private void btnLoadVideo_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Video files (*.mp4;*.avi;*.mov;*.wmv)|*.mp4;*.avi;*.mov;*.wmv|All files (*.*)|*.*";
                ofd.FilterIndex = 1;
                ofd.RestoreDirectory = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string videoPath = ofd.FileName;
                    Send($"VIDEO:{videoPath}");
                    AppendLog($"Видео загружено: {videoPath}");
                }
            }
        }

        private void btnApplyConnection_Click(object sender, EventArgs e)
        {
            ConfigureConnection();
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            lstLog.Items.Clear();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            isStreaming = false;
            streamUdp?.Close();
            streamThread?.Join(1000);
            udp?.Close();
        }
    }
}
