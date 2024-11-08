using System;
using System.Drawing;
using System.Configuration;
using System.Windows.Forms;
using System.Collections.Generic;
using UPDL_Speed_Tracker.Google_API;

namespace UPDL_Speed_Tracker
{
    public partial class Form1 : Form
    {
        #region TopPanel Variable
        //FormControl_Panel Variable
        //
        //MinimizeLabel - Minimize App
        private void MinimizeLabel_Click(object sender, System.EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        //
        //CloseLabel: Close Appl
        private void CloseLabel_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        //Moveable/Dragable App Variable
        private bool isDragging = false;
        private Point cursorStartPoint;

        private void TopPanel_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;

            cursorStartPoint = new Point(e.X, e.Y);
        }

        private void TopPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point pointScreen = PointToScreen(e.Location);
                this.Location = new Point(pointScreen.X - cursorStartPoint.X, pointScreen.Y - cursorStartPoint.Y);
            }
        }

        private void TopPanel_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }
        #endregion

        #region BottomPanel Variable
        //Border Line
        private void BottomPanel_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, BottomPanel.ClientRectangle,
                Color.DarkCyan, 1, ButtonBorderStyle.Solid,
                Color.Black, 0, ButtonBorderStyle.None,
                Color.DarkCyan, 1, ButtonBorderStyle.Solid,
                Color.DarkCyan, 1, ButtonBorderStyle.Solid);
        }
        #endregion

        //GetAppSetting
        private static string GetAppSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        //Google Sheet API
        private GoogleSheetsHelper gsheetsHelper = new GoogleSheetsHelper();
        private string gsheetID = GetAppSetting("GoogleSheetsID"); //Replace value of Google Sheets ID in App.config
        private string gsheetName = GetAppSetting("GoogleSpreadsheetName"); //Replace value of Google Spreadsheet Name in App.config
        private int startRow = 6; //Number of row to start store data

        public Form1()
        {
            InitializeComponent();

            //Reset Cycle ComboBox to Default
            Cycle_ComboBox.SelectedIndex = 0;

            //Get current/realtime Date & Time
            GetCurrentDateTime.CurrentDate(this);
            GetCurrentDateTime.CurrentTime(this);
        }

        //
        //FileSize_TextBox: Set to only can input number
        private void FileSize_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar))
            {
                if (!char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
        }

        //
        //Submit Button: Store data to Google Sheet
        private void SubmitButton_Click(object sender, EventArgs e)
        {
            //Define the range for each column
            string range_DateCycle;
            string range_GWC;
            string range_CIDS;
            string range_ItemsCount;

            //Find the next available row
            int nextRow = startRow;

            //Collect data from form fields
            string chooseDateSubmit = SubmitDate_DatePicker.Value.ToString("dd/MM/yyyy");
            string chooseCycle = Cycle_ComboBox.SelectedItem.ToString();

            string gwcStartTime = UPLOADStart_TimePicker.Value.ToString("T");
            string gwcEndTime = UPLOADEnd_TimePicker.Value.ToString("T");

            string cidsStartTime = DOWNLOADStart_TimePicker.Value.ToString("T");
            string cidsEndTime = DOWNLOADEnd_TimePicker.Value.ToString("T");

            int itemsCount = int.Parse(FileSize_TextBox.Text);

            //Prepare data to be written to Google Sheets by Group
            var valueDateCycle = new List<IList<object>>
            {
                new List<object> { chooseDateSubmit, chooseCycle }
            };

            var valueGWC = new List<IList<object>>
            {
                new List<object> { gwcStartTime, gwcEndTime }
            };

            var valueCIDS = new List<IList<object>>
            {
                new List<object> { cidsStartTime, cidsEndTime }
            };

            var valueItemsCount = new List<IList<object>>
            {
                new List<object> { itemsCount }
            };

            while (true)
            {
                range_DateCycle = $"{gsheetName}!B{nextRow}";
                range_GWC = $"{gsheetName}!D{nextRow}";
                range_CIDS = $"{gsheetName}!G{nextRow}";
                range_ItemsCount = $"{gsheetName}!J{nextRow}";

                // Check if the cell row has data or not
                bool rowHasData = gsheetsHelper.RowHasData(gsheetID, range_DateCycle) ||
                                  gsheetsHelper.RowHasData(gsheetID, range_GWC) ||
                                  gsheetsHelper.RowHasData(gsheetID, range_CIDS) ||
                                  gsheetsHelper.RowHasData(gsheetID, range_ItemsCount);

                if (!rowHasData)
                {
                    //If the row has no data, break the loop
                    break;
                }

                //If row has data, move to the next row
                nextRow++;
            }

            //Append grouped data to Google Sheets
            gsheetsHelper.AddDataToSheet(gsheetID, range_DateCycle, valueDateCycle);
            gsheetsHelper.AddDataToSheet(gsheetID, range_GWC, valueGWC);
            gsheetsHelper.AddDataToSheet(gsheetID, range_CIDS, valueCIDS);
            gsheetsHelper.AddDataToSheet(gsheetID, range_ItemsCount, valueItemsCount);

            //Update the current row to the next available row
            startRow = nextRow + 1;

            MessageBox.Show("Data submitted successfully!");

            //Reset All field to Default after success submit data
            Clear.ResetDateTime(this);
            Clear.ResetAll(this);
        }
    }
}
