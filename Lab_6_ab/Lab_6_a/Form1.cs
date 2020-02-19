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

namespace Lab_6_a
{
	public partial class Form1 : Form
	{
		private delegate void CloseFormCallDelegate();
		private delegate void EnableButtonNextDelegate();
		private delegate void UpdateButtonDelegate(Button button, bool isLive);

		private const int boardSize = 16;
		private const int quarterBoardCount = 4;
		private const int quarterBoardSize = 9;

		private Tuple<int, int>[] offsets = new Tuple<int, int>[quarterBoardCount];

		private readonly int buttonXOffset = 20;
		private readonly int buttonYOffset = 20;
		private Size cellSize = new Size(width: 40, height: 40);
		private readonly int buttonInterval = 42;

		private readonly Color deadCellColor = Color.DarkGray;
		private readonly Color liveCellColor = Color.Green;

		private bool[,,] quarterBoards = new bool[quarterBoardCount, quarterBoardSize, quarterBoardSize];
		private bool[,] verticalCenter = new bool[2, boardSize];
		private bool[,] horizontalCenter = new bool[boardSize, 2];

		private Button[,] buttons = new Button[boardSize, boardSize];

		private Semaphore semaphoreToNext = new Semaphore(0, quarterBoardCount);
		private Semaphore semaphoreToDraw = new Semaphore(0, quarterBoardCount);
		private readonly Semaphore semaphoreToUpdate = new Semaphore(quarterBoardCount, quarterBoardCount);
		private readonly Barrier barrier = new Barrier(quarterBoardCount);

		private Thread[] toNext = new Thread[quarterBoardCount];
		private Thread output = null;
		private Thread runner = null;
		private volatile bool isRunning = true;
		private readonly object toLock = new object();
		private volatile bool isPlay = false;
		private volatile bool isReadyToNext = true;


		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			for (int i = 0; i < quarterBoardCount; ++i)
			{
				for (int j = 0; j < quarterBoardSize; ++j)
				{
					for (int k = 0; k < quarterBoardSize; ++k)
					{
						quarterBoards[i, j, k] = false;
					}
				}
			}

			for (int i = 0; i < 2; ++i)
			{
				for (int j = 0; j < boardSize; ++j)
				{
					verticalCenter[i, j] = false;
					horizontalCenter[j, i] = false;
				}
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

			offsets[0] = new Tuple<int, int>(0, 0);
			offsets[1] = new Tuple<int, int>(quarterBoardSize - 2, 0);
			offsets[2] = new Tuple<int, int>(0, quarterBoardSize - 2);
			offsets[3] = new Tuple<int, int>(quarterBoardSize - 2, quarterBoardSize - 2);

			for (int i = 0; i < quarterBoardCount; ++i)
			{
				toNext[i] = new Thread(new ParameterizedThreadStart((object data) =>
				{
					Tuple<int, int, int> tuple = (Tuple<int, int, int>)data;
					int index = tuple.Item1;
					int xOffset = tuple.Item2;
					int yOffset = tuple.Item3;

					while (isRunning)
					{
						semaphoreToNext.WaitOne();
						if (!isRunning)
						{
							return;
						}

						bool[,] currentTurn = new bool[quarterBoardSize, quarterBoardSize];

						lock (quarterBoards)
						{
							for (int j = 0; j < quarterBoardSize; ++j)
							{
								for (int k = 0; k < quarterBoardSize; ++k)
								{
									currentTurn[j, k] = quarterBoards[index, j, k];
								}
							}

							for (int k = 0; k < quarterBoardSize; ++k)
							{
								if (index == 0)
								{
									currentTurn[quarterBoardSize - 1, k] = verticalCenter[1, k];
									currentTurn[k, quarterBoardSize - 1] = horizontalCenter[k, 1];
								}
								else if (index == 1)
								{
									currentTurn[0, k] = verticalCenter[0, k];
									currentTurn[k, quarterBoardSize - 1] = horizontalCenter[k + quarterBoardSize - 2, 1];
								}
								else if (index == 2)
								{
									currentTurn[quarterBoardSize - 1, k] = verticalCenter[1, k + quarterBoardSize - 2];
									currentTurn[k, 0] = horizontalCenter[k, 0];
								}
								else if (index == 3)
								{
									currentTurn[0, k] = verticalCenter[0, k + quarterBoardSize - 2];
									currentTurn[k, 0] = horizontalCenter[k + quarterBoardSize - 2, 0];
								}
							}
						}

						barrier.SignalAndWait();

						bool[,] nextTurn = new bool[quarterBoardSize, quarterBoardSize];

						for (int j = 0; j < quarterBoardSize; ++j)
						{
							for (int k = 0; k < quarterBoardSize; ++k)
							{
								nextTurn[j, k] = false;
							}
						}

						for (int j = 1; j < quarterBoardSize - 1; ++j)
						{
							for (int k = 1; k < quarterBoardSize - 1; ++k)
							{
								int sumAround = 0;
								for (int l = -1; l <= 1; ++l)
								{
									for (int b = -1; b <= 1; ++b)
									{
										if (l != 0 || b != 0)
										{
											if (currentTurn[j + l, k + b])
											{
												++sumAround;
											}
										}
									}
								}

								if (currentTurn[j, k] && (sumAround < 2 || sumAround > 3))
								{
									nextTurn[j, k] = false;
								}
								else if (currentTurn[j, k] && (sumAround == 2 || sumAround == 3))
								{
									nextTurn[j, k] = true;
								}
								else if (!currentTurn[j, k] && sumAround == 3)
								{
									nextTurn[j, k] = true;
								}
							}
						}

						barrier.SignalAndWait();

						semaphoreToUpdate.WaitOne();

						lock (quarterBoards)
						{
							for (int j = 0; j < quarterBoardSize; ++j)
							{
								for (int k = 0; k < quarterBoardSize; ++k)
								{
									quarterBoards[index, j, k] = nextTurn[j, k];
								}
							}
						}

						semaphoreToDraw.Release();
					}
				}));
				toNext[i].Start(new Tuple<int, int, int>(i, offsets[i].Item1, offsets[i].Item2));
			}

			output = new Thread(() =>
			{
				while (isRunning)
				{
					for (int i = 0; i < quarterBoardCount; ++i)
					{
						semaphoreToDraw.WaitOne();
					}

					if (!isRunning)
					{
						return;
					}

					for (int i = 0; i < quarterBoardSize - 1; ++i)
					{
						verticalCenter[0, i] = quarterBoards[0, quarterBoardSize - 2, i];
						verticalCenter[1, i] = quarterBoards[1, 1, i];
					}
					for (int i = 1; i < quarterBoardSize; ++i)
					{
						verticalCenter[0, i + quarterBoardSize - 2] = quarterBoards[2, quarterBoardSize - 2, i];
						verticalCenter[1, i + quarterBoardSize - 2] = quarterBoards[3, 1, i];
					}
					for (int i = 0; i < quarterBoardSize - 1; ++i)
					{
						horizontalCenter[i, 0] = quarterBoards[0, i, quarterBoardSize - 2];
						horizontalCenter[i, 1] = quarterBoards[2, i, 1];
					}
					for (int i = 1; i < quarterBoardSize; ++i)
					{
						horizontalCenter[i + quarterBoardSize - 2, 0] = quarterBoards[1, i, quarterBoardSize - 2];
						horizontalCenter[i + quarterBoardSize - 2, 1] = quarterBoards[3, i, 1];
					}

					lock (toLock)
					{
						isReadyToNext = true;
					}

					for (int i = 0; i < quarterBoardSize - 1; ++i)
					{
						for (int j = 0; j < quarterBoardSize - 1; ++j)
						{
							// 1 area
							UpdateButton(buttons[i, j], quarterBoards[0, i, j]);

							// 2 area
							UpdateButton(buttons[i + quarterBoardSize - 1, j], quarterBoards[1, i + 1, j]);

							// 3 area
							UpdateButton(buttons[i, j + quarterBoardSize - 1], quarterBoards[2, i, j + 1]);

							// 4 area
							UpdateButton(buttons[i + quarterBoardSize - 1, j + quarterBoardSize - 1], quarterBoards[3, i + 1, j + 1]);
						}
					}

					semaphoreToUpdate.Release(quarterBoardCount);
				}
			});
			output.Start();

			runner = new Thread(() =>
			{
				while (isRunning)
				{
					lock (toLock)
					{
						if (isPlay)
						{
							if (isReadyToNext)
							{
								isReadyToNext = false;
								semaphoreToNext.Release(quarterBoardCount);
							}
						}
					}
				}
			});
			runner.Start();
		}

		private void UpdateButton(Button button, bool isLive)
		{
			if (button.InvokeRequired)
			{
				UpdateButtonDelegate d = new UpdateButtonDelegate(UpdateButton);
				button.Invoke(d, new object[] { button, isLive });
			}
			else
			{
				if (!isLive)
				{
					button.BackColor = deadCellColor;
				}
				else
				{
					button.BackColor = liveCellColor;
				}
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

				semaphoreToNext.Release(4);
				semaphoreToDraw.Release(4);

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

		private void Button_Click(object sender, EventArgs e)
		{
			Button button = (Button)sender;
			string name = button.Name;
			int position = name.IndexOf(" ");
			int i = Int32.Parse(name.Substring(0, position));
			int j = Int32.Parse(name.Substring(position + 1, name.Length - position - 1));

			bool current = true;

			if (i < quarterBoardSize - 1 && j < quarterBoardSize - 1)
			{
				current = quarterBoards[0, i, j];
				quarterBoards[0, i, j] = !current;
			}
			if (i >= quarterBoardSize - 1 && j < quarterBoardSize - 1)
			{
				current = quarterBoards[1, i - offsets[1].Item1, j - offsets[1].Item2];
				quarterBoards[1, i - offsets[1].Item1, j - offsets[1].Item2] = !current;
			}
			if (i < quarterBoardSize - 1 && j >= quarterBoardSize - 1)
			{
				current = quarterBoards[2, i - offsets[2].Item1, j - offsets[2].Item2];
				quarterBoards[2, i - offsets[2].Item1, j - offsets[2].Item2] = !current;
			}
			if (i >= quarterBoardSize - 1 && j >= quarterBoardSize - 1)
			{
				current = quarterBoards[3, i - offsets[3].Item1, j - offsets[3].Item2];
				quarterBoards[3, i - offsets[3].Item1, j - offsets[3].Item2] = !current;
			}
			
			if (i >= quarterBoardSize - 2 && i < quarterBoardSize)
			{
				current = verticalCenter[i - quarterBoardSize + 2, j];
				verticalCenter[i - quarterBoardSize + 2, j] = !current;
			}
			if (j >= quarterBoardSize - 2 && j < quarterBoardSize)
			{
				current = horizontalCenter[i, j - quarterBoardSize + 2];
				horizontalCenter[i, j - quarterBoardSize + 2] = !current;
			}

			if (current)
			{
				buttons[i, j].BackColor = deadCellColor;
			}
			else
			{
				buttons[i, j].BackColor = liveCellColor;
			}
		}

		private void ButtonNext_Click(object sender, EventArgs e)
		{
			lock (toLock)
			{
				if (isReadyToNext)
				{
					isReadyToNext = false;
					semaphoreToNext.Release(quarterBoardCount);
				}
			}
		}

		private void ButtonPlay_Click(object sender, EventArgs e)
		{
			lock (toLock)
			{
				if (isPlay)
				{
					isPlay = false;
					buttonPlay.Text = "Play";
				}
				else
				{
					isPlay = true;
					buttonPlay.Text = "Pause";
				}
			}
		}
	}
}
