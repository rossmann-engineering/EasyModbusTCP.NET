# EasyModbusTCP.NET - <a href="EasyModbusTCP.NET">www.EasyModbusTCP.NET</a>

Modbus TCP, Modbus UDP and Modbus RTU client/server library for .NET<br>
Industry approved!!<br>
Fast and secure access from PC or Embedded Systems to many PLC-Systems and other components for industry automation. <br>
Only a few lines of codes are needed to read or write data from or to a PLC. 

<h2><a href="http://www.easymodbustcp.net">Implementation Guide and Codesamples: www.easymodbustcp.net</a></h2>

Additional Software tools e.g. Modbus Server Simulator, makes software development fast and easy. 

Download Library (*.DLL) from NuGet or from: <a href="https://sourceforge.net/projects/easymodbustcp/files/latest/download" rel="nofollow"><img alt="Download EasyModbusTCP/UDP/RTU .NET" src="https://a.fsdn.com/con/app/sf-download-button"></a>



Supported Function Codes:

- Read Coils (FC1)
- Read Discrete Inputs (FC2)
- Read Holding Registers (FC3)
- Read Input Registers (FC4)
- Write Single Coil (FC5)
- Write Single Register (FC6)
- Write Multiple Coils (FC15)
- Write Multiple Registers (FC16)
- Read/Write Multiple Registers (FC23)


Modbus TCP, Modbus UDP and Modbus RTU client/server library


Poll主机
2023-6-27：修复NumberOfRetries无效错误 if (NumberOfRetries > countRetries)
2023-6-27：修复TCP连接断线无法触发ConnectedChanged的问题
2020-8-15：增加响应延时属性ResposeDelay 事件ResposeDelayChanged
2020-8-11：修正Modbus主机模式下退出报不能为Null异常错误 详见:~ModbusClient()
2020-8-11：修正UDP连接connected属性一直为True的问题
2020-8-2：规范化ReceiveDataChanged(Byte[] data) SendDataChanged(Byte[] data)回调
2020-8-1：增加ModbusType 规范化编程
2020-8-1：增加UDP模式发送回传（全模式支持发送、接收通信数据回传）
2020-7-31：修正Modbus主机模式下连接超时 详见:connectTimeout 

Client从机
2023-6-29：删除numberOfClientsChanged、NumberOfConnectedClientsChanged事件
2020-8-13：解决ModbusUDP无法二次启动问题 关闭未结束线程 listenerThread
2020-8-2：增加ReceiveDataChanged(Byte[] data) SendDataChanged(Byte[] data)回调
2020-8-2：解决从机模式接收数据debug信息全部为00的问题
2020-8-1：增加ModbusType 规范化编程
2020-8-1：解决UDP从机模式关闭后不能打开的问题
2020-7-30：解决ModbusRTU从机模式下数据接收错误的问题 详见:SerialHandler


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
