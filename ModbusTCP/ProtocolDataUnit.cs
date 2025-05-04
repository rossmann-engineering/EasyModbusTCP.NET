using System;

namespace ModbusTCP;

/// <summary>
/// Protocol Data Unit (PDU) - As Specified in the Modbus Appliction Protocol specification V1.1b3 Page 3
/// The PDU consists of:
/// Function code - 1 byte
/// Data - x bytes
/// The "Data" section is described in the Documentation of the Function code and will be created according to the given Function code
/// </summary>
class ProtocolDataUnit {

    public byte FunctionCode { get; set; }
    private byte[] data;
    public ushort StartingAddressRead { get; set; }
    public ushort StartingAddressWrite { get; set; }
    public ushort QuantityRead { get; set; }
    public ushort QuantityWrite { get; set; }
    public byte ByteCount { get; }
    public byte ErroCode { get; }
    public byte ExceptionCode { get; }
    public int[] RegisterDataInt { get; set; }

    public bool[] RegisterDataBool { get; set; }

    public byte[] Data {
        //return the data in case of a request
        get {
            byte[] returnvalue = null;
            switch (FunctionCode) {

                // FC01 (0x01) "Read Coils" Page 11
                case 1:
                    returnvalue =                       [
                        BitConverter.GetBytes(StartingAddressRead)[1],
                        BitConverter.GetBytes(StartingAddressRead)[0],
                        BitConverter.GetBytes(QuantityRead)[1],
                        BitConverter.GetBytes(QuantityRead)[0],
                    ];
                    break;
                // FC02 (0x02) "Read Discrete Inputs" Page 12
                case 2:
                    returnvalue = [
                        BitConverter.GetBytes(StartingAddressRead)[1],
                        BitConverter.GetBytes(StartingAddressRead)[0],
                        BitConverter.GetBytes(QuantityRead)[1],
                        BitConverter.GetBytes(QuantityRead)[0],
                    ];
                    break;
            }
            return returnvalue;
        }
        //set the data in case of a response
        set {
            switch (FunctionCode) {
                // FC 01: Read Coils and 02 Read Discrete Inputs provide the same response
                case 1:
                case 2:
                    RegisterDataBool = new bool[QuantityRead];
                    for (int i = 0; i < QuantityRead; i++) {
                        int intData = data[i / 8];
                        int mask = System.Convert.ToInt32(Math.Pow(2, i % 8));
                        RegisterDataBool[i] = System.Convert.ToBoolean((intData & mask) / mask);
                    }
                    break;
            }
        }


    }

}
