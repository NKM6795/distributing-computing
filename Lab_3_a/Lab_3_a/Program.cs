using System;
using System.Threading;

namespace Lab_3_a
{
	class Semaphore
	{
		object lockObject = new object();
		bool isLock = false;
		
		public Semaphore()
		{

		}

		public bool TryLock()
		{
			lock(lockObject)
			{
				if (isLock)
				{
					return false;
				}
				else
				{
					isLock = true;
					return true;
				}
			}
		}

		public void Lock()
		{
			while (!TryLock())
			{
				Thread.Yield();
			}
		}

		public void UnLock()
		{
			lock (lockObject)
			{
				isLock = false;
			}
		}
	}

	class Hive
	{
		private Semaphore accessToHoney = new Semaphore();
		private Semaphore pingToBear = new Semaphore();

		private int iterationCount = 10;
		private int currentIteration = 0;
		private int maxSize = 100;
		private int currentSize = 0;

		private Thread bear = null;
		private Thread[] bees = null;
		private int beesCount = 10;

		private volatile bool bearIsRunning = false;
		private volatile bool beesIsRunning = false;

		public Hive()
		{
			pingToBear.Lock();
		}

		~Hive()
		{
			bearIsRunning = false;
			beesIsRunning = false;
		}

		public void StartBear()
		{
			bearIsRunning = true;	

			bear = new Thread(() =>
			{
				while (bearIsRunning)
				{
					pingToBear.Lock();

					accessToHoney.Lock();

					currentSize = 0;
					Console.WriteLine("Cup is empty");

					++currentIteration;

					if (currentIteration == iterationCount)
					{
						bearIsRunning = false;
						beesIsRunning = false;
					}

					accessToHoney.UnLock();
				}
			});
			bear.Start();
		}

		public void StartBees()
		{
			beesIsRunning = true;

			bees = new Thread[beesCount];
			for (int i = 0; i < beesCount; ++i)
			{
				bees[i] = new Thread(() =>
				{
					while (beesIsRunning)
					{
						accessToHoney.Lock();

						if (currentSize < maxSize)
						{
							++currentSize;

							if (currentSize == maxSize)
							{
								pingToBear.UnLock();
							}
						}

						accessToHoney.UnLock();
					}
				});
				bees[i].Start();
			}
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			Hive hive = new Hive();

			hive.StartBear();
			hive.StartBees();

			Thread.Sleep(100);
		}
	}
}
