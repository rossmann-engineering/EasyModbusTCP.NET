using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace EasyModbusServerSimulator
{
    
    public partial class PropertyForm : Form
    {
        public delegate void settingsChangedEvent();
        public event settingsChangedEvent SettingsChangedEvent;

        Settings settings = new Settings();
        Settings settingsFromMainForm = new Settings();
        public PropertyForm(Settings settings)
        {
            this.settingsFromMainForm.Port = settings.Port;
            this.settingsFromMainForm.ModbusTypeSelection = settings.ModbusTypeSelection;
            this.settings = settings;
            InitializeComponent();
            propertyGrid1.SelectedObject = settings;
        }

        private void btnDischard_Click(object sender, EventArgs e)
        {
            settings.Port = settingsFromMainForm.Port;
            settings.ModbusTypeSelection = settingsFromMainForm.ModbusTypeSelection;
            if (SettingsChangedEvent != null)
                SettingsChangedEvent();
            this.Close();
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
           
            if (SettingsChangedEvent != null)
                SettingsChangedEvent();
            this.Close();
        }

        private void propertyGrid1_Click(object sender, EventArgs e)
        {

        }
    }
}
