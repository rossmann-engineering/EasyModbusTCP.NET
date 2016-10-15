/*
 * Created by SharpDevelop.
 * User: www.rossmann-engineering.de
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
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
                modbusClient = new EasyModbus.ModbusClient();
                txtIpAddress.Visible = true;
                txtIpAddressInput.Visible = true;
                txtPort.Visible = true;
                txtPortInput.Visible = true;
                txtCOMPort.Visible = false;
                cbbSelectComPort.Visible = false;
                txtSlaveAddress.Visible = false;
                txtSlaveAddressInput.Visible = false;
            }
            if (cbbSelctionModbus.SelectedIndex == 1)
            {
                cbbSelectComPort.SelectedIndex = 0;
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
                modbusClient = new EasyModbus.ModbusClient(cbbSelectComPort.SelectedItem.ToString());
                modbusClient.UnitIdentifier = byte.Parse(txtSlaveAddressInput.Text);
                modbusClient.receiveDataChanged += new EasyModbus.ModbusClient.ReceiveDataChanged(UpdateReceiveData);
				modbusClient.sendDataChanged += new EasyModbus.ModbusClient.SendDataChanged(UpdateSendData);
 
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
	}
}
