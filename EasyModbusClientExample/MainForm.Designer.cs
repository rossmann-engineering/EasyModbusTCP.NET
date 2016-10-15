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
			this.txtAnwerFromServer = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.cbbSelctionModbus = new System.Windows.Forms.ComboBox();
			this.txtCOMPort = new System.Windows.Forms.Label();
			this.cbbSelectComPort = new System.Windows.Forms.ComboBox();
			this.txtSlaveAddress = new System.Windows.Forms.Label();
			this.txtSlaveAddressInput = new System.Windows.Forms.TextBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
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
			this.btnReadCoils.Location = new System.Drawing.Point(34, 106);
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
			this.btnReadDiscreteInputs.Location = new System.Drawing.Point(34, 135);
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
			this.btnReadHoldingRegisters.Location = new System.Drawing.Point(34, 164);
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
			this.btnReadInputRegisters.Location = new System.Drawing.Point(34, 193);
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
			this.txtStartingAddressInput.Location = new System.Drawing.Point(220, 128);
			this.txtStartingAddressInput.Name = "txtStartingAddressInput";
			this.txtStartingAddressInput.Size = new System.Drawing.Size(39, 20);
			this.txtStartingAddressInput.TabIndex = 9;
			this.txtStartingAddressInput.Text = "1";
			// 
			// txtStartingAddress
			// 
			this.txtStartingAddress.Location = new System.Drawing.Point(220, 108);
			this.txtStartingAddress.Name = "txtStartingAddress";
			this.txtStartingAddress.Size = new System.Drawing.Size(89, 17);
			this.txtStartingAddress.TabIndex = 10;
			this.txtStartingAddress.Text = "Starting Address";
			// 
			// txtNumberOfValues
			// 
			this.txtNumberOfValues.Location = new System.Drawing.Point(220, 163);
			this.txtNumberOfValues.Name = "txtNumberOfValues";
			this.txtNumberOfValues.Size = new System.Drawing.Size(100, 17);
			this.txtNumberOfValues.TabIndex = 12;
			this.txtNumberOfValues.Text = "Number of Values";
			// 
			// txtNumberOfValuesInput
			// 
			this.txtNumberOfValuesInput.Location = new System.Drawing.Point(220, 183);
			this.txtNumberOfValuesInput.Name = "txtNumberOfValuesInput";
			this.txtNumberOfValuesInput.Size = new System.Drawing.Size(39, 20);
			this.txtNumberOfValuesInput.TabIndex = 11;
			this.txtNumberOfValuesInput.Text = "1";
			// 
			// lsbAnswerFromServer
			// 
			this.lsbAnswerFromServer.FormattingEnabled = true;
			this.lsbAnswerFromServer.Location = new System.Drawing.Point(326, 106);
			this.lsbAnswerFromServer.Name = "lsbAnswerFromServer";
			this.lsbAnswerFromServer.Size = new System.Drawing.Size(188, 160);
			this.lsbAnswerFromServer.TabIndex = 13;
			// 
			// txtAnwerFromServer
			// 
			this.txtAnwerFromServer.Location = new System.Drawing.Point(323, 86);
			this.txtAnwerFromServer.Name = "txtAnwerFromServer";
			this.txtAnwerFromServer.Size = new System.Drawing.Size(159, 17);
			this.txtAnwerFromServer.TabIndex = 14;
			this.txtAnwerFromServer.Text = "Answer from Modbus-Server";
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = global::EasyModbusClientExample.Properties.Resources.PLCLoggerCompact;
			this.pictureBox1.Location = new System.Drawing.Point(449, 9);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(65, 66);
			this.pictureBox1.TabIndex = 15;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
			// 
			// linkLabel1
			// 
			this.linkLabel1.AutoSize = true;
			this.linkLabel1.Location = new System.Drawing.Point(278, 10);
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
			this.cbbSelctionModbus.Location = new System.Drawing.Point(34, 2);
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
			this.txtSlaveAddress.Size = new System.Drawing.Size(84, 19);
			this.txtSlaveAddress.TabIndex = 20;
			this.txtSlaveAddress.Text = "Slave Address";
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
			this.textBox1.Location = new System.Drawing.Point(12, 273);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBox1.Size = new System.Drawing.Size(502, 157);
			this.textBox1.TabIndex = 22;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(522, 442);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.txtSlaveAddressInput);
			this.Controls.Add(this.txtSlaveAddress);
			this.Controls.Add(this.cbbSelectComPort);
			this.Controls.Add(this.txtCOMPort);
			this.Controls.Add(this.cbbSelctionModbus);
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.txtAnwerFromServer);
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
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

        private System.Windows.Forms.ListBox lsbAnswerFromServer;
        private System.Windows.Forms.Label txtAnwerFromServer;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.ComboBox cbbSelctionModbus;
        private System.Windows.Forms.Label txtCOMPort;
        private System.Windows.Forms.ComboBox cbbSelectComPort;
        private System.Windows.Forms.Label txtSlaveAddress;
        private System.Windows.Forms.TextBox txtSlaveAddressInput;
        private System.Windows.Forms.TextBox textBox1;
	}
}
