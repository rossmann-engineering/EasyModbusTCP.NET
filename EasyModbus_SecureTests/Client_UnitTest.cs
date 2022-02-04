using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyModbusSecure;
using EasyModbusSecure.Exceptions;
using Moq;
using System.Net.Sockets;
using System.Reflection;

namespace EasyModbus_SecureTests
{

    //public interface IClientAdapter
    //{
    //    //bool[] ReadCoils(int startingAddress, int quantity);

    //    //void Connect();
    //}

    //public class ClientAdapter : IClientAdapter
    //{
    //    //private static TcpClient tcpClient;
    //    //private static bool connected;

    //    readonly ModbusSecureClient _ModbusSecureClient;

    //    public ClientAdapter() => _ModbusSecureClient = new ModbusSecureClient();

    //    //bool[] IClientAdapter.ReadCoils() =>
    //    //    _ModbusSecureClient.ReadCoils(startingAddress, quantity);

    //    //void IClientAdapter.Connect()
    //    //{
    //    //    tcpClient = new TcpClient();
    //    //    connected = true;
    //    //}
    //}

    [TestClass]
    public class Client_UnitTest
    {
        //private static TcpClient tcpClient;
        //private static bool connected;

        [TestMethod]
        [ExpectedException(typeof(ConnectionException),"No parameteres where given to the client constructor")]
        public void ReadCoils_Throws_ConnectionException()
        {
            ModbusSecureClient modbusClient = new ModbusSecureClient();
            bool[] readCoils = modbusClient.ReadCoils(9, 2);
            
            //Assert.AreNotEqual(readCoils, new bool[] { });

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "startingAddress is out of range")]
        public void ReadCoils_Throws_ArgumentException_startingAddress()
        {            
            //Mock<IClientAdapter> modbusClient = new Mock<IClientAdapter>();
            ModbusSecureClient modbusClient = new ModbusSecureClient("127.0.0.1", 802);

            // Very hacky way to change the value of "tcpClient" with the use of reflection
            // TODO: At the refactoring phase fix that issue for the accessibility
            Type type = typeof(ModbusSecureClient);
            FieldInfo tcpClient = type.GetField("tcpClient",
                BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            tcpClient.SetValue(modbusClient, new TcpClient());          

            //modbusClient.Connect();
            modbusClient.ReadCoils(65536, 2001);       

        }

        private class TestableModbusSecureClient : ModbusSecureClient
        {
            public TestableModbusSecureClient(string ipAddress, int port) : base(ipAddress, port)
            {
            }

            //public override void Connect()
            //{
            //    tcpClient = new TcpClient();
            //    connected = true;
            //}
        }

    }
}
