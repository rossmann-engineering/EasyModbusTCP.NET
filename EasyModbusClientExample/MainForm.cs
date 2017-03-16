/*
 * Attribution-NonCommercial-NoDerivatives 4.0 International (CC BY-NC-ND 4.0)
 * www.rossmann-engineering.de
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace EasyModbusClientExample
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		private EasyModbus.ModbusClient modbusClient;
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
            
			modbusClient = new EasyModbus.ModbusClient();
            modbusClient.receiveDataChanged += new EasyModbus.ModbusClient.ReceiveDataChanged(UpdateReceiveData);
            modbusClient.sendDataChanged += new EasyModbus.ModbusClient.SendDataChanged(UpdateSendData);
            //          modbusClient.LogFileFilename = "logFiletxt.txt";

            //modbusClient.Baudrate = 9600;
            //modbusClient.UnitIdentifier = 2;

        }
		
		delegate void UpdateReceiveDataCallback(object sender);
		void UpdateReceiveData(object sender)
		{
			if (textBox1.InvokeRequired)
			{
				UpdateReceiveDataCallback d = new UpdateReceiveDataCallback(UpdateReceiveData);
				this.Invoke(d, new object[] { this });
			}
			else
			{
				textBox1.AppendText("Rx: ");
				string hex = BitConverter.ToString(modbusClient.receiveData);
  				hex = hex.Replace("-"," ");
				textBox1.AppendText(hex);
				textBox1.AppendText(System.Environment.NewLine);
			}
			
		}
		
		void UpdateSendData(object sender)
		{
			textBox1.AppendText("Tx: ");
			//for (int i = 0; i < modbusClient.receiveData.Length; i++)
				  string hex = BitConverter.ToString(modbusClient.sendData);
  						//return hex.Replace("-","");
  						hex = hex.Replace("-"," ");
			//	textBox1.AppendText(modbusClient.receiveData[i].ToString()+" ");
			textBox1.AppendText(hex);
			textBox1.AppendText(System.Environment.NewLine);
			
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
                    modbusClient.IPAddress = txtIpAddressInput.Text;
                    modbusClient.Port = int.Parse(txtPortInput.Text);
                    modbusClient.Connect();
                }
                bool[] serverResponse = modbusClient.ReadCoils(int.Parse(txtStartingAddressInput.Text)-1, int.Parse(txtNumberOfValuesInput.Text));
                lsbAnswerFromServer.Items.Clear();
                for (int i = 0; i < serverResponse.Length; i++)
                {
                    lsbAnswerFromServer.Items.Add(serverResponse[i]);
                }
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
                    modbusClient.IPAddress = txtIpAddressInput.Text;
                    modbusClient.Port = int.Parse(txtPortInput.Text);
                    modbusClient.Connect();
                }
                bool[] serverResponse = modbusClient.ReadDiscreteInputs(int.Parse(txtStartingAddressInput.Text)-1, int.Parse(txtNumberOfValuesInput.Text));
                lsbAnswerFromServer.Items.Clear();
                for (int i = 0; i < serverResponse.Length; i++)
                {
                    lsbAnswerFromServer.Items.Add(serverResponse[i]);
                }
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
                    modbusClient.IPAddress = txtIpAddressInput.Text;
                    modbusClient.Port = int.Parse(txtPortInput.Text);
                    modbusClient.Connect();
                }


               int[] serverResponse = modbusClient.ReadHoldingRegisters(int.Parse(txtStartingAddressInput.Text)-1, int.Parse(txtNumberOfValuesInput.Text));

                lsbAnswerFromServer.Items.Clear();
                for (int i = 0; i < serverResponse.Length; i++)
                {
                	lsbAnswerFromServer.Items.Add(serverResponse[i]);
                  
                }
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
                    modbusClient.IPAddress = txtIpAddressInput.Text;
                    modbusClient.Port = int.Parse(txtPortInput.Text);
                    modbusClient.Connect();
                }
                
                int[] serverResponse = modbusClient.ReadInputRegisters(int.Parse(txtStartingAddressInput.Text)-1, int.Parse(txtNumberOfValuesInput.Text));
  
                lsbAnswerFromServer.Items.Clear();
                for (int i = 0; i < serverResponse.Length; i++)
                {
                	lsbAnswerFromServer.Items.Add(serverResponse[i]);
                }
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
            if (cbbSelctionModbus.SelectedIndex == 0)
            {
                
                txtIpAddress.Visible = true;
                txtIpAddressInput.Visible = true;
                txtPort.Visible = true;
                txtPortInput.Visible = true;
                txtCOMPort.Visible = false;
                cbbSelectComPort.Visible = false;
                txtSlaveAddress.Visible = false;
                txtSlaveAddressInput.Visible = false;
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
            modbusClient = new EasyModbus.ModbusClient(cbbSelectComPort.SelectedItem.ToString());
            modbusClient.UnitIdentifier = byte.Parse(txtSlaveAddressInput.Text);
            modbusClient.receiveDataChanged += new EasyModbus.ModbusClient.ReceiveDataChanged(UpdateReceiveData);
            modbusClient.sendDataChanged += new EasyModbus.ModbusClient.SendDataChanged(UpdateSendData);

        }
		
		void TxtSlaveAddressInputTextChanged(object sender, EventArgs e)
		{
			modbusClient.UnitIdentifier = byte.Parse(txtSlaveAddressInput.Text);		
		}

        bool listBoxPrepareCoils = false;
        private void btnPrepareCoils_Click(object sender, EventArgs e)
        {
            if (!listBoxPrepareCoils)
            {
                lsbAnswerFromServer.Items.Clear();
            }
            listBoxPrepareCoils = true;
            listBoxPrepareRegisters = false;
            lsbWriteToServer.Items.Add(txtCoilValue.Text);

        }
        bool listBoxPrepareRegisters = false;
        private void button1_Click(object sender, EventArgs e)
        {
            if (!listBoxPrepareRegisters)
            {
                lsbAnswerFromServer.Items.Clear();
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
                    modbusClient.IPAddress = txtIpAddressInput.Text;
                    modbusClient.Port = int.Parse(txtPortInput.Text);
                    modbusClient.Connect();
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
                    modbusClient.IPAddress = txtIpAddressInput.Text;
                    modbusClient.Port = int.Parse(txtPortInput.Text);
                    modbusClient.Connect();
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
                    modbusClient.IPAddress = txtIpAddressInput.Text;
                    modbusClient.Port = int.Parse(txtPortInput.Text);
                    modbusClient.Connect();
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
                    modbusClient.IPAddress = txtIpAddressInput.Text;
                    modbusClient.Port = int.Parse(txtPortInput.Text);
                    modbusClient.Connect();
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

        private void lsbAnswerFromServer_DoubleClick(object sender, EventArgs e)
        {
            int rowindex = lsbAnswerFromServer.SelectedIndex;

           



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

        private void button2_Click(object sender, EventArgs e)
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

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbbSelctionModbus.SelectedIndex == 0)
                {
                    modbusClient = new EasyModbus.ModbusClient(txtIpAddressInput.Text, int.Parse(txtPortInput.Text));


                    modbusClient.receiveDataChanged += new EasyModbus.ModbusClient.ReceiveDataChanged(UpdateReceiveData);
                    modbusClient.sendDataChanged += new EasyModbus.ModbusClient.SendDataChanged(UpdateSendData);
                    modbusClient.connectedChanged += new EasyModbus.ModbusClient.ConnectedChanged(UpdateConnectedChanged);

                    modbusClient.Connect();
                }
                if (cbbSelctionModbus.SelectedIndex == 1)
                {

                    modbusClient = new EasyModbus.ModbusClient(cbbSelectComPort.SelectedItem.ToString());
                    modbusClient.receiveDataChanged += new EasyModbus.ModbusClient.ReceiveDataChanged(UpdateReceiveData);
                    modbusClient.sendDataChanged += new EasyModbus.ModbusClient.SendDataChanged(UpdateSendData);
                    modbusClient.connectedChanged += new EasyModbus.ModbusClient.ConnectedChanged(UpdateConnectedChanged);
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

                    modbusClient.receiveDataChanged += new EasyModbus.ModbusClient.ReceiveDataChanged(UpdateReceiveData);
                    modbusClient.sendDataChanged += new EasyModbus.ModbusClient.SendDataChanged(UpdateSendData);
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

        private void button4_Click(object sender, EventArgs e)
        {
            modbusClient.Disconnect();
        }
    }
}
