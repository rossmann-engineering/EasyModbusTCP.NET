using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            EasyModbus.ModbusClient modbusClient = new EasyModbus.ModbusClient("COM3");
            modbusClient.Connect();
            Console.WriteLine("Execute FC5");
            modbusClient.WriteSingleCoil(0, true);
            Console.WriteLine("Execute FC6");
            modbusClient.WriteSingleRegister(0, 1234);
            Console.WriteLine("Execute FC15");
            modbusClient.WriteMultipleCoils(0, new bool[] { true, false, true, false, true, false, true });
            Console.WriteLine("Execute FC16");
            modbusClient.WriteMultipleRegisters(5, new int[] { 1, 2, 3, 4, 5, 6, 6 });
            Console.WriteLine("Execute FC3");
            Console.WriteLine("Value of Holding Register 1000: " + modbusClient.ReadHoldingRegisters(1000, 1)[0]);
            Console.ReadKey();
        }
    }
}
