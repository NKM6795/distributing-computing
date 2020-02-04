using System;
using System.Threading;

namespace Lab_2_a
{
	class Hive
	{
		private bool[,] honeycombs = { { true, false, false, false },
										{ false, false, false, false },
										{ false, false, false, false },
										{ false, false, false, false } };

		private int width = 4;
		private int height = 4;

		private int xCurrent = 0;
		private int yCurrent = 0;


		private Thread runner = null;
		private bool isRunnerMoving = true;

		private bool[] findResult;

		private Random random = new Random();

		public Hive()
		{

		}

		~Hive()
		{
			isRunnerMoving = false;
		}

		public void StartRunningAway()
		{
			runner = new Thread(() =>
			{
				while (isRunnerMoving)
				{
					// 0 - try left else right
					// 1 - try right else left
					// 2 - try up else down
					// 3 - try down else up
					int direct = random.Next(0, 4);

					int xOffset = 0;
					int yOffset = 0;
					if ((direct == 0 && xCurrent > 0) || (direct == 1 && xCurrent == width - 1))
					{
						xOffset = -1;
					}
					if ((direct == 0 && xCurrent == 0) || (direct == 1 && xCurrent < width - 1))
					{
						xOffset = 1;
					}
					if ((direct == 2 && yCurrent > 0) || (direct == 3 && yCurrent == height - 1))
					{
						yOffset = -1;
					}
					if ((direct == 2 && yCurrent == 0) || (direct == 3 && yCurrent < height - 1))
					{
						yOffset = 1;
					}

					lock (honeycombs)
					{
						honeycombs[xCurrent, yCurrent] = false;
						xCurrent += xOffset;
						yCurrent += yOffset;
						honeycombs[xCurrent, yCurrent] = true;
					}
				}
			});

			runner.Start();
		}

		private void FindInRow(int index)
		{
			bool[] currentArray = new bool[width];
			lock(honeycombs)
			{
				for (int i = 0; i < width; ++i)
				{
					currentArray[i] = honeycombs[index, i];
				}
			}

			for (int i = 0; i < height; ++i)
			{
				if (currentArray[i])
				{
					findResult[index] = true;
				}
			}
		}

		public bool Find()
		{
			findResult = new bool[height];
			for (int i = 0; i < height; ++i)
			{
				findResult[i] = false;
			}

			Thread[] threads = new Thread[height];
			for (int i = 0; i < height; ++i)
			{
				threads[i] = new Thread(new ParameterizedThreadStart((object index) => { FindInRow((int)index); }));
				threads[i].Start(i);
			}

			for (int i = 0; i < height; ++i)
			{
				threads[i].Join();
			}

			for (int i = 0; i < height; ++i)
			{
				if (findResult[i])
				{
					return true;
				}
			}
			return false;
		}
	}



	class Program
	{
		static void Main(string[] args)
		{
			Hive hive = new Hive();

			hive.StartRunningAway();

			int iterationCount = 10;
			for (int i = 0; i < iterationCount; ++i)
			{
				Console.WriteLine("Inter = {0}, res = {1}", i, hive.Find());
			}
		}
	}
}
