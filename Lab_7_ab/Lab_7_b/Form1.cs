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

namespace Lab_7_b
{
	public partial class Form1 : Form
	{
		private delegate void CloseFormCallDelegate();
		private delegate void UpdatePictureBoxLocationDelegate(PictureBox pictureBox, Point location);
		private delegate void UpdatePictureBoxImageDelegate(PictureBox pictureBox, bool isRight);
		private delegate void AddNewBulletDelegate(int xPosition, int yPosition);
		private delegate void RemoveBulletDelegate(PictureBox bullet);

		private const int duckCount = 2;

		private readonly PictureBox[] ducks = new PictureBox[duckCount];
		private List<PictureBox> bullets = new List<PictureBox>();
		private readonly Thread[] ducksMoving = new Thread[duckCount];
		private Thread hunt = null;
		private volatile bool needShoot = false;
		private readonly object toLockShootPosition = new object();
		private volatile int shootPosition = 0;
		private readonly bool[] needRestartDucks = new bool[duckCount];
		private volatile bool isRunning = true;

		private readonly int xBeginLeftArea = -100;
		private readonly int xEndLeftArea = -80;
		private readonly int xBeginRightArea = 1380;
		private readonly int xEndRightArea = 1400;
		private readonly int yBeginArea = 0;
		private readonly int yEndArea = 300;

		private readonly int step = 20;

		private readonly Random random = new Random();

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
					Image = global::Lab_7_b.Properties.Resources.redRight,
					Location = new Point(-100, -100),
					Name = i.ToString(),
					Size = Size = new System.Drawing.Size(77, 63),
					TabIndex = 0,
					TabStop = false
				};

				pictureBox.MouseClick += new MouseEventHandler(this.Mouse_Click);

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

			hunt = new Thread(() =>
			{
				int yPosition = 720;

				while (isRunning)
				{
					bool isNeedShoot = false;
					int xPosition = 0;

					lock (toLockShootPosition)
					{
						if (needShoot)
						{
							isNeedShoot = true;
							needShoot = false;
							xPosition = shootPosition;
						}
					}

					if (isNeedShoot)
					{
						AddNewBullet(xPosition, yPosition);

					}
					else
					{
						bool[] isAlive = new bool[bullets.Count];
						for (int i = 0; i < bullets.Count; ++i)
						{
							isAlive[i] = true;
						}

						for (int i = 0; i < bullets.Count; ++i)
						{
							PictureBox bullet = bullets[i];
							if (bullet.Location.Y < -20)
							{
								RemoveBullet(bullet);
							}
							else
							{
								bool isHit = false;
								for (int j = 0; j < duckCount && !isHit; ++j)
								{
									if (IsOverlap(bullet, ducks[j]))
									{
										RestartDuck(j);

										RemoveBullet(bullet);
										isHit = true;
									}
								}

								if (!isHit)
								{
									Point currentPosition = bullet.Location;
									UpdatePictureBoxLocation(bullet, new Point(currentPosition.X, currentPosition.Y - step));
								}
								else
								{
									isAlive[i] = false;
								}
							}
						}

						List<PictureBox> updatedBullets = new List<PictureBox>();
						for (int i = 0; i < bullets.Count; ++i)
						{
							if (isAlive[i])
							{
								updatedBullets.Add(bullets[i]);
							}
						}

						bullets = updatedBullets;
					}

					Thread.Sleep(100);
				}
			});
			hunt.Start();
		}

		private bool IsOverlap(PictureBox first, PictureBox second)
		{
			Point topLeft_1 = first.Location;
			Point bottomRight_1 = new Point(topLeft_1.X + first.Size.Width, topLeft_1.Y + first.Size.Height);
			Point topLeft_2 = second.Location;
			Point bottomRight_2 = new Point(topLeft_2.X + second.Size.Width, topLeft_2.Y + second.Size.Height);
			
			if (topLeft_1.X > bottomRight_2.X || topLeft_2.X > bottomRight_1.X)
			{
				return false;
			}

			if (topLeft_1.Y > bottomRight_2.Y || topLeft_2.Y > bottomRight_1.Y)
			{
				return false;
			}

			return true;
		}

		private void RestartDuck(int index)
		{
			lock (needRestartDucks)
			{
				needRestartDucks[index] = true;
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
					pictureBox.Image = global::Lab_7_b.Properties.Resources.redRight;
				}
				else
				{
					pictureBox.Image = global::Lab_7_b.Properties.Resources.redLeft;
				}
			}
		}
		
		private void AddNewBullet(int xPosition, int yPosition)
		{
			if (InvokeRequired)
			{
				AddNewBulletDelegate d = new AddNewBulletDelegate(AddNewBullet);
				Invoke(d, new object[] { xPosition, yPosition });
			}
			else
			{
				PictureBox pictureBox = new PictureBox
				{
					BackColor = Color.FromArgb(56, 56, 252),
					ForeColor = Color.FromArgb(56, 56, 252),
					Image = global::Lab_7_b.Properties.Resources.bullet1,
					Location = new Point(xPosition, yPosition),
					Name = "bullet",
					Size = Size = new System.Drawing.Size(8, 13),
					TabIndex = 0,
					TabStop = false
				};

				pictureBox.MouseClick += new MouseEventHandler(this.Mouse_Click);

				Controls.Add(pictureBox);
				bullets.Add(pictureBox);
			}
		}
		
		private void RemoveBullet(PictureBox bullet)
		{
			if (InvokeRequired)
			{
				RemoveBulletDelegate d = new RemoveBulletDelegate(RemoveBullet);
				Invoke(d, new object[] { bullet });
			}
			else
			{
				Controls.Remove(bullet);
			}
		}



		private void Mouse_Click(object sender, MouseEventArgs e)
		{
			lock (toLockShootPosition)
			{
				needShoot = true;
				shootPosition = e.Location.X;
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
