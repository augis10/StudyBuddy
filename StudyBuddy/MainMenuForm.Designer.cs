﻿namespace StudyBuddy
{
    partial class MainMenuForm
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
            this.greetingsLabel = new System.Windows.Forms.Label();
            this.karmaProgressBar = new System.Windows.Forms.ProgressBar();
            this.progressLabel = new System.Windows.Forms.Label();
            this.textBox_SearchTopic = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.adviseButton = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // greetingsLabel
            // 
            this.greetingsLabel.AutoSize = true;
            this.greetingsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            this.greetingsLabel.Location = new System.Drawing.Point(338, 362);
            this.greetingsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.greetingsLabel.Name = "greetingsLabel";
            this.greetingsLabel.Size = new System.Drawing.Size(303, 47);
            this.greetingsLabel.TabIndex = 0;
            this.greetingsLabel.Text = "Welcome back!";
            this.greetingsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.greetingsLabel.Click += new System.EventHandler(this.Label1_Click);
            // 
            // karmaProgressBar
            // 
            this.karmaProgressBar.Location = new System.Drawing.Point(346, 105);
            this.karmaProgressBar.Maximum = 1000;
            this.karmaProgressBar.Name = "karmaProgressBar";
            this.karmaProgressBar.Size = new System.Drawing.Size(276, 32);
            this.karmaProgressBar.Step = 1;
            this.karmaProgressBar.TabIndex = 1;
            this.karmaProgressBar.Value = 100;
            this.karmaProgressBar.Click += new System.EventHandler(this.ProgressBar1_Click);
            // 
            // progressLabel
            // 
            this.progressLabel.AutoSize = true;
            this.progressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.progressLabel.Location = new System.Drawing.Point(342, 69);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(238, 29);
            this.progressLabel.TabIndex = 2;
            this.progressLabel.Text = "Your karma progress";
            this.progressLabel.Click += new System.EventHandler(this.Label2_Click);
            // 
            // textBox_SearchTopic
            // 
            this.textBox_SearchTopic.Location = new System.Drawing.Point(346, 286);
            this.textBox_SearchTopic.Name = "textBox_SearchTopic";
            this.textBox_SearchTopic.Size = new System.Drawing.Size(276, 26);
            this.textBox_SearchTopic.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label3.Location = new System.Drawing.Point(311, 244);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(353, 39);
            this.label3.TabIndex = 4;
            this.label3.Text = "What do you need advice on?";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // adviseButton
            // 
            this.adviseButton.BackColor = System.Drawing.Color.Yellow;
            this.adviseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.adviseButton.Location = new System.Drawing.Point(747, 68);
            this.adviseButton.Name = "adviseButton";
            this.adviseButton.Size = new System.Drawing.Size(254, 93);
            this.adviseButton.TabIndex = 5;
            this.adviseButton.Text = "I am ready to advise";
            this.adviseButton.UseVisualStyleBackColor = false;
            this.adviseButton.Click += new System.EventHandler(this.Button1_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 786);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1113, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 15);
            this.toolStripStatusLabel1.Click += new System.EventHandler(this.ToolStripStatusLabel1_Click);
            // 
            // MainMenuForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1113, 808);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.adviseButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_SearchTopic);
            this.Controls.Add(this.progressLabel);
            this.Controls.Add(this.karmaProgressBar);
            this.Controls.Add(this.greetingsLabel);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MainMenuForm";
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label greetingsLabel;
        private System.Windows.Forms.ProgressBar karmaProgressBar;
        private System.Windows.Forms.Label progressLabel;
        private System.Windows.Forms.TextBox textBox_SearchTopic;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button adviseButton;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    }
}