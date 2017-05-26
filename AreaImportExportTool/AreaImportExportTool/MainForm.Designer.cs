namespace AreaImportExportTool
{
    partial class MainForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmdExport = new System.Windows.Forms.Button();
            this.source_txtTeamProject = new System.Windows.Forms.TextBox();
            this.source_txtCollectionUri = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkIterationStructure = new System.Windows.Forms.CheckBox();
            this.chkAreaStructure = new System.Windows.Forms.CheckBox();
            this.cmdImport = new System.Windows.Forms.Button();
            this.cmdClose = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.target_txtCollectionUri = new System.Windows.Forms.TextBox();
            this.target_txtTeamProject = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cmdExport);
            this.groupBox1.Controls.Add(this.source_txtTeamProject);
            this.groupBox1.Controls.Add(this.source_txtCollectionUri);
            this.groupBox1.Location = new System.Drawing.Point(14, 13);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Size = new System.Drawing.Size(587, 160);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Source TFS";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(145, 16);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(147, 30);
            this.button1.TabIndex = 4;
            this.button1.Text = "Set Source Server";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 91);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Team &Project:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Collection:";
            // 
            // cmdExport
            // 
            this.cmdExport.Location = new System.Drawing.Point(474, 123);
            this.cmdExport.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmdExport.Name = "cmdExport";
            this.cmdExport.Size = new System.Drawing.Size(107, 29);
            this.cmdExport.TabIndex = 3;
            this.cmdExport.Text = "&Export...";
            this.cmdExport.UseVisualStyleBackColor = true;
            this.cmdExport.Click += new System.EventHandler(this.cmdExport_Click);
            // 
            // source_txtTeamProject
            // 
            this.source_txtTeamProject.Location = new System.Drawing.Point(146, 88);
            this.source_txtTeamProject.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.source_txtTeamProject.Name = "source_txtTeamProject";
            this.source_txtTeamProject.ReadOnly = true;
            this.source_txtTeamProject.Size = new System.Drawing.Size(435, 23);
            this.source_txtTeamProject.TabIndex = 3;
            this.source_txtTeamProject.TextChanged += new System.EventHandler(this.chkIterationStructure_CheckedChanged);
            // 
            // source_txtCollectionUri
            // 
            this.source_txtCollectionUri.Location = new System.Drawing.Point(145, 53);
            this.source_txtCollectionUri.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.source_txtCollectionUri.Name = "source_txtCollectionUri";
            this.source_txtCollectionUri.ReadOnly = true;
            this.source_txtCollectionUri.Size = new System.Drawing.Size(435, 23);
            this.source_txtCollectionUri.TabIndex = 1;
            this.source_txtCollectionUri.TextChanged += new System.EventHandler(this.chkIterationStructure_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkIterationStructure);
            this.groupBox2.Controls.Add(this.chkAreaStructure);
            this.groupBox2.Location = new System.Drawing.Point(14, 345);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox2.Size = new System.Drawing.Size(385, 90);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Choose items to im-/export";
            this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
            // 
            // chkIterationStructure
            // 
            this.chkIterationStructure.AutoSize = true;
            this.chkIterationStructure.Checked = true;
            this.chkIterationStructure.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIterationStructure.Location = new System.Drawing.Point(26, 60);
            this.chkIterationStructure.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkIterationStructure.Name = "chkIterationStructure";
            this.chkIterationStructure.Size = new System.Drawing.Size(120, 19);
            this.chkIterationStructure.TabIndex = 1;
            this.chkIterationStructure.Text = "&Iteration structure";
            this.chkIterationStructure.UseVisualStyleBackColor = true;
            this.chkIterationStructure.CheckedChanged += new System.EventHandler(this.chkIterationStructure_CheckedChanged);
            // 
            // chkAreaStructure
            // 
            this.chkAreaStructure.AutoSize = true;
            this.chkAreaStructure.Checked = true;
            this.chkAreaStructure.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAreaStructure.Location = new System.Drawing.Point(26, 28);
            this.chkAreaStructure.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkAreaStructure.Name = "chkAreaStructure";
            this.chkAreaStructure.Size = new System.Drawing.Size(100, 19);
            this.chkAreaStructure.TabIndex = 0;
            this.chkAreaStructure.Text = "&Area structure";
            this.chkAreaStructure.UseVisualStyleBackColor = true;
            this.chkAreaStructure.CheckedChanged += new System.EventHandler(this.chkIterationStructure_CheckedChanged);
            // 
            // cmdImport
            // 
            this.cmdImport.Location = new System.Drawing.Point(474, 120);
            this.cmdImport.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmdImport.Name = "cmdImport";
            this.cmdImport.Size = new System.Drawing.Size(107, 29);
            this.cmdImport.TabIndex = 4;
            this.cmdImport.Text = "&Import...";
            this.cmdImport.UseVisualStyleBackColor = true;
            this.cmdImport.Click += new System.EventHandler(this.cmdImport_Click);
            // 
            // cmdClose
            // 
            this.cmdClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdClose.Location = new System.Drawing.Point(1212, 562);
            this.cmdClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(113, 29);
            this.cmdClose.TabIndex = 5;
            this.cmdClose.Text = "&Close";
            this.cmdClose.UseVisualStyleBackColor = true;
            this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(145, 16);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(147, 31);
            this.button2.TabIndex = 6;
            this.button2.Text = "Set Target Server";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // target_txtCollectionUri
            // 
            this.target_txtCollectionUri.Location = new System.Drawing.Point(145, 53);
            this.target_txtCollectionUri.Name = "target_txtCollectionUri";
            this.target_txtCollectionUri.ReadOnly = true;
            this.target_txtCollectionUri.Size = new System.Drawing.Size(435, 23);
            this.target_txtCollectionUri.TabIndex = 7;
            // 
            // target_txtTeamProject
            // 
            this.target_txtTeamProject.Location = new System.Drawing.Point(146, 86);
            this.target_txtTeamProject.Name = "target_txtTeamProject";
            this.target_txtTeamProject.ReadOnly = true;
            this.target_txtTeamProject.Size = new System.Drawing.Size(435, 23);
            this.target_txtTeamProject.TabIndex = 8;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(607, 29);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(743, 406);
            this.textBox1.TabIndex = 9;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(6, 46);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(394, 23);
            this.textBox2.TabIndex = 10;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(1022, 26);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(107, 70);
            this.button3.TabIndex = 11;
            this.button3.Text = "Migrate Team Paths";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 15);
            this.label4.TabIndex = 12;
            this.label4.Text = "Source Team";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.target_txtCollectionUri);
            this.groupBox3.Controls.Add(this.target_txtTeamProject);
            this.groupBox3.Controls.Add(this.cmdImport);
            this.groupBox3.Controls.Add(this.button2);
            this.groupBox3.Location = new System.Drawing.Point(14, 180);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(587, 158);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Target TFS";
            this.groupBox3.Enter += new System.EventHandler(this.groupBox3_Enter);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 89);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 15);
            this.label5.TabIndex = 10;
            this.label5.Text = "Team &Project:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 15);
            this.label3.TabIndex = 9;
            this.label3.Text = "Collection:";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.textBox5);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.textBox2);
            this.groupBox4.Controls.Add(this.button3);
            this.groupBox4.Location = new System.Drawing.Point(14, 442);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(1135, 150);
            this.groupBox4.TabIndex = 14;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Teams paths migration";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(520, 23);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(74, 15);
            this.label6.TabIndex = 16;
            this.label6.Text = "Target Team";
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(521, 50);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(397, 23);
            this.textBox5.TabIndex = 15;
            // 
            // MainForm
            // 
            this.AcceptButton = this.cmdExport;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdClose;
            this.ClientSize = new System.Drawing.Size(1358, 604);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.cmdClose);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RippleRock - Area Import/Export Tool - DISCLAIMER Use this tool at your own risk!" +
    " Test it out before running against a live system!";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox source_txtCollectionUri;
        private System.Windows.Forms.TextBox source_txtTeamProject;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chkIterationStructure;
        private System.Windows.Forms.CheckBox chkAreaStructure;
        private System.Windows.Forms.Button cmdExport;
        private System.Windows.Forms.Button cmdImport;
        private System.Windows.Forms.Button cmdClose;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox target_txtCollectionUri;
        private System.Windows.Forms.TextBox target_txtTeamProject;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Label label6;
    }
}

