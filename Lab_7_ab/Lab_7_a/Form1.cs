using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab_7_a
{
	public partial class Form1 : Form
	{
		private delegate void CloseFormCallDelegate();
		private delegate void UpdatePictureBoxLocationDelegate(PictureBox pictureBox, Point location);
		private delegate void UpdatePictureBoxImageDelegate(PictureBox pictureBox, bool isRight);

		private const int duckCount = 2;

		private PictureBox[] ducks = new PictureBox[duckCount];
		private Thread[] ducksMoving = new Thread[duckCount];
		private bool[] needRestartDucks = new bool[duckCount];
		private volatile bool isRunning = true;

		private int xBeginLeftArea = -100;
		private int xEndLeftArea = -80;
		private int xBeginRightArea = 1380;
		private int xEndRightArea = 1400;
		private int yBeginArea = 0;
		private int yEndArea = 300;

		private int step = 10;

		private Random random = new Random();

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			for (int i = 0; i < duckCount; ++i)
			{
				needRestartDucks[i] = true;
			}

			for (int i = 0; i < duckCount; ++i)
			{
				PictureBox pictureBox = new PictureBox
				{
					BackColor = Color.FromArgb(56, 56, 252),
					ForeColor = Color.FromArgb(56, 56, 252),
					Image = global::Lab_7_a.Properties.Resources.redRight,
					Location = new Point(-100, -100),
					Name = i.ToString(),
					Size = Size = new System.Drawing.Size(77, 63),
					TabIndex = 0,
					TabStop = false
				};

				pictureBox.Click += new System.EventHandler(this.PictureBox_Click);

				Controls.Add(pictureBox);
				ducks[i] = pictureBox;
			}

			for (int i = 0; i < duckCount; ++i)
			{
				ducksMoving[i] = new Thread(new ParameterizedThreadStart((object data) =>
				{
					int index = (int)data;

					bool rightDirect = true;

					while (isRunning)
					{
						bool needRestart = false;

						lock (needRestartDucks)
						{
							if (needRestartDucks[index])
							{
								needRestart = true;
								needRestartDucks[index] = false;
							}
						}

						if (needRestart)
						{
							int yPosition = yBeginArea + random.Next(yEndArea - yBeginArea);

							int xPosition = 0;

							if (random.Next(xEndLeftArea - xBeginLeftArea + xEndRightArea - xBeginRightArea) < xEndLeftArea - xBeginLeftArea)
							{
								xPosition = xBeginLeftArea + random.Next(xEndLeftArea - xBeginLeftArea);
							}
							else
							{
								xPosition = xBeginRightArea + random.Next(xEndRightArea - xBeginRightArea);
							}


							UpdatePictureBoxLocation(ducks[index], new Point(xPosition, yPosition));
						}
						else
						{
							Point currentPosition = ducks[index].Location;

							if (currentPosition.X < xBeginLeftArea)
							{
								rightDirect = true;
								UpdatePictureBoxImage(ducks[index], true);
							}
							else if (currentPosition.X > xEndRightArea)
							{
								rightDirect = false;
								UpdatePictureBoxImage(ducks[index], false);
							}

							if (rightDirect)
							{
								UpdatePictureBoxLocation(ducks[index], new Point(currentPosition.X + step, currentPosition.Y));
							}
							else
							{
								UpdatePictureBoxLocation(ducks[index], new Point(currentPosition.X - step, currentPosition.Y));
							}
						}

						Thread.Sleep(100);
					}
				}));

				ducksMoving[i].Start(i);
			}
		}
		private void PictureBox_Click(object sender, EventArgs e)
		{
			PictureBox pictureBox = (PictureBox)sender;
			string index = pictureBox.Name;
			int i = Int32.Parse(index);

			lock (needRestartDucks)
			{
				needRestartDucks[i] = true;
			}
		}

		private void UpdatePictureBoxLocation(PictureBox pictureBox, Point location)
		{
			if (pictureBox.InvokeRequired)
			{
				UpdatePictureBoxLocationDelegate d = new UpdatePictureBoxLocationDelegate(UpdatePictureBoxLocation);
				pictureBox.Invoke(d, new object[] { pictureBox, location });
			}
			else
			{
				SuspendLayout();
				pictureBox.Location = location;
				ResumeLayout();
			}
		}

		private void UpdatePictureBoxImage(PictureBox pictureBox, bool isRight)
		{
			if (pictureBox.InvokeRequired)
			{
				UpdatePictureBoxImageDelegate d = new UpdatePictureBoxImageDelegate(UpdatePictureBoxImage);
				pictureBox.Invoke(d, new object[] { pictureBox, isRight });
			}
			else
			{
				if (isRight)
				{
					pictureBox.Image = global::Lab_7_a.Properties.Resources.redRight;
				}
				else
				{
					pictureBox.Image = global::Lab_7_a.Properties.Resources.redLeft;
				}
			}
		}


		private void Form1_Closing(object sender, FormClosingEventArgs e)
		{
			if (this.isRunning)
			{
				this.isRunning = false;

				e.Cancel = true;

				new Thread(() =>
				{
					Thread.Sleep(200);
					CloseForm();
				}).Start();
			}
		}

		private void CloseForm()
		{
			if (this.InvokeRequired)
			{
				CloseFormCallDelegate d = new CloseFormCallDelegate(CloseForm);
				this.Invoke(d, new object[] { });
			}
			else
			{
				this.Close();
			}
		}
	}
}
