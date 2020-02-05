using System;
using System.IO;
using System.Threading;

namespace Lab_4_a
{
	class WritersAndReaders
	{
		private string[] fullNames = { "person_1", "person_2", "person_3", "person_4", "person_5", "person_6", "person_7", "person_8", };
		private string[] phones = { "phone_1", "phone_2", "phone_3", "phone_4", "phone_5", "phone_6", "phone_7", "phone_8", };
		private int size;

		private ReaderWriterLock rwl = new ReaderWriterLock();

		private Random random = new Random();

		private int iterationCount = 10;

		private string fileName = "data.dat";
		private int timeOut = -1;

		public WritersAndReaders()
		{
			size = fullNames.Length;

			using (FileStream fs = File.Create(fileName))
			{
			}

			string[] lines = { "person_1 - phone_1" };
			File.WriteAllLines(fileName, lines);

		}

		public void StartPhoneByFullName()
		{
			new Thread(() =>
			{
				for (int i = 0; i < iterationCount; ++i)
				{
					rwl.AcquireReaderLock(timeOut);

					string[] lines = File.ReadAllLines(fileName);

					rwl.ReleaseReaderLock();

					string fullName = fullNames[random.Next(size)];
					string phone = "undefine";

					foreach (string current in lines)
					{
						int position = current.IndexOf(" - ");
						string currentFullName = current.Substring(0, position);
						string currentPhone = current.Substring(position + 3, current.Length - position - 3);

						if (currentFullName.Equals(fullName))
						{
							phone = currentPhone;
						}
					}

					Console.WriteLine("Result for " + fullName + " is " + phone);


					Thread.Sleep(100);
				}
			}).Start();
		}
		public void StartFullNameByPhone()
		{
			new Thread(() =>
			{
				for (int i = 0; i < iterationCount; ++i)
				{
					rwl.AcquireReaderLock(timeOut);

					string[] lines = File.ReadAllLines(fileName);

					rwl.ReleaseReaderLock();

					string phone = phones[random.Next(size)];
					string fullName = "undefine";

					foreach (string current in lines)
					{
						int position = current.IndexOf(" - ");
						string currentFullName = current.Substring(0, position);
						string currentPhone = current.Substring(position + 3, current.Length - position - 3);

						if (currentPhone.Equals(phone))
						{
							fullName = currentFullName;
						}
					}

					Console.WriteLine("Result for " + phone + " is " + fullName);


					Thread.Sleep(100);
				}
			}).Start();
		}

		public void UpdateFile()
		{
			new Thread(() =>
			{
				for (int i = 0; i < iterationCount; ++i)
				{
					rwl.AcquireWriterLock(timeOut);

					string[] lines = File.ReadAllLines(fileName);

					if (random.Next(1) == 0)
					{
						int index = random.Next(size);

						string newLine = fullNames[index] + " - " + phones[index];

						Array.Resize<string>(ref lines, lines.Length + 1);

						lines[^1] = newLine;
					}
					else
					{
						if (lines.Length > 0)
						{
							Array.Resize<string>(ref lines, lines.Length - 1);
						}
					}

					File.WriteAllLines(fileName, lines);

					rwl.ReleaseWriterLock();

					Thread.Sleep(100);
				}
			}).Start();
		}
	}


	class Program
	{
		static void Main(string[] args)
		{
			WritersAndReaders writersAndReaders = new WritersAndReaders();


			writersAndReaders.StartPhoneByFullName();
			writersAndReaders.StartFullNameByPhone();
			writersAndReaders.UpdateFile();

			Thread.Sleep(1000);
		}
	}
}
