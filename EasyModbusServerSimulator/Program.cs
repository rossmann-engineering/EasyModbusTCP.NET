/*
 * Attribution-NonCommercial-NoDerivatives 4.0 International (CC BY-NC-ND 4.0)
 * User: www.rossmann-engineering.de 
 */
using System;
using System.Windows.Forms;
using EasyModbus;

namespace EasyModbusServerSimulator
{
	/// <summary>
	/// Class with program entry point.
	/// </summary>
	static class Program
	{
		/// <summary>
		/// Program entry point.
		/// </summary>
		[STAThread]
        static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
		
	}
}
