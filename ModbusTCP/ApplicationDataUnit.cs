using System;
using System.Collections.Generic;

namespace ModbusTCP;

/// <summary>
/// Application Data Unit (ADU) - As Specified in the Modbus Appliction Protocol specification V1.1b3 Page 3 (or Modbus Messaging on TCP/IP Implementation Guide Page 4)
/// The ADU consists of:
/// 
/// MBAP Header (only for Modbus TCP)
/// Transaction Identifier - 2 Bytes
/// Protocol Identiifier - 2 Bytes
/// Length - 2 Bytes
/// 
/// Additional Address - 1 byte
/// PDU - x Byte
/// CRC - 2 byte (not used for Modbus TCP)
/// The "Data" section is described in the Documentation of the Function code and will be created according to the given Function code
/// </summary>
class ApplicationDataUnit : ProtocolDataUnit {

    public ushort TransactionIdentifier { get; set; }
    private ushort protocolIdentifier = 0;

    public byte UnitIdentifier { get; set; }

    /// <summary>
    /// Constructor, the Function code has to be handed over at instantiation
    /// </summary>
    /// <param name="functionCode"></param>
    public ApplicationDataUnit(byte functionCode) {
        this.FunctionCode = functionCode;
    }

    public byte[] Mbap_Header {
        get {
            ushort length = 0x0006;
            if (FunctionCode == 15) {
                byte byteCount = (byte)((RegisterDataBool.Length % 8 != 0 ? RegisterDataBool.Length / 8 + 1 : (RegisterDataBool.Length / 8)));
                length = (ushort)(7 + byteCount);
            }
            if (FunctionCode == 16)
                length = (ushort)(7 + RegisterDataInt.Length * 2);
            if (FunctionCode == 23)
                length = (ushort)(11 + RegisterDataInt.Length * 2);

            byte[] returnvalue = [
                BitConverter.GetBytes((ushort)TransactionIdentifier)[1],
                BitConverter.GetBytes((ushort)TransactionIdentifier)[0],
                BitConverter.GetBytes((ushort)protocolIdentifier)[1],
                BitConverter.GetBytes((ushort)protocolIdentifier)[0],
                BitConverter.GetBytes((ushort)length)[1],
                BitConverter.GetBytes((ushort)length)[0],
                UnitIdentifier
            ];

            return returnvalue;
        }
    }

    public byte[] Payload {
        // Return the Payload in case of a request
        get {
            List<byte> returnvalue = [.. this.Mbap_Header, FunctionCode, .. Data];
            byte[] crc = BitConverter.GetBytes(Crc.Calculate([.. returnvalue], (ushort)(returnvalue.Count - 8), 6));
            returnvalue.AddRange(crc);
            return [.. returnvalue];
        }
        // Set the Payload in case of a resonse
        set {
            TransactionIdentifier = BitConverter.ToUInt16(value, 0);
            UnitIdentifier = value[6];
        }
    }

}
