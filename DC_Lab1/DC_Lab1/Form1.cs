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

namespace DC_Lab1
{
	public partial class Form1 : Form
	{
		private delegate void UpdateTrackBarCallDelegate(bool is_increment);
		private delegate void CloseFormCallDelegate();
		private Thread thread1 = null;
		private Thread thread2 = null;
		private volatile bool is_running = true;
		private static int iteration_count = 10000000;

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			this.trackBar1.Value = 50;

			this.thread1 = new Thread(() =>
			{
				while (this.is_running)
				{
					UpdateTrackBar(false);

					int i = 0;
					while (i < iteration_count)
					{
						++i;
					}
					//Thread.Sleep(10);
				}
			}
			);

			this.thread2 = new Thread(() =>
			{
				while (this.is_running)
				{
					UpdateTrackBar(true);

					int i = 0;
					while (i < iteration_count)
					{
						++i;
					}
					//Thread.Sleep(10);
				}
			}
			);

			this.thread1.Start();
			this.thread2.Start();


			this.listBox1.SelectedIndex = 2;
			this.listBox2.SelectedIndex = 2;
		}
		private void Form1_Closing(object sender, FormClosingEventArgs e)
		{
			if (this.is_running)
			{
				this.is_running = false;
				e.Cancel = true;

				new Thread(() =>
				{
					Thread.Sleep(50);
					CloseForm();
				}).Start();
			}
		}


		private void UpdateTrackBar(bool is_increment)
		{
			if (this.trackBar1.InvokeRequired)
			{
				UpdateTrackBarCallDelegate d = new UpdateTrackBarCallDelegate(UpdateTrackBar);
				this.trackBar1.Invoke(d, new object[] { is_increment });
			}
			else
			{
				int val = this.trackBar1.Value;

				if (!is_increment && val > 10)
				{
					--this.trackBar1.Value;
				}
				else if (is_increment && val < 90)
				{
					++this.trackBar1.Value;
				}
			}
		}

		

		private void CloseForm()
		{
			if (this.InvokeRequired)
			{
				CloseFormCallDelegate d = new CloseFormCallDelegate(CloseForm);
				this.trackBar1.Invoke(d, new object[] {});
			}
			else
			{
				this.Close();
			}
		}

		private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.thread1.Priority = (ThreadPriority) Enum.Parse(typeof(ThreadPriority), this.listBox1.SelectedItem.ToString());
		}

		private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.thread2.Priority = (ThreadPriority) Enum.Parse(typeof(ThreadPriority), this.listBox2.SelectedItem.ToString());
		}
	}
}
