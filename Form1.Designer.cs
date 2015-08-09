namespace VirtualTheatre
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.chatDisplay = new System.Windows.Forms.TextBox();
            this.chatType = new System.Windows.Forms.TextBox();
            this.buttonCreateSession = new System.Windows.Forms.Button();
            this.seekBar = new System.Windows.Forms.TrackBar();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.buttonPlay = new System.Windows.Forms.Button();
            this.buttonPause = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.logBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonJoinSession = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.seekBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(11, 37);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(375, 326);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.DoubleClick += new System.EventHandler(this.pictureBox1_DoubleClick);
            // 
            // chatDisplay
            // 
            this.chatDisplay.BackColor = System.Drawing.Color.Snow;
            this.chatDisplay.Location = new System.Drawing.Point(11, 398);
            this.chatDisplay.MaxLength = 2147483647;
            this.chatDisplay.Multiline = true;
            this.chatDisplay.Name = "chatDisplay";
            this.chatDisplay.ReadOnly = true;
            this.chatDisplay.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.chatDisplay.Size = new System.Drawing.Size(611, 133);
            this.chatDisplay.TabIndex = 6;
            // 
            // chatType
            // 
            this.chatType.AcceptsTab = true;
            this.chatType.Location = new System.Drawing.Point(11, 544);
            this.chatType.MaxLength = 2147483647;
            this.chatType.Multiline = true;
            this.chatType.Name = "chatType";
            this.chatType.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.chatType.Size = new System.Drawing.Size(610, 54);
            this.chatType.TabIndex = 5;
            this.chatType.TextChanged += new System.EventHandler(this.chatType_TextChanged);
            // 
            // buttonCreateSession
            // 
            this.buttonCreateSession.AutoSize = true;
            this.buttonCreateSession.Location = new System.Drawing.Point(442, 33);
            this.buttonCreateSession.Name = "buttonCreateSession";
            this.buttonCreateSession.Size = new System.Drawing.Size(79, 28);
            this.buttonCreateSession.TabIndex = 0;
            this.buttonCreateSession.Text = "New Session";
            this.buttonCreateSession.UseVisualStyleBackColor = true;
            this.buttonCreateSession.Click += new System.EventHandler(this.buttonCreateSession_Click);
            // 
            // seekBar
            // 
            this.seekBar.Location = new System.Drawing.Point(11, 365);
            this.seekBar.Name = "seekBar";
            this.seekBar.Size = new System.Drawing.Size(375, 45);
            this.seekBar.TabIndex = 8;
            this.seekBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.seekBar.Scroll += new System.EventHandler(this.seekBar_Scroll);
            // 
            // trackBar1
            // 
            this.trackBar1.LargeChange = 1;
            this.trackBar1.Location = new System.Drawing.Point(392, 54);
            this.trackBar1.Maximum = 40;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBar1.Size = new System.Drawing.Size(45, 291);
            this.trackBar1.TabIndex = 9;
            this.trackBar1.Value = 10;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // buttonPlay
            // 
            this.buttonPlay.Location = new System.Drawing.Point(430, 354);
            this.buttonPlay.Name = "buttonPlay";
            this.buttonPlay.Size = new System.Drawing.Size(60, 25);
            this.buttonPlay.TabIndex = 2;
            this.buttonPlay.Text = "Play";
            this.buttonPlay.UseVisualStyleBackColor = true;
            this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
            // 
            // buttonPause
            // 
            this.buttonPause.Location = new System.Drawing.Point(496, 354);
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.Size = new System.Drawing.Size(60, 25);
            this.buttonPause.TabIndex = 3;
            this.buttonPause.Text = "Pause";
            this.buttonPause.UseVisualStyleBackColor = true;
            this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(562, 354);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(60, 25);
            this.buttonStop.TabIndex = 4;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // logBox
            // 
            this.logBox.AcceptsReturn = true;
            this.logBox.AcceptsTab = true;
            this.logBox.BackColor = System.Drawing.SystemColors.Window;
            this.logBox.Location = new System.Drawing.Point(430, 65);
            this.logBox.Multiline = true;
            this.logBox.Name = "logBox";
            this.logBox.ReadOnly = true;
            this.logBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.logBox.Size = new System.Drawing.Size(191, 274);
            this.logBox.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(392, 346);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(25, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "100";
            // 
            // buttonJoinSession
            // 
            this.buttonJoinSession.Location = new System.Drawing.Point(528, 33);
            this.buttonJoinSession.Name = "buttonJoinSession";
            this.buttonJoinSession.Size = new System.Drawing.Size(79, 28);
            this.buttonJoinSession.TabIndex = 1;
            this.buttonJoinSession.Text = "Join Session";
            this.buttonJoinSession.UseVisualStyleBackColor = true;
            this.buttonJoinSession.Click += new System.EventHandler(this.buttonJoinSession_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(636, 24);
            this.menuStrip1.TabIndex = 11;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "Help";
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.helpToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(636, 609);
            this.Controls.Add(this.buttonJoinSession);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonPause);
            this.Controls.Add(this.buttonPlay);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.buttonCreateSession);
            this.Controls.Add(this.chatType);
            this.Controls.Add(this.chatDisplay);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.seekBar);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "VT: Virtual Theatre";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.seekBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox chatDisplay;
        private System.Windows.Forms.TextBox chatType;
        private System.Windows.Forms.Button buttonCreateSession;
        private System.Windows.Forms.TrackBar seekBar;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Button buttonPlay;
        private System.Windows.Forms.Button buttonPause;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.TextBox logBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonJoinSession;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    }
}

