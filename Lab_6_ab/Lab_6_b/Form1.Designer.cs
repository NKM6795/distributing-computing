namespace Lab_6_b
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
			this.buttonNext = new System.Windows.Forms.Button();
			this.buttonCurrentColor = new System.Windows.Forms.Button();
			this.buttonPlay = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// buttonNext
			// 
			this.buttonNext.Location = new System.Drawing.Point(800, 136);
			this.buttonNext.Name = "buttonNext";
			this.buttonNext.Size = new System.Drawing.Size(186, 64);
			this.buttonNext.TabIndex = 0;
			this.buttonNext.Text = "Next";
			this.buttonNext.UseVisualStyleBackColor = true;
			this.buttonNext.Click += new System.EventHandler(this.ButtonNext_Click);
			// 
			// buttonCurrentColor
			// 
			this.buttonCurrentColor.Enabled = false;
			this.buttonCurrentColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonCurrentColor.Location = new System.Drawing.Point(812, 227);
			this.buttonCurrentColor.Name = "buttonCurrentColor";
			this.buttonCurrentColor.Size = new System.Drawing.Size(150, 133);
			this.buttonCurrentColor.TabIndex = 1;
			this.buttonCurrentColor.UseVisualStyleBackColor = false;
			// 
			// buttonPlay
			// 
			this.buttonPlay.Location = new System.Drawing.Point(800, 37);
			this.buttonPlay.Name = "buttonPlay";
			this.buttonPlay.Size = new System.Drawing.Size(186, 64);
			this.buttonPlay.TabIndex = 2;
			this.buttonPlay.Text = "Play";
			this.buttonPlay.UseVisualStyleBackColor = true;
			this.buttonPlay.Click += new System.EventHandler(this.ButtonPlay_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1029, 861);
			this.Controls.Add(this.buttonPlay);
			this.Controls.Add(this.buttonCurrentColor);
			this.Controls.Add(this.buttonNext);
			this.Name = "Form1";
			this.Text = "Form1";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_Closing);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonNext;
		private System.Windows.Forms.Button buttonCurrentColor;
		private System.Windows.Forms.Button buttonPlay;
	}
}

