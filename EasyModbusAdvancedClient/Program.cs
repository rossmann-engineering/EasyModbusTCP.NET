/*
 * Attribution-NonCommercial-NoDerivatives 4.0 International (CC BY-NC-ND 4.0)
 * User: srossmann
 * Date: 25.11.2015
 * Time: 06:50
 */
using System;
using System.Windows.Forms;

namespace EasyModbusAdvancedClient
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
