using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Security.Cryptography;

namespace GoogleAuth
{
	public partial class Form1 : Form
	{
		GoogleTOTP tf;
		private long lastInterval;

		public Form1()
		{
			InitializeComponent();
			tf = new GoogleTOTP();
			timer1.Start();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			pictureBox1.Image = tf.GenerateImage(pictureBox1.Width, pictureBox1.Height, txtUserEmail.Text);
			txtOutput.Text = tf.GeneratePin();
		}
  
		private void timer1_Tick(object sender, EventArgs e)
		{
			long thisInterval = tf.getCurrentInterval();
			if (lastInterval != thisInterval)
			{
				txtOutput.Text = tf.GeneratePin();
				lastInterval = thisInterval;
			}
		}
	}
}
