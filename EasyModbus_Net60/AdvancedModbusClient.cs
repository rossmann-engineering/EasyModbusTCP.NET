using System;
using System.Collections.Generic;
using System.Text;

namespace EasyModbus
{
    public partial class ModbusClient
    {
        /*
        public enum DataType { Short = 0, UShort = 1, Long = 2, ULong = 3, Float = 4, Double = 5 };
        public object[] ReadHoldingRegisters(int startingAddress, int quantity, DataType dataType, RegisterOrder registerOrder)
        {
            int quantityToRead = quantity;
            if (dataType == DataType.Long | dataType == DataType.ULong | dataType == DataType.Float)
                quantityToRead = quantity * 2;
            if (dataType == DataType.Float)
                quantityToRead = quantity * 4;
            int[] response = this.ReadHoldingRegisters(startingAddress, quantityToRead);
            switch (dataType)
            {
                case DataType.Short: return  response.Cast<object>().ToArray();
                    break;
                default: return response.Cast<object>().ToArray();
                    break;

            }
               
            
        }
        */
    }
}
