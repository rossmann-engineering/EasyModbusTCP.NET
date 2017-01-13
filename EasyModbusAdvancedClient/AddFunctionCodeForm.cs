/*
 * Attribution-NonCommercial-NoDerivatives 4.0 International (CC BY-NC-ND 4.0)
 * User: srossmann
 * Date: 26.11.2015
 * Time: 06:55
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace EasyModbusAdvancedClient
{
	/// <summary>
	/// Description of AddFunctionCodeForm.
	/// </summary>
	public partial class AddFunctionCodeForm : Form
	{
		EasyModbusManager easyModbusManager;
		int selectedConnection;
		private bool editMode = false;
		private int indexToEdit;
		public AddFunctionCodeForm(EasyModbusManager easyModbusManager, int selectedConnection)
		{
			this.easyModbusManager = easyModbusManager;
			this.selectedConnection = selectedConnection;
			InitializeComponent();
			propertyGrid1.SelectedObject = new FunctionProperties();
		}
		
		public AddFunctionCodeForm(EasyModbusManager easyModbusManager, int selectedConnection, int indexToEdit)
		{
			this.easyModbusManager = easyModbusManager;
			this.selectedConnection = selectedConnection;
			InitializeComponent();
			FunctionProperties functionProperty;
			functionProperty = easyModbusManager.connectionPropertiesList[selectedConnection].FunctionPropertiesList[indexToEdit];
			propertyGrid1.SelectedObject = functionProperty;
			editMode = true;
			this.indexToEdit = indexToEdit;
		}
		
		void Button2Click(object sender, EventArgs e)
		{
			this.Close();
		}
		
		void Button1Click(object sender, EventArgs e)
		{
			if (!editMode)
				this.easyModbusManager.AddFunctionProperty((FunctionProperties)propertyGrid1.SelectedObject, selectedConnection);
			else
				this.easyModbusManager.EditFunctionProperty((FunctionProperties)propertyGrid1.SelectedObject, selectedConnection, indexToEdit);
			this.Close();
		}
	}
}
