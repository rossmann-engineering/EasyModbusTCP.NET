using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using System.Net;
using System.IO.Ports;
using System.Reflection;
namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            //    int[] registers = EasyModbus.ModbusClient.ConvertStringToRegisters("hello");
            // SerialPort serialport = new SerialPort("COM3");
            /*          serialport.PortName = "COM3";
                      serialport.BaudRate = 9600;
                      serialport.Parity = Parity.None;
                      serialport.StopBits = StopBits.One;
                      byte[] buffer = new byte[50];
                      serialport.Open();
                      byte[] bufferout = new byte[50];
                      int numberOfBytesRead = 0;
                      do
                      {
                          int quantity = serialport.Read(buffer, 0, 15);
                          Buffer.BlockCopy(buffer, 0, bufferout, numberOfBytesRead, quantity);
                          numberOfBytesRead = numberOfBytesRead + quantity;
                      }
                      while (numberOfBytesRead < 5);
                      for (int i = 0; i < 15; i++)
                      Console.WriteLine(bufferout[i].ToString());
                      serialport.Write("ddddddddd");*/
            //EasyModbus.ModbusClient modbusClient = new EasyModbus.ModbusClient("COM4");
            EasyModbus.ModbusClient modbusClient = new EasyModbus.ModbusClient("127.0.0.1", 502);
            modbusClient.ConnectionTimeout = 5000;
            modbusClient.LogFileFilename = "logData.txt";
            modbusClient.Connect();
            while (true)
            {
                

                //      Console.WriteLine("Execute FC5");
                //      modbusClient.WriteSingleCoil(0, true);
                //      Console.WriteLine("Execute FC6");
                //      modbusClient.WriteSingleRegister(0, 1234);
                //      Console.WriteLine("Execute FC15");
                //      modbusClient.WriteMultipleCoils(0, new bool[] { true, false, true, false, true, false, true });
                //Console.WriteLine("Execute FC16");
                //modbusClient.WriteMultipleRegisters(0, EasyModbus.ModbusClient.ConvertStringToRegisters("hallo2"));
                //modbusClient.Disconnect();
                //System.Threading.Thread.Sleep(100);
                //modbusClient.Connect();

                //Console.WriteLine("Execute FC3");
                //Console.WriteLine("Value of Holding Register 1000: " + modbusClient.ReadHoldingRegisters(1000, 1)[0]);

                Console.WriteLine("Read and Publish Input Registers");
                modbusClient.ReadInputRegisters(0, 10, "www.mqtt-dashboard.com");
                modbusClient.ReadHoldingRegisters(0, 10, "www.mqtt-dashboard.com");

                
                System.Threading.Thread.Sleep(1000);
            }
            modbusClient.Disconnect();
            Console.ReadKey();
        }
    }
}
