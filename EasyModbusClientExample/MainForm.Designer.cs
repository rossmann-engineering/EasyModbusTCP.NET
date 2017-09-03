/*
 * Created by SharpDevelop.
 * User: srossmann
 * Date: 13.02.2015
 * Time: 11:09
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace EasyModbusClientExample
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.TextBox txtIpAddressInput;
		private System.Windows.Forms.Label txtIpAddress;
		private System.Windows.Forms.Label txtPort;
        private System.Windows.Forms.TextBox txtPortInput;
		private System.Windows.Forms.Button btnReadCoils;
		private System.Windows.Forms.Button btnReadDiscreteInputs;
		private System.Windows.Forms.Button btnReadHoldingRegisters;
		private System.Windows.Forms.Button btnReadInputRegisters;
		private System.Windows.Forms.TextBox txtStartingAddressInput;
		private System.Windows.Forms.Label txtStartingAddress;
		private System.Windows.Forms.Label txtNumberOfValues;
		private System.Windows.Forms.TextBox txtNumberOfValuesInput;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.txtIpAddressInput = new System.Windows.Forms.TextBox();
            this.txtIpAddress = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.Label();
            this.txtPortInput = new System.Windows.Forms.TextBox();
            this.btnReadCoils = new System.Windows.Forms.Button();
            this.btnReadDiscreteInputs = new System.Windows.Forms.Button();
            this.btnReadHoldingRegisters = new System.Windows.Forms.Button();
            this.btnReadInputRegisters = new System.Windows.Forms.Button();
            this.txtStartingAddressInput = new System.Windows.Forms.TextBox();
            this.txtStartingAddress = new System.Windows.Forms.Label();
            this.txtNumberOfValues = new System.Windows.Forms.Label();
            this.txtNumberOfValuesInput = new System.Windows.Forms.TextBox();
            this.lsbAnswerFromServer = new System.Windows.Forms.ListBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.cbbSelctionModbus = new System.Windows.Forms.ComboBox();
            this.txtCOMPort = new System.Windows.Forms.Label();
            this.cbbSelectComPort = new System.Windows.Forms.ComboBox();
            this.txtSlaveAddress = new System.Windows.Forms.Label();
            this.txtSlaveAddressInput = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnWriteMultipleRegisters = new System.Windows.Forms.Button();
            this.btnWriteMultipleCoils = new System.Windows.Forms.Button();
            this.btnWriteSingleRegister = new System.Windows.Forms.Button();
            this.btnWriteSingleCoil = new System.Windows.Forms.Button();
            this.txtCoilValue = new System.Windows.Forms.TextBox();
            this.txtRegisterValue = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnPrepareCoils = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblReadOperations = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtStartingAddressOutput = new System.Windows.Forms.TextBox();
            this.lsbWriteToServer = new System.Windows.Forms.ListBox();
            this.lblParity = new System.Windows.Forms.Label();
            this.lblStopbits = new System.Windows.Forms.Label();
            this.cbbParity = new System.Windows.Forms.ComboBox();
            this.cbbStopbits = new System.Windows.Forms.ComboBox();
            this.txtBaudrate = new System.Windows.Forms.TextBox();
            this.lblBaudrate = new System.Windows.Forms.Label();
            this.txtConnectedStatus = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // txtIpAddressInput
            // 
            this.txtIpAddressInput.Location = new System.Drawing.Point(34, 55);
            this.txtIpAddressInput.Name = "txtIpAddressInput";
            this.txtIpAddressInput.Size = new System.Drawing.Size(118, 20);
            this.txtIpAddressInput.TabIndex = 0;
            this.txtIpAddressInput.Text = "127.0.0.1";
            // 
            // txtIpAddress
            // 
            this.txtIpAddress.Location = new System.Drawing.Point(34, 35);
            this.txtIpAddress.Name = "txtIpAddress";
            this.txtIpAddress.Size = new System.Drawing.Size(100, 14);
            this.txtIpAddress.TabIndex = 1;
            this.txtIpAddress.Text = "Server IP-Address";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(158, 35);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(73, 17);
            this.txtPort.TabIndex = 3;
            this.txtPort.Text = "Server Port";
            // 
            // txtPortInput
            // 
            this.txtPortInput.Location = new System.Drawing.Point(158, 55);
            this.txtPortInput.Name = "txtPortInput";
            this.txtPortInput.Size = new System.Drawing.Size(56, 20);
            this.txtPortInput.TabIndex = 2;
            this.txtPortInput.Text = "502";
            // 
            // btnReadCoils
            // 
            this.btnReadCoils.Location = new System.Drawing.Point(12, 176);
            this.btnReadCoils.Name = "btnReadCoils";
            this.btnReadCoils.Size = new System.Drawing.Size(161, 23);
            this.btnReadCoils.TabIndex = 5;
            this.btnReadCoils.Text = "Read Coils - FC1";
            this.btnReadCoils.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReadCoils.UseVisualStyleBackColor = true;
            this.btnReadCoils.Click += new System.EventHandler(this.BtnReadCoilsClick);
            // 
            // btnReadDiscreteInputs
            // 
            this.btnReadDiscreteInputs.Location = new System.Drawing.Point(12, 205);
            this.btnReadDiscreteInputs.Name = "btnReadDiscreteInputs";
            this.btnReadDiscreteInputs.Size = new System.Drawing.Size(161, 23);
            this.btnReadDiscreteInputs.TabIndex = 6;
            this.btnReadDiscreteInputs.Text = "Read Discrete Inputs - FC2";
            this.btnReadDiscreteInputs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReadDiscreteInputs.UseVisualStyleBackColor = true;
            this.btnReadDiscreteInputs.Click += new System.EventHandler(this.btnReadDiscreteInputs_Click);
            // 
            // btnReadHoldingRegisters
            // 
            this.btnReadHoldingRegisters.Location = new System.Drawing.Point(12, 234);
            this.btnReadHoldingRegisters.Name = "btnReadHoldingRegisters";
            this.btnReadHoldingRegisters.Size = new System.Drawing.Size(161, 23);
            this.btnReadHoldingRegisters.TabIndex = 7;
            this.btnReadHoldingRegisters.Text = "Read Holding Registers - FC3";
            this.btnReadHoldingRegisters.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReadHoldingRegisters.UseVisualStyleBackColor = true;
            this.btnReadHoldingRegisters.Click += new System.EventHandler(this.btnReadHoldingRegisters_Click);
            // 
            // btnReadInputRegisters
            // 
            this.btnReadInputRegisters.Location = new System.Drawing.Point(12, 263);
            this.btnReadInputRegisters.Name = "btnReadInputRegisters";
            this.btnReadInputRegisters.Size = new System.Drawing.Size(161, 23);
            this.btnReadInputRegisters.TabIndex = 8;
            this.btnReadInputRegisters.Text = "Read Input Registers - FC4";
            this.btnReadInputRegisters.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReadInputRegisters.UseVisualStyleBackColor = true;
            this.btnReadInputRegisters.Click += new System.EventHandler(this.btnReadInputRegisters_Click);
            // 
            // txtStartingAddressInput
            // 
            this.txtStartingAddressInput.Location = new System.Drawing.Point(198, 198);
            this.txtStartingAddressInput.Name = "txtStartingAddressInput";
            this.txtStartingAddressInput.Size = new System.Drawing.Size(39, 20);
            this.txtStartingAddressInput.TabIndex = 9;
            this.txtStartingAddressInput.Text = "1";
            // 
            // txtStartingAddress
            // 
            this.txtStartingAddress.Location = new System.Drawing.Point(198, 178);
            this.txtStartingAddress.Name = "txtStartingAddress";
            this.txtStartingAddress.Size = new System.Drawing.Size(89, 17);
            this.txtStartingAddress.TabIndex = 10;
            this.txtStartingAddress.Text = "Starting Address";
            // 
            // txtNumberOfValues
            // 
            this.txtNumberOfValues.Location = new System.Drawing.Point(198, 233);
            this.txtNumberOfValues.Name = "txtNumberOfValues";
            this.txtNumberOfValues.Size = new System.Drawing.Size(100, 17);
            this.txtNumberOfValues.TabIndex = 12;
            this.txtNumberOfValues.Text = "Number of Values";
            // 
            // txtNumberOfValuesInput
            // 
            this.txtNumberOfValuesInput.Location = new System.Drawing.Point(198, 253);
            this.txtNumberOfValuesInput.Name = "txtNumberOfValuesInput";
            this.txtNumberOfValuesInput.Size = new System.Drawing.Size(39, 20);
            this.txtNumberOfValuesInput.TabIndex = 11;
            this.txtNumberOfValuesInput.Text = "1";
            // 
            // lsbAnswerFromServer
            // 
            this.lsbAnswerFromServer.FormattingEnabled = true;
            this.lsbAnswerFromServer.Location = new System.Drawing.Point(310, 147);
            this.lsbAnswerFromServer.Name = "lsbAnswerFromServer";
            this.lsbAnswerFromServer.Size = new System.Drawing.Size(188, 160);
            this.lsbAnswerFromServer.TabIndex = 13;
            this.lsbAnswerFromServer.DoubleClick += new System.EventHandler(this.lsbAnswerFromServer_DoubleClick);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(417, 13);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(165, 13);
            this.linkLabel1.TabIndex = 16;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "http://www.EasyModbusTCP.net";
            // 
            // cbbSelctionModbus
            // 
            this.cbbSelctionModbus.FormattingEnabled = true;
            this.cbbSelctionModbus.Items.AddRange(new object[] {
            "ModbusTCP (Ethernet)",
            "ModbusRTU (Serial)"});
            this.cbbSelctionModbus.Location = new System.Drawing.Point(34, 6);
            this.cbbSelctionModbus.Name = "cbbSelctionModbus";
            this.cbbSelctionModbus.Size = new System.Drawing.Size(180, 21);
            this.cbbSelctionModbus.TabIndex = 17;
            this.cbbSelctionModbus.Text = "ModbusTCP (Ethernet)";
            this.cbbSelctionModbus.SelectedIndexChanged += new System.EventHandler(this.cbbSelctionModbus_SelectedIndexChanged);
            // 
            // txtCOMPort
            // 
            this.txtCOMPort.Location = new System.Drawing.Point(34, 35);
            this.txtCOMPort.Name = "txtCOMPort";
            this.txtCOMPort.Size = new System.Drawing.Size(100, 14);
            this.txtCOMPort.TabIndex = 18;
            this.txtCOMPort.Text = "COM-Port";
            this.txtCOMPort.Visible = false;
            // 
            // cbbSelectComPort
            // 
            this.cbbSelectComPort.FormattingEnabled = true;
            this.cbbSelectComPort.Items.AddRange(new object[] {
            "COM1",
            "COM2",
            "COM3",
            "COM4",
            "COM5",
            "COM6",
            "COM7",
            "COM8"});
            this.cbbSelectComPort.Location = new System.Drawing.Point(34, 55);
            this.cbbSelectComPort.Name = "cbbSelectComPort";
            this.cbbSelectComPort.Size = new System.Drawing.Size(121, 21);
            this.cbbSelectComPort.TabIndex = 19;
            this.cbbSelectComPort.Visible = false;
            this.cbbSelectComPort.SelectedIndexChanged += new System.EventHandler(this.cbbSelectComPort_SelectedIndexChanged);
            // 
            // txtSlaveAddress
            // 
            this.txtSlaveAddress.Location = new System.Drawing.Point(158, 35);
            this.txtSlaveAddress.Name = "txtSlaveAddress";
            this.txtSlaveAddress.Size = new System.Drawing.Size(56, 19);
            this.txtSlaveAddress.TabIndex = 20;
            this.txtSlaveAddress.Text = "Slave ID";
            this.txtSlaveAddress.Visible = false;
            // 
            // txtSlaveAddressInput
            // 
            this.txtSlaveAddressInput.Location = new System.Drawing.Point(158, 55);
            this.txtSlaveAddressInput.Name = "txtSlaveAddressInput";
            this.txtSlaveAddressInput.Size = new System.Drawing.Size(56, 20);
            this.txtSlaveAddressInput.TabIndex = 21;
            this.txtSlaveAddressInput.Text = "1";
            this.txtSlaveAddressInput.Visible = false;
            this.txtSlaveAddressInput.TextChanged += new System.EventHandler(this.TxtSlaveAddressInputTextChanged);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 506);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(641, 157);
            this.textBox1.TabIndex = 22;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // btnWriteMultipleRegisters
            // 
            this.btnWriteMultipleRegisters.Location = new System.Drawing.Point(12, 453);
            this.btnWriteMultipleRegisters.Name = "btnWriteMultipleRegisters";
            this.btnWriteMultipleRegisters.Size = new System.Drawing.Size(161, 23);
            this.btnWriteMultipleRegisters.TabIndex = 30;
            this.btnWriteMultipleRegisters.Text = "Write Multiple Registers - FC16";
            this.btnWriteMultipleRegisters.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnWriteMultipleRegisters.UseVisualStyleBackColor = true;
            this.btnWriteMultipleRegisters.Click += new System.EventHandler(this.btnWriteMultipleRegisters_Click);
            // 
            // btnWriteMultipleCoils
            // 
            this.btnWriteMultipleCoils.Location = new System.Drawing.Point(12, 424);
            this.btnWriteMultipleCoils.Name = "btnWriteMultipleCoils";
            this.btnWriteMultipleCoils.Size = new System.Drawing.Size(161, 23);
            this.btnWriteMultipleCoils.TabIndex = 29;
            this.btnWriteMultipleCoils.Text = "Write Multiple Coils - FC15";
            this.btnWriteMultipleCoils.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnWriteMultipleCoils.UseVisualStyleBackColor = true;
            this.btnWriteMultipleCoils.Click += new System.EventHandler(this.btnWriteMultipleCoils_Click);
            // 
            // btnWriteSingleRegister
            // 
            this.btnWriteSingleRegister.Location = new System.Drawing.Point(12, 395);
            this.btnWriteSingleRegister.Name = "btnWriteSingleRegister";
            this.btnWriteSingleRegister.Size = new System.Drawing.Size(161, 23);
            this.btnWriteSingleRegister.TabIndex = 28;
            this.btnWriteSingleRegister.Text = "Write Single Register - FC6";
            this.btnWriteSingleRegister.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnWriteSingleRegister.UseVisualStyleBackColor = true;
            this.btnWriteSingleRegister.Click += new System.EventHandler(this.btnWriteSingleRegister_Click);
            // 
            // btnWriteSingleCoil
            // 
            this.btnWriteSingleCoil.Location = new System.Drawing.Point(12, 366);
            this.btnWriteSingleCoil.Name = "btnWriteSingleCoil";
            this.btnWriteSingleCoil.Size = new System.Drawing.Size(161, 23);
            this.btnWriteSingleCoil.TabIndex = 27;
            this.btnWriteSingleCoil.Text = "Write Single Coil - FC5";
            this.btnWriteSingleCoil.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnWriteSingleCoil.UseVisualStyleBackColor = true;
            this.btnWriteSingleCoil.Click += new System.EventHandler(this.btnWriteSingleCoil_Click);
            // 
            // txtCoilValue
            // 
            this.txtCoilValue.BackColor = System.Drawing.SystemColors.Info;
            this.txtCoilValue.Location = new System.Drawing.Point(563, 413);
            this.txtCoilValue.Name = "txtCoilValue";
            this.txtCoilValue.ReadOnly = true;
            this.txtCoilValue.Size = new System.Drawing.Size(81, 20);
            this.txtCoilValue.TabIndex = 31;
            this.txtCoilValue.Text = "FALSE";
            this.txtCoilValue.DoubleClick += new System.EventHandler(this.txtCoilValue_DoubleClick);
            // 
            // txtRegisterValue
            // 
            this.txtRegisterValue.BackColor = System.Drawing.SystemColors.Info;
            this.txtRegisterValue.Location = new System.Drawing.Point(563, 461);
            this.txtRegisterValue.Name = "txtRegisterValue";
            this.txtRegisterValue.Size = new System.Drawing.Size(81, 20);
            this.txtRegisterValue.TabIndex = 32;
            this.txtRegisterValue.Text = "0";
            this.txtRegisterValue.TextChanged += new System.EventHandler(this.txtRegisterValue_TextChanged);
            // 
            // button2
            // 
            this.button2.Cursor = System.Windows.Forms.Cursors.Default;
            this.button2.Image = global::EasyModbusClientExample.Properties.Resources.circle_minus;
            this.button2.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button2.Location = new System.Drawing.Point(518, 340);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(64, 52);
            this.button2.TabIndex = 34;
            this.button2.Text = "clear entry";
            this.button2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnClear
            // 
            this.btnClear.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnClear.Image = global::EasyModbusClientExample.Properties.Resources.circle_delete1;
            this.btnClear.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnClear.Location = new System.Drawing.Point(585, 340);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(64, 52);
            this.btnClear.TabIndex = 33;
            this.btnClear.Text = "clear all";
            this.btnClear.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // button1
            // 
            this.button1.Image = global::EasyModbusClientExample.Properties.Resources.arrow_left;
            this.button1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button1.Location = new System.Drawing.Point(507, 457);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(146, 43);
            this.button1.TabIndex = 26;
            this.button1.Text = "Prepare Registers";
            this.button1.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnPrepareCoils
            // 
            this.btnPrepareCoils.Image = global::EasyModbusClientExample.Properties.Resources.arrow_left;
            this.btnPrepareCoils.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPrepareCoils.Location = new System.Drawing.Point(507, 408);
            this.btnPrepareCoils.Name = "btnPrepareCoils";
            this.btnPrepareCoils.Size = new System.Drawing.Size(146, 43);
            this.btnPrepareCoils.TabIndex = 25;
            this.btnPrepareCoils.Text = "Prepare Coils";
            this.btnPrepareCoils.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.btnPrepareCoils.UseVisualStyleBackColor = true;
            this.btnPrepareCoils.Click += new System.EventHandler(this.btnPrepareCoils_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::EasyModbusClientExample.Properties.Resources.PLCLoggerCompact;
            this.pictureBox1.Location = new System.Drawing.Point(588, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(65, 66);
            this.pictureBox1.TabIndex = 15;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // lblReadOperations
            // 
            this.lblReadOperations.AutoSize = true;
            this.lblReadOperations.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblReadOperations.Location = new System.Drawing.Point(19, 127);
            this.lblReadOperations.Name = "lblReadOperations";
            this.lblReadOperations.Size = new System.Drawing.Size(206, 20);
            this.lblReadOperations.TabIndex = 37;
            this.lblReadOperations.Text = "Read values from Server";
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.Lime;
            this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3.Location = new System.Drawing.Point(310, 65);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(89, 47);
            this.button3.TabIndex = 38;
            this.button3.Text = "connect";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.Red;
            this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button4.Location = new System.Drawing.Point(409, 65);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(89, 47);
            this.button4.TabIndex = 39;
            this.button4.Text = "disconnect";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(19, 310);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(185, 20);
            this.label1.TabIndex = 40;
            this.label1.Text = "Write values to Server";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(198, 395);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 17);
            this.label4.TabIndex = 42;
            this.label4.Text = "Starting Address";
            // 
            // txtStartingAddressOutput
            // 
            this.txtStartingAddressOutput.Location = new System.Drawing.Point(198, 415);
            this.txtStartingAddressOutput.Name = "txtStartingAddressOutput";
            this.txtStartingAddressOutput.Size = new System.Drawing.Size(39, 20);
            this.txtStartingAddressOutput.TabIndex = 41;
            this.txtStartingAddressOutput.Text = "1";
            // 
            // lsbWriteToServer
            // 
            this.lsbWriteToServer.FormattingEnabled = true;
            this.lsbWriteToServer.Location = new System.Drawing.Point(310, 340);
            this.lsbWriteToServer.Name = "lsbWriteToServer";
            this.lsbWriteToServer.Size = new System.Drawing.Size(188, 160);
            this.lsbWriteToServer.TabIndex = 43;
            // 
            // lblParity
            // 
            this.lblParity.Location = new System.Drawing.Point(96, 82);
            this.lblParity.Name = "lblParity";
            this.lblParity.Size = new System.Drawing.Size(56, 19);
            this.lblParity.TabIndex = 46;
            this.lblParity.Text = "Parity";
            this.lblParity.Visible = false;
            // 
            // lblStopbits
            // 
            this.lblStopbits.Location = new System.Drawing.Point(158, 82);
            this.lblStopbits.Name = "lblStopbits";
            this.lblStopbits.Size = new System.Drawing.Size(56, 19);
            this.lblStopbits.TabIndex = 48;
            this.lblStopbits.Text = "Stopbits";
            this.lblStopbits.Visible = false;
            // 
            // cbbParity
            // 
            this.cbbParity.FormattingEnabled = true;
            this.cbbParity.Items.AddRange(new object[] {
            "Even",
            "Odd",
            "None"});
            this.cbbParity.Location = new System.Drawing.Point(97, 101);
            this.cbbParity.Name = "cbbParity";
            this.cbbParity.Size = new System.Drawing.Size(55, 21);
            this.cbbParity.TabIndex = 50;
            this.cbbParity.Visible = false;
            // 
            // cbbStopbits
            // 
            this.cbbStopbits.FormattingEnabled = true;
            this.cbbStopbits.Items.AddRange(new object[] {
            "1",
            "1.5",
            "2"});
            this.cbbStopbits.Location = new System.Drawing.Point(158, 101);
            this.cbbStopbits.Name = "cbbStopbits";
            this.cbbStopbits.Size = new System.Drawing.Size(55, 21);
            this.cbbStopbits.TabIndex = 51;
            this.cbbStopbits.Visible = false;
            // 
            // txtBaudrate
            // 
            this.txtBaudrate.Location = new System.Drawing.Point(35, 102);
            this.txtBaudrate.Name = "txtBaudrate";
            this.txtBaudrate.Size = new System.Drawing.Size(56, 20);
            this.txtBaudrate.TabIndex = 53;
            this.txtBaudrate.Text = "9600";
            this.txtBaudrate.Visible = false;
            this.txtBaudrate.TextChanged += new System.EventHandler(this.txtBaudrate_TextChanged);
            // 
            // lblBaudrate
            // 
            this.lblBaudrate.Location = new System.Drawing.Point(35, 82);
            this.lblBaudrate.Name = "lblBaudrate";
            this.lblBaudrate.Size = new System.Drawing.Size(56, 19);
            this.lblBaudrate.TabIndex = 52;
            this.lblBaudrate.Text = "Baudrate";
            this.lblBaudrate.Visible = false;
            // 
            // txtConnectedStatus
            // 
            this.txtConnectedStatus.BackColor = System.Drawing.Color.Red;
            this.txtConnectedStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.txtConnectedStatus.Location = new System.Drawing.Point(3, 669);
            this.txtConnectedStatus.Name = "txtConnectedStatus";
            this.txtConnectedStatus.Size = new System.Drawing.Size(665, 32);
            this.txtConnectedStatus.TabIndex = 54;
            this.txtConnectedStatus.Text = "Not connected to Server";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(669, 702);
            this.Controls.Add(this.txtConnectedStatus);
            this.Controls.Add(this.txtBaudrate);
            this.Controls.Add(this.lblBaudrate);
            this.Controls.Add(this.cbbStopbits);
            this.Controls.Add(this.cbbParity);
            this.Controls.Add(this.lblStopbits);
            this.Controls.Add(this.lblParity);
            this.Controls.Add(this.lsbWriteToServer);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtStartingAddressOutput);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.lblReadOperations);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.txtRegisterValue);
            this.Controls.Add(this.txtCoilValue);
            this.Controls.Add(this.btnWriteMultipleRegisters);
            this.Controls.Add(this.btnWriteMultipleCoils);
            this.Controls.Add(this.btnWriteSingleRegister);
            this.Controls.Add(this.btnWriteSingleCoil);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnPrepareCoils);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.txtSlaveAddressInput);
            this.Controls.Add(this.txtSlaveAddress);
            this.Controls.Add(this.cbbSelectComPort);
            this.Controls.Add(this.txtCOMPort);
            this.Controls.Add(this.cbbSelctionModbus);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lsbAnswerFromServer);
            this.Controls.Add(this.txtNumberOfValues);
            this.Controls.Add(this.txtNumberOfValuesInput);
            this.Controls.Add(this.txtStartingAddress);
            this.Controls.Add(this.txtStartingAddressInput);
            this.Controls.Add(this.btnReadInputRegisters);
            this.Controls.Add(this.btnReadHoldingRegisters);
            this.Controls.Add(this.btnReadDiscreteInputs);
            this.Controls.Add(this.btnReadCoils);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.txtPortInput);
            this.Controls.Add(this.txtIpAddress);
            this.Controls.Add(this.txtIpAddressInput);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "EasyModbus Client";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

        private System.Windows.Forms.ListBox lsbAnswerFromServer;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.ComboBox cbbSelctionModbus;
        private System.Windows.Forms.Label txtCOMPort;
        private System.Windows.Forms.ComboBox cbbSelectComPort;
        private System.Windows.Forms.Label txtSlaveAddress;
        private System.Windows.Forms.TextBox txtSlaveAddressInput;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnPrepareCoils;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnWriteMultipleRegisters;
        private System.Windows.Forms.Button btnWriteMultipleCoils;
        private System.Windows.Forms.Button btnWriteSingleRegister;
        private System.Windows.Forms.Button btnWriteSingleCoil;
        private System.Windows.Forms.TextBox txtCoilValue;
        private System.Windows.Forms.TextBox txtRegisterValue;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label lblReadOperations;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtStartingAddressOutput;
        private System.Windows.Forms.ListBox lsbWriteToServer;
        private System.Windows.Forms.Label lblParity;
        private System.Windows.Forms.Label lblStopbits;
        private System.Windows.Forms.ComboBox cbbParity;
        private System.Windows.Forms.ComboBox cbbStopbits;
        private System.Windows.Forms.TextBox txtBaudrate;
        private System.Windows.Forms.Label lblBaudrate;
        private System.Windows.Forms.TextBox txtConnectedStatus;
    }
}
