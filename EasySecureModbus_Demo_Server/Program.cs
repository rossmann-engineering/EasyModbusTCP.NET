using EasyModbusSecure;
using System;
using System.Collections.Generic;

namespace EasySecureModbus_Demo_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Program application = new Program();
            application.startServer(args[0], args[1]); // For now we pass only the password, later can be cert path as well
        }

        public void startServer(string certPath, string certPass)
        {
            //convert that to use CLI argument
            List<ValueTuple<string, List<byte>>> roles = new List<ValueTuple<string, List<byte>>>
            {
                //ValueTuple.Create("Maintainer", (byte)0x01),
                ValueTuple.Create("Operator", new List<byte> { (byte)0x01, (byte)0x0F, (byte)0x06} ),
                //ValueTuple.Create("Operator", new List<byte> { (byte)0x01, (byte)0x0F, (byte)0x06, (byte)0x03 } ),
                ValueTuple.Create("Engineer", new List<byte> { (byte)0x01 })
            };
            ModbusSecureServerAuthZ modbusServer = new ModbusSecureServerAuthZ(certPath, certPass, true, roles);
            //ModbusSecureServer modbusServer = new ModbusSecureServer("..\\..\\certs2\\server.pfx", certPass, true, roles);
            //ModbusSecureServer modbusServer = new ModbusSecureServer("..\\..\\certs\\server.pfx", certPass, false);

            modbusServer.FunctionCode1AuthZDisabled = false;
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
