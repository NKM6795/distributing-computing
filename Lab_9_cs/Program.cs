using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Lab_9_cs
{
	public class MatrixMultiplication
	{
		private const int size = 1000;
		private const int worldSize = 4;

		private float[] a = new float[size * size];
		private float[] b = new float[size * size];
		private float[] c = new float[size * size];

		private Random random = new Random();

		public MatrixMultiplication()
		{
			for (int i = 0; i < size; ++i)
			{
				for (int j = 0; j < size; ++j)
				{
					a[i * size + j] = (((float)random.NextDouble()) - 0.5f) * 50.0f;
				}
			}
			for (int i = 0; i < size; ++i)
			{
				for (int j = 0; j < size; ++j)
				{
					b[i * size + j] = (((float)random.NextDouble()) - 0.5f) * 50.0f;
				}
			}
			for (int i = 0; i < size; ++i)
			{
				for (int j = 0; j < size; ++j)
				{
					c[i * size + j] = 0.0f;
				}
			}
		}

		public float consistentMultiplicationTime()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();

			for (int i = 0; i < size; ++i)
			{
				for (int j = 0; j < size; ++j)
				{
					float temp = 0.0f;
					for (int k = 0; k < size; ++k)
					{
						temp += a[i * size + k] * b[k * size + j];
					}
					c[i * size + j] = temp;
				}
			}

			stopwatch.Stop();

			return (float)stopwatch.ElapsedMilliseconds / 1000.0f;
		}


		public float tapeCircuitMultiplicationTime()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();

			Parallel.For(0, size, new ParallelOptions { MaxDegreeOfParallelism = worldSize }, i =>
			{
				for (int j = 0; j < size; j++)
				{
					float temp = 0;
					for (int k = 0; k < size; k++)
					{
						temp += a[i * size + k] * b[k * size + j];
					}
					c[i * size + j] = temp;
				}
			});

			stopwatch.Stop();

			return (float)stopwatch.ElapsedMilliseconds / 1000.0f;
		}
	}



	class Program
	{
		static void Main(string[] args)
		{
			MatrixMultiplication matrixMultiplication = new MatrixMultiplication();

			//Console.WriteLine(matrixMultiplication.consistentMultiplicationTime());
			Console.WriteLine(matrixMultiplication.tapeCircuitMultiplicationTime());
		}
	}
}
