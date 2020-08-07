using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace WindowStreamer
{
    partial class WindowFinder
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
            this.gotWindowPanel = new System.Windows.Forms.Panel();
            this.stopStreaming = new System.Windows.Forms.Button();
            this.targetFpsEntry = new System.Windows.Forms.NumericUpDown();
            this.sizeHeight = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.sizeWidth = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.portLabel = new System.Windows.Forms.Label();
            this.gotWindowPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.targetFpsEntry)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sizeHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sizeWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // gotWindowPanel
            // 
            this.gotWindowPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gotWindowPanel.Controls.Add(this.stopStreaming);
            this.gotWindowPanel.Controls.Add(this.targetFpsEntry);
            this.gotWindowPanel.Controls.Add(this.sizeHeight);
            this.gotWindowPanel.Controls.Add(this.label2);
            this.gotWindowPanel.Controls.Add(this.sizeWidth);
            this.gotWindowPanel.Controls.Add(this.label1);
            this.gotWindowPanel.Controls.Add(this.portLabel);
            this.gotWindowPanel.Location = new System.Drawing.Point(12, 12);
            this.gotWindowPanel.Name = "gotWindowPanel";
            this.gotWindowPanel.Size = new System.Drawing.Size(181, 73);
            this.gotWindowPanel.TabIndex = 3;
            this.gotWindowPanel.Visible = false;
            // 
            // stopStreaming
            // 
            this.stopStreaming.Location = new System.Drawing.Point(61, 42);
            this.stopStreaming.Name = "stopStreaming";
            this.stopStreaming.Size = new System.Drawing.Size(75, 23);
            this.stopStreaming.TabIndex = 7;
            this.stopStreaming.Text = "Stop";
            this.stopStreaming.UseVisualStyleBackColor = true;
            this.stopStreaming.Click += new System.EventHandler(this.stopStreaming_Click);
            // 
            // targetFpsEntry
            // 
            this.targetFpsEntry.DecimalPlaces = 1;
            this.targetFpsEntry.Location = new System.Drawing.Point(3, 43);
            this.targetFpsEntry.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.targetFpsEntry.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.targetFpsEntry.Name = "targetFpsEntry";
            this.targetFpsEntry.Size = new System.Drawing.Size(52, 20);
            this.targetFpsEntry.TabIndex = 6;
            this.targetFpsEntry.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.targetFpsEntry.ValueChanged += new System.EventHandler(this.targetFpsEntry_ValueChanged);
            // 
            // sizeHeight
            // 
            this.sizeHeight.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.sizeHeight.Location = new System.Drawing.Point(106, 16);
            this.sizeHeight.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.sizeHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.sizeHeight.Name = "sizeHeight";
            this.sizeHeight.Size = new System.Drawing.Size(64, 20);
            this.sizeHeight.TabIndex = 5;
            this.sizeHeight.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.sizeHeight.ValueChanged += new System.EventHandler(this.sizeHeight_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Size";
            // 
            // sizeWidth
            // 
            this.sizeWidth.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.sizeWidth.Location = new System.Drawing.Point(36, 16);
            this.sizeWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.sizeWidth.Name = "sizeWidth";
            this.sizeWidth.Size = new System.Drawing.Size(64, 20);
            this.sizeWidth.TabIndex = 3;
            this.sizeWidth.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.sizeWidth.ValueChanged += new System.EventHandler(this.sizeWidth_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Listening on port";
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(94, 0);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(35, 13);
            this.portLabel.TabIndex = 2;
            this.portLabel.Text = "label2";
            // 
            // WindowFinder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(205, 97);
            this.Controls.Add(this.gotWindowPanel);
            this.MaximizeBox = false;
            this.Name = "WindowFinder";
            this.Text = "WindowStreamer";
            this.TopMost = true;
            this.gotWindowPanel.ResumeLayout(false);
            this.gotWindowPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.targetFpsEntry)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sizeHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sizeWidth)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private Label label1;
        private Label portLabel;
        private Panel gotWindowPanel;
        private Label label2;
        private NumericUpDown sizeWidth;
        private NumericUpDown sizeHeight;
        private NumericUpDown targetFpsEntry;
        private Button stopStreaming;
    }
}

