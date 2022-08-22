/*
Copyright (c) 2018-2020 Rossmann-Engineering
Permission is hereby granted, free of charge, 
to any person obtaining a copy of this software
and associated documentation files (the "Software"),
to deal in the Software without restriction, 
including without limitation the rights to use, 
copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit 
persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission 
notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Runtime.Serialization;

namespace EasyModbus.Exceptions
{
	/// <summary>
	/// Exception to be thrown if serial port is not opened
	/// </summary>
	public class SerialPortNotOpenedException : ModbusException
	{
   		public SerialPortNotOpenedException()
        	: base()
    	{
    	}

    	public SerialPortNotOpenedException(string message)
      	  : base(message)
    	{
    	}

    	public SerialPortNotOpenedException(string message, Exception innerException)
        	: base(message, innerException)
    	{
    	}

    	protected SerialPortNotOpenedException(SerializationInfo info, StreamingContext context)
        	: base(info, context)
   	 	{
   	 	}
	}
	
	/// <summary>
	/// Exception to be thrown if Connection to Modbus device failed
	/// </summary>
	public class ConnectionException : ModbusException
	{
   		public ConnectionException()
        	: base()
    	{
    	}

    	public ConnectionException(string message)
      	  : base(message)
    	{
    	}

    	public ConnectionException(string message, Exception innerException)
        	: base(message, innerException)
    	{
    	}

    	protected ConnectionException(SerializationInfo info, StreamingContext context)
        	: base(info, context)
   	 	{
   	 	}
	}
	
	/// <summary>
	/// Exception to be thrown if Modbus Server returns error code "Function code not supported"
	/// </summary>
	public class FunctionCodeNotSupportedException : ModbusException
	{
   		public FunctionCodeNotSupportedException()
        	: base()
    	{
    	}

    	public FunctionCodeNotSupportedException(string message)
      	  : base(message)
    	{
    	}

    	public FunctionCodeNotSupportedException(string message, Exception innerException)
        	: base(message, innerException)
    	{
    	}

    	protected FunctionCodeNotSupportedException(SerializationInfo info, StreamingContext context)
        	: base(info, context)
   	 	{
   	 	}
	}
	
	/// <summary>
	/// Exception to be thrown if Modbus Server returns error code "quantity invalid"
	/// </summary>
	public class QuantityInvalidException : ModbusException
	{
   		public QuantityInvalidException()
        	: base()
    	{
    	}

    	public QuantityInvalidException(string message)
      	  : base(message)
    	{
    	}

    	public QuantityInvalidException(string message, Exception innerException)
        	: base(message, innerException)
    	{
    	}

    	protected QuantityInvalidException(SerializationInfo info, StreamingContext context)
        	: base(info, context)
   	 	{
   	 	}
	}
	
	/// <summary>
	/// Exception to be thrown if Modbus Server returns error code "starting adddress and quantity invalid"
	/// </summary>
	public class StartingAddressInvalidException : ModbusException
	{
   		public StartingAddressInvalidException()
        	: base()
    	{
    	}

    	public StartingAddressInvalidException(string message)
      	  : base(message)
    	{
    	}

    	public StartingAddressInvalidException(string message, Exception innerException)
        	: base(message, innerException)
    	{
    	}

    	protected StartingAddressInvalidException(SerializationInfo info, StreamingContext context)
        	: base(info, context)
   	 	{
   	 	}
	}
	
	/// <summary>
	/// Exception to be thrown if Modbus Server returns error code "Function Code not executed (0x04)"
	/// </summary>
	public class ModbusException : Exception
	{
   		public ModbusException()
        	: base()
    	{
    	}

    	public ModbusException(string message)
      	  : base(message)
    	{
    	}

    	public ModbusException(string message, Exception innerException)
        	: base(message, innerException)
    	{
    	}

    	protected ModbusException(SerializationInfo info, StreamingContext context)
        	: base(info, context)
   	 	{
   	 	}
	}
	
	/// <summary>
	/// Exception to be thrown if CRC Check failed
	/// </summary>
	public class CRCCheckFailedException : ModbusException
	{
   		public CRCCheckFailedException()
        	: base()
    	{
    	}

    	public CRCCheckFailedException(string message)
      	  : base(message)
    	{
    	}

    	public CRCCheckFailedException(string message, Exception innerException)
        	: base(message, innerException)
    	{
    	}

    	protected CRCCheckFailedException(SerializationInfo info, StreamingContext context)
        	: base(info, context)
   	 	{
   	 	}
	}
	
}
