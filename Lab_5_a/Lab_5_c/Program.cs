using System;

namespace Lab_5_c
{
	using System;
	using System.Threading;

	class MatrixComparator
	{
		private const int threadCount = 3;
		private const int matrixSize = 5;
		private Barrier barrier = new Barrier(threadCount);

		private Thread[] threads = new Thread[threadCount];
		private object toLockIsEnd = new object();
		private volatile bool isRunning = true;

		private int[,,] matrices = new int[threadCount, matrixSize, matrixSize];

		private Random random = new Random();

		public MatrixComparator()
		{
			for (int i = 0; i < threadCount; ++i)
			{
				for (int j = 0; j < matrixSize; ++j)
				{
					for (int k = 0; k < matrixSize; ++k)
					{
						matrices[i, j, k] = random.Next(10);
					}
				}
			}

			for (int i = 0; i < threadCount; ++i)
			{
				threads[i] = new Thread(new ParameterizedThreadStart((object i) =>
				{
					int index = (int)i;

					while (isRunning)
					{
						if (random.Next(2) == 0)
						{
							int xToChange = random.Next(matrixSize);
							int yToChange = random.Next(matrixSize);
							lock (toLockIsEnd)
							{
								if (random.Next(2) == 0)
								{
									++matrices[index, xToChange, yToChange];
								}
								else
								{
									--matrices[index, xToChange, yToChange];
								}
							}
						}

						barrier.SignalAndWait();

						lock (toLockIsEnd)
						{
							int[] sums = new int[threadCount];

							for (int j = 0; j < threadCount; ++j)
							{
								for (int k = 0; k < matrixSize; ++k)
								{
									for (int l = 0; l < matrixSize; ++l)
									{
										sums[j] += matrices[j, k, l];
									}
								}
							}

							bool isEnd = true;
							for (int j = 1; j < threadCount; ++j)
							{
								if (sums[j - 1] != sums[j])
								{
									isEnd = false;
								}
							}
							if (isEnd)
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
			MatrixComparator matrixComparator = new MatrixComparator();
			Thread.Sleep(1000);
		}
	}
}
