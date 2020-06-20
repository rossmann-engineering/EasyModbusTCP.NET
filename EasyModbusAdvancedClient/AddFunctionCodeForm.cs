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
