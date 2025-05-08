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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace EasyModbusClientExample
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		private EasyModbus.ModbusClient modbusClient;

        private int _startingAddress;
        private int[] _serverResponse;

        public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
            
			modbusClient = new EasyModbus.ModbusClient();
            modbusClient.ReceiveDataChanged += new EasyModbus.ModbusClient.ReceiveDataChangedHandler(UpdateReceiveData);
            modbusClient.SendDataChanged += new EasyModbus.ModbusClient.SendDataChangedHandler(UpdateSendData);
            modbusClient.ConnectedChanged += new EasyModbus.ModbusClient.ConnectedChangedHandler(UpdateConnectedChanged);
            //          modbusClient.LogFileFilename = "logFiletxt.txt";

            //modbusClient.Baudrate = 9600;
            //modbusClient.UnitIdentifier = 2;

        }

        string receiveData = null;
		
		void UpdateReceiveData(object sender)
		{
            receiveData = "Rx: " + BitConverter.ToString(modbusClient.receiveData).Replace("-", " ") + System.Environment.NewLine;
            Thread thread = new Thread(updateReceiveTextBox);
            thread.Start();
        }
        delegate void UpdateReceiveDataCallback();
        void updateReceiveTextBox()
        {
            if (textBox1.InvokeRequired)
            {
                UpdateReceiveDataCallback d = new UpdateReceiveDataCallback(updateReceiveTextBox);
                this.Invoke(d, new object[] {  });
            }
            else
            {
                textBox1.AppendText(receiveData);
            }
        }

        string sendData = null;
        void UpdateSendData(object sender)
		{
            sendData = "Tx: " + BitConverter.ToString(modbusClient.sendData).Replace("-", " ") + System.Environment.NewLine;
            Thread thread = new Thread(updateSendTextBox);
            thread.Start();

        }

        void updateSendTextBox()
        {
            if (textBox1.InvokeRequired)
            {
                UpdateReceiveDataCallback d = new UpdateReceiveDataCallback(updateSendTextBox);
                this.Invoke(d, new object[] { });
            }
            else
            {
                textBox1.AppendText(sendData);
            }
        }
		
		void BtnConnectClick(object sender, EventArgs e)
		{
			modbusClient.IPAddress = txtIpAddressInput.Text;
			modbusClient.Port = int.Parse(txtPortInput.Text);
			modbusClient.Connect();
		}
		void BtnReadCoilsClick(object sender, EventArgs e)
		{
            try
            {
                if (!modbusClient.Connected)
                {
                    buttonConnect_Click(null, null);
                }
                bool[] serverResponse = modbusClient.ReadCoils(int.Parse(txtStartingAddressInput.Text)-1, int.Parse(txtNumberOfValuesInput.Text));
                textBoxReadResult.Text = string.Join(Environment.NewLine, serverResponse.Select((r, index) => $"{index}\t{r}"));
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message,"Exception Reading values from Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReadDiscreteInputs_Click(object sender, EventArgs e)
        {
            try
            {
                if (!modbusClient.Connected)
                {
                    buttonConnect_Click(null, null);
                }
                bool[] serverResponse = modbusClient.ReadDiscreteInputs(int.Parse(txtStartingAddressInput.Text)-1, int.Parse(txtNumberOfValuesInput.Text));
                textBoxReadResult.Text = string.Join(Environment.NewLine, serverResponse.Select((r, index) => $"{index}\t{r}"));
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Exception Reading values from Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReadHoldingRegisters_Click(object sender, EventArgs e)
        {
            try
            {
                if (!modbusClient.Connected)
                {
                    buttonConnect_Click(null, null);
                }

                _startingAddress = int.Parse(txtStartingAddressInput.Text);
                _serverResponse = modbusClient.ReadHoldingRegisters(_startingAddress - 1, int.Parse(txtNumberOfValuesInput.Text));
                FillReadResultText();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Exception Reading values from Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReadInputRegisters_Click(object sender, EventArgs e)
        {
            try
            {
                if (!modbusClient.Connected)
                {
                    buttonConnect_Click(null, null);
                }

                _startingAddress = int.Parse(txtStartingAddressInput.Text);
                _serverResponse = modbusClient.ReadInputRegisters(_startingAddress - 1, int.Parse(txtNumberOfValuesInput.Text));
                FillReadResultText();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Exception Reading values from Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.EasyModbusTCP.net"); 
        }

        private void cbbSelctionModbus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (modbusClient.Connected)
                modbusClient.Disconnect();

            if (cbbSelctionModbus.SelectedIndex == 0)
            {
                
                txtIpAddress.Visible = true;
                txtIpAddressInput.Visible = true;
                txtPort.Visible = true;
                txtPortInput.Visible = true;
                txtCOMPort.Visible = false;
                cbbSelectComPort.Visible = false;
                txtSlaveAddress.Visible = true;
                txtSlaveAddressInput.Visible = true;
                lblBaudrate.Visible = false;
                lblParity.Visible = false;
                lblStopbits.Visible = false;
                txtBaudrate.Visible = false;
                cbbParity.Visible = false;
                cbbStopbits.Visible = false;
            }
            if (cbbSelctionModbus.SelectedIndex == 1)
            {
                cbbSelectComPort.SelectedIndex = 0;
                cbbParity.SelectedIndex = 0;
                cbbStopbits.SelectedIndex = 0;
                if (cbbSelectComPort.SelectedText == "")
                    cbbSelectComPort.SelectedItem.ToString();
                txtIpAddress.Visible = false;
                txtIpAddressInput.Visible = false;
                txtPort.Visible = false;
                txtPortInput.Visible = false;
                txtCOMPort.Visible = true;
                cbbSelectComPort.Visible = true;
                txtSlaveAddress.Visible = true;
                txtSlaveAddressInput.Visible = true;
                lblBaudrate.Visible = true;
                lblParity.Visible = true;
                lblStopbits.Visible = true;
                txtBaudrate.Visible = true;
                cbbParity.Visible = true;
                cbbStopbits.Visible = true;

 
            }
        }

        private void cbbSelectComPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (modbusClient.Connected)
                modbusClient.Disconnect();
            modbusClient.SerialPort = cbbSelectComPort.SelectedItem.ToString();

            modbusClient.UnitIdentifier = byte.Parse(txtSlaveAddressInput.Text);

        }
		
		void TxtSlaveAddressInputTextChanged(object sender, EventArgs e)
		{
            try
            {
                modbusClient.UnitIdentifier = byte.Parse(txtSlaveAddressInput.Text);
            }
            catch (FormatException)
            { }	
		}

        bool listBoxPrepareCoils = false;
        private void btnPrepareCoils_Click(object sender, EventArgs e)
        {
            if (!listBoxPrepareCoils)
            {
                textBoxReadResult.Text = string.Empty;
            }
            listBoxPrepareCoils = true;
            listBoxPrepareRegisters = false;
            lsbWriteToServer.Items.Add(txtCoilValue.Text);

        }
        bool listBoxPrepareRegisters = false;
        private void buttonPrepareRegisters_Click(object sender, EventArgs e)
        {
            if (!listBoxPrepareRegisters)
            {
                textBoxReadResult.Text = string.Empty;
            }
            listBoxPrepareRegisters = true;
            listBoxPrepareCoils = false;
            lsbWriteToServer.Items.Add(int.Parse(txtRegisterValue.Text));
        }

        private void btnWriteSingleCoil_Click(object sender, EventArgs e)
        {
            try
            {
                if (!modbusClient.Connected)
                {
                    buttonConnect_Click(null, null);
                }

                bool coilsToSend = false;

                coilsToSend = bool.Parse(lsbWriteToServer.Items[0].ToString());
    

                modbusClient.WriteSingleCoil(int.Parse(txtStartingAddressOutput.Text) - 1, coilsToSend);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Exception writing values to Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnWriteSingleRegister_Click(object sender, EventArgs e)
        {
            try
            {
                if (!modbusClient.Connected)
                {
                    buttonConnect_Click(null, null);
                }

                int registerToSend = 0;

                registerToSend = int.Parse(lsbWriteToServer.Items[0].ToString());


                modbusClient.WriteSingleRegister(int.Parse(txtStartingAddressOutput.Text) - 1, registerToSend);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Exception writing values to Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnWriteMultipleCoils_Click(object sender, EventArgs e)
        {
            try
            {
                if (!modbusClient.Connected)
                {
                    buttonConnect_Click(null, null);
                }

                bool[] coilsToSend = new bool[lsbWriteToServer.Items.Count];

                for (int i = 0; i < lsbWriteToServer.Items.Count; i++)
                {

                    coilsToSend[i] = bool.Parse(lsbWriteToServer.Items[i].ToString());
                }


                modbusClient.WriteMultipleCoils(int.Parse(txtStartingAddressOutput.Text) - 1, coilsToSend);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Exception writing values to Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnWriteMultipleRegisters_Click(object sender, EventArgs e)
        {
            try
            {
                if (!modbusClient.Connected)
                {
                    buttonConnect_Click(null, null);
                }

                int[] registersToSend = new int[lsbWriteToServer.Items.Count];

                for (int i = 0; i < lsbWriteToServer.Items.Count; i++)
                {

                    registersToSend[i] = int.Parse(lsbWriteToServer.Items[i].ToString());
                }


                modbusClient.WriteMultipleRegisters(int.Parse(txtStartingAddressOutput.Text) - 1, registersToSend);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Exception writing values to Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtCoilValue_DoubleClick(object sender, EventArgs e)
        {
            if (txtCoilValue.Text.Equals("FALSE"))
                txtCoilValue.Text = "TRUE";
            else
                txtCoilValue.Text = "FALSE";
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lsbWriteToServer.Items.Clear();
        }

        private void buttonClearEntry_Click(object sender, EventArgs e)
        {
            int rowindex = lsbWriteToServer.SelectedIndex;
            if(rowindex >= 0)
                lsbWriteToServer.Items.RemoveAt(rowindex);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtRegisterValue_TextChanged(object sender, EventArgs e)
        {

        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (modbusClient.Connected)
                    modbusClient.Disconnect();
                if (cbbSelctionModbus.SelectedIndex == 0)
                {
                   

                    modbusClient.IPAddress = txtIpAddressInput.Text;
                    modbusClient.Port = int.Parse(txtPortInput.Text);
                    modbusClient.SerialPort = null;
                    //modbusClient.receiveDataChanged += new EasyModbus.ModbusClient.ReceiveDataChanged(UpdateReceiveData);
                    //modbusClient.sendDataChanged += new EasyModbus.ModbusClient.SendDataChanged(UpdateSendData);
                    //modbusClient.connectedChanged += new EasyModbus.ModbusClient.ConnectedChanged(UpdateConnectedChanged);

                    modbusClient.Connect();
                }
                if (cbbSelctionModbus.SelectedIndex == 1)
                {
                    modbusClient.SerialPort = cbbSelectComPort.SelectedItem.ToString();
                    
                    modbusClient.UnitIdentifier = byte.Parse(txtSlaveAddressInput.Text);
                    modbusClient.Baudrate = int.Parse(txtBaudrate.Text);
                    if (cbbParity.SelectedIndex == 0)
                        modbusClient.Parity = System.IO.Ports.Parity.Even;
                    if (cbbParity.SelectedIndex == 1)
                        modbusClient.Parity = System.IO.Ports.Parity.Odd;
                    if (cbbParity.SelectedIndex == 2)
                        modbusClient.Parity = System.IO.Ports.Parity.None;

                    if (cbbStopbits.SelectedIndex == 0)
                        modbusClient.StopBits = System.IO.Ports.StopBits.One;
                    if (cbbStopbits.SelectedIndex == 1)
                        modbusClient.StopBits = System.IO.Ports.StopBits.OnePointFive;
                    if (cbbStopbits.SelectedIndex == 2)
                        modbusClient.StopBits = System.IO.Ports.StopBits.Two;

                    modbusClient.Connect();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Unable to connect to Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateConnectedChanged(object sender)
        {
            if (modbusClient.Connected)
            {
                txtConnectedStatus.Text = "Connected to Server";
                txtConnectedStatus.BackColor = Color.Green;
            }
            else
            {
                txtConnectedStatus.Text = "Not Connected to Server";
                txtConnectedStatus.BackColor = Color.Red;
            }
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            modbusClient.Disconnect();
        }

        private void txtBaudrate_TextChanged(object sender, EventArgs e)
        {
            if (modbusClient.Connected)
                modbusClient.Disconnect();
            modbusClient.Baudrate = int.Parse(txtBaudrate.Text);

          
        }

        private void checkBoxFloatOffset_CheckedChanged(object sender, EventArgs e)
        {
            FillReadResultText();
        }

        private void FillReadResultText()
        {
            var text = new StringBuilder("Addr\tHEX\tint16\tuint16\tfloat" + Environment.NewLine);
            text.Append(string.Join(Environment.NewLine,
                Enumerable.Range(0, _serverResponse.Length).Select((index) =>
                {
                    var r = _serverResponse[index];
                    var bytes = BitConverter.GetBytes(r);
                    var hex = $"0x{bytes[1].ToString("X2")}{bytes[0].ToString("X2")}";
                    var int16 = BitConverter.ToInt16(bytes, 0);
                    var uint16 = BitConverter.ToUInt16(bytes, 0);
                    var single = string.Empty;

                    if (((_startingAddress + index) % 2 == (checkBoxFloatOffset.Checked ? 0 : 1)) && (index + 1 < _serverResponse.Length))
                    {
                        var nextBytes = BitConverter.GetBytes(_serverResponse[index + 1]);
                        var data = new byte[4];
                        data[0] = nextBytes[0];
                        data[1] = nextBytes[1];
                        data[2] = bytes[0];
                        data[3] = bytes[1];
                        single = BitConverter.ToSingle(data, 0).ToString();
                    }

                    return $"{_startingAddress + index}\t{hex}\t{int16}\t{uint16}\t{single}";
                })
            ));
            textBoxReadResult.Text = text.ToString();
        }
    }
}
