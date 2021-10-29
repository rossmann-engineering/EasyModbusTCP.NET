using EasyModbusSecure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasuySecureModbus_Demo
{
    class Program
    {
        static void Main(string[] args) // For now we pass only the password, later can be cert path as well
        {
            ModbusSecureClient modbusClient = new ModbusSecureClient("127.0.0.1", 802, "..\\..\\certs2\\client.pfx", args[0], true);    //Ip-Address and Port of Modbus-TCP-Server
            //ModbusSecureClient modbusClient = new ModbusSecureClient("127.0.0.1", 802);    //Ip-Address and Port of Modbus-TCP-Server
            //ModbusSecureClient modbusClient = new ModbusSecureClient("127.0.0.1", 802, null, args[0], true);    //Ip-Address and Port of Modbus-TCP-Server
            //ModbusSecureClient modbusClient = new ModbusSecureClient("127.0.0.1", 802, "", args[0], true);    //Ip-Address and Port of Modbus-TCP-Server
            //ModbusSecureClient modbusClient = new ModbusSecureClient("127.0.0.1", 802, "..\\..\\certs\\something.pfx", args[0], true);    //Ip-Address and Port of Modbus-TCP-Server
            
            modbusClient.LogFileFilename = "..\\..\\logs\\ClientLogs.txt";
            modbusClient.Connect();                                                    //Connect to Server

            if(modbusClient.Connected == false)
            {
                Console.WriteLine("Connection could not be established ");
                Console.WriteLine("Press any key to continue . . . ");
                Console.ReadKey(true);
                return;
            }

            //modbusClient.WriteMultipleCoils(4, new bool[] { true, true, true, true, true, true, true, true, true, true });    //Write Coils starting with Address 5
            //modbusClient.WriteSingleRegister(0, 5);
            bool[] readCoils = modbusClient.ReadCoils(9, 2);                        //Read 10 Coils from Server, starting with address 10
            //int[] readHoldingRegisters = modbusClient.ReadHoldingRegisters(0, 10);    //Read 2 Holding Registers from Server, starting with Address 1

            // Console Output
            for (int i = 0; i < readCoils.Length; i++)
                Console.WriteLine("Value of Coil " + (9 + i + 1) + " " + readCoils[i].ToString());


            modbusClient.WriteMultipleCoils(4, new bool[] { true, false, true, true, true, false, true, true, true, true });    //Write Coils starting with Address 1

            readCoils = modbusClient.ReadCoils(0, 2);
            // Console Output 2
            for (int i = 0; i < readCoils.Length; i++)
                Console.WriteLine("Value of Coil " + (9 + i + 1) + " " + readCoils[i].ToString());

            //for (int i = 0; i < readHoldingRegisters.Length; i++)
            //    Console.WriteLine("Value of HoldingRegister " + (i + 1) + " " + readHoldingRegisters[i].ToString());


            //modbusClient.WriteSingleRegister(0, 7);
            //int [] readHoldingRegister = modbusClient.ReadHoldingRegisters(0, 1);
            //for (int i = 0; i < readHoldingRegister.Length; i++)
            //    Console.WriteLine("Value of HoldingRegister " + (i + 1) + " " + readHoldingRegister[i].ToString());

            modbusClient.Disconnect();                                                //Disconnect from Server


            // Second attempt to check session caching
            //modbusClient.Connect();

            //if (modbusClient.Connected == false)
            //{
            //    Console.WriteLine("Connection could not be established ");
            //    Console.WriteLine("Press any key to continue . . . ");
            //    Console.ReadKey(true);
            //    return;
            //}

            //readCoils = modbusClient.ReadCoils(0, 2);
            //// Console Output 2
            //for (int i = 0; i < readCoils.Length; i++)
            //    Console.WriteLine("Value of Coil " + (9 + i + 1) + " " + readCoils[i].ToString());

            //modbusClient.Disconnect();

            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
        }
    }
}
