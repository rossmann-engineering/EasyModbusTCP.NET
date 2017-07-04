using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyModbus;

namespace Mqtt
{
    class Program
    {
        static void Main(string[] args)
        {
            EasyModbus2Mqtt easyModbus2Mqtt = new EasyModbus2Mqtt();
            easyModbus2Mqtt.MqttBrokerAddress = "www.mqtt-dashboard.com";
            easyModbus2Mqtt.IPAddress = "127.0.0.1";
            easyModbus2Mqtt.AddReadOrder(FunctionCode.ReadCoils, 10, 0, 500);
            easyModbus2Mqtt.AddReadOrder(FunctionCode.ReadHoldingRegisters, 10, 0, 500);
            easyModbus2Mqtt.start();


        }
    }
}
