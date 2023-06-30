using EasyModbus;
using System;

namespace ConsoleAppStd
{
    class Program
    {

        static void Main(string[] args)
        {
            //EasyModbus.ModbusClient modbusClient = new EasyModbus.ModbusClient("COM3");
            //modbusClient.UnitIdentifier = 254;
            //modbusClient.Connect();
            EasyModbus.ModbusClient modbusClient = new EasyModbus.ModbusClient();
            modbusClient.Connect("127.0.0.1", 502);
            ushort startingAddress = 0;
            int maxQuantity = 10;
            // DOTest(startingAddress, maxQuantity, modbusClient);
            // DITest(startingAddress,maxQuantity,modbusClient);

            //AOTest(startingAddress,maxQuantity,modbusClient);
            //AIReadTest(startingAddress, maxQuantity, modbusClient);
            AIWriteTest(startingAddress, maxQuantity, modbusClient);

            modbusClient.Disconnect();
            Console.ReadKey();
        }

        private static void AIWriteTest(ushort startingAddress, int _, ModbusClient modbusClient)
        {
            modbusClient.WriteMultipleRegisters(startingAddress, new byte[] { 0, 99, 0, 77, 0, 55, 0, 33, 0, 11 });
            modbusClient.WriteMultipleRegisters(startingAddress, new short[] { 1010, 99, 88, 77, 66, 55, 44, 33, 22, 11 });
            modbusClient.WriteMultipleRegisters(startingAddress, new ushort[] { 11, 22, 33, 44, 55 });
            modbusClient.WriteMultipleRegisters(startingAddress, new int[] { 11, 22, 33, 44, 55 });
            modbusClient.WriteMultipleRegisters(startingAddress, new long[] { 11, 22 });

            modbusClient.WriteSingleRegister(startingAddress, (byte)11);
            modbusClient.WriteSingleRegister(startingAddress, (short)22);
            var ret = modbusClient.ReadWriteMultipleRegisters(startingAddress, 2, 5, new int[] { 32, 34 });
        }

        private static void AIReadTest(ushort startingAddress, int maxQuantity, ModbusClient modbusClient)
        {
            {
                Console.WriteLine($"Start AIReadTest Byte");
                var response = modbusClient.ReadInputRegisters<byte>(startingAddress, maxQuantity * 2);
                for (int i = 0; i < response.Length; i++)
                {
                    Console.WriteLine($"Byte Value of InputRegisters #{i + 1:00}: {response[i]}");
                }
                Console.WriteLine($"--------------------------------------");
            }

            {
                Console.WriteLine($"Start AIReadTest Int16");
                var response = modbusClient.ReadInputRegisters<short>(startingAddress, maxQuantity);
                for (int i = 0; i < response.Length; i++)
                {
                    Console.WriteLine($"Int16 Value of InputRegisters #{i + 1:00}: {response[i]}");
                }
                Console.WriteLine($"--------------------------------------");
            }

            {
                Console.WriteLine($"Start AIReadTest Int32");
                var response = modbusClient.ReadInputRegisters<int>(startingAddress, maxQuantity / 2);
                for (int i = 0; i < response.Length; i++)
                {
                    Console.WriteLine($"Int32 Value of InputRegisters #{i + 1:00}: {response[i]}");
                }
                Console.WriteLine($"--------------------------------------");
            }

            {
                Console.WriteLine($"Start AIReadTest Int64");
                var response = modbusClient.ReadInputRegisters<long>(startingAddress, maxQuantity / 4);
                for (int i = 0; i < response.Length; i++)
                {
                    Console.WriteLine($"Int64 Value of InputRegisters #{i + 1:00}: {response[i]}");
                }
                Console.WriteLine($"Finish AIReadTest");
                Console.WriteLine($"--------------------------------------");
            }
        }

        private static void AOTest(ushort startingAddress, int maxQuantity, ModbusClient modbusClient)
        {
            {
                Console.WriteLine($"Start AOTest Byte");
                var response = modbusClient.ReadHoldingRegisters<byte>(startingAddress, maxQuantity * 2);
                for (int i = 0; i < response.Length; i++)
                {
                    Console.WriteLine($"Byte Value of HoldingRegisters #{i + 1:00}: {response[i]}");
                }
                Console.WriteLine($"--------------------------------------");
            }

            {
                Console.WriteLine($"Start AOTest Int16");
                var response = modbusClient.ReadHoldingRegisters<short>(startingAddress, maxQuantity);
                for (int i = 0; i < response.Length; i++)
                {
                    Console.WriteLine($"Int16 Value of HoldingRegisters #{i + 1:00}: {response[i]}");
                }
                Console.WriteLine($"--------------------------------------");
            }

            {
                Console.WriteLine($"Start AOTest Int32");
                var response = modbusClient.ReadHoldingRegisters<int>(startingAddress, maxQuantity / 2);
                for (int i = 0; i < response.Length; i++)
                {
                    Console.WriteLine($"Int32 Value of HoldingRegisters #{i + 1:00}: {response[i]}");
                }
                Console.WriteLine($"--------------------------------------");
            }

            {
                Console.WriteLine($"Start AOTest Int64");
                var response = modbusClient.ReadHoldingRegisters<long>(startingAddress, maxQuantity / 4);
                for (int i = 0; i < response.Length; i++)
                {
                    Console.WriteLine($"Int64 Value of HoldingRegisters #{i + 1:00}: {response[i]}");
                }
                Console.WriteLine($"Finish AOTest");
                Console.WriteLine($"--------------------------------------");
            }
        }

        private static void DITest(ushort startingAddress, int maxQuantity, ModbusClient modbusClient)
        {
            {
                Console.WriteLine($"Start DITest");
                var response = modbusClient.ReadDiscreteInputs(startingAddress, maxQuantity);

                for (int i = 0; i < response.Length; i++)
                {
                    Console.WriteLine($"Value of Discrete Input #{i + 1:00}: {response[i]}");
                }
                Console.WriteLine($"Finish DITest");
                Console.WriteLine($"--------------------------------------");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startingAddress"></param>
        /// <param name="maxQuantity">最大读取寄存器数量</param>
        /// <param name="modbusClient"></param>
        private static void DOTest(ushort startingAddress, int maxQuantity, ModbusClient modbusClient)
        {
            {
                Console.WriteLine($"Start DOTest");
                var response = modbusClient.ReadCoils(startingAddress, maxQuantity);

                for (int i = 0; i < response.Length; i++)
                {
                    Console.WriteLine($"Value of Discrete Output #{i + 1:00}: {response[i]}");
                    response[i] = !response[i];
                }

                Console.WriteLine($"--------------------------------------");
                Console.WriteLine($"取反");
                Console.WriteLine($"--------------------------------------");

                modbusClient.WriteMultipleCoils(0, response);

                response = modbusClient.ReadCoils(startingAddress, maxQuantity);

                for (int i = 0; i < response.Length; i++)
                {
                    Console.WriteLine($"Value of Discrete Output #{i + 1:00}: {response[i]}");
                }
                Console.WriteLine($"Finish DOTest");
                Console.WriteLine($"--------------------------------------");
            }
        }
    }

}
