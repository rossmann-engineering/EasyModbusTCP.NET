/*
 * Attribution-NonCommercial-NoDerivatives 4.0 International (CC BY-NC-ND 4.0)
 * User: srossmann
 * Date: 13.02.2015
 * Time: 11:09
 */
using System;
using System.Windows.Forms;

namespace EasyModbusClientExample
{
	/// <summary>
	/// Class with program entry point.
	/// </summary>
	internal sealed class Program
	{
		/// <summary>
		/// Program entry point.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
		
	}
}
