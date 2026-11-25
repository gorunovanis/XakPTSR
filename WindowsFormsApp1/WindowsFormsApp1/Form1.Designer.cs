namespace WindowsFormsApp1
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.layoutRoot = new System.Windows.Forms.TableLayoutPanel();
            this.headerPanel = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.grpConnection = new System.Windows.Forms.GroupBox();
            this.flowConnection = new System.Windows.Forms.FlowLayoutPanel();
            this.labelIp = new System.Windows.Forms.Label();
            this.txtIp = new System.Windows.Forms.TextBox();
            this.labelPort = new System.Windows.Forms.Label();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.btnApplyConnection = new System.Windows.Forms.Button();
            this.lblConnectionStatus = new System.Windows.Forms.Label();
            this.grpVRView = new System.Windows.Forms.GroupBox();
            this.picVRView = new System.Windows.Forms.PictureBox();
            this.grpSession = new System.Windows.Forms.GroupBox();
            this.flowSession = new System.Windows.Forms.FlowLayoutPanel();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnResume = new System.Windows.Forms.Button();
            this.btnEmdr = new System.Windows.Forms.Button();
            this.btnSafe = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnFinish = new System.Windows.Forms.Button();
            this.btnAskSud = new System.Windows.Forms.Button();
            this.grpWorlds = new System.Windows.Forms.GroupBox();
            this.flowWorlds = new System.Windows.Forms.FlowLayoutPanel();
            this.btnWorldForest = new System.Windows.Forms.Button();
            this.btnWorldSea = new System.Windows.Forms.Button();
            this.btnLoadVideo = new System.Windows.Forms.Button();
            this.grpSud = new System.Windows.Forms.GroupBox();
            this.lblSud = new System.Windows.Forms.Label();
            this.trackSud = new System.Windows.Forms.TrackBar();
            this.grpNotes = new System.Windows.Forms.GroupBox();
            this.txtNote = new System.Windows.Forms.TextBox();
            this.btnSendNote = new System.Windows.Forms.Button();
            this.grpLog = new System.Windows.Forms.GroupBox();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.lstLog = new System.Windows.Forms.ListBox();
            this.layoutRoot.SuspendLayout();
            this.headerPanel.SuspendLayout();
            this.grpConnection.SuspendLayout();
            this.flowConnection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.grpVRView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picVRView)).BeginInit();
            this.grpSession.SuspendLayout();
            this.flowSession.SuspendLayout();
            this.grpWorlds.SuspendLayout();
            this.flowWorlds.SuspendLayout();
            this.grpSud.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackSud)).BeginInit();
            this.grpNotes.SuspendLayout();
            this.grpLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // layoutRoot
            // 
            this.layoutRoot.ColumnCount = 2;
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.layoutRoot.Controls.Add(this.headerPanel, 0, 0);
            this.layoutRoot.Controls.Add(this.grpConnection, 0, 1);
            this.layoutRoot.Controls.Add(this.grpVRView, 1, 1);
            this.layoutRoot.Controls.Add(this.grpSession, 0, 2);
            this.layoutRoot.Controls.Add(this.grpWorlds, 1, 2);
            this.layoutRoot.Controls.Add(this.grpSud, 0, 3);
            this.layoutRoot.Controls.Add(this.grpNotes, 1, 3);
            this.layoutRoot.Controls.Add(this.grpLog, 0, 4);
            this.layoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRoot.Location = new System.Drawing.Point(0, 0);
            this.layoutRoot.Name = "layoutRoot";
            this.layoutRoot.RowCount = 5;
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.layoutRoot.Size = new System.Drawing.Size(1024, 600);
            this.layoutRoot.TabIndex = 0;
            // 
            // headerPanel
            // 
            this.headerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(28)))));
            this.layoutRoot.SetColumnSpan(this.headerPanel, 2);
            this.headerPanel.Controls.Add(this.lblTitle);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerPanel.Location = new System.Drawing.Point(0, 0);
            this.headerPanel.Margin = new System.Windows.Forms.Padding(0);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(1024, 60);
            this.headerPanel.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(20, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(347, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "VR Пульт — управление сессией";
            // 
            // grpConnection
            // 
            this.layoutRoot.SetColumnSpan(this.grpConnection, 2);
            this.grpConnection.Controls.Add(this.flowConnection);
            this.grpConnection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpConnection.ForeColor = System.Drawing.Color.White;
            this.grpConnection.Location = new System.Drawing.Point(10, 70);
            this.grpConnection.Margin = new System.Windows.Forms.Padding(10);
            this.grpConnection.Name = "grpConnection";
            this.grpConnection.Padding = new System.Windows.Forms.Padding(10);
            this.grpConnection.Size = new System.Drawing.Size(1004, 70);
            this.grpConnection.TabIndex = 1;
            this.grpConnection.TabStop = false;
            this.grpConnection.Text = "Соединение";
            // 
            // flowConnection
            // 
            this.flowConnection.AutoSize = true;
            this.flowConnection.Controls.Add(this.labelIp);
            this.flowConnection.Controls.Add(this.txtIp);
            this.flowConnection.Controls.Add(this.labelPort);
            this.flowConnection.Controls.Add(this.numPort);
            this.flowConnection.Controls.Add(this.btnApplyConnection);
            this.flowConnection.Controls.Add(this.lblConnectionStatus);
            this.flowConnection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowConnection.Location = new System.Drawing.Point(10, 26);
            this.flowConnection.Name = "flowConnection";
            this.flowConnection.Size = new System.Drawing.Size(984, 34);
            this.flowConnection.TabIndex = 0;
            this.flowConnection.WrapContents = false;
            // 
            // labelIp
            // 
            this.labelIp.AutoSize = true;
            this.labelIp.Location = new System.Drawing.Point(3, 6);
            this.labelIp.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.labelIp.Name = "labelIp";
            this.labelIp.Size = new System.Drawing.Size(60, 15);
            this.labelIp.TabIndex = 0;
            this.labelIp.Text = "VR-адрес:";
            // 
            // txtIp
            // 
            this.txtIp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtIp.Location = new System.Drawing.Point(69, 3);
            this.txtIp.Name = "txtIp";
            this.txtIp.Size = new System.Drawing.Size(140, 23);
            this.txtIp.TabIndex = 1;
            // 
            // labelPort
            // 
            this.labelPort.AutoSize = true;
            this.labelPort.Location = new System.Drawing.Point(215, 6);
            this.labelPort.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(38, 15);
            this.labelPort.TabIndex = 2;
            this.labelPort.Text = "Порт:";
            // 
            // numPort
            // 
            this.numPort.Location = new System.Drawing.Point(259, 3);
            this.numPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numPort.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numPort.Name = "numPort";
            this.numPort.Size = new System.Drawing.Size(80, 23);
            this.numPort.TabIndex = 3;
            this.numPort.Value = new decimal(new int[] {
            9876,
            0,
            0,
            0});
            // 
            // btnApplyConnection
            // 
            this.btnApplyConnection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnApplyConnection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApplyConnection.ForeColor = System.Drawing.Color.White;
            this.btnApplyConnection.Location = new System.Drawing.Point(345, 3);
            this.btnApplyConnection.Name = "btnApplyConnection";
            this.btnApplyConnection.Size = new System.Drawing.Size(120, 27);
            this.btnApplyConnection.TabIndex = 4;
            this.btnApplyConnection.Text = "Применить";
            this.btnApplyConnection.UseVisualStyleBackColor = false;
            this.btnApplyConnection.Click += new System.EventHandler(this.btnApplyConnection_Click);
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.AutoSize = true;
            this.lblConnectionStatus.ForeColor = System.Drawing.Color.LightGray;
            this.lblConnectionStatus.Location = new System.Drawing.Point(471, 6);
            this.lblConnectionStatus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(130, 15);
            this.lblConnectionStatus.TabIndex = 5;
            this.lblConnectionStatus.Text = "Соединение не задано";
            // 
            // grpVRView
            // 
            this.grpVRView.Controls.Add(this.picVRView);
            this.grpVRView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpVRView.ForeColor = System.Drawing.Color.White;
            this.grpVRView.Location = new System.Drawing.Point(10, 160);
            this.grpVRView.Margin = new System.Windows.Forms.Padding(10);
            this.grpVRView.Name = "grpVRView";
            this.grpVRView.Padding = new System.Windows.Forms.Padding(10);
            this.grpVRView.Size = new System.Drawing.Size(594, 109);
            this.grpVRView.TabIndex = 7;
            this.grpVRView.TabStop = false;
            this.grpVRView.Text = "Вид из VR";
            // 
            // picVRView
            // 
            this.picVRView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picVRView.Location = new System.Drawing.Point(10, 26);
            this.picVRView.Name = "picVRView";
            this.picVRView.Size = new System.Drawing.Size(574, 73);
            this.picVRView.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picVRView.TabIndex = 0;
            this.picVRView.TabStop = false;
            // 
            // grpSession
            // 
            this.grpSession.Controls.Add(this.flowSession);
            this.grpSession.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpSession.ForeColor = System.Drawing.Color.White;
            this.grpSession.Location = new System.Drawing.Point(624, 160);
            this.grpSession.Margin = new System.Windows.Forms.Padding(10);
            this.grpSession.Name = "grpSession";
            this.grpSession.Padding = new System.Windows.Forms.Padding(10);
            this.grpSession.Size = new System.Drawing.Size(390, 109);
            this.grpSession.TabIndex = 2;
            this.grpSession.TabStop = false;
            this.grpSession.Text = "Управление сессией";
            // 
            // flowSession
            // 
            this.flowSession.Controls.Add(this.btnPause);
            this.flowSession.Controls.Add(this.btnResume);
            this.flowSession.Controls.Add(this.btnEmdr);
            this.flowSession.Controls.Add(this.btnSafe);
            this.flowSession.Controls.Add(this.btnStop);
            this.flowSession.Controls.Add(this.btnFinish);
            this.flowSession.Controls.Add(this.btnAskSud);
            this.flowSession.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowSession.Location = new System.Drawing.Point(10, 26);
            this.flowSession.Name = "flowSession";
            this.flowSession.Size = new System.Drawing.Size(370, 73);
            this.flowSession.TabIndex = 0;
            // 
            // btnPause
            // 
            this.btnPause.ForeColor = System.Drawing.Color.Black;
            this.btnPause.Location = new System.Drawing.Point(3, 3);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(90, 32);
            this.btnPause.TabIndex = 0;
            this.btnPause.Text = "Пауза";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnResume
            // 
            this.btnResume.ForeColor = System.Drawing.Color.Black;
            this.btnResume.Location = new System.Drawing.Point(99, 3);
            this.btnResume.Name = "btnResume";
            this.btnResume.Size = new System.Drawing.Size(105, 32);
            this.btnResume.TabIndex = 1;
            this.btnResume.Text = "Продолжить";
            this.btnResume.UseVisualStyleBackColor = true;
            this.btnResume.Click += new System.EventHandler(this.btnResume_Click);
            // 
            // btnEmdr
            // 
            this.btnEmdr.ForeColor = System.Drawing.Color.Black;
            this.btnEmdr.Location = new System.Drawing.Point(210, 3);
            this.btnEmdr.Name = "btnEmdr";
            this.btnEmdr.Size = new System.Drawing.Size(90, 32);
            this.btnEmdr.TabIndex = 2;
            this.btnEmdr.Text = "EMDR";
            this.btnEmdr.UseVisualStyleBackColor = true;
            this.btnEmdr.Click += new System.EventHandler(this.btnEmdr_Click);
            // 
            // btnSafe
            // 
            this.btnSafe.ForeColor = System.Drawing.Color.Black;
            this.btnSafe.Location = new System.Drawing.Point(3, 41);
            this.btnSafe.Name = "btnSafe";
            this.btnSafe.Size = new System.Drawing.Size(120, 32);
            this.btnSafe.TabIndex = 3;
            this.btnSafe.Text = "Safe Place";
            this.btnSafe.UseVisualStyleBackColor = true;
            this.btnSafe.Click += new System.EventHandler(this.btnSafe_Click);
            // 
            // btnStop
            // 
            this.btnStop.ForeColor = System.Drawing.Color.Black;
            this.btnStop.Location = new System.Drawing.Point(129, 41);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(120, 32);
            this.btnStop.TabIndex = 4;
            this.btnStop.Text = "Emergency STOP";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnFinish
            // 
            this.btnFinish.ForeColor = System.Drawing.Color.Black;
            this.btnFinish.Location = new System.Drawing.Point(3, 79);
            this.btnFinish.Name = "btnFinish";
            this.btnFinish.Size = new System.Drawing.Size(120, 32);
            this.btnFinish.TabIndex = 5;
            this.btnFinish.Text = "Завершить";
            this.btnFinish.UseVisualStyleBackColor = true;
            this.btnFinish.Click += new System.EventHandler(this.btnFinish_Click);
            // 
            // btnAskSud
            // 
            this.btnAskSud.ForeColor = System.Drawing.Color.Black;
            this.btnAskSud.Location = new System.Drawing.Point(129, 79);
            this.btnAskSud.Name = "btnAskSud";
            this.btnAskSud.Size = new System.Drawing.Size(130, 32);
            this.btnAskSud.TabIndex = 6;
            this.btnAskSud.Text = "Запросить SUD";
            this.btnAskSud.UseVisualStyleBackColor = true;
            this.btnAskSud.Click += new System.EventHandler(this.btnAskSud_Click);
            // 
            // grpWorlds
            // 
            this.grpWorlds.Controls.Add(this.flowWorlds);
            this.grpWorlds.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpWorlds.ForeColor = System.Drawing.Color.White;
            this.grpWorlds.Location = new System.Drawing.Point(10, 289);
            this.grpWorlds.Margin = new System.Windows.Forms.Padding(10);
            this.grpWorlds.Name = "grpWorlds";
            this.grpWorlds.Padding = new System.Windows.Forms.Padding(10);
            this.grpWorlds.Size = new System.Drawing.Size(594, 109);
            this.grpWorlds.TabIndex = 3;
            this.grpWorlds.TabStop = false;
            this.grpWorlds.Text = "Переключение миров";
            // 
            // flowWorlds
            // 
            this.flowWorlds.Controls.Add(this.btnWorldForest);
            this.flowWorlds.Controls.Add(this.btnWorldSea);
            this.flowWorlds.Controls.Add(this.btnLoadVideo);
            this.flowWorlds.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowWorlds.Location = new System.Drawing.Point(10, 26);
            this.flowWorlds.Name = "flowWorlds";
            this.flowWorlds.Size = new System.Drawing.Size(574, 73);
            this.flowWorlds.TabIndex = 0;
            // 
            // btnWorldForest
            // 
            this.btnWorldForest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(139)))), ((int)(((byte)(34)))));
            this.btnWorldForest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWorldForest.ForeColor = System.Drawing.Color.White;
            this.btnWorldForest.Location = new System.Drawing.Point(3, 3);
            this.btnWorldForest.Name = "btnWorldForest";
            this.btnWorldForest.Size = new System.Drawing.Size(100, 45);
            this.btnWorldForest.TabIndex = 0;
            this.btnWorldForest.Text = "Лес";
            this.btnWorldForest.UseVisualStyleBackColor = false;
            this.btnWorldForest.Click += new System.EventHandler(this.btnWorldForest_Click);
            // 
            // btnWorldSea
            // 
            this.btnWorldSea.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(144)))), ((int)(((byte)(255)))));
            this.btnWorldSea.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWorldSea.ForeColor = System.Drawing.Color.White;
            this.btnWorldSea.Location = new System.Drawing.Point(109, 3);
            this.btnWorldSea.Name = "btnWorldSea";
            this.btnWorldSea.Size = new System.Drawing.Size(100, 45);
            this.btnWorldSea.TabIndex = 1;
            this.btnWorldSea.Text = "Море";
            this.btnWorldSea.UseVisualStyleBackColor = false;
            this.btnWorldSea.Click += new System.EventHandler(this.btnWorldSea_Click);
            // 
            // btnLoadVideo
            // 
            this.btnLoadVideo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnLoadVideo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLoadVideo.ForeColor = System.Drawing.Color.White;
            this.btnLoadVideo.Location = new System.Drawing.Point(215, 3);
            this.btnLoadVideo.Name = "btnLoadVideo";
            this.btnLoadVideo.Size = new System.Drawing.Size(120, 45);
            this.btnLoadVideo.TabIndex = 3;
            this.btnLoadVideo.Text = "Загрузить видео";
            this.btnLoadVideo.UseVisualStyleBackColor = false;
            this.btnLoadVideo.Click += new System.EventHandler(this.btnLoadVideo_Click);
            // 
            // grpSud
            // 
            this.grpSud.Controls.Add(this.lblSud);
            this.grpSud.Controls.Add(this.trackSud);
            this.grpSud.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpSud.ForeColor = System.Drawing.Color.White;
            this.grpSud.Location = new System.Drawing.Point(624, 289);
            this.grpSud.Margin = new System.Windows.Forms.Padding(10);
            this.grpSud.Name = "grpSud";
            this.grpSud.Padding = new System.Windows.Forms.Padding(10);
            this.grpSud.Size = new System.Drawing.Size(390, 109);
            this.grpSud.TabIndex = 4;
            this.grpSud.TabStop = false;
            this.grpSud.Text = "SUD (уровень дистресса)";
            // 
            // lblSud
            // 
            this.lblSud.AutoSize = true;
            this.lblSud.Location = new System.Drawing.Point(20, 29);
            this.lblSud.Name = "lblSud";
            this.lblSud.Size = new System.Drawing.Size(90, 15);
            this.lblSud.TabIndex = 1;
            this.lblSud.Text = "Уровень SUD: 0";
            // 
            // trackSud
            // 
            this.trackSud.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.trackSud.Location = new System.Drawing.Point(10, 54);
            this.trackSud.Name = "trackSud";
            this.trackSud.Size = new System.Drawing.Size(370, 45);
            this.trackSud.TabIndex = 0;
            this.trackSud.Value = 5;
            this.trackSud.Scroll += new System.EventHandler(this.trackSud_Scroll);
            // 
            // grpNotes
            // 
            this.grpNotes.Controls.Add(this.txtNote);
            this.grpNotes.Controls.Add(this.btnSendNote);
            this.grpNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpNotes.ForeColor = System.Drawing.Color.White;
            this.grpNotes.Location = new System.Drawing.Point(10, 418);
            this.grpNotes.Margin = new System.Windows.Forms.Padding(10);
            this.grpNotes.Name = "grpNotes";
            this.grpNotes.Padding = new System.Windows.Forms.Padding(10);
            this.grpNotes.Size = new System.Drawing.Size(594, 152);
            this.grpNotes.TabIndex = 5;
            this.grpNotes.TabStop = false;
            this.grpNotes.Text = "Заметки для лога";
            // 
            // txtNote
            // 
            this.txtNote.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtNote.Location = new System.Drawing.Point(10, 26);
            this.txtNote.Multiline = true;
            this.txtNote.Name = "txtNote";
            this.txtNote.Size = new System.Drawing.Size(574, 87);
            this.txtNote.TabIndex = 0;
            // 
            // btnSendNote
            // 
            this.btnSendNote.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnSendNote.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnSendNote.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSendNote.ForeColor = System.Drawing.Color.White;
            this.btnSendNote.Location = new System.Drawing.Point(10, 119);
            this.btnSendNote.Name = "btnSendNote";
            this.btnSendNote.Size = new System.Drawing.Size(574, 23);
            this.btnSendNote.TabIndex = 1;
            this.btnSendNote.Text = "Отправить заметку";
            this.btnSendNote.UseVisualStyleBackColor = false;
            this.btnSendNote.Click += new System.EventHandler(this.btnSendNote_Click);
            // 
            // grpLog
            // 
            this.layoutRoot.SetColumnSpan(this.grpLog, 2);
            this.grpLog.Controls.Add(this.btnClearLog);
            this.grpLog.Controls.Add(this.lstLog);
            this.grpLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpLog.ForeColor = System.Drawing.Color.White;
            this.grpLog.Location = new System.Drawing.Point(10, 590);
            this.grpLog.Margin = new System.Windows.Forms.Padding(10);
            this.grpLog.Name = "grpLog";
            this.grpLog.Padding = new System.Windows.Forms.Padding(10);
            this.grpLog.Size = new System.Drawing.Size(1004, 1);
            this.grpLog.TabIndex = 6;
            this.grpLog.TabStop = false;
            this.grpLog.Text = "Журнал команд";
            // 
            // btnClearLog
            // 
            this.btnClearLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearLog.Location = new System.Drawing.Point(892, 22);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(96, 25);
            this.btnClearLog.TabIndex = 1;
            this.btnClearLog.Text = "Очистить";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // lstLog
            // 
            this.lstLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstLog.FormattingEnabled = true;
            this.lstLog.ItemHeight = 15;
            this.lstLog.Location = new System.Drawing.Point(10, 26);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(984, 0);
            this.lstLog.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(32)))), ((int)(((byte)(36)))));
            this.ClientSize = new System.Drawing.Size(1024, 600);
            this.Controls.Add(this.layoutRoot);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(900, 500);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VR Remote Console";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.layoutRoot.ResumeLayout(false);
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            this.grpConnection.ResumeLayout(false);
            this.grpConnection.PerformLayout();
            this.flowConnection.ResumeLayout(false);
            this.flowConnection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.grpVRView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picVRView)).EndInit();
            this.grpSession.ResumeLayout(false);
            this.flowSession.ResumeLayout(false);
            this.grpWorlds.ResumeLayout(false);
            this.flowWorlds.ResumeLayout(false);
            this.grpSud.ResumeLayout(false);
            this.grpSud.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackSud)).EndInit();
            this.grpNotes.ResumeLayout(false);
            this.grpNotes.PerformLayout();
            this.grpLog.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel layoutRoot;
        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox grpConnection;
        private System.Windows.Forms.FlowLayoutPanel flowConnection;
        private System.Windows.Forms.Label labelIp;
        private System.Windows.Forms.TextBox txtIp;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.Button btnApplyConnection;
        private System.Windows.Forms.Label lblConnectionStatus;
        private System.Windows.Forms.GroupBox grpSession;
        private System.Windows.Forms.FlowLayoutPanel flowSession;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnResume;
        private System.Windows.Forms.Button btnEmdr;
        private System.Windows.Forms.Button btnSafe;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnFinish;
        private System.Windows.Forms.Button btnAskSud;
        private System.Windows.Forms.GroupBox grpWorlds;
        private System.Windows.Forms.FlowLayoutPanel flowWorlds;
        private System.Windows.Forms.Button btnWorldForest;
        private System.Windows.Forms.Button btnWorldSea;
        private System.Windows.Forms.Button btnLoadVideo;
        private System.Windows.Forms.GroupBox grpSud;
        private System.Windows.Forms.Label lblSud;
        private System.Windows.Forms.TrackBar trackSud;
        private System.Windows.Forms.GroupBox grpNotes;
        private System.Windows.Forms.TextBox txtNote;
        private System.Windows.Forms.Button btnSendNote;
        private System.Windows.Forms.GroupBox grpLog;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.GroupBox grpVRView;
        private System.Windows.Forms.PictureBox picVRView;
    }
}

