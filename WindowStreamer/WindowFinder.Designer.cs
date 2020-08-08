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
            this.hideStreamedCheckbox = new System.Windows.Forms.CheckBox();
            this.imageQuality = new System.Windows.Forms.NumericUpDown();
            this.stopStreaming = new System.Windows.Forms.Button();
            this.targetFpsEntry = new System.Windows.Forms.NumericUpDown();
            this.sizeHeight = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.sizeWidth = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.gotWindowPanel = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.noWindowPanel = new System.Windows.Forms.TableLayoutPanel();
            this.visibleWindowLabel = new System.Windows.Forms.Label();
            this.hiddenWindowLabel = new System.Windows.Forms.Label();
            this.serverUrlLabel = new System.Windows.Forms.LinkLabel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.imageQuality)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.targetFpsEntry)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sizeHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sizeWidth)).BeginInit();
            this.gotWindowPanel.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.noWindowPanel.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // hideStreamedCheckbox
            // 
            this.hideStreamedCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.hideStreamedCheckbox.AutoSize = true;
            this.hideStreamedCheckbox.Location = new System.Drawing.Point(3, 3);
            this.hideStreamedCheckbox.Name = "hideStreamedCheckbox";
            this.hideStreamedCheckbox.Size = new System.Drawing.Size(87, 17);
            this.hideStreamedCheckbox.TabIndex = 9;
            this.hideStreamedCheckbox.Text = "Hide window";
            this.hideStreamedCheckbox.UseVisualStyleBackColor = true;
            this.hideStreamedCheckbox.CheckedChanged += new System.EventHandler(this.hideStreamedCheckbox_CheckedChanged);
            // 
            // imageQuality
            // 
            this.imageQuality.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.imageQuality.Location = new System.Drawing.Point(89, 74);
            this.imageQuality.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.imageQuality.Name = "imageQuality";
            this.imageQuality.Size = new System.Drawing.Size(59, 20);
            this.imageQuality.TabIndex = 8;
            this.imageQuality.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.imageQuality.ValueChanged += new System.EventHandler(this.imageQuality_ValueChanged);
            // 
            // stopStreaming
            // 
            this.stopStreaming.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.stopStreaming.AutoSize = true;
            this.stopStreaming.Location = new System.Drawing.Point(96, 3);
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
            this.targetFpsEntry.Location = new System.Drawing.Point(89, 48);
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
            this.sizeHeight.Location = new System.Drawing.Point(73, 3);
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
            this.label2.Location = new System.Drawing.Point(3, 13);
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
            this.sizeWidth.Location = new System.Drawing.Point(3, 3);
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
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Server URL";
            // 
            // gotWindowPanel
            // 
            this.gotWindowPanel.AutoSize = true;
            this.gotWindowPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gotWindowPanel.ColumnCount = 2;
            this.gotWindowPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.49212F));
            this.gotWindowPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67.50788F));
            this.gotWindowPanel.Controls.Add(this.imageQuality, 1, 3);
            this.gotWindowPanel.Controls.Add(this.label1, 0, 0);
            this.gotWindowPanel.Controls.Add(this.label2, 0, 1);
            this.gotWindowPanel.Controls.Add(this.targetFpsEntry, 1, 2);
            this.gotWindowPanel.Controls.Add(this.flowLayoutPanel1, 1, 1);
            this.gotWindowPanel.Controls.Add(this.label3, 0, 2);
            this.gotWindowPanel.Controls.Add(this.label4, 0, 3);
            this.gotWindowPanel.Controls.Add(this.serverUrlLabel, 1, 0);
            this.gotWindowPanel.Controls.Add(this.flowLayoutPanel2, 1, 4);
            this.gotWindowPanel.Location = new System.Drawing.Point(12, 12);
            this.gotWindowPanel.Name = "gotWindowPanel";
            this.gotWindowPanel.RowCount = 5;
            this.gotWindowPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.gotWindowPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.gotWindowPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.gotWindowPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.gotWindowPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.gotWindowPanel.Size = new System.Drawing.Size(267, 117);
            this.gotWindowPanel.TabIndex = 5;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.sizeWidth);
            this.flowLayoutPanel1.Controls.Add(this.sizeHeight);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(89, 16);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(140, 26);
            this.flowLayoutPanel1.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(27, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "FPS";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 71);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Quality";
            // 
            // noWindowPanel
            // 
            this.noWindowPanel.ColumnCount = 2;
            this.noWindowPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.noWindowPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.noWindowPanel.Controls.Add(this.visibleWindowLabel, 0, 0);
            this.noWindowPanel.Controls.Add(this.hiddenWindowLabel, 1, 0);
            this.noWindowPanel.Location = new System.Drawing.Point(304, 12);
            this.noWindowPanel.Name = "noWindowPanel";
            this.noWindowPanel.RowCount = 1;
            this.noWindowPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.noWindowPanel.Size = new System.Drawing.Size(200, 100);
            this.noWindowPanel.TabIndex = 6;
            // 
            // visibleWindowLabel
            // 
            this.visibleWindowLabel.AutoSize = true;
            this.visibleWindowLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.visibleWindowLabel.Location = new System.Drawing.Point(3, 0);
            this.visibleWindowLabel.Name = "visibleWindowLabel";
            this.visibleWindowLabel.Size = new System.Drawing.Size(94, 100);
            this.visibleWindowLabel.TabIndex = 0;
            this.visibleWindowLabel.Text = "With visible window";
            this.visibleWindowLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // hiddenWindowLabel
            // 
            this.hiddenWindowLabel.AutoSize = true;
            this.hiddenWindowLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hiddenWindowLabel.Location = new System.Drawing.Point(103, 0);
            this.hiddenWindowLabel.Name = "hiddenWindowLabel";
            this.hiddenWindowLabel.Size = new System.Drawing.Size(94, 100);
            this.hiddenWindowLabel.TabIndex = 1;
            this.hiddenWindowLabel.Text = "Hiding window";
            this.hiddenWindowLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // serverUrlLabel
            // 
            this.serverUrlLabel.AutoSize = true;
            this.serverUrlLabel.Location = new System.Drawing.Point(89, 0);
            this.serverUrlLabel.Name = "serverUrlLabel";
            this.serverUrlLabel.Size = new System.Drawing.Size(16, 13);
            this.serverUrlLabel.TabIndex = 10;
            this.serverUrlLabel.TabStop = true;
            this.serverUrlLabel.Text = "...";
            this.serverUrlLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.serverUrlLabel_LinkClicked);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel2.Controls.Add(this.hideStreamedCheckbox);
            this.flowLayoutPanel2.Controls.Add(this.stopStreaming);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(89, 100);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(174, 14);
            this.flowLayoutPanel2.TabIndex = 11;
            // 
            // WindowFinder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(316, 204);
            this.Controls.Add(this.noWindowPanel);
            this.Controls.Add(this.gotWindowPanel);
            this.MaximizeBox = false;
            this.Name = "WindowFinder";
            this.Text = "WindowStreamer";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.imageQuality)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.targetFpsEntry)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sizeHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sizeWidth)).EndInit();
            this.gotWindowPanel.ResumeLayout(false);
            this.gotWindowPanel.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.noWindowPanel.ResumeLayout(false);
            this.noWindowPanel.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Label label1;
        private Label label2;
        private NumericUpDown sizeWidth;
        private NumericUpDown sizeHeight;
        private NumericUpDown targetFpsEntry;
        private Button stopStreaming;
        private NumericUpDown imageQuality;
        private CheckBox hideStreamedCheckbox;
        private TableLayoutPanel gotWindowPanel;
        private FlowLayoutPanel flowLayoutPanel1;
        private Label label3;
        private Label label4;
        private TableLayoutPanel noWindowPanel;
        private Label visibleWindowLabel;
        private Label hiddenWindowLabel;
        private LinkLabel serverUrlLabel;
        private FlowLayoutPanel flowLayoutPanel2;
    }
}

