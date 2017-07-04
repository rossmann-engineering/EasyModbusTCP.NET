using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplicationMqqt
{
    class Program
    {
        static void Main(string[] args)
        {
            EasyModbus.EasyModbus2Mqtt easyModbus2Mqtt= new EasyModbus.EasyModbus2Mqtt();
            easyModbus2Mqtt.MqttBrokerAddress = "www.mqtt-dashboard.com";
            easyModbus2Mqtt.IPAddress = "127.0.0.1";
           easyModbus2Mqtt.AddReadOrder(EasyModbus.FunctionCode.ReadCoils, 2, 0, 200, new string[] { "easymodbusclient/customtopic1", "easymodbusclient/customtopic2" });
            easyModbus2Mqtt.AddReadOrder(EasyModbus.FunctionCode.ReadHoldingRegisters, 10, 0, 200);
            easyModbus2Mqtt.AddReadOrder(EasyModbus.FunctionCode.ReadInputRegisters, 10, 0, 200);
           easyModbus2Mqtt.AddReadOrder(EasyModbus.FunctionCode.ReadDiscreteInputs, 10, 0, 200);
            easyModbus2Mqtt.start();
        }
    }
}
