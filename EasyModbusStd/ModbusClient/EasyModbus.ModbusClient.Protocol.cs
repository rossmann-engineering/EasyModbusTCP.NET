using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyModbus
{



    /// <summary>
    /// Protocol Data Unit (PDU) - As Specified in the Modbus Appliction Protocol specification V1.1b3 Page 3
    /// The PDU consists of:
    /// Function code - 1 byte
    /// Data - x bytes
    /// The "Data" section is described in the Documentation of the Function code and will be created according to the given Function code
    /// </summary>
    class ProtocolDataUnit
    {
        public byte FunctionCode { get; set; }
        private byte[] data;
        public ushort StartingAddressRead { get; set; }
        public ushort StartingAddressWrite { get; set; }
        public ushort QuantityRead { get; set; }
        public ushort QuantityWrite { get; set; }
        public byte ByteCount { get; }
        public byte ErroCode { get; }
        public byte ExceptionCode { get; }
        public object[] RegisterData { get; set; }

        public byte[] Data
        {
            get
            {
                Byte[] returnvalue = null;
                switch (FunctionCode)
                {

                    // FC01 (0x01) "Read Coils" Page 11
                    case 1:
                        returnvalue = new byte[]
                           {
                            BitConverter.GetBytes((ushort)StartingAddressRead)[1],
                            BitConverter.GetBytes((ushort)StartingAddressRead)[0],
                            BitConverter.GetBytes((ushort)QuantityRead)[1],
                            BitConverter.GetBytes((ushort)QuantityRead)[0],
                            
                           };
                        break;
                    // FC02 (0x02) "Read Discrete Inputs" Page 12
                    case 2:
                        returnvalue = new byte[]
                           {
                            BitConverter.GetBytes((ushort)StartingAddressRead)[1],
                            BitConverter.GetBytes((ushort)StartingAddressRead)[0],
                            BitConverter.GetBytes((ushort)QuantityRead)[1],
                            BitConverter.GetBytes((ushort)QuantityRead)[0],

                           };
                        break;
                }
                return returnvalue;
            }
        }

    }

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
    class ApplicationDataUnit : ProtocolDataUnit
    {
        
        public ushort TransactionIdentifier { get; set; }
        private ushort protocolIdentifier = 0;
        
        public byte UnitIdentifier { get; set; }
        /// <summary>
        /// Constructor, the Function code has to be handed over at instantiation
        /// </summary>
        /// <param name="functionCode"></param>
        public ApplicationDataUnit(byte functionCode)
        {
            this.FunctionCode = functionCode;
        }

        public byte[] Mbap_Header
        {
            get
            {
                ushort length = 0x0006;
                if (FunctionCode == 15)
                {
                    byte byteCount = (byte)((RegisterData.Length % 8 != 0 ? RegisterData.Length / 8 + 1 : (RegisterData.Length / 8)));
                    length = (ushort)(7 + byteCount);
                }
                if (FunctionCode == 16)
                    length = (ushort)(7 + RegisterData.Length * 2);
                if (FunctionCode == 23)
                    length = (ushort)(11 + RegisterData.Length * 2);

                Byte[] returnvalue = new byte[]
                {
                    BitConverter.GetBytes((ushort)TransactionIdentifier)[1],
                    BitConverter.GetBytes((ushort)TransactionIdentifier)[0],
                    BitConverter.GetBytes((ushort)protocolIdentifier)[1],
                    BitConverter.GetBytes((ushort)protocolIdentifier)[0],
                    BitConverter.GetBytes((ushort)length)[1],
                    BitConverter.GetBytes((ushort)length)[0],
                    UnitIdentifier
                };

                return returnvalue;

            }
        }



       public byte[] Payload {
            get
            {
                List<byte> returnvalue = new List<byte>();

                returnvalue.AddRange(this.Mbap_Header);
                returnvalue.Add(FunctionCode);
                returnvalue.AddRange(Data);

                byte [] crc = BitConverter.GetBytes(ModbusClient.calculateCRC(returnvalue.ToArray(), (ushort)(returnvalue.Count - 8), 6));
                returnvalue.AddRange(crc);
                return returnvalue.ToArray();


            }


        }


    }




    }
