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

namespace Lab_6_b
{
	public partial class Form1 : Form
	{
		private delegate void CloseFormCallDelegate();
		private delegate void EnableButtonNextDelegate();
		private delegate void UpdateButtonDelegate(Button button, Color color);

		private const int boardSize = 32;
		private const int civilizationCount = 4;

		private readonly int buttonXOffset = 20;
		private readonly int buttonYOffset = 20;
		private Size cellSize = new Size(width: 20, height: 20);
		private readonly int buttonInterval = 22;
		private readonly int colorButtonXOffset = 800;
		private readonly int colorButtonYOffset = 380;

		private readonly Color deadCellColor = Color.DarkGray;
		private readonly Color[] liveCellColors = new Color[civilizationCount] { Color.Green, Color.Red, Color.Yellow, Color.Blue };

		private int currentCivilization = 0;

		private readonly bool[,,] boards = new bool[civilizationCount, boardSize, boardSize];

		private readonly Button[,] buttons = new Button[boardSize, boardSize];

		private readonly Semaphore semaphoreToNext = new Semaphore(0, civilizationCount);
		private readonly Semaphore semaphoreToDraw = new Semaphore(0, civilizationCount);
		private readonly Barrier barrier = new Barrier(civilizationCount);

		private readonly Thread[] toNext = new Thread[civilizationCount];
		private Thread output = null;
		private volatile bool isRunning = true;


		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			for (int i = 0; i < civilizationCount; ++i)
			{
				for (int j = 0; j < boardSize; ++j)
				{
					for (int k = 0; k < boardSize; ++k)
					{
						boards[i, j, k] = false;
					}
				}
			}

			buttonCurrentColor.BackColor = liveCellColors[currentCivilization];

			for (int i = 0; i < civilizationCount; ++i)
			{
				Button button = new Button
				{
					BackColor = liveCellColors[i],
					FlatStyle = FlatStyle.Flat,
					Location = new Point(colorButtonXOffset + buttonInterval * i, colorButtonYOffset),
					Name = i.ToString(),
					Size = cellSize,
					TabIndex = 0,
					UseVisualStyleBackColor = false
				};

				button.Click += new System.EventHandler(this.ColorButton_Click);
				Controls.Add(button);
			}

			for (int i = 0; i < boardSize; ++i)
			{
				for (int j = 0; j < boardSize; ++j)
				{
					Button button = new Button
					{
						BackColor = deadCellColor,
						FlatStyle = FlatStyle.Flat,
						Location = new Point(buttonXOffset + buttonInterval * i, buttonYOffset + buttonInterval * j),
						Name = i.ToString() + " " + j.ToString(),
						Size = cellSize,
						TabIndex = 0,
						UseVisualStyleBackColor = false
					};

					if (i == 0 || j == 0 || i == boardSize - 1 || j == boardSize - 1)
					{
						button.Visible = false;
					}
					else
					{
						button.Click += new System.EventHandler(this.Button_Click);
					}

					Controls.Add(button);
					buttons[i, j] = button;
				}
			}

			for (int i = 0; i < civilizationCount; ++i)
			{
				toNext[i] = new Thread(new ParameterizedThreadStart((object data) =>
				{
					int index = (int)data;

					while (isRunning)
					{
						semaphoreToNext.WaitOne();
						if (!isRunning)
						{
							return;
						}

						bool[,] currentTurn = new bool[boardSize, boardSize];

						lock (boards)
						{
							for (int j = 0; j < boardSize; ++j)
							{
								for (int k = 0; k < boardSize; ++k)
								{
									currentTurn[j, k] = boards[index, j, k];
								}
							}
						}

						barrier.SignalAndWait();


						bool[,] nextTurn = new bool[boardSize, boardSize];

						for (int j = 0; j < boardSize; ++j)
						{
							for (int k = 0; k < boardSize; ++k)
							{
								nextTurn[j, k] = false;
							}
						}

						for (int j = 1; j < boardSize - 1; ++j)
						{
							for (int k = 1; k < boardSize - 1; ++k)
							{
								int sumAround = GetSumAround(index, j, k);

								int maxOtherSum = 0;
								for (int l = 0; l < civilizationCount; ++l)
								{
									if (l != index)
									{
										maxOtherSum = Math.Max(GetSumAround(l, j, k), maxOtherSum);
									}
								}

								if (currentTurn[j, k] && (sumAround < 2 || sumAround > 3))
								{
									nextTurn[j, k] = false;
								}
								else if (currentTurn[j, k] && sumAround == 3)
								{
									nextTurn[j, k] = true;
								}
								else if (currentTurn[j, k] && sumAround == 2)
								{
									if (maxOtherSum == 3)
									{
										nextTurn[j, k] = false;
									}
									else
									{
										nextTurn[j, k] = true;
									}
								}
								else if (!currentTurn[j, k] && sumAround == 3)
								{
									if (maxOtherSum == 3)
									{
										nextTurn[j, k] = false;
									}
									else
									{
										nextTurn[j, k] = true;
									}
								}
							}
						}

						barrier.SignalAndWait();

						lock (boards)
						{
							for (int j = 0; j < boardSize; ++j)
							{
								for (int k = 0; k < boardSize; ++k)
								{
									boards[index, j, k] = nextTurn[j, k];
								}
							}
						}

						semaphoreToDraw.Release();
					}
				}));
				toNext[i].Start(i);
			}

			output = new Thread(() =>
			{
				while (isRunning)
				{
					for (int i = 0; i < civilizationCount; ++i)
					{
						semaphoreToDraw.WaitOne();
					}

					if (!isRunning)
					{
						return;
					}


					for (int i = 0; i < boardSize - 1; ++i)
					{
						for (int j = 0; j < boardSize - 1; ++j)
						{
							Color color = deadCellColor;

							for (int l = 0; l < civilizationCount; ++l)
							{
								if (boards[l, i, j])
								{
									color = liveCellColors[l];
								}
							}

							UpdateButton(buttons[i, j], color);
						}
					}

					EnableButtonNext();
				}
			});
			output.Start();
		}

		private int GetSumAround(int civilizationIndex, int i, int j)
		{
			int sumAround = 0;
			for (int l = -1; l <= 1; ++l)
			{
				for (int b = -1; b <= 1; ++b)
				{
					if (l != 0 || b != 0)
					{
						if (boards[civilizationIndex, i + l, j + b])
						{
							++sumAround;
						}
					}
				}
			}

			return sumAround;
		}

		private void UpdateButton(Button button, Color color)
		{
			if (button.InvokeRequired)
			{
				UpdateButtonDelegate d = new UpdateButtonDelegate(UpdateButton);
				button.Invoke(d, new object[] { button, color });
			}
			else
			{
				button.BackColor = color;
			}
		}

		private void EnableButtonNext()
		{
			if (buttonNext.InvokeRequired)
			{
				EnableButtonNextDelegate d = new EnableButtonNextDelegate(EnableButtonNext);
				buttonNext.Invoke(d, new object[] { });
			}
			else
			{
				buttonNext.Enabled = true;
			}
		}

		private void Form1_Closing(object sender, FormClosingEventArgs e)
		{
			if (this.isRunning)
			{
				this.isRunning = false;

				semaphoreToNext.Release(civilizationCount);
				semaphoreToDraw.Release(civilizationCount);

				e.Cancel = true;

				new Thread(() =>
				{
					Thread.Sleep(100);
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

		private void ColorButton_Click(object sender, EventArgs e)
		{
			Button button = (Button)sender;
			string index = button.Name;
			currentCivilization = Int32.Parse(index);

			buttonCurrentColor.BackColor = liveCellColors[currentCivilization];
		}

		private void Button_Click(object sender, EventArgs e)
		{
			Button button = (Button)sender;
			string name = button.Name;
			int position = name.IndexOf(" ");
			int i = Int32.Parse(name.Substring(0, position));
			int j = Int32.Parse(name.Substring(position + 1, name.Length - position - 1));

			bool current = boards[currentCivilization, i, j];

			for (int l = 0; l < civilizationCount; ++l)
			{
				boards[l, i, j] = false;
			}

			boards[currentCivilization, i, j] = !current;

			if (current)
			{
				buttons[i, j].BackColor = deadCellColor;
			}
			else
			{
				buttons[i, j].BackColor = liveCellColors[currentCivilization];
			}
		}

		private void ButtonNext_Click(object sender, EventArgs e)
		{
			semaphoreToNext.Release(civilizationCount);
			buttonNext.Enabled = false;
		}
	}
}
