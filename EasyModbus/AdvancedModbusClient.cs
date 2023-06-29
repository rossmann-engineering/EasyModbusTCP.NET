using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyModbus
{
    public partial class ModbusClient
    {
        /// <summary>
        /// Read Holding Registers from Master device (FC3).
        /// 读取AO
        /// </summary>
        /// <typeparam name="T"> only byte short ushort int uint float long ulong double </typeparam>
        /// <param name="startingAddress">从第几个寄存器开始读取 一个寄存器 2 byte </param>
        /// <param name="quantity">要读取多少数据</param>
        /// <param name="registerOrder">接收到的原始数据 是高位在前还是地位在前  高低位关系理解可能有误 还未测试 如错误 取相反值即可</param>
        /// <returns></returns>
        public T[] ReadHoldingRegisters<T>(ushort startingAddress, int quantity, RegisterOrder registerOrder = RegisterOrder.HighLow) where T : struct
        {
            var quantityToRead = GetQuanityToRead<T>(quantity);

            var data = this.ReadHoldingRegistersBuffer(startingAddress, quantityToRead);

            return SetRegistersResult<T>(quantity, registerOrder, data);
        }

        /// <summary>
        /// Read Input Registers from Master device (FC4).
        /// 读取AI
        /// </summary>
        /// <typeparam name="T"> only byte short ushort int uint float long ulong double </typeparam>
        /// <param name="startingAddress">从第几个寄存器开始读取 一个寄存器 2 byte </param>
        /// <param name="quantity">要读取多少数据</param>
        /// <param name="registerOrder">接收到的原始数据 是高位在前还是地位在前  高低位关系理解可能有误 还未测试 如错误 取相反值即可</param>
        /// <returns></returns>
        public T[] ReadInputRegisters<T>(ushort startingAddress, int quantity, RegisterOrder registerOrder = RegisterOrder.HighLow) where T : struct
        {
            var quantityToRead = GetQuanityToRead<T>(quantity);

            var data = this.ReadInputRegistersBuffer(startingAddress, quantityToRead);

            return SetRegistersResult<T>(quantity, registerOrder, data);
        }

        /// <summary>
        /// Write multiple registers to Master device (FC16).
        /// 写AO
        /// </summary>
        /// <typeparam name="T"> only byte short ushort int uint float long ulong double </typeparam>
        /// <param name="startingAddress">从第几个寄存器开始读取 一个寄存器 2 byte</param>
        /// <param name="values"></param>
        /// <param name="registerOrder">接收到的原始数据 是高位在前还是地位在前  高低位关系理解可能有误 还未测试 如错误 取相反值即可</param>
        public void WriteMultipleRegisters<T>(ushort startingAddress, T[] values, RegisterOrder registerOrder = RegisterOrder.HighLow) where T : struct
        {
            if (debug)
            {
                StringBuilder debugString = new StringBuilder();
                for (int i = 0; i < values.Length; i++)
                {
                    debugString.Append(values[i]);
                    debugString.Append(" ");
                }
                StoreLogData.Instance.Store("FC16 (Write multiple Registers to Server device), StartingAddress: " + startingAddress + ", Values: " + debugString, System.DateTime.Now);
            }

            WriteMultipleRegistersBuffer(startingAddress, SetRegistersWriteData(values, registerOrder));
        }

        /// <summary>
        /// Write single Register to Master device (FC6).
        /// 写AO
        /// </summary>
        /// <typeparam name="T"> only byte short ushort </typeparam>
        /// <param name="startingAddress">从第几个寄存器开始写 一个寄存器 2 byte</param>
        /// <param name="values"></param>
        /// <param name="registerOrder">接收到的原始数据 是高位在前还是地位在前  高低位关系理解可能有误 还未测试 如错误 取相反值即可</param>
        public void WriteSingleRegister<T>(ushort startingAddress, T value, RegisterOrder registerOrder = RegisterOrder.HighLow) where T : struct
        {
            if (debug)
            {
                StoreLogData.Instance.Store("FC6 (Write single register to Master device), StartingAddress: " + startingAddress + ", Value: " + value, System.DateTime.Now);
            }

            byte[] data;
            if (typeof(T) == typeof(byte))
            {
                data = new byte[2];
                data[0] = Convert.ToByte(value);
            }
            else if (typeof(T) == typeof(short))
            {
                data = BitConverter.GetBytes(Convert.ToInt16(value));
            }
            else if (typeof(T) == typeof(ushort))
            {
                data = BitConverter.GetBytes(Convert.ToUInt16(value));
            }
            else
            {
                throw new Exception("只支持 byte short ushort ");
            }

            if (registerOrder == RegisterOrder.LowHigh)
            {
                byte temp = data[0];
                data[0] = data[1];
                data[1] = temp;
            }

            WriteSingleRegisterBuffer(startingAddress, data);
        }

        /// <summary>
        /// Read/Write Multiple Registers (FC23).
        /// 读写AO
        /// </summary>
        /// <param name="startingAddressRead">从第几个寄存器开始读取 一个寄存器 2 byte <</param>
        /// <param name="quantityRead">要读取多少数据</param>
        /// <param name="startingAddressWrite">从第几个寄存器开始写 一个寄存器 2 byte</param>
        /// <param name="values">Values to write</param>
        /// <returns>Int Array which contains the Holding registers</returns>
        public T[] ReadWriteMultipleRegisters<T>(ushort startingAddressRead, int quantityRead, ushort startingAddressWrite, T[] values
            , RegisterOrder registerOrder = RegisterOrder.HighLow) where T : struct
        {
            if (debug)
            {
                var debugString = new StringBuilder();
                for (int i = 0; i < values.Length; i++)
                {
                    debugString.Append(values[i]);
                    debugString.Append(" ");
                }
                StoreLogData.Instance.Store("FC23 (Read and Write multiple Registers to Server device), StartingAddress Read: " + startingAddressRead + ", Quantity Read: " + quantityRead + ", startingAddressWrite: " + startingAddressWrite + ", Values: " + debugString, System.DateTime.Now);
            }

            var quantityToRead = GetQuanityToRead<T>(quantityRead);

            var data = this.ReadWriteMultipleRegistersBuffer(startingAddressRead, quantityToRead, startingAddressWrite, SetRegistersWriteData(values, registerOrder));

            return SetRegistersResult<T>(quantityRead, registerOrder, data);
        }

        /// <summary>
        /// 设置AO 写数据
        /// </summary>
        /// <typeparam name="T"> only byte short ushort int uint float long ulong double </typeparam>
        /// <param name="values"></param>
        /// <param name="registerOrder"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static byte[] SetRegistersWriteData<T>(T[] values, RegisterOrder registerOrder) where T : struct
        {
            byte[] data;
            if (typeof(T) == typeof(byte))
            {
                if (values.Length % 2 == 0)
                {
                    data = values.Cast<byte>().ToArray();
                }
                else
                {
                    data = new byte[values.Length + 1];

                    Array.Copy(values, data, values.Length);
                }
            }
            else if (typeof(T) == typeof(short))
            {
                var valueRets = values.Cast<short>().ToArray();
                data = new byte[valueRets.Length * 2];

                for (int i = 0; i < valueRets.Length; i++)
                {
                    var tempBytes = BitConverter.GetBytes(valueRets[i]);

                    for (int j = 0; j < tempBytes.Length; j++)
                    {
                        if (registerOrder == RegisterOrder.HighLow)
                        {
                            data[i * tempBytes.Length + j] = tempBytes[tempBytes.Length - j - 1];
                        }
                        else
                        {
                            data[i * tempBytes.Length + j] = tempBytes[j];
                        }
                    }
                }
            }
            else if (typeof(T) == typeof(ushort))
            {
                var valueRets = values.Cast<ushort>().ToArray();
                data = new byte[valueRets.Length * 2];

                for (int i = 0; i < valueRets.Length; i++)
                {
                    var tempBytes = BitConverter.GetBytes(valueRets[i]);

                    for (int j = 0; j < tempBytes.Length; j++)
                    {
                        if (registerOrder == RegisterOrder.HighLow)
                        {
                            data[i * tempBytes.Length + j] = tempBytes[tempBytes.Length - j - 1];
                        }
                        else
                        {
                            data[i * tempBytes.Length + j] = tempBytes[j];
                        }
                    }
                }
            }
            else if (typeof(T) == typeof(int))
            {
                var valueRets = values.Cast<int>().ToArray();
                data = new byte[valueRets.Length * 4];

                for (int i = 0; i < valueRets.Length; i++)
                {
                    var tempBytes = BitConverter.GetBytes(valueRets[i]);

                    for (int j = 0; j < tempBytes.Length; j++)
                    {
                        if (registerOrder == RegisterOrder.HighLow)
                        {
                            data[i * tempBytes.Length + j] = tempBytes[tempBytes.Length - j - 1];
                        }
                        else
                        {
                            data[i * tempBytes.Length + j] = tempBytes[j];
                        }
                    }
                }
            }
            else if (typeof(T) == typeof(uint))
            {
                var valueRets = values.Cast<uint>().ToArray();
                data = new byte[valueRets.Length * 4];

                for (int i = 0; i < valueRets.Length; i++)
                {
                    var tempBytes = BitConverter.GetBytes(valueRets[i]);

                    for (int j = 0; j < tempBytes.Length; j++)
                    {
                        if (registerOrder == RegisterOrder.HighLow)
                        {
                            data[i * tempBytes.Length + j] = tempBytes[tempBytes.Length - j - 1];
                        }
                        else
                        {
                            data[i * tempBytes.Length + j] = tempBytes[j];
                        }
                    }
                }
            }
            else if (typeof(T) == typeof(float))
            {
                var valueRets = values.Cast<float>().ToArray();
                data = new byte[valueRets.Length * 4];

                for (int i = 0; i < valueRets.Length; i++)
                {
                    var tempBytes = BitConverter.GetBytes(valueRets[i]);

                    for (int j = 0; j < tempBytes.Length; j++)
                    {
                        if (registerOrder == RegisterOrder.HighLow)
                        {
                            data[i * tempBytes.Length + j] = tempBytes[tempBytes.Length - j - 1];
                        }
                        else
                        {
                            data[i * tempBytes.Length + j] = tempBytes[j];
                        }
                    }
                }
            }
            else if (typeof(T) == typeof(double))
            {
                var valueRets = values.Cast<double>().ToArray();
                data = new byte[valueRets.Length * 8];

                for (int i = 0; i < valueRets.Length; i++)
                {
                    var tempBytes = BitConverter.GetBytes(valueRets[i]);

                    for (int j = 0; j < tempBytes.Length; j++)
                    {
                        if (registerOrder == RegisterOrder.HighLow)
                        {
                            data[i * tempBytes.Length + j] = tempBytes[tempBytes.Length - j - 1];
                        }
                        else
                        {
                            data[i * tempBytes.Length + j] = tempBytes[j];
                        }
                    }
                }
            }
            else if (typeof(T) == typeof(long))
            {
                var valueRets = values.Cast<long>().ToArray();
                data = new byte[valueRets.Length * 8];

                for (int i = 0; i < valueRets.Length; i++)
                {
                    var tempBytes = BitConverter.GetBytes(valueRets[i]);

                    for (int j = 0; j < tempBytes.Length; j++)
                    {
                        if (registerOrder == RegisterOrder.HighLow)
                        {
                            data[i * tempBytes.Length + j] = tempBytes[tempBytes.Length - j - 1];
                        }
                        else
                        {
                            data[i * tempBytes.Length + j] = tempBytes[j];
                        }
                    }
                }
            }
            else if (typeof(T) == typeof(ulong))
            {
                var valueRets = values.Cast<ulong>().ToArray();
                data = new byte[valueRets.Length * 8];

                for (int i = 0; i < valueRets.Length; i++)
                {
                    var tempBytes = BitConverter.GetBytes(valueRets[i]);

                    for (int j = 0; j < tempBytes.Length; j++)
                    {
                        if (registerOrder == RegisterOrder.HighLow)
                        {
                            data[i * tempBytes.Length + j] = tempBytes[tempBytes.Length - j - 1];
                        }
                        else
                        {
                            data[i * tempBytes.Length + j] = tempBytes[j];
                        }
                    }
                }
            }
            else
            {
                throw new Exception("只支持 byte short ushort int uint float long ulong double ");
            }

            return data;
        }

        /// <summary>
        /// 获取实际读取几个寄存器
        /// </summary>
        /// <typeparam name="T">  only byte short ushort int uint float long ulong double  </typeparam>
        /// <param name="quantity"></param>
        /// <returns></returns>
        /// <exception cref="Exception"><typeparamref name="T"/> 数据类型不符合要求 </exception>
        private static int GetQuanityToRead<T>(int quantity) where T : struct
        {
            int quantityToRead;
            if (typeof(T) == typeof(byte))
            {
                quantityToRead = quantity / 2;
                if (quantity % 2 != 0)
                {
                    quantityToRead = quantityToRead + 1;
                }
            }
            else if (typeof(T) == typeof(short)
                || typeof(T) == typeof(ushort))
            {
                quantityToRead = quantity;
            }
            else if (typeof(T) == typeof(int)
                || typeof(T) == typeof(uint)
                || typeof(T) == typeof(float))
            {
                quantityToRead = quantity * 2;
            }
            else if (typeof(T) == typeof(double)
                || typeof(T) == typeof(long)
                || typeof(T) == typeof(ulong))
            {
                quantityToRead = quantity * 4;
            }
            else
            {
                throw new Exception("只支持 byte short ushort int uint float long ulong double ");
            }

            return quantityToRead;
        }

        /// <summary>
        /// 设置AI/AO实际返回值
        /// </summary>
        /// <typeparam name="T"> only byte short ushort int uint float long ulong double </typeparam>
        /// <param name="quantity"></param>
        /// <param name="registerOrder"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static T[] SetRegistersResult<T>(int quantity, RegisterOrder registerOrder, byte[] data) where T : struct
        {
            T[] result = new T[quantity];
            if (typeof(T) == typeof(byte))
            {
                Array.Copy(data.Cast<T>().ToArray(), result, result.Length);
            }
            else if (typeof(T) == typeof(short))
            {
                var ret = new short[quantity];
                byte[] temp = new byte[2];

                for (int i = 0; i < quantity; i++)
                {
                    if (registerOrder == RegisterOrder.HighLow)
                    {
                        for (int j = 0; j < temp.Length; j++)
                        {
                            temp[j] = data[i * temp.Length + j];
                        }

                        for (int j = 0; j < temp.Length; j++)
                        {
                            data[i * temp.Length + j] = temp[temp.Length - j - 1];
                        }
                    }

                    ret[i] = BitConverter.ToInt16(data, i * temp.Length);
                }
                Array.Copy(ret.Cast<T>().ToArray(), result, result.Length);
            }
            else if (typeof(T) == typeof(ushort))
            {
                var ret = new ushort[quantity];
                byte[] temp = new byte[2];

                for (int i = 0; i < quantity; i++)
                {
                    if (registerOrder == RegisterOrder.HighLow)
                    {
                        for (int j = 0; j < temp.Length; j++)
                        {
                            temp[j] = data[i * temp.Length + j];
                        }

                        for (int j = 0; j < temp.Length; j++)
                        {
                            data[i * temp.Length + j] = temp[temp.Length - j - 1];
                        }
                    }

                    ret[i] = BitConverter.ToUInt16(data, i * temp.Length);
                }
                Array.Copy(ret.Cast<T>().ToArray(), result, result.Length);
            }
            else if (typeof(T) == typeof(int))
            {
                var ret = new int[quantity];
                byte[] temp = new byte[4];

                for (int i = 0; i < quantity; i++)
                {
                    if (registerOrder == RegisterOrder.HighLow)
                    {
                        for (int j = 0; j < temp.Length; j++)
                        {
                            temp[j] = data[i * temp.Length + j];
                        }

                        for (int j = 0; j < temp.Length; j++)
                        {
                            data[i * temp.Length + j] = temp[temp.Length - j - 1];
                        }
                    }

                    ret[i] = BitConverter.ToInt32(data, i * temp.Length);
                }
                Array.Copy(ret.Cast<T>().ToArray(), result, result.Length);
            }
            else if (typeof(T) == typeof(uint))
            {
                var ret = new uint[quantity];
                byte[] temp = new byte[4];

                for (int i = 0; i < quantity; i++)
                {
                    if (registerOrder == RegisterOrder.HighLow)
                    {
                        for (int j = 0; j < temp.Length; j++)
                        {
                            temp[j] = data[i * temp.Length + j];
                        }

                        for (int j = 0; j < temp.Length; j++)
                        {
                            data[i * temp.Length + j] = temp[temp.Length - j - 1];
                        }
                    }

                    ret[i] = BitConverter.ToUInt32(data, i * temp.Length);
                }
                Array.Copy(ret.Cast<T>().ToArray(), result, result.Length);
            }
            else if (typeof(T) == typeof(float))
            {
                var ret = new float[quantity];
                byte[] temp = new byte[4];

                for (int i = 0; i < quantity; i++)
                {
                    if (registerOrder == RegisterOrder.HighLow)
                    {
                        for (int j = 0; j < temp.Length; j++)
                        {
                            temp[j] = data[i * temp.Length + j];
                        }

                        for (int j = 0; j < temp.Length; j++)
                        {
                            data[i * temp.Length + j] = temp[temp.Length - j - 1];
                        }
                    }

                    ret[i] = BitConverter.ToSingle(data, i * temp.Length);
                }
                Array.Copy(ret.Cast<T>().ToArray(), result, result.Length);
            }
            else if (typeof(T) == typeof(double))
            {
                var ret = new double[quantity];
                byte[] temp = new byte[8];

                for (int i = 0; i < quantity; i++)
                {
                    if (registerOrder == RegisterOrder.HighLow)
                    {
                        for (int j = 0; j < temp.Length; j++)
                        {
                            temp[j] = data[i * temp.Length + j];
                        }

                        for (int j = 0; j < temp.Length; j++)
                        {
                            data[i * temp.Length + j] = temp[temp.Length - j - 1];
                        }
                    }

                    ret[i] = BitConverter.ToDouble(data, i * temp.Length);
                }
                Array.Copy(ret.Cast<T>().ToArray(), result, result.Length);
            }
            else if (typeof(T) == typeof(long))
            {
                var ret = new long[quantity];
                byte[] temp = new byte[8];

                for (int i = 0; i < quantity; i++)
                {
                    if (registerOrder == RegisterOrder.HighLow)
                    {
                        for (int j = 0; j < temp.Length; j++)
                        {
                            temp[j] = data[i * temp.Length + j];
                        }

                        for (int j = 0; j < temp.Length; j++)
                        {
                            data[i * temp.Length + j] = temp[temp.Length - j - 1];
                        }
                    }

                    ret[i] = BitConverter.ToInt64(data, i * temp.Length);
                }
                Array.Copy(ret.Cast<T>().ToArray(), result, result.Length);
            }
            else if (typeof(T) == typeof(ulong))
            {
                var ret = new ulong[quantity];
                byte[] temp = new byte[8];

                for (int i = 0; i < quantity; i++)
                {
                    if (registerOrder == RegisterOrder.HighLow)
                    {
                        for (int j = 0; j < temp.Length; j++)
                        {
                            temp[j] = data[i * temp.Length + j];
                        }

                        for (int j = 0; j < temp.Length; j++)
                        {
                            data[i * temp.Length + j] = temp[temp.Length - j - 1];
                        }
                    }

                    ret[i] = BitConverter.ToUInt64(data, i * temp.Length);
                }
                Array.Copy(ret.Cast<T>().ToArray(), result, result.Length);
            }
            return result;
        }
    }

}
