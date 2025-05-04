using System;
using System.Text;

namespace EasyModbus;

internal static class Convert {

    /// <summary>
    /// Converts 64 Bit double prec Value to four ModbusRegisters
    /// </summary>
    /// <param name="doubleValue">double value which has to be converted into four registers</param>
    /// <returns>Register values</returns>
    public static int[] ConvertDoubleToRegisters(double doubleValue) {
        byte[] doubleBytes = BitConverter.GetBytes(doubleValue);
        byte[] highRegisterBytes = [doubleBytes[6], doubleBytes[7], 0, 0];
        byte[] highLowRegisterBytes = [doubleBytes[4], doubleBytes[5], 0, 0];
        byte[] lowHighRegisterBytes = [doubleBytes[2], doubleBytes[3], 0, 0];
        byte[] lowRegisterBytes = [doubleBytes[0], doubleBytes[1], 0, 0];
        int[] returnValue =
        [BitConverter.ToInt32(lowRegisterBytes, 0), BitConverter.ToInt32(lowHighRegisterBytes, 0), BitConverter.ToInt32(highLowRegisterBytes, 0), BitConverter.ToInt32(highRegisterBytes, 0)];
        return returnValue;
    }

    /// <summary>
    /// Converts 64 Bit double prec. Value to four ModbusRegisters - Registers can be swapped
    /// </summary>
    /// <param name="doubleValue">double value which has to be converted into four registers</param>
    /// <param name="registerOrder">Desired Word Order (Low Register first or High Register first</param>
    /// <returns>Register values</returns>
    public static int[] ConvertDoubleToRegisters(double doubleValue, RegisterOrder registerOrder) {
        int[] registerValues = ConvertDoubleToRegisters(doubleValue);
        int[] returnValue = registerValues;
        if (registerOrder == RegisterOrder.HighLow)
            returnValue = [registerValues[3], registerValues[2], registerValues[1], registerValues[0]];
        return returnValue;
    }

    /// <summary>
    /// Converts float to two ModbusRegisters - Example:  modbusClient.WriteMultipleRegisters(24, EasyModbus.ModbusClient.ConvertFloatToTwoRegisters((float)1.22));
    /// </summary>
    /// <param name="floatValue">Float value which has to be converted into two registers</param>
    /// <returns>Register values</returns>
    public static int[] ConvertFloatToRegisters(float floatValue) {
        byte[] floatBytes = BitConverter.GetBytes(floatValue);
        byte[] highRegisterBytes = [floatBytes[2], floatBytes[3], 0, 0];
        byte[] lowRegisterBytes = [floatBytes[0], floatBytes[1], 0, 0];
        int[] returnValue = [BitConverter.ToInt32(lowRegisterBytes, 0), BitConverter.ToInt32(highRegisterBytes, 0)];
        return returnValue;
    }

    /// <summary>
    /// Converts float to two ModbusRegisters Registers - Registers can be swapped
    /// </summary>
    /// <param name="floatValue">Float value which has to be converted into two registers</param>
    /// <param name="registerOrder">Desired Word Order (Low Register first or High Register first</param>
    /// <returns>Register values</returns>
    public static int[] ConvertFloatToRegisters(float floatValue, RegisterOrder registerOrder) {
        int[] registerValues = ConvertFloatToRegisters(floatValue);
        int[] returnValue = registerValues;
        if (registerOrder == RegisterOrder.HighLow)
            returnValue = [registerValues[1], registerValues[0]];
        return returnValue;
    }

    /// <summary>
    /// Converts 32 Bit Value to two ModbusRegisters
    /// </summary>
    /// <param name="intValue">Int value which has to be converted into two registers</param>
    /// <returns>Register values</returns>
    public static int[] ConvertIntToRegisters(int intValue) {
        byte[] doubleBytes = BitConverter.GetBytes(intValue);
        byte[] highRegisterBytes = [doubleBytes[2], doubleBytes[3], 0, 0];
        byte[] lowRegisterBytes = [doubleBytes[0], doubleBytes[1], 0, 0];
        int[] returnValue = [BitConverter.ToInt32(lowRegisterBytes, 0), BitConverter.ToInt32(highRegisterBytes, 0)];
        return returnValue;
    }

    /// <summary>
    /// Converts 32 Bit Value to two ModbusRegisters Registers - Registers can be swapped
    /// </summary>
    /// <param name="intValue">Double value which has to be converted into two registers</param>
    /// <param name="registerOrder">Desired Word Order (Low Register first or High Register first</param>
    /// <returns>Register values</returns>
    public static int[] ConvertIntToRegisters(Int32 intValue, RegisterOrder registerOrder) {
        int[] registerValues = ConvertIntToRegisters(intValue);
        int[] returnValue = registerValues;
        if (registerOrder == RegisterOrder.HighLow)
            returnValue = [registerValues[1], registerValues[0]];
        return returnValue;
    }

    /// <summary>
    /// Converts 64 Bit Value to four ModbusRegisters
    /// </summary>
    /// <param name="longValue">long value which has to be converted into four registers</param>
    /// <returns>Register values</returns>
    public static int[] ConvertLongToRegisters(Int64 longValue) {
        byte[] longBytes = BitConverter.GetBytes(longValue);
        byte[] highRegisterBytes = [longBytes[6], longBytes[7], 0, 0];
        byte[] highLowRegisterBytes = [longBytes[4], longBytes[5], 0, 0];
        byte[] lowHighRegisterBytes = [longBytes[2], longBytes[3], 0, 0];
        byte[] lowRegisterBytes = [longBytes[0], longBytes[1], 0, 0];
        int[] returnValue = [BitConverter.ToInt32(lowRegisterBytes, 0), BitConverter.ToInt32(lowHighRegisterBytes, 0), BitConverter.ToInt32(highLowRegisterBytes, 0), BitConverter.ToInt32(highRegisterBytes, 0)];
        return returnValue;
    }

    /// <summary>
    /// Converts 64 Bit Value to four ModbusRegisters - Registers can be swapped
    /// </summary>
    /// <param name="longValue">long value which has to be converted into four registers</param>
    /// <param name="registerOrder">Desired Word Order (Low Register first or High Register first</param>
    /// <returns>Register values</returns>
    public static int[] ConvertLongToRegisters(Int64 longValue, RegisterOrder registerOrder) {
        int[] registerValues = ConvertLongToRegisters(longValue);
        int[] returnValue = registerValues;
        if (registerOrder == RegisterOrder.HighLow)
            returnValue = [registerValues[3], registerValues[2], registerValues[1], registerValues[0]];
        return returnValue;
    }

    /// <summary>
    /// Convert four 16 Bit Registers to 64 Bit double prec. value Register Order "LowHigh": Reg0: Low Word.....Reg3: High Word, "HighLow": Reg0: High Word.....Reg3: Low Word
    /// </summary>
    /// <param name="registers">four Register values received from Modbus</param>
    /// <returns>64 bit value</returns>
    public static double ConvertRegistersToDouble(int[] registers) {
        if (registers.Length != 4) {
            throw new ArgumentException("Input Array length invalid - Array langth must be '4'");
        }
        int highRegister = registers[3];
        int highLowRegister = registers[2];
        int lowHighRegister = registers[1];
        int lowRegister = registers[0];
        byte[] highRegisterBytes = BitConverter.GetBytes(highRegister);
        byte[] highLowRegisterBytes = BitConverter.GetBytes(highLowRegister);
        byte[] lowHighRegisterBytes = BitConverter.GetBytes(lowHighRegister);
        byte[] lowRegisterBytes = BitConverter.GetBytes(lowRegister);
        byte[] longBytes = [lowRegisterBytes[0], lowRegisterBytes[1], lowHighRegisterBytes[0], lowHighRegisterBytes[1], highLowRegisterBytes[0], highLowRegisterBytes[1], highRegisterBytes[0], highRegisterBytes[1]];
        return BitConverter.ToDouble(longBytes, 0);
    }

    /// <summary>
    /// Convert four 16 Bit Registers to 64 Bit double prec. value - Registers can be swapped
    /// </summary>
    /// <param name="registers">four Register values received from Modbus</param>
    /// <param name="registerOrder">Desired Word Order (Low Register first or High Register first</param>
    /// <returns>Connected double prec. float value</returns>
    public static double ConvertRegistersToDouble(int[] registers, RegisterOrder registerOrder) {
        if (registers.Length != 4)
            throw new ArgumentException("Input Array length invalid - Array langth must be '4'");
        int[] swappedRegisters = [registers[0], registers[1], registers[2], registers[3]];
        if (registerOrder == RegisterOrder.HighLow)
            swappedRegisters = [registers[3], registers[2], registers[1], registers[0]];
        return ConvertRegistersToDouble(swappedRegisters);
    }

    /// <summary>
    /// Converts two ModbusRegisters to Float - Example: EasyModbus.ModbusClient.ConvertRegistersToFloat(modbusClient.ReadHoldingRegisters(19,2))
    /// </summary>
    /// <param name="registers">Two Register values received from Modbus</param>
    /// <returns>Connected float value</returns>
    public static float ConvertRegistersToFloat(int[] registers) {
        if (registers.Length != 2)
            throw new ArgumentException("Input Array length invalid - Array langth must be '2'");
        int highRegister = registers[1];
        int lowRegister = registers[0];
        byte[] highRegisterBytes = BitConverter.GetBytes(highRegister);
        byte[] lowRegisterBytes = BitConverter.GetBytes(lowRegister);
        byte[] floatBytes = [lowRegisterBytes[0], lowRegisterBytes[1], highRegisterBytes[0], highRegisterBytes[1]];
        return BitConverter.ToSingle(floatBytes, 0);
    }


    /// <summary>
    /// Converts two ModbusRegisters to Float, Registers can by swapped
    /// </summary>
    /// <param name="registers">Two Register values received from Modbus</param>
    /// <param name="registerOrder">Desired Word Order (Low Register first or High Register first</param>
    /// <returns>Connected float value</returns>
    public static float ConvertRegistersToFloat(int[] registers, RegisterOrder registerOrder) {
        int[] swappedRegisters = [registers[0], registers[1]];
        if (registerOrder == RegisterOrder.HighLow)
            swappedRegisters = [registers[1], registers[0]];
        return ConvertRegistersToFloat(swappedRegisters);
    }

    /// <summary>
    /// Converts two ModbusRegisters to 32 Bit Integer value
    /// </summary>
    /// <param name="registers">Two Register values received from Modbus</param>
    /// <returns>Connected 32 Bit Integer value</returns>
    public static Int32 ConvertRegistersToInt(int[] registers) {
        if (registers.Length != 2)
            throw new ArgumentException("Input Array length invalid - Array langth must be '2'");
        int highRegister = registers[1];
        int lowRegister = registers[0];
        byte[] highRegisterBytes = BitConverter.GetBytes(highRegister);
        byte[] lowRegisterBytes = BitConverter.GetBytes(lowRegister);
        byte[] doubleBytes = [lowRegisterBytes[0], lowRegisterBytes[1], highRegisterBytes[0], highRegisterBytes[1]];
        return BitConverter.ToInt32(doubleBytes, 0);
    }

    /// <summary>
    /// Converts two ModbusRegisters to 32 Bit Integer Value - Registers can be swapped
    /// </summary>
    /// <param name="registers">Two Register values received from Modbus</param>
    /// <param name="registerOrder">Desired Word Order (Low Register first or High Register first</param>
    /// <returns>Connecteds 32 Bit Integer value</returns>
    public static Int32 ConvertRegistersToInt(int[] registers, RegisterOrder registerOrder) {
        int[] swappedRegisters = [registers[0], registers[1]];
        if (registerOrder == RegisterOrder.HighLow)
            swappedRegisters = [registers[1], registers[0]];
        return ConvertRegistersToInt(swappedRegisters);
    }

    /// <summary>
    /// Convert four 16 Bit Registers to 64 Bit Integer value Register Order "LowHigh": Reg0: Low Word.....Reg3: High Word, "HighLow": Reg0: High Word.....Reg3: Low Word
    /// </summary>
    /// <param name="registers">four Register values received from Modbus</param>
    /// <returns>64 bit value</returns>

    public static Int64 ConvertRegistersToLong(int[] registers) {
        if (registers.Length != 4)
            throw new ArgumentException("Input Array length invalid - Array langth must be '4'");
        int highRegister = registers[3];
        int highLowRegister = registers[2];
        int lowHighRegister = registers[1];
        int lowRegister = registers[0];
        byte[] highRegisterBytes = BitConverter.GetBytes(highRegister);
        byte[] highLowRegisterBytes = BitConverter.GetBytes(highLowRegister);
        byte[] lowHighRegisterBytes = BitConverter.GetBytes(lowHighRegister);
        byte[] lowRegisterBytes = BitConverter.GetBytes(lowRegister);
        byte[] longBytes = [lowRegisterBytes[0], lowRegisterBytes[1], lowHighRegisterBytes[0], lowHighRegisterBytes[1], highLowRegisterBytes[0], highLowRegisterBytes[1], highRegisterBytes[0], highRegisterBytes[1]];
        return BitConverter.ToInt64(longBytes, 0);
    }

    /// <summary>
    /// Convert four 16 Bit Registers to 64 Bit Integer value - Registers can be swapped
    /// </summary>
    /// <param name="registers">four Register values received from Modbus</param>
    /// <param name="registerOrder">Desired Word Order (Low Register first or High Register first</param>
    /// <returns>Connected 64 Bit Integer value</returns>
    public static Int64 ConvertRegistersToLong(int[] registers, RegisterOrder registerOrder) {
        if (registers.Length != 4)
            throw new ArgumentException("Input Array length invalid - Array langth must be '4'");
        int[] swappedRegisters = [registers[0], registers[1], registers[2], registers[3]];
        if (registerOrder == RegisterOrder.HighLow)
            swappedRegisters = [registers[3], registers[2], registers[1], registers[0]];
        return ConvertRegistersToLong(swappedRegisters);
    }

    /// <summary>
    /// Converts 16 - Bit Register values to String
    /// </summary>
    /// <param name="registers">Register array received via Modbus</param>
    /// <param name="offset">First Register containing the String to convert</param>
    /// <param name="stringLength">number of characters in String (must be even)</param>
    /// <returns>Converted String</returns>
    public static string ConvertRegistersToString(int[] registers, int offset, int stringLength) {
        byte[] result = new byte[stringLength];
        byte[] registerResult = new byte[2];

        for (int i = 0; i < stringLength / 2; i++) {
            registerResult = BitConverter.GetBytes(registers[offset + i]);
            result[i * 2] = registerResult[0];
            result[i * 2 + 1] = registerResult[1];
        }
        return Encoding.Default.GetString(result);
    }

    /// <summary>
    /// Converts a String to 16 - Bit Registers
    /// </summary>
    /// <param name="registers">Register array received via Modbus</param>
    /// <returns>Converted String</returns>
    public static int[] ConvertStringToRegisters(string stringToConvert) {
        byte[] array = Encoding.ASCII.GetBytes(stringToConvert);
        int[] returnarray = new int[stringToConvert.Length / 2 + stringToConvert.Length % 2];
        for (int i = 0; i < returnarray.Length; i++) {
            returnarray[i] = array[i * 2];
            if (i * 2 + 1 < array.Length) {
                returnarray[i] = returnarray[i] | ((int)array[i * 2 + 1] << 8);
            }
        }
        return returnarray;
    }
}