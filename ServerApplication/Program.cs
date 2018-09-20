using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication
{

    class Program
    {
        static void Main(string[] args)
        {
            Program application = new Program();
            application.startServer();

        }

        public void startServer()
        {
            EasyModbus.ModbusServer modbusServer = new EasyModbus.ModbusServer();
            modbusServer.Listen();
            modbusServer.HoldingRegistersChanged += new EasyModbus.ModbusServer.HoldingRegistersChangedHandler(holdingRegistersChanged);
            Console.ReadKey();
            
            modbusServer.StopListening();
        }

        public void holdingRegistersChanged(int startingAddress, int quantity)
        {
            Console.WriteLine(startingAddress);
            Console.WriteLine(quantity);
        }
    }
}
