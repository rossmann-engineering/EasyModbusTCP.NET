using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace EasyModbusServerSimulator
{
    public class Settings
    {
        public enum ModbusType { ModbusTCP, ModbusUDP, ModbusRTU };
        private int port = 502;
        [DescriptionAttribute("Listenig Port for Modbus-TCP or Modbus-UDP Server")]
        [CategoryAttribute("ModbusProperties")]
        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                port = value;
            }
        }

        private ModbusType modbusType;
        [DescriptionAttribute("Activate Modbus UDP; Disable Modbus TCP")]
        [CategoryAttribute("ModbusProperties")]
        public ModbusType ModbusTypeSelection
        {
            get
            {
                return modbusType;
            }
            set
            {
                modbusType = value;
            }
        }

        private string comPort;
        [DescriptionAttribute("ComPort Used for Modbus RTU connection ")]
        [CategoryAttribute("Modbus RTU Properties")]
        public string ComPort
        {
            get
            {
                return comPort;
            }
            set
            {
                comPort = value;
            }
        }

        private byte slaveAddress;
        [DescriptionAttribute("UnitIdentifier (Slave address) for Modbus RTU connection")]
        [CategoryAttribute("Modbus RTU Properties")]
        public byte SlaveAddress
        {
            get
            {
                return slaveAddress;
            }
            set
            {
                slaveAddress = value;
            }
        }

    }
}
