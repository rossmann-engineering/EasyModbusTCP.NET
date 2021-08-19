using EasyModbusSecure;
using System;

namespace EasySecureModbus_Demo_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Program application = new Program();
            application.startServer(args[0]); // For now we pass only the password, later can be cert path as well
        }

        public void startServer(string certPass)
        {
            //convert that to use CLI argument
            ModbusSecureServer modbusServer = new ModbusSecureServer("..\\..\\certs\\server.pfx", certPass, true);
            //ModbusSecureServer modbusServer = new ModbusSecureServer("..\\..\\certs\\server.pfx", certPass, false);
            modbusServer.LogFileFilename = "..\\..\\logs\\ServerLogs.txt";
            modbusServer.Listen();

            modbusServer.HoldingRegistersChanged += new ModbusSecureServer.HoldingRegistersChangedHandler(holdingRegistersChanged);
            modbusServer.CoilsChanged += new ModbusSecureServer.CoilsChangedHandler(coilsChanged);
            Console.ReadKey();
           
            modbusServer.StopListening();
        }

        public static void holdingRegistersChanged(int startingAddress, int quantity)
        {
            Console.WriteLine(startingAddress);
            Console.WriteLine(quantity);
        }

        public static void coilsChanged(int startingAddress, int quantity)
        {
            Console.WriteLine(startingAddress);
            Console.WriteLine(quantity);
        }
    }
}
