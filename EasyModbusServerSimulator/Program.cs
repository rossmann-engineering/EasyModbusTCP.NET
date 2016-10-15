/*
 * Created by SharpDevelop.
 * User: www.rossmann-engineering.de 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
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
