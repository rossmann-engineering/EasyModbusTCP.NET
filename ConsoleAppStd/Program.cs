using System;

namespace ConsoleAppStd
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            EasyModbus.ModbusClient modbusClient = new EasyModbus.ModbusClient();
            modbusClient.Connect("127.0.0.1", 502);
            bool[] response = modbusClient.ReadDiscreteInputs(0, 2);

            modbusClient.Disconnect();
            Console.WriteLine("Value of Discrete Input #1: " + response[0].ToString());
            Console.WriteLine("Value of Discrete Input #2: " + response[1].ToString());
            Console.ReadKey();
        }
    }
}
