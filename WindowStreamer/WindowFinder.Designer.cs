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
            this.previewBox = new System.Windows.Forms.PictureBox();
            this.gotWindowPanel = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.sizeHeight = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.sizeWidth = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.portLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.previewBox)).BeginInit();
            this.gotWindowPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sizeHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sizeWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // previewBox
            // 
            this.previewBox.AccessibleName = "";
            this.previewBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.previewBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.previewBox.Location = new System.Drawing.Point(3, 3);
            this.previewBox.Name = "previewBox";
            this.previewBox.Size = new System.Drawing.Size(722, 302);
            this.previewBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.previewBox.TabIndex = 0;
            this.previewBox.TabStop = false;
            // 
            // gotWindowPanel
            // 
            this.gotWindowPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gotWindowPanel.Controls.Add(this.panel1);
            this.gotWindowPanel.Controls.Add(this.previewBox);
            this.gotWindowPanel.Location = new System.Drawing.Point(12, 12);
            this.gotWindowPanel.Name = "gotWindowPanel";
            this.gotWindowPanel.Size = new System.Drawing.Size(728, 447);
            this.gotWindowPanel.TabIndex = 1;
            this.gotWindowPanel.Visible = false;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.sizeHeight);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.sizeWidth);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.portLabel);
            this.panel1.Location = new System.Drawing.Point(3, 311);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(722, 133);
            this.panel1.TabIndex = 3;
            // 
            // sizeHeight
            // 
            this.sizeHeight.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.sizeHeight.Location = new System.Drawing.Point(109, 16);
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
            this.label2.Location = new System.Drawing.Point(6, 18);
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
            this.sizeWidth.Location = new System.Drawing.Point(39, 16);
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
            this.ClientSize = new System.Drawing.Size(752, 471);
            this.Controls.Add(this.gotWindowPanel);
            this.Name = "WindowFinder";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.previewBox)).EndInit();
            this.gotWindowPanel.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sizeHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sizeWidth)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private PictureBox previewBox;
        private Panel gotWindowPanel;
        private Label label1;
        private Label portLabel;
        private Panel panel1;
        private Label label2;
        private NumericUpDown sizeWidth;
        private NumericUpDown sizeHeight;
    }
}

