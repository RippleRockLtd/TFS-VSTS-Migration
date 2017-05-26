namespace ASOS_MIG
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_FallbackAsignee = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_OffsetFromId = new System.Windows.Forms.TextBox();
            this.textBox_OverrideAll = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_logFilePath = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.button4 = new System.Windows.Forms.Button();
            this.textBox_InitialCount = new System.Windows.Forms.TextBox();
            this.textBox_CreatedCount = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_IncludingAdditional = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(24, 27);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Source";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(24, 104);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Target";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(105, 27);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(473, 22);
            this.textBox1.TabIndex = 2;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(584, 27);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(279, 22);
            this.textBox2.TabIndex = 3;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(105, 104);
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(473, 22);
            this.textBox3.TabIndex = 4;
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(584, 104);
            this.textBox4.Name = "textBox4";
            this.textBox4.ReadOnly = true;
            this.textBox4.Size = new System.Drawing.Size(279, 22);
            this.textBox4.TabIndex = 5;
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(24, 289);
            this.textBox5.Multiline = true;
            this.textBox5.Name = "textBox5";
            this.textBox5.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox5.Size = new System.Drawing.Size(1394, 308);
            this.textBox5.TabIndex = 6;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(24, 260);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 7;
            this.button3.Text = "RUN";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // textBox6
            // 
            this.textBox6.Location = new System.Drawing.Point(105, 68);
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new System.Drawing.Size(352, 22);
            this.textBox6.TabIndex = 8;
            // 
            // textBox7
            // 
            this.textBox7.Location = new System.Drawing.Point(334, 151);
            this.textBox7.Name = "textBox7";
            this.textBox7.Size = new System.Drawing.Size(228, 22);
            this.textBox7.TabIndex = 9;
            this.textBox7.Text = "C:\\attachmentprocessing";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(52, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 17);
            this.label1.TabIndex = 10;
            this.label1.Text = "Query";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(178, 153);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(150, 17);
            this.label2.TabIndex = 11;
            this.label2.Text = "Folder for attachments";
            // 
            // textBox8
            // 
            this.textBox8.Location = new System.Drawing.Point(880, 12);
            this.textBox8.Multiline = true;
            this.textBox8.Name = "textBox8";
            this.textBox8.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox8.Size = new System.Drawing.Size(541, 161);
            this.textBox8.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(178, 191);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(122, 17);
            this.label3.TabIndex = 13;
            this.label3.Text = "Fallback Assignee";
            // 
            // textBox_FallbackAsignee
            // 
            this.textBox_FallbackAsignee.Location = new System.Drawing.Point(334, 188);
            this.textBox_FallbackAsignee.Name = "textBox_FallbackAsignee";
            this.textBox_FallbackAsignee.Size = new System.Drawing.Size(228, 22);
            this.textBox_FallbackAsignee.TabIndex = 14;
            this.textBox_FallbackAsignee.Text = "Peter Haynes";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(178, 232);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(121, 17);
            this.label4.TabIndex = 15;
            this.label4.Text = "Offset the From Id";
            // 
            // textBox_OffsetFromId
            // 
            this.textBox_OffsetFromId.Location = new System.Drawing.Point(334, 227);
            this.textBox_OffsetFromId.Name = "textBox_OffsetFromId";
            this.textBox_OffsetFromId.Size = new System.Drawing.Size(228, 22);
            this.textBox_OffsetFromId.TabIndex = 16;
            this.textBox_OffsetFromId.Text = "0";
            // 
            // textBox_OverrideAll
            // 
            this.textBox_OverrideAll.Location = new System.Drawing.Point(669, 188);
            this.textBox_OverrideAll.Name = "textBox_OverrideAll";
            this.textBox_OverrideAll.Size = new System.Drawing.Size(206, 22);
            this.textBox_OverrideAll.TabIndex = 17;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(581, 191);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 17);
            this.label5.TabIndex = 18;
            this.label5.Text = "Override All";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(894, 191);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(70, 17);
            this.label6.TabIndex = 19;
            this.label6.Text = "Log to file";
            // 
            // textBox_logFilePath
            // 
            this.textBox_logFilePath.Location = new System.Drawing.Point(970, 188);
            this.textBox_logFilePath.Name = "textBox_logFilePath";
            this.textBox_logFilePath.Size = new System.Drawing.Size(376, 22);
            this.textBox_logFilePath.TabIndex = 20;
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.Description = "Path to the log file";
            this.folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.folderBrowserDialog1.SelectedPath = "C:\\Migration\\Logs";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(1352, 188);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(66, 23);
            this.button4.TabIndex = 21;
            this.button4.Text = "Choose";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // textBox_InitialCount
            // 
            this.textBox_InitialCount.Location = new System.Drawing.Point(669, 260);
            this.textBox_InitialCount.Name = "textBox_InitialCount";
            this.textBox_InitialCount.ReadOnly = true;
            this.textBox_InitialCount.Size = new System.Drawing.Size(97, 22);
            this.textBox_InitialCount.TabIndex = 22;
            // 
            // textBox_CreatedCount
            // 
            this.textBox_CreatedCount.Location = new System.Drawing.Point(917, 260);
            this.textBox_CreatedCount.Name = "textBox_CreatedCount";
            this.textBox_CreatedCount.ReadOnly = true;
            this.textBox_CreatedCount.Size = new System.Drawing.Size(100, 22);
            this.textBox_CreatedCount.TabIndex = 23;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(617, 266);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(45, 17);
            this.label7.TabIndex = 24;
            this.label7.Text = "Count";
            // 
            // textBox_IncludingAdditional
            // 
            this.textBox_IncludingAdditional.Location = new System.Drawing.Point(793, 260);
            this.textBox_IncludingAdditional.Name = "textBox_IncludingAdditional";
            this.textBox_IncludingAdditional.ReadOnly = true;
            this.textBox_IncludingAdditional.Size = new System.Drawing.Size(92, 22);
            this.textBox_IncludingAdditional.TabIndex = 25;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1430, 609);
            this.Controls.Add(this.textBox_IncludingAdditional);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBox_CreatedCount);
            this.Controls.Add(this.textBox_InitialCount);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.textBox_logFilePath);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox_OverrideAll);
            this.Controls.Add(this.textBox_OffsetFromId);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_FallbackAsignee);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox8);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox7);
            this.Controls.Add(this.textBox6);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "RippleRock WorkItem migration tool. DISCLAIMER Use this tool at your own risk! Te" +
    "st it out before running against a live system!";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.TextBox textBox7;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox8;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_FallbackAsignee;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_OffsetFromId;
        private System.Windows.Forms.TextBox textBox_OverrideAll;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox_logFilePath;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox textBox_InitialCount;
        private System.Windows.Forms.TextBox textBox_CreatedCount;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_IncludingAdditional;
    }
}

