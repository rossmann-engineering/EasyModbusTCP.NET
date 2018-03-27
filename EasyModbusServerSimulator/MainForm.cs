using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using EasyModbus;
using System.Reflection;

namespace EasyModbusServerSimulator
{
    public partial class MainForm : Form
    {
        Settings settings = new Settings();
        EasyModbus.ModbusServer easyModbusTCPServer;
        private UInt16 startingAddressDiscreteInputs = 1;
        private UInt16 startingAddressCoils = 1;
        private UInt16 startingAddressHoldingRegisters = 1;
        private UInt16 startingAddressInputRegisters = 1;
        private bool showProtocolInformations = true;
        private bool preventInvokeDiscreteInputs = false;
        private bool preventInvokeCoils = false;
        private bool preventInvokeInputRegisters = false;
        private bool preventInvokeHoldingRegisters = false;

        public MainForm()
        {
            InitializeComponent();
            Assembly.GetExecutingAssembly().GetName().Version.ToString();
            lblVersion.Text = "Version: " + Assembly.GetExecutingAssembly().GetName().Version.Major.ToString() +"."+ Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString();
            easyModbusTCPServer = new EasyModbus.ModbusServer();
            
            easyModbusTCPServer.Listen();

            
            easyModbusTCPServer.CoilsChanged += new ModbusServer.CoilsChangedHandler(CoilsChanged);
            easyModbusTCPServer.HoldingRegistersChanged += new ModbusServer.HoldingRegistersChangedHandler(HoldingRegistersChanged);
            easyModbusTCPServer.NumberOfConnectedClientsChanged += new ModbusServer.NumberOfConnectedClientsChangedHandler(NumberOfConnectionsChanged);
            easyModbusTCPServer.LogDataChanged += new ModbusServer.LogDataChangedHandler(LogDataChanged);    
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                numericUpDown1.Value = startingAddressDiscreteInputs;
                dataGridView1.Rows.Clear();
                for (int i = startingAddressDiscreteInputs; i < 20 + startingAddressDiscreteInputs; i++)
                {
                    dataGridView1.Rows.Add(i, easyModbusTCPServer.discreteInputs[i]);
                    
                    if (easyModbusTCPServer.discreteInputs[i])
                        dataGridView1[1, i - startingAddressDiscreteInputs].Style.BackColor = Color.Green;
                    else
                        dataGridView1[1, i - startingAddressDiscreteInputs].Style.BackColor = Color.Red;
                }
            }
            if (tabControl1.SelectedIndex == 1)
            {
                
                numericUpDown1.Value = startingAddressCoils;
                dataGridView2.Rows.Clear();
                for (int i = startingAddressCoils; i < 20 + startingAddressCoils; i++)
                {
                    dataGridView2.Rows.Add(i, easyModbusTCPServer.coils[i]);
                    if (easyModbusTCPServer.coils[i])
                        dataGridView2[1, i - startingAddressCoils].Style.BackColor = Color.Green;
                    else
                        dataGridView2[1, i - startingAddressCoils].Style.BackColor = Color.Red;
                }
            }
            if (tabControl1.SelectedIndex == 2)
            {
                numericUpDown1.Value = startingAddressInputRegisters;
                dataGridView3.Rows.Clear();
                for (int i = startingAddressInputRegisters; i < 20 + startingAddressInputRegisters; i++)
                    dataGridView3.Rows.Add(i, easyModbusTCPServer.inputRegisters[i]);
            }
            if (tabControl1.SelectedIndex == 3)
            {
                numericUpDown1.Value = startingAddressHoldingRegisters;
                dataGridView4.Rows.Clear();
                for (int i = startingAddressHoldingRegisters; i < 20 + startingAddressHoldingRegisters; i++)
                    dataGridView4.Rows.Add(i, easyModbusTCPServer.holdingRegisters[i]);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tabControl1_SelectedIndexChanged(null, null);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
                startingAddressDiscreteInputs = (UInt16)numericUpDown1.Value;
            if (tabControl1.SelectedIndex == 1)
                startingAddressCoils = (UInt16)numericUpDown1.Value;
            if (tabControl1.SelectedIndex == 2)
                startingAddressInputRegisters = (UInt16)numericUpDown1.Value;
            if (tabControl1.SelectedIndex == 3)
                startingAddressHoldingRegisters = (UInt16)numericUpDown1.Value;
            tabControl1_SelectedIndexChanged(null, null);

        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowindex = dataGridView1.SelectedCells[0].RowIndex;
            if (easyModbusTCPServer.discreteInputs[rowindex+startingAddressDiscreteInputs] == false)
                easyModbusTCPServer.discreteInputs[rowindex+startingAddressDiscreteInputs] = true;
            else
                easyModbusTCPServer.discreteInputs[rowindex + startingAddressDiscreteInputs] = false;
            tabControl1_SelectedIndexChanged(null, null);

        }

        delegate void coilsChangedCallback(int coil, int numberOfCoil);
        private void CoilsChanged(int coil, int numberOfCoil)
        {
            if (preventInvokeCoils)
                return;
            if (this.tabControl1.InvokeRequired)
            {

                {
                    coilsChangedCallback d = new coilsChangedCallback(CoilsChanged);
                    this.Invoke(d, coil, numberOfCoil);
                }
            }
            else
            {
                if (tabControl1.SelectedIndex == 1)
                    tabControl1_SelectedIndexChanged(null, null);
            }
        }

        delegate void registersChangedCallback(int register, int numberOfRegisters);
        bool registersChanegesLocked;
        private void HoldingRegistersChanged(int register, int numberOfRegisters)
        {
            if (preventInvokeHoldingRegisters)
                return;

                try
                {
                    if (this.tabControl1.InvokeRequired)
                    {
                        {
                            if (!registersChanegesLocked)
                                lock (this)
                                {
                                    registersChanegesLocked = true;

                                    registersChangedCallback d = new registersChangedCallback(HoldingRegistersChanged);
                                    this.Invoke(d, register, numberOfRegisters);
                                }
                        }
                    }
                    else
                    {
                        if (tabControl1.SelectedIndex == 3)
                            tabControl1_SelectedIndexChanged(null, null);
                    }
                }
                catch (Exception) { }
                registersChanegesLocked = false;
            }
        

        bool LockNumberOfConnectionsChanged=false;
        delegate void numberOfConnectionsCallback();
        private void NumberOfConnectionsChanged()
        {
            if (this.label3.InvokeRequired & !LockNumberOfConnectionsChanged)
            {
                {
                    lock (this)
                    {
                        LockNumberOfConnectionsChanged = true;
                        numberOfConnectionsCallback d = new numberOfConnectionsCallback(NumberOfConnectionsChanged);
                        try
                        {
                            this.Invoke(d);
                        }
                        catch (Exception) { }
                        finally
                        {
                            LockNumberOfConnectionsChanged = false;
                        }
                    }
                }
            }
            else
            {
                try
                {
                    label3.Text = easyModbusTCPServer.NumberOfConnections.ToString();
                }
                catch (Exception)
                { }
            }
        }

        private void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowindex = dataGridView2.SelectedCells[0].RowIndex;
            if (easyModbusTCPServer.coils[rowindex + startingAddressCoils] == false)
                easyModbusTCPServer.coils[rowindex + startingAddressCoils] = true;
            else
                easyModbusTCPServer.coils[rowindex + startingAddressCoils] = false;
            tabControl1_SelectedIndexChanged(null, null);
          
        }

        private void dataGridView3_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView3.SelectedCells.Count > 0)
            {
                int rowindex = dataGridView3.SelectedCells[0].RowIndex;
                try
                {
                    easyModbusTCPServer.inputRegisters[rowindex + startingAddressInputRegisters] = Int16.Parse(dataGridView3.SelectedCells[0].Value.ToString());
                }
                catch (Exception) { }
                tabControl1_SelectedIndexChanged(null, null);
            }
        }

        private void dataGridView4_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView4.SelectedCells.Count > 0)
            {
                int rowindex = dataGridView4.SelectedCells[0].RowIndex;
                try
                {
                    easyModbusTCPServer.holdingRegisters[rowindex + startingAddressHoldingRegisters] = Int16.Parse(dataGridView4.SelectedCells[0].Value.ToString());
                }
                catch (Exception) { }
                tabControl1_SelectedIndexChanged(null, null);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.EasyModbusTCP.net");    
        }

        private void vScrollBar1_ValueChanged(object sender, EventArgs e)
        {
           startingAddressDiscreteInputs=(ushort)vScrollBar1.Value;
           tabControl1_SelectedIndexChanged(null, null);
        }

        private void vScrollBar2_ValueChanged(object sender, EventArgs e)
        {
            startingAddressCoils = (ushort)vScrollBar2.Value;
            tabControl1_SelectedIndexChanged(null, null);
        }

        private void vScrollBar3_ValueChanged(object sender, EventArgs e)
        {
            startingAddressInputRegisters = (ushort)vScrollBar3.Value;
            tabControl1_SelectedIndexChanged(null, null);
        }

        private void vScrollBar4_ValueChanged(object sender, EventArgs e)
        {
            startingAddressHoldingRegisters = (ushort)vScrollBar4.Value;
            tabControl1_SelectedIndexChanged(null, null);
        }

        delegate void logDataChangedCallback();
        bool locked;
        private void LogDataChanged()
        {

            if (showProtocolInformations == true)
            {

                if (this.listBox1.InvokeRequired)
                {
                    if (!locked)
                    {
                        lock (this)
                        {
                            locked = true;
                            try
                            {
                                logDataChangedCallback d = new logDataChangedCallback(LogDataChanged);
                                this.Invoke(d);
                            }
                            catch (Exception)
                            {
                            }
                           

                        }

                    }
                }
                else
                {

                    try
                    {
                        listBox1.Items.Clear();
                        string listBoxData;
                        for (int i = 0; i < easyModbusTCPServer.ModbusLogData.Length; i++)
                        {
                            if (easyModbusTCPServer.ModbusLogData[i] == null)
                                break;
                            if (easyModbusTCPServer.ModbusLogData[i].request)
                            {
                                listBoxData = easyModbusTCPServer.ModbusLogData[i].timeStamp.ToString("H:mm:ss.ff") + " Request from Client - Functioncode: " + easyModbusTCPServer.ModbusLogData[i].functionCode.ToString();
                                if (easyModbusTCPServer.ModbusLogData[i].functionCode <= 4)
                                {
                                    listBoxData = listBoxData + " ; Starting Address: " + easyModbusTCPServer.ModbusLogData[i].startingAdress.ToString() + " Quantity: " + easyModbusTCPServer.ModbusLogData[i].quantity.ToString();
                                }
                                if (easyModbusTCPServer.ModbusLogData[i].functionCode == 5)
                                {
                                    listBoxData = listBoxData + " ; Output Address: " + easyModbusTCPServer.ModbusLogData[i].startingAdress.ToString() + " Output Value: ";
                                    if (easyModbusTCPServer.ModbusLogData[i].receiveCoilValues[0] == 0)
                                        listBoxData = listBoxData + "False";
                                    if (easyModbusTCPServer.ModbusLogData[i].receiveCoilValues[0] == 0xFF00)
                                        listBoxData = listBoxData + "True";
                                }
                                if (easyModbusTCPServer.ModbusLogData[i].functionCode == 6)
                                {
                                    listBoxData = listBoxData + " ; Starting Address: " + easyModbusTCPServer.ModbusLogData[i].startingAdress.ToString() + " Register Value: " + easyModbusTCPServer.ModbusLogData[i].receiveRegisterValues[0].ToString();
                                }
                                if (easyModbusTCPServer.ModbusLogData[i].functionCode == 15)
                                {
                                    listBoxData = listBoxData + " ; Starting Address: " + easyModbusTCPServer.ModbusLogData[i].startingAdress.ToString() + " Quantity: " + easyModbusTCPServer.ModbusLogData[i].quantity.ToString() + " Byte Count: " + easyModbusTCPServer.ModbusLogData[i].byteCount.ToString() + " Values Received: ";
                                    for (int j = 0; j < easyModbusTCPServer.ModbusLogData[i].quantity; j++)
                                    {
                                        int shift = j % 16;
                                        if ((i == easyModbusTCPServer.ModbusLogData[i].quantity - 1) & (easyModbusTCPServer.ModbusLogData[i].quantity % 2 != 0))
                                        {
                                            if (shift < 8)
                                                shift = shift + 8;
                                            else
                                                shift = shift - 8;
                                        }
                                        int mask = 0x1;
                                        mask = mask << (shift);
                                        if ((easyModbusTCPServer.ModbusLogData[i].receiveCoilValues[j / 16] & (ushort)mask) == 0)
                                            listBoxData = listBoxData + " False";
                                        else
                                            listBoxData = listBoxData + " True";
                                    }
                                }
                                if (easyModbusTCPServer.ModbusLogData[i].functionCode == 16)
                                {
                                    listBoxData = listBoxData + " ; Starting Address: " + easyModbusTCPServer.ModbusLogData[i].startingAdress.ToString() + " Quantity: " + easyModbusTCPServer.ModbusLogData[i].quantity.ToString() + " Byte Count: " + easyModbusTCPServer.ModbusLogData[i].byteCount.ToString() + " Values Received: ";
                                    for (int j = 0; j < easyModbusTCPServer.ModbusLogData[i].quantity; j++)
                                    {
                                        listBoxData = listBoxData + " " + easyModbusTCPServer.ModbusLogData[i].receiveRegisterValues[j];
                                    }
                                }
                                if (easyModbusTCPServer.ModbusLogData[i].functionCode == 23)
                                {
                                    listBoxData = listBoxData + " ; Starting Address Read: " + easyModbusTCPServer.ModbusLogData[i].startingAddressRead.ToString() + " ; Quantity Read: " + easyModbusTCPServer.ModbusLogData[i].quantityRead.ToString() + " ; Starting Address Write: " + easyModbusTCPServer.ModbusLogData[i].startingAddressWrite.ToString() + " ; Quantity Write: " + easyModbusTCPServer.ModbusLogData[i].quantityWrite.ToString() + " ; Byte Count: " + easyModbusTCPServer.ModbusLogData[i].byteCount.ToString() + " ; Values Received: ";
                                    for (int j = 0; j < easyModbusTCPServer.ModbusLogData[i].quantityWrite; j++)
                                    {
                                        listBoxData = listBoxData + " " + easyModbusTCPServer.ModbusLogData[i].receiveRegisterValues[j];
                                    }
                                }

                                listBox1.Items.Add(listBoxData);
                            }
                            if (easyModbusTCPServer.ModbusLogData[i].response)
                            {
                                if (easyModbusTCPServer.ModbusLogData[i].exceptionCode > 0)
                                {
                                    listBoxData = easyModbusTCPServer.ModbusLogData[i].timeStamp.ToString("H:mm:ss.ff");
                                    listBoxData = listBoxData + (" Response To Client - Error code: " + Convert.ToString(easyModbusTCPServer.ModbusLogData[i].errorCode, 16));
                                    listBoxData = listBoxData + " Exception Code: " + easyModbusTCPServer.ModbusLogData[i].exceptionCode.ToString();
                                    listBox1.Items.Add(listBoxData);


                                }
                                else
                                {
                                    listBoxData = (easyModbusTCPServer.ModbusLogData[i].timeStamp.ToString("H:mm:ss.ff") + " Response To Client - Functioncode: " + easyModbusTCPServer.ModbusLogData[i].functionCode.ToString());

                                    if (easyModbusTCPServer.ModbusLogData[i].functionCode <= 4)
                                    {
                                        listBoxData = listBoxData + " ; Bytecount: " + easyModbusTCPServer.ModbusLogData[i].byteCount.ToString() + " ; Send values: ";
                                    }
                                    if (easyModbusTCPServer.ModbusLogData[i].functionCode == 5)
                                    {
                                        listBoxData = listBoxData + " ; Starting Address: " + easyModbusTCPServer.ModbusLogData[i].startingAdress.ToString() + " ; Output Value: ";
                                        if (easyModbusTCPServer.ModbusLogData[i].receiveCoilValues[0] == 0)
                                            listBoxData = listBoxData + "False";
                                        if (easyModbusTCPServer.ModbusLogData[i].receiveCoilValues[0] == 0xFF00)
                                            listBoxData = listBoxData + "True";
                                    }
                                    if (easyModbusTCPServer.ModbusLogData[i].functionCode == 6)
                                    {
                                        listBoxData = listBoxData + " ; Starting Address: " + easyModbusTCPServer.ModbusLogData[i].startingAdress.ToString() + " ; Register Value: " + easyModbusTCPServer.ModbusLogData[i].receiveRegisterValues[0].ToString();
                                    }
                                    if (easyModbusTCPServer.ModbusLogData[i].functionCode == 15)
                                    {
                                        listBoxData = listBoxData + " ; Starting Address: " + easyModbusTCPServer.ModbusLogData[i].startingAdress.ToString() + " ; Quantity: " + easyModbusTCPServer.ModbusLogData[i].quantity.ToString();
                                    }
                                    if (easyModbusTCPServer.ModbusLogData[i].functionCode == 16)
                                    {
                                        listBoxData = listBoxData + " ; Starting Address: " + easyModbusTCPServer.ModbusLogData[i].startingAdress.ToString() + " ; Quantity: " + easyModbusTCPServer.ModbusLogData[i].quantity.ToString();
                                    }
                                    if (easyModbusTCPServer.ModbusLogData[i].functionCode == 23)
                                    {
                                        listBoxData = listBoxData + " ; ByteCount: " + easyModbusTCPServer.ModbusLogData[i].byteCount.ToString() + " ; Send Register Values: ";
                                    }
                                    if (easyModbusTCPServer.ModbusLogData[i].sendCoilValues != null)
                                    {
                                        for (int j = 0; j < easyModbusTCPServer.ModbusLogData[i].sendCoilValues.Length; j++)
                                        {
                                            listBoxData = listBoxData + easyModbusTCPServer.ModbusLogData[i].sendCoilValues[j].ToString() + " ";
                                        }
                                    }
                                    if (easyModbusTCPServer.ModbusLogData[i].sendRegisterValues != null)
                                    {
                                        for (int j = 0; j < easyModbusTCPServer.ModbusLogData[i].sendRegisterValues.Length; j++)
                                        {
                                            listBoxData = listBoxData + easyModbusTCPServer.ModbusLogData[i].sendRegisterValues[j].ToString() + " ";
                                        }
                                    }
                                    listBox1.Items.Add(listBoxData);
                                }
                            }
                        }
                    }
                    catch (Exception) { }

                    locked = false;


                }
                
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                showProtocolInformations = true;
            else
                showProtocolInformations = false;
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        
        {

            easyModbusTCPServer.StopListening();
            Environment.Exit(0);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            easyModbusTCPServer.FunctionCode1Disabled = !checkBox2.Checked;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            easyModbusTCPServer.FunctionCode2Disabled = !checkBox3.Checked;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            easyModbusTCPServer.FunctionCode3Disabled = !checkBox4.Checked;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            easyModbusTCPServer.FunctionCode4Disabled = !checkBox5.Checked;
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            easyModbusTCPServer.FunctionCode5Disabled = !checkBox6.Checked;
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            easyModbusTCPServer.FunctionCode6Disabled = !checkBox7.Checked;
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            easyModbusTCPServer.FunctionCode15Disabled = !checkBox9.Checked;
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            easyModbusTCPServer.FunctionCode16Disabled = !checkBox8.Checked;
        }

        private void dataGridView2_MouseEnter(object sender, EventArgs e)
        {
            preventInvokeCoils = true;
        }

        private void dataGridView2_MouseLeave(object sender, EventArgs e)
        {
            preventInvokeCoils = false;
        }

        private void tabControl1_MouseEnter(object sender, EventArgs e)
        {
            
        }

        private void tabControl1_MouseLeave(object sender, EventArgs e)
        {
            
        }

        private void dataGridView4_MouseEnter(object sender, EventArgs e)
        {
            preventInvokeHoldingRegisters = true;
        }

        private void dataGridView4_MouseLeave(object sender, EventArgs e)
        {
            preventInvokeHoldingRegisters = false;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void btnProperties_Click(object sender, EventArgs e)
        {
            settings.ComPort = easyModbusTCPServer.SerialPort;
            settings.SlaveAddress = easyModbusTCPServer.UnitIdentifier;
            PropertyForm propertryForm = new PropertyForm(settings);
            propertryForm.SettingsChangedEvent += new PropertyForm.settingsChangedEvent(SettingsChanged);
            propertryForm.Show();
        }

        private void SettingsChanged()
        {
            easyModbusTCPServer.StopListening();
            easyModbusTCPServer.Port = settings.Port;
            easyModbusTCPServer.SerialPort = settings.ComPort;
            easyModbusTCPServer.UnitIdentifier = settings.SlaveAddress;
            if (settings.ModbusTypeSelection == Settings.ModbusType.ModbusUDP)
            {
                easyModbusTCPServer.UDPFlag = true;
                easyModbusTCPServer.SerialFlag = false;
                label4.Text = "...Modbus-UDP Server Listening (Port " + settings.Port + ")...";
            }
            else if (settings.ModbusTypeSelection == Settings.ModbusType.ModbusTCP)
            {
                easyModbusTCPServer.UDPFlag = false;
                easyModbusTCPServer.SerialFlag = false;
                label4.Text = "...Modbus-TCP Server Listening (Port " + settings.Port + ")...";
            }
            else if (settings.ModbusTypeSelection == Settings.ModbusType.ModbusRTU)
            {
                easyModbusTCPServer.UDPFlag = false;
                easyModbusTCPServer.SerialFlag = true;
                label4.Text = "...Modbus-RTU Client Listening (Com-Port: " + settings.ComPort + ")...";
            }
            easyModbusTCPServer.PortChanged = true;
            
            easyModbusTCPServer.Listen();
        }
		void EasyModbusTCPServerBindingSourceCurrentChanged(object sender, EventArgs e)
		{
	
		}

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.EasyModbusTCP.net"); 
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            easyModbusTCPServer.FunctionCode23Disabled = !checkBox10.Checked;
        }



        private void panel1_MouseLeave(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                showProtocolInformations = true;
            else
                showProtocolInformations = false;
        }

        private void panel1_MouseEnter(object sender, EventArgs e)
        {
            showProtocolInformations = false;
        }

        private void panel1_MouseLeave_1(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                showProtocolInformations = true;
            else
                showProtocolInformations = false;
        }



        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (checkBox1.Checked)
                showProtocolInformations = true;
            else
                showProtocolInformations = false;
        }

        private int xLastLocation;
        private int yLastLocation;
        private void listBox1_MouseMove(object sender, MouseEventArgs e)
        {
            


            if ((Math.Abs(e.Location.X - xLastLocation) < 50) & (Math.Abs(e.Location.Y - yLastLocation) < 50))
                return;
            xLastLocation = e.Location.X;
            yLastLocation = e.Location.Y;

                showProtocolInformations = false;
            string strToolTip = "";

            //Get the item
            int nIdx = listBox1.IndexFromPoint(e.Location);
            if ((nIdx >= 0) && (nIdx < listBox1.Items.Count))
                strToolTip = listBox1.Items[nIdx].ToString();

            toolTip1.SetToolTip(listBox1, strToolTip);
             
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Info infoWindow = new Info();
            infoWindow.Show();
        }

        private void setupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settings.ComPort = easyModbusTCPServer.SerialPort;
            settings.SlaveAddress = easyModbusTCPServer.UnitIdentifier;
            PropertyForm propertryForm = new PropertyForm(settings);
            propertryForm.SettingsChangedEvent += new PropertyForm.settingsChangedEvent(SettingsChanged);
            propertryForm.Show();
        }
        int topic = 0;
        private void cbEnableWebView_CheckedChanged(object sender, EventArgs e)
        {
            Random rnd = new Random();
            topic = rnd.Next(0, 99999);
            easyModbusTCPServer.MqttRootTopic = "easymodbusserver/"+ topic;
            if (cbEnableWebView.Checked)
            {
                easyModbusTCPServer.DeleteRetainedMessages(easyModbusTCPServer.MqttRootTopic);
                easyModbusTCPServer.MqttBrokerAddress = "www.mqtt-dashboard.com";
                linkLabel2.Text = "http://www.easymodbustcp.net/webview/easymodbuswebview.html ?topic=" + topic;
                linkLabel2.Visible = true;
                label6.Visible = true;


            }

            else
            {
                easyModbusTCPServer.MqttBrokerAddress = null;
                linkLabel2.Visible = false;
                label6.Visible = false;
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.easymodbustcp.net/webview/easymodbuswebview.html ?topic=" + topic);
        }
    }
}
