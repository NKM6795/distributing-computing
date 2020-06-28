using System;
using System.Windows.Forms;

namespace DC_Lab1
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
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.listBox2 = new System.Windows.Forms.ListBox();
			this.trackBar1 = new System.Windows.Forms.TrackBar();
			this.start1_button = new System.Windows.Forms.Button();
			this.start2_button = new System.Windows.Forms.Button();
			this.finish1_button = new System.Windows.Forms.Button();
			this.finish2_button = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
			this.SuspendLayout();
			// 
			// listBox1
			// 
			this.listBox1.FormattingEnabled = true;
			this.listBox1.Items.AddRange(new object[] {
            "4",
            "3",
            "2",
            "1",
            "0"});
			this.listBox1.Location = new System.Drawing.Point(43, 46);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(76, 69);
			this.listBox1.TabIndex = 0;
			this.listBox1.SelectedIndexChanged += new System.EventHandler(this.ListBox1_SelectedIndexChanged);
			// 
			// listBox2
			// 
			this.listBox2.FormattingEnabled = true;
			this.listBox2.Items.AddRange(new object[] {
            "4",
            "3",
            "2",
            "1",
            "0"});
			this.listBox2.Location = new System.Drawing.Point(632, 46);
			this.listBox2.Name = "listBox2";
			this.listBox2.Size = new System.Drawing.Size(76, 69);
			this.listBox2.TabIndex = 1;
			this.listBox2.SelectedIndexChanged += new System.EventHandler(this.ListBox2_SelectedIndexChanged);
			// 
			// trackBar1
			// 
			this.trackBar1.Location = new System.Drawing.Point(188, 46);
			this.trackBar1.Maximum = 100;
			this.trackBar1.Name = "trackBar1";
			this.trackBar1.Size = new System.Drawing.Size(402, 45);
			this.trackBar1.TabIndex = 3;
			// 
			// start1_button
			// 
			this.start1_button.Location = new System.Drawing.Point(43, 174);
			this.start1_button.Name = "start1_button";
			this.start1_button.Size = new System.Drawing.Size(75, 23);
			this.start1_button.TabIndex = 4;
			this.start1_button.Text = "Start 1";
			this.start1_button.UseVisualStyleBackColor = true;
			this.start1_button.Click += new System.EventHandler(this.Start1_button_Click);
			// 
			// start2_button
			// 
			this.start2_button.Location = new System.Drawing.Point(632, 174);
			this.start2_button.Name = "start2_button";
			this.start2_button.Size = new System.Drawing.Size(75, 23);
			this.start2_button.TabIndex = 5;
			this.start2_button.Text = "Start 2";
			this.start2_button.UseVisualStyleBackColor = true;
			this.start2_button.Click += new System.EventHandler(this.Start2_button_Click);
			// 
			// finish1_button
			// 
			this.finish1_button.Location = new System.Drawing.Point(43, 203);
			this.finish1_button.Name = "finish1_button";
			this.finish1_button.Size = new System.Drawing.Size(75, 23);
			this.finish1_button.TabIndex = 6;
			this.finish1_button.Text = "Finish 1";
			this.finish1_button.UseVisualStyleBackColor = true;
			this.finish1_button.Click += new System.EventHandler(this.Finish1_button_Click);
			// 
			// finish2_button
			// 
			this.finish2_button.Location = new System.Drawing.Point(632, 203);
			this.finish2_button.Name = "finish2_button";
			this.finish2_button.Size = new System.Drawing.Size(75, 23);
			this.finish2_button.TabIndex = 7;
			this.finish2_button.Text = "Finish 1";
			this.finish2_button.UseVisualStyleBackColor = true;
			this.finish2_button.Click += new System.EventHandler(this.Finish2_button_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.finish2_button);
			this.Controls.Add(this.finish1_button);
			this.Controls.Add(this.start2_button);
			this.Controls.Add(this.start1_button);
			this.Controls.Add(this.listBox2);
			this.Controls.Add(this.listBox1);
			this.Controls.Add(this.trackBar1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_Closing);
			this.Load += new System.EventHandler(this.Form1_Load);
			((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}


		#endregion

		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.ListBox listBox2;
		private System.Windows.Forms.TrackBar trackBar1;
		private Button start1_button;
		private Button start2_button;
		private Button finish1_button;
		private Button finish2_button;
	}
}

