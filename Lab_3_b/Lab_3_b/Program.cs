using System;
using System.Threading;
using System.Collections.Generic;

namespace Lab_3_b
{
	class HairSalon
	{
		private Semaphore hairdresserSleep = new Semaphore(1, 1);
		private Semaphore hairdresserWork = new Semaphore(1, 1);
		private Semaphore visitorSleep = new Semaphore(1, 1);

		private Thread hairdresser = null;
		private volatile bool hairdresserIsRunning = false;

		public HairSalon()
		{
			hairdresserSleep.WaitOne();
			visitorSleep.WaitOne();

			hairdresserIsRunning = true;
			hairdresser = new Thread(() =>
			{
				while (hairdresserIsRunning)
				{
					hairdresserSleep.WaitOne();
					if (!hairdresserIsRunning)
					{
						return;
					}

					Thread.Sleep(100);

					Console.WriteLine("Processed the visitor");

					visitorSleep.Release();
					hairdresserWork.Release();
				}

			});
			hairdresser.Start();
		}

		~HairSalon()
		{
			EndWork();
		}


		public void AddVisitor()
		{
			Thread visitor = new Thread(() =>
			{
				hairdresserWork.WaitOne();

				hairdresserSleep.Release();

				visitorSleep.WaitOne();
			});
			visitor.Start();
		}

		public void EndWork()
		{
			hairdresserIsRunning = false;
			hairdresserSleep.Release();
		}
	}


	class Program
	{
		static void Main(string[] args)
		{
			HairSalon hairSalon = new HairSalon();

			for (int i = 0; i < 10; ++i)
			{
				hairSalon.AddVisitor();
			}
			Thread.Sleep(1000);
			hairSalon.EndWork();
		}
	}
}
