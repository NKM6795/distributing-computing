using System;
using System.Threading;

namespace Lab_5_b
{
	class StringUpdater
	{
		private const int threadCount = 4;
		private const int stringSize = 20;
		private Barrier barrier = new Barrier(threadCount);

		private Thread[] threads = new Thread[threadCount];
		private object toLockIsEnd = new object();
		private volatile bool isRunning = true;

		private char[,] strings = new char[threadCount, stringSize];

		private Random random = new Random();

		public StringUpdater()
		{
			for (int i = 0; i < threadCount; ++i)
			{
				for (int j = 0; j < stringSize; ++j)
				{
					strings[i, j] = (char)((int)'A' + random.Next(4));
				}
			}

			for (int i = 0; i < threadCount; ++i)
			{
				threads[i] = new Thread(new ParameterizedThreadStart((object i) =>
				{
					int index = (int)i;

					while (isRunning)
					{
						int toChange = random.Next(stringSize);
						if (random.Next(2) == 0)
						{
							lock (toLockIsEnd)
							{
								if (strings[index, toChange] == 'A')
								{
									strings[index, toChange] = 'C';
								}
								else if (strings[index, toChange] == 'C')
								{
									strings[index, toChange] = 'A';
								}
								else if (strings[index, toChange] == 'B')
								{
									strings[index, toChange] = 'D';
								}
								else if (strings[index, toChange] == 'D')
								{
									strings[index, toChange] = 'B';
								}
							}
						}

						barrier.SignalAndWait();

						lock (toLockIsEnd)
						{
							int[] sums = new int[threadCount];

							for (int j = 0; j < threadCount; ++j)
							{
								for (int k = 0; k < stringSize; ++k)
								{
									if (strings[j, k] == 'A' || strings[j, k] == 'B')
									{
										++sums[j];
									}
								}
							}

							Array.Sort(sums);

							int last = sums[0];
							int inRow = 0;
							int maxInRow = 0;

							for (int j = 1; j < threadCount; ++j)
							{
								if (sums[j] == last)
								{
									++inRow;
								}
								else
								{
									maxInRow = Math.Max(maxInRow, inRow);
									inRow = 0;
									last = sums[j];
								}
							}
							maxInRow = Math.Max(maxInRow, inRow);
							
							if (maxInRow == 2)
							{
								isRunning = false;
							}

							if (!isRunning)
							{
								Console.WriteLine("Complited");
							}
						}

						barrier.SignalAndWait();

					}
				}));
				threads[i].Start(i);
			}
		}
	}


	class Program
	{
		static void Main(string[] args)
		{
			StringUpdater recruits = new StringUpdater();
			Thread.Sleep(1000);
		}
	}
}
