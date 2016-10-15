/*
 * Erstellt mit SharpDevelop.
 * Benutzer: Stefan Roßmann
 * Datum: 17.06.2011
 * Zeit: 23:08
 * 
 * Main - Dient nur zur Simulation und Debuging
 */
using System;

namespace SRModbusTCP
{
	class Program
	{     
		public static void Main(string[] args)
		{
            SRModbusTCP.ModbusTCP modbusClient = new SRModbusTCP.ModbusTCP("127.0.0.1", 502);
            modbusClient.UDPFlag = false;
			Console.WriteLine("Hello World!");
            bool[] modbusData = new bool[8]
            {true,false,true,false,true,false,true,false};
            int[] modbusRegisterData = new int[10] { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };
			// TODO: Implement Functionality Here
            modbusClient.Connect();
            while (true)
            {
                System.Threading.Thread.Sleep(50);
                if (modbusClient.Connected)
                {
                    
                    modbusClient.WriteSingleCoil(0, false);
                    modbusClient.WriteSingleCoil(1, true);
                    modbusClient.WriteSingleRegister(16, 1237);
                    modbusClient.WriteSingleRegister(17, 1237);
                    modbusClient.WriteSingleRegister(18, 1237);
                    modbusClient.WriteMultipleCoils(5, modbusData);
                    modbusClient.WriteMultipleRegisters(20, modbusRegisterData);

                    //  int[] responsedata = modbusClient.ReadInputRegisters(0, 32);
                    bool[] boolresponsedata = modbusClient.ReadCoils(0, 1);
                    // for (int i = 0; i < responsedata.Length; i++)
                    // Console.WriteLine(responsedata[i].ToString());
                    for (int i = 0; i < boolresponsedata.Length; i++)
                        Console.WriteLine(boolresponsedata[i].ToString());
                }
            }
                
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
}