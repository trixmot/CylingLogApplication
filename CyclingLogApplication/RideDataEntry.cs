﻿using System;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Data.SqlClient;
//using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
using System.Windows.Forms;

namespace CyclingLogApplication
{
    public partial class RideDataEntry : Form
    {
        private static int formLoad = 1;
        private static int formClosing = 0;
        private SqlConnection sqlConnection;// = new SqlConnection(@"Data Source=.\SQLEXPRESS;AttachDbFilename=""\\Mac\Home\Documents\Visual Studio 2015\Projects\CyclingLogApplication\CyclingLogApplication\CyclingLogDatabase.mdf"";Integrated Security=True");
        private DatabaseConnection databaseConnection;// = new DatabaseConnection(@"Data Source=.\SQLEXPRESS;AttachDbFilename=""\\Mac\Home\Documents\Visual Studio 2015\Projects\CyclingLogApplication\CyclingLogApplication\CyclingLogDatabase.mdf"";Integrated Security=True");

        //MainForm mainForm;
        public RideDataEntry(MainForm mainForm)
        {
            InitializeComponent();
            //MainForm mainForm = new MainForm("");
            sqlConnection = mainForm.GetsqlConnectionString();
            databaseConnection = mainForm.GetsDatabaseConnectionString();
            
            //Hidden field to store the record id of the current displaying record that has been loaded on the page:
            tbRecordID.Hide();
            tbWeekNumber.Hide();
            tbRecordID.Text = "0";
            lbRideDataEntryError.Hide();
            lbRideDataEntryError.Text = "";

            // Set the Minimum, Maximum, and initial Value.
            numericUpDown1.Value = 0;
            numericUpDown1.Maximum = 200;
            numericUpDown1.Minimum = 0;
            numericUpDown1.DecimalPlaces = 2;
            numericUpDown1.Increment = 0.10M;

            numDistanceRideDataEntry.Value = 0;
            numDistanceRideDataEntry.Maximum = 200;
            numDistanceRideDataEntry.Minimum = 0;
            numDistanceRideDataEntry.DecimalPlaces = 2;
            numDistanceRideDataEntry.Increment = 0.01M;

            //tbComments.ScrollBars = ScrollBars.Horizontal;

            dtpTimeRideDataEntry.Format = DateTimePickerFormat.Custom;
            //For 24 H format
            dtpTimeRideDataEntry.CustomFormat = "HH:mm:ss";
            dtpTimeRideDataEntry.ShowUpDown = true;
            dtpTimeRideDataEntry.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);

            //Update Ride Type cbRideTypeDataEntry:
            //cbRideTypeDataEntry.Items.Add("Recovery");
            //cbRideTypeDataEntry.Items.Add("Base");
            //cbRideTypeDataEntry.Items.Add("Distance");
            //cbRideTypeDataEntry.Items.Add("Speed");
            //cbRideTypeDataEntry.Items.Add("Race");

            List<string> routeList = mainForm.GetRoutes();
            for (int i = 0; i < routeList.Count; i++)
            {
                cbRouteDataEntry.Items.Add(routeList.ElementAt(i));
            }

            List<string> logYearList = mainForm.GetLogYears();
            for (int i = 0; i < logYearList.Count; i++)
            {
                cbLogYearDataEntry.Items.Add(logYearList.ElementAt(i));
            }

            //Set index for the LogYear:
            int logYearIndex = Convert.ToInt32(mainForm.GetLastLogSelectedDataEntry());

            if (logYearIndex == -1)
            {
                lbRideDataEntryError.Show();
                lbRideDataEntryError.Text = "No Log Year selected.";
            }
            else
            {
                //Note: the cbLogYearDataEntry is loaded from the mainForm load:
                lbRideDataEntryError.Hide();
            }

            formLoad = 1;
            numericUpDown2.Value = 1;
            numericUpDown2.Maximum = 1;
            numericUpDown2.Minimum = 1;
            numericUpDown2.Enabled = false;

            List<string> bikeList = mainForm.ReadDataNames("Table_Bikes", "Name");
            //Load Bike values:
            foreach (var val in bikeList)
            {
                cbBikeDataEntrySelection.Items.Add(val);
            }
        }

        public void AddLogYearDataEntry(string item)
        {
            cbLogYearDataEntry.Items.Add(item);
        }

        public void RemoveLogYearDataEntry(string item)
        {
            cbLogYearDataEntry.Items.Remove(item);
        }

        public void AddRouteDataEntry(string item)
        {
            cbRouteDataEntry.Items.Add(item);
        }

        public void RemoveRouteDataEntry(string item)
        {
            cbRouteDataEntry.Items.Remove(item);
        }

        public void AddBikeDataEntry(string item)
        {
            cbBikeDataEntrySelection.Items.Add(item);
        }

        public void RemoveBikeDataEntry(string item)
        {
            cbBikeDataEntrySelection.Items.Remove(item);
        }

        public void SetLastLogYearSelected(int index)
        {
            cbLogYearDataEntry.SelectedIndex = index;
        }

        //Diable x close option:
        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle |= CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        private void DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (formClosing == 0 && chk1RideDataEntry.Checked)
            {
                if (cbLogYearDataEntry.SelectedIndex == -1)
                {
                    lbRideDataEntryError.Show();
                    lbRideDataEntryError.Text = "No Log Year selected";

                    return;
                }
                formLoad = 0;

                // Look up to see if there is an entry by this date:
                GetRideData(dtpRideDate.Text, 1);
            }
        }

        private void GetRideData(string date, int recordDateIndex)
        {
            if (formLoad == 1)
            {
                return;
            }

            List<object> objectValues = new List<object>();
            objectValues.Add(date);

            int logID;
            int logLevel;

            using (MainForm mainForm = new MainForm(""))
            {
                logID = mainForm.GetLogYearIndex(cbLogYearDataEntry.SelectedItem.ToString());
                logLevel = mainForm.GetLogLevel();
            }
            objectValues.Add(Convert.ToInt32(logID));

            string movingTime;
            string rideDistance;
            string avgSpeed;
            string bike;
            string rideType;
            string wind;
            string temp;
            string avgCadence;
            string avgHeartRate;
            string maxHeartRate;
            string caloriesField;
            string totalAscent;
            string totalDescent;
            string maxSpeed;
            string avgPower;
            string maxPower;
            string route;
            string comments;
            string location;
            string recordID;
            string weekNumber;
            string effort;

            int recordIndex = 0;

            try
            {
                //ExecuteScalarFunction
                using (var results = ExecuteSimpleQueryConnection("GetRideData", objectValues))
                {
                    if (results.HasRows)
                    {
                        while (results.Read())
                        {
                            recordIndex++;
                            if (recordDateIndex == recordIndex)
                            {
                                //MessageBox.Show(String.Format("{0}", results[0]));
                                lbRideDataEntryError.Hide();

                                movingTime = results[0].ToString();
                                rideDistance = results[1].ToString();
                                avgSpeed = results[2].ToString();
                                bike = results[3].ToString();
                                rideType = results[4].ToString();
                                wind = results[5].ToString();
                                temp = results[6].ToString();
                                avgCadence = results[7].ToString();
                                avgHeartRate = results[8].ToString();
                                maxHeartRate = results[9].ToString();
                                caloriesField = results[10].ToString();
                                totalAscent = results[11].ToString();
                                totalDescent = results[12].ToString();
                                maxSpeed = results[13].ToString();
                                avgPower = results[14].ToString();
                                maxPower = results[15].ToString();
                                route = results[16].ToString();
                                comments = results[17].ToString();
                                location = results[18].ToString();
                                recordID = results[19].ToString();
                                weekNumber = results[20].ToString();
                                effort = results[21].ToString();

                                //Load ride data page:
                                dtpTimeRideDataEntry.Value = Convert.ToDateTime(movingTime);
                                numDistanceRideDataEntry.Value = Convert.ToDecimal(rideDistance);
                                numericUpDown1.Value = Convert.ToDecimal(avgSpeed);
                                cbBikeDataEntrySelection.SelectedIndex = cbBikeDataEntrySelection.Items.IndexOf(bike);
                                cbRideTypeDataEntry.SelectedIndex = cbRideTypeDataEntry.Items.IndexOf(rideType);
                                numericUpDown4.Value = Convert.ToDecimal(wind);
                                numericUpDown3.Value = Convert.ToDecimal(temp);
                                avg_cadence.Text = avgCadence;
                                avg_heart_rate.Text = avgHeartRate;
                                max_heart_rate.Text = maxHeartRate;
                                calories.Text = caloriesField;
                                total_ascent.Text = totalAscent;
                                total_descent.Text = totalDescent;
                                max_speed.Text = maxSpeed;
                                avg_power.Text = avgPower;
                                max_power.Text = maxPower;
                                cbRouteDataEntry.SelectedIndex = cbRouteDataEntry.Items.IndexOf(route);
                                tbComments.Text = comments;
                                tbRecordID.Text = recordID;
                                tbWeekNumber.Text = weekNumber;
                                cbLocationDataEntry.SelectedIndex = cbLocationDataEntry.Items.IndexOf(location);
                                cbEffortRideDataEntry.SelectedIndex = cbEffortRideDataEntry.Items.IndexOf(effort);
                            }
                            else
                            {
                                Logger.Log("Ride data for an unselected date index was selected.", 0, logLevel);
                            }
                        }
                    }
                    else
                    {
                        lbRideDataEntryError.Show();
                        lbRideDataEntryError.Text = "No ride data found for the selected date.";
                        tbRecordID.Text = "0";
                        ClearDataEntryFields();
                        recordIndex = 1;
                    }

                    numericUpDown2.Maximum = recordIndex;
                    numericUpDown2.Minimum = 1;
                }

                if (recordIndex == 1)
                {
                    numericUpDown2.Enabled = false;
                }
                else
                {
                    numericUpDown2.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("[ERROR]: Exception while trying to retrive ride data." + ex.Message.ToString());
            }
        }

        private void SubmitData(object sender, EventArgs e)
        {
            lbRideDataEntryError.Text = "";
            lbRideDataEntryError.Hide();

            RideInformationChange("Add", "Ride_Information_Add");
        }

        private void CloseRideDataEntry(object sender, EventArgs e)
        {
            using (MainForm mainForm = new MainForm(""))
            {
                mainForm.SetLastBikeSelected(cbBikeDataEntrySelection.SelectedIndex);
                mainForm.SetLastLogSelectedDataEntry(cbLogYearDataEntry.SelectedIndex);
                //Close();
                //this.Invoke(new MethodInvoker(delegate { this.Close(); }), null);
                //DialogResult result = MessageBox.Show("Any unsaved changes will be lost, do you want to continue?", "Exit Data Entry Form", MessageBoxButtons.YesNo);
                //if (result == DialogResult.Yes)
                // {
                //Close();
                formClosing = 1;
                this.Invoke(new MethodInvoker(delegate { this.Close(); }), null);
                //}
            }
        }

        //private void textBox1_Validating(object sender, CancelEventArgs e)
        //{
        //    TextBox box = sender as TextBox;
        //    string pattern = "\\d{1,2}:\\d{2}\\s*(AM|PM)";
        //    //textBox1.Text = string.Format("{00:00:00}", 55);
        //    if (box != null)
        //    {
        //        if (!Regex.IsMatch(box.Text, pattern, RegexOptions.CultureInvariant))
        //        {
        //            MessageBox.Show("Not a valid time format ('hh:mm AM|PM').");
        //            e.Cancel = true;
        //            box.Select(0, box.Text.Length);
        //        }
        //    }
        //}

        //private void RideDataEntry_FormClosing(object sender, FormClosingEventArgs e)
        //{

        //    DialogResult result = MessageBox.Show("Do you really want to exit?", "Dialog Title", MessageBoxButtons.YesNo);
        //    if (result == DialogResult.Yes)
        //    {
        //        //Close();
        //        //this.Invoke(new MethodInvoker(delegate { this.Close(); }), null);
        //        RideDataEntry.ActiveForm.Close();
        //    }
        //    else
        //    {
        //        e.Cancel = true;
        //    }
        //}

        private void RideInformationChange(string changeType, string procedureName)
        {
            lbRideDataEntryError.Hide();

            try
            {
                //Make sure certain required fields are filled in:
                DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                if (dtpTimeRideDataEntry.Value == dt)
                {
                    lbRideDataEntryError.Text = "The Ride Time must be greater than 0:00:00.";
                    lbRideDataEntryError.Show();
                    return;
                }
                decimal avgspeed = numericUpDown1.Value;
                if (avgspeed == 0)
                {
                    lbRideDataEntryError.Text = "The Average Speed must be greater than 0.";
                    lbRideDataEntryError.Show();
                    return;
                }
                if (cbLogYearDataEntry.SelectedIndex < 0)
                {
                    lbRideDataEntryError.Text = "A Log year must be selected.";
                    lbRideDataEntryError.Show();
                    return;
                }
                if (cbRouteDataEntry.SelectedIndex < 0)
                {
                    lbRideDataEntryError.Text = "A Route must be selected.";
                    lbRideDataEntryError.Show();
                    return;
                }
                if (cbBikeDataEntrySelection.SelectedIndex < 0)
                {
                    lbRideDataEntryError.Text = "A Bike must be selected.";
                    lbRideDataEntryError.Show();
                    return;
                }
                if (cbRideTypeDataEntry.SelectedIndex < 0)
                {
                    lbRideDataEntryError.Text = "A Ride Type must be selected.";
                    lbRideDataEntryError.Show();
                    return;
                }
                if (cbLocationDataEntry.SelectedIndex < 0)
                {
                    lbRideDataEntryError.Text = "A Ride Location must be selected.";
                    lbRideDataEntryError.Show();
                    return;
                }
                if (cbEffortRideDataEntry.SelectedIndex < 0)
                {
                    lbRideDataEntryError.Text = "An Effort option must be selected.";
                    lbRideDataEntryError.Show();
                    return;
                }

                //===============================
                string year = "";
                List<object> objectValuesLogYear = new List<object>();
                objectValuesLogYear.Add(cbLogYearDataEntry.SelectedItem);

                //ExecuteScalarFunction
                using (var results = ExecuteSimpleQueryConnection("Log_Year_Get", objectValuesLogYear))
                {
                    if (results.HasRows)
                    {
                        while (results.Read())
                        {
                            year = results[0].ToString();

                        }
                    }
                    else
                    {
                        // lbMaintError.Text = "No entry found for the selected Bike and Date.";
                        return;
                    }
                }

                DateTime rideDate = dtpRideDate.Value;
                if (!year.Equals(rideDate.Year.ToString()))
                {
                    lbRideDataEntryError.Text = "The Log year does not match up with the current ride date.";
                    lbRideDataEntryError.Show();
                    return;
                }

                //===============================
                int logSetting;
                int logIndex;

                using (MainForm mainForm = new MainForm(""))
                {
                    logSetting = mainForm.GetLogLevel();
                    logIndex = mainForm.GetLogYearIndex(cbLogYearDataEntry.SelectedItem.ToString());
                }
                string recordID = tbRecordID.Text;

                if (changeType.Equals("Update"))
                {
                    if (tbRecordID.Text.Equals("0"))
                    {
                        MessageBox.Show("The current ride can not be updated since it was not loaded from the database.");
                        Logger.Log("The current ride can not be updated since it was not loaded from the database." + recordID, 0, logSetting);

                        return;
                    }
                    DialogResult result = MessageBox.Show("Updating the ride in the database. Do you want to continue?", "Update Data", MessageBoxButtons.YesNo);
                    if (result == DialogResult.No)
                    {
                        return;
                    }
                }

                // Check recordID value:
                if (tbRecordID.Text.Equals("0") && changeType.Equals("Add"))
                {
                    DialogResult result = MessageBox.Show("Detected that the current data was retrieved from a record that was already saved to the database. Do you want to continue adding the record?", "Add Ride Data", MessageBoxButtons.YesNo);
                    if (result == DialogResult.No)
                    {
                        Logger.Log("Detected that the current data was retrieved from a record that was already saved to the database. RecordID" + recordID, 0, logSetting);

                        return;
                    }
                }
                else
                {
                    if (changeType.Equals("Add"))
                    {
                        DialogResult result = MessageBox.Show("Adding the ride to the database. Do you want to continue?", "Add Data", MessageBoxButtons.YesNo);
                        if (result == DialogResult.No)
                        {
                            return;
                        }
                    }

                }

                List<object> objectValues = new List<object>();
                objectValues.Add(dtpTimeRideDataEntry.Value);                           //Moving Time:
                objectValues.Add(numDistanceRideDataEntry.Value);                       //Ride Distance:
                objectValues.Add(numericUpDown1.Value);                                 //Average Speed:
                objectValues.Add(cbBikeDataEntrySelection.SelectedItem.ToString());              //Bike:
                objectValues.Add(cbRideTypeDataEntry.SelectedItem.ToString());          //Ride Type:
                double windspeed = (double)numericUpDown4.Value;
                objectValues.Add(numericUpDown4.Value);                                 //Wind:
                double temp = (double)numericUpDown3.Value;
                objectValues.Add(numericUpDown3.Value);                                 //Temp:
                objectValues.Add(dtpRideDate.Value);                                    //Date:

                if (avg_cadence.Text.Equals("") || avg_cadence.Text.Equals("--"))
                {
                    objectValues.Add(0);                                                
                }
                else
                {
                    objectValues.Add(Convert.ToDouble(avg_cadence.Text));                //Average Cadence:
                }
                
                if (avg_heart_rate.Text.Equals("") || avg_heart_rate.Text.Equals("--"))
                {
                    objectValues.Add(0);                                                //Average Heart Rate:
                } else
                {
                    objectValues.Add(Convert.ToDouble(avg_heart_rate.Text));              
                }
                
                if (max_heart_rate.Text.Equals("") || max_heart_rate.Text.Equals("--"))
                {
                    objectValues.Add(0);                                                   
                } else
                {
                    objectValues.Add(Convert.ToDouble(max_heart_rate.Text));              //Max Heart Rate:
                }

                if (calories.Text.Equals("") || calories.Text.Equals("--"))
                {
                    objectValues.Add(0);
                }
                else {
                    objectValues.Add(Convert.ToDouble(calories.Text));                    //Calories:
                }

                if (total_ascent.Text.Equals("") || total_ascent.Text.Equals("--"))
                {
                    objectValues.Add(0);
                }
                else
                {
                    objectValues.Add(Convert.ToDouble(total_ascent.Text));                //Total Ascent:
                }

                if (total_descent.Text.Equals("") || total_descent.Text.Equals("--"))
                {
                    objectValues.Add(0);
                }
                else
                {
                    objectValues.Add(Convert.ToDouble(total_descent.Text));               //Total Descent:
                }

                if (max_speed.Text.Equals("") || max_speed.Text.Equals("--"))
                {
                    objectValues.Add(0);
                }
                else
                {
                    objectValues.Add(Convert.ToDouble(max_speed.Text));                   //Max Speed:
                }

                if (avg_power.Text.Equals("") || avg_power.Text.Equals("--"))
                {
                    objectValues.Add(0);                                               //Average Power:
                }
                else
                {
                    objectValues.Add(Convert.ToDouble(avg_power.Text));                 //Average Power:
                }

                if (max_power.Text.Equals("") || max_power.Text.Equals("--"))
                { 
                    objectValues.Add(0);                                                //Max Power:
                }
                else
                {
                    objectValues.Add(Convert.ToDouble(max_power.Text));                 //Max Power:
                }

                objectValues.Add(cbRouteDataEntry.SelectedItem.ToString());                 //Route:
                objectValues.Add(tbComments.Text);                                          //Comments:
                objectValues.Add(logIndex);                                             //LogYear index:

                DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
                Calendar cal = dfi.Calendar;
                int weekValue = cal.GetWeekOfYear(dtpRideDate.Value, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);

                if (changeType.Equals("Update"))                                        //Week number:
                {
                    objectValues.Add(Int32.Parse(tbWeekNumber.Text));
                }
                else
                {
                    objectValues.Add(weekValue);
                }

                objectValues.Add(cbLocationDataEntry.SelectedItem.ToString());//Location:
                double winchill = 0;

                if (windspeed > 3 && temp > 50)
                {
                    winchill = 35.74 + (0.6215) * (temp) - (35.75) * (Math.Pow(windspeed, 0.16)) + (0.4275) * (Math.Pow(windspeed, 0.16));
                    objectValues.Add(winchill.ToString());          //Winchill:
                }
                else
                {
                    objectValues.Add("");                           //Winchill:
                }

                objectValues.Add(cbEffortRideDataEntry.SelectedItem.ToString());                    //Effort:

                if (changeType.Equals("Update"))
                {
                    objectValues.Add(recordID);                           //Record ID:
                }

                using (var results = ExecuteSimpleQueryConnection(procedureName, objectValues))
                {
                    if (results == null)
                    {
                        if (changeType.Equals("Update"))
                        {
                            MessageBox.Show("[ERROR] There was a problem updating the ride.");
                        }
                        else
                        {
                            MessageBox.Show("[ERROR] There was a problem adding the ride.");
                        }
                    }
                    else
                    {
                        if (changeType.Equals("Update"))
                        {
                            MessageBox.Show("The ride has been updated successfully.");
                        }
                        else
                        {
                            MessageBox.Show("The ride has been added successfully.");
                        }
                    }

                    return;
                }

            }
            catch (Exception ex)
            {
                Logger.LogError("[ERROR]: Exception while trying to update ride information." + ex.Message.ToString());
            }
        }

        public SqlDataReader ExecuteSimpleQueryConnection(string ProcedureName, List<object> _Parameters)
        {
            string tmpProcedureName = "EXECUTE " + ProcedureName + " ";
            SqlDataReader ToReturn = null;

            try
            {
                for (int i = 0; i < _Parameters.Count; i++)
                {
                    tmpProcedureName += "@" + i.ToString() + ",";
                }

                tmpProcedureName = tmpProcedureName.TrimEnd(',') + ";";
                ToReturn = databaseConnection.ExecuteQueryConnection(tmpProcedureName, _Parameters);

            }
            catch (Exception ex)
            {
                Logger.Log("[ERROR] SQL Query - Exception occurred:  " + ex.Message.ToString(), 0, 0);
            }

            return ToReturn;
        }

        private void RideDataEntryLoad(object sender, EventArgs e)
        {
            // NOTE: This line of code loads data into the 'cyclingLogDatabaseDataSet.Table_Ride_Information' table. You can move, or remove it, as needed.
            this.table_Ride_InformationTableAdapter.Fill(this.cyclingLogDatabaseDataSet.Table_Ride_Information);
            MainForm mainForm = new MainForm("");
            cbBikeDataEntrySelection.SelectedIndex = mainForm.GetLastBikeSelected();
            // cbLogYearDataEntry.SelectedIndex = cbRouteDataEntry.FindStringExact("");
        }

        private void RideDataEntryFormClosed(object sender, FormClosedEventArgs e)
        {
            //clearDataEntryFields();
        }

        private void ImportData(object sender, EventArgs e)
        {
            string[] headingList = new string[16];
            string[] splitList = new string[16];
            //string[] tempSplitList = new string[16];
            //string[] summary = new string[16];
            string tempStr;

            lbRideDataEntryError.Text = "";
            lbRideDataEntryError.Hide();

            // Only the Summary line is required:
            try
            {
                using (OpenFileDialog openfileDialog = new OpenFileDialog() { Filter = "CSV|*.csv", Multiselect = false })
                {
                    if (openfileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string line;
                        using (StreamReader file = new StreamReader(openfileDialog.FileName))
                        {
                            int rowCount = 0;

                            while ((line = file.ReadLine()) != null)
                            {
                                var tempList = line.Split(',');

                                if (rowCount == 0)
                                {
                                    //Line 1 is the headings
                                    headingList = line.Split(',');
                                    //MessageBox.Show(headingList[0]);
                                }
                                else if (tempList[0].Equals("Summary"))
                                {
                                    splitList = line.Split(',');
                                    //MessageBox.Show(summary[0]);
                                }
                                //else if (rowCount > 0)
                                //{
                                //    //splitList = line.Split(',');
                                //    //MessageBox.Show(splitList[9]);
                                //}
                                //else
                                //{
                                //    // split item and need to add to or avg in with the previous split
                                //    tempSplitList = line.Split(',');
                                //    //MessageBox.Show(tempSplitList[0]);
                                //}
                                rowCount++;
                            }
                        }
                    } else
                    {
                        return;
                    }
                }

                //0Data items:
                //1Time
                //2Moving Time
                //3Distance
                //4Elevation Gain
                //5Elevation Loss
                //6Avg Speed
                //7Avg Moving Speed//
                //8Max Speed
                //9Avg HR
                //10Max HR
                //11Avg Bike Cadence
                //12Max Bike Cadence
                //13Normalized power
                //14Avg Power
                //15Max Power
                //16Avg Temperature
                //17Calories

                //Split time and enter hours-min-sec
                string temp = splitList[2];

                //Check if the moving time is not entered:
                if (temp.Equals("--"))
                {
                    temp = splitList[1];
                } 

                string[] temp2 = temp.Split(':');

                if (temp2.Length == 3)
                {
                    temp2[2] = temp2[2].Split('.')[0];
                }

                //Check size to see if hours is included:
                //hh:mm:ss
                if (temp2.Length == 0)
                {
                    //MessageBox.Show("Count is 0");
                    dtpTimeRideDataEntry.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);                  
                }
                else if (temp2.Length == 1)
                {
                    //MessageBox.Show("Count is 1");
                    dtpTimeRideDataEntry.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, Int32.Parse(temp2[0]));                   
                }
                else if (temp2.Length == 2)
                {
                    //MessageBox.Show("Count is 2");
                    dtpTimeRideDataEntry.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, Int32.Parse(temp2[0]), Int32.Parse(temp2[1]));                  
                }
                else if (temp2.Length == 3)
                {
                    //MessageBox.Show("Count is 3");
                    dtpTimeRideDataEntry.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Int32.Parse(temp2[0]), Int32.Parse(temp2[1]), Int32.Parse(temp2[2]));                  
                }

                numDistanceRideDataEntry.Maximum = (200);
                numDistanceRideDataEntry.Value = System.Convert.ToDecimal(splitList[3]);                                              //4Ride Distance:

                //NOTE: Need to check if any of the values have double quotes and is so, also need to include the next index since they were split because of the comma ex("1,200"):
                //==============================================================
                //4Total Ascent:
                int colIndex = 4;
                int headingIndex = 4;
                tempStr = splitList[colIndex];
                //MessageBox.Show("value: " + tempStr[0]);
                if (tempStr[0].Equals('"'))
                {
                    //MessageBox.Show("A double quote was found");
                    tempStr = splitList[colIndex] + splitList[colIndex + 1];
                    colIndex++;
                    total_ascent.Text = tempStr.Replace("\"", "");
                }
                else
                {
                    //MessageBox.Show("No double quote found");
                    total_ascent.Text = splitList[colIndex];
                }
                colIndex++;
                headingIndex++;
                //==============================================================
                //5Total Descent:
                tempStr = splitList[colIndex];
                if (tempStr[0].Equals('"'))
                {
                    tempStr = splitList[colIndex] + splitList[colIndex + 1];
                    colIndex++;
                    total_descent.Text = tempStr.Replace("\"", "");
                }
                else
                {
                    total_descent.Text = splitList[colIndex];
                }
                colIndex++;
                headingIndex++;
                //==============================================================                                                                        
                colIndex++;
                headingIndex++;

                //Check if the Average moving speed is available:
                string avgMovingSpeed = splitList[colIndex];

                if (avgMovingSpeed.Equals("--"))
                {
                    numericUpDown1.Value = System.Convert.ToDecimal(splitList[colIndex-1]);                                     //7Average moving Speed:
                } else
                {
                    numericUpDown1.Value = System.Convert.ToDecimal(splitList[colIndex]);                                     //8Average moving Speed:
                }
                
                colIndex++;
                headingIndex++;
                max_speed.Text = splitList[colIndex];                                                                       //9Max Speed:
                colIndex++;
                headingIndex++;

                for (int index = colIndex; index < splitList.Length; index++)
                {
                    if (headingList[headingIndex].Equals("Avg HR"))
                    {
                        avg_heart_rate.Text = splitList[index];                                                              //10Average Cadence:
                    }
                    else if (headingList[headingIndex].Equals("Max HR"))
                    {
                        max_heart_rate.Text = splitList[index];                                                               //Max Heart Rate:
                    }
                    else if (headingList[headingIndex].Equals("Avg Bike Cadence"))
                    {
                        avg_cadence.Text = splitList[index];                                                                   //10Average Cadence:
                    }
                    else if (headingList[headingIndex].Equals("Avg Temperature"))
                    {
                        //Check if the avg temp is available:
                        string avgTemp = splitList[index];

                        if (avgTemp.Equals("--"))
                        {
                            numericUpDown3.Value = 72;    //12Temp:
                        } else
                        {
                            numericUpDown3.Value = decimal.Round(System.Convert.ToDecimal(splitList[index]), 2, MidpointRounding.AwayFromZero);    //12Temp:
                        }
                    }
                    else if (headingList[headingIndex].Equals("Calories"))
                    {
                        tempStr = splitList[index];
                        if (tempStr[0].Equals('"'))
                        {
                            tempStr = splitList[index] + splitList[index + 1];
                            index++;
                            calories.Text = tempStr.Replace("\"", "");
                        }
                        else
                        {
                            calories.Text = splitList[index];
                        }
                    }
                    //else if (headingList[headingIndex].Equals("Location"))
                    //{
                    //    cbLocationDataEntry.SelectedIndex = cbLocationDataEntry.Items.IndexOf(splitList[index]);                //Location:
                    //}

                    headingIndex++;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("[ERROR]: Exception while trying to retrive ride data. " + ex.Message.ToString());
                MessageBox.Show("[ERROR] Exception occurred. Refer to the log for more information. ");
            }

            tbRecordID.Text = "import";
        }

        // Used to clear the form on date changes:
        private void ClearDataEntryFields()
        {
            //Reset and clear values:
            dtpTimeRideDataEntry.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);//Moving Time:
            numDistanceRideDataEntry.Value = 0;                                                                         //Ride Distance:
            numericUpDown1.Value = 0;                                                                                   //Average Speed:
            cbBikeDataEntrySelection.SelectedIndex = cbBikeDataEntrySelection.FindStringExact(""); ;                    //Bike:
            cbRideTypeDataEntry.SelectedIndex = cbRideTypeDataEntry.FindStringExact(""); ;                              //Ride Type:
            numericUpDown4.Value = 0;                                                                                   //Wind:
            numericUpDown3.Value = 0;                                                                                   //Temp:
            //dtpRideDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0); ;     //Date:
            avg_cadence.Text = "";                                                                                      //Average Cadence:
            avg_heart_rate.Text = "";                                                                                   //Average Heart Rate:
            max_heart_rate.Text = "";                                                                                   //Max Heart Rate:
            calories.Text = "";                                                                                         //Calories:
            total_ascent.Text = "";                                                                                     //Total Ascent:
            total_descent.Text = "";                                                                                    //Total Descent:
            max_speed.Text = "";                                                                                        //Max Speed:
            avg_power.Text = "";                                                                                        //Average Power:
            max_power.Text = "";                                                                                        //Max Power:
            cbRouteDataEntry.SelectedIndex = cbRouteDataEntry.FindStringExact(""); ;                                    //Route:
            tbComments.Text = "";                                                                                       //Comments:
            //cbLogYearDataEntry.SelectedIndex = cbLogYearDataEntry.FindStringExact("");                                //LogYear index:
            cbLocationDataEntry.SelectedIndex = cbLocationDataEntry.FindStringExact("");
            cbEffortRideDataEntry.SelectedIndex = cbEffortRideDataEntry.FindStringExact("");
            tbWeekNumber.Text = "0";
            tbRecordID.Text = "0";
        }

        private void ClearDataEntryFields_click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Clearing all fields. Do you want to continue?", "Clear Fields", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                lbRideDataEntryError.Text = "";
                lbRideDataEntryError.Hide();

                ClearDataEntryFields();
            }
        }

        private void CbLogYearDataEntry_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (MainForm mainForm = new MainForm(""))
            {
                mainForm.SetLastLogSelected(cbLogYearDataEntry.SelectedIndex);
                if (cbLogYearDataEntry.SelectedIndex == -1)
                {
                    lbRideDataEntryError.Show();
                    lbRideDataEntryError.Text = "No Log Year selected.";
                }
                else
                {
                    lbRideDataEntryError.Hide();
                }
            }
        }

        private void BtUpdateRideDateEntry_Click(object sender, EventArgs e)
        {
            RideInformationChange("Update", "Ride_Information_Update");
        }

        private void BtDeleteRideDataEntry_Click(object sender, EventArgs e)
        {
            //Get ride recordID:
            string rideRecordID = tbRecordID.Text;

            DialogResult result = MessageBox.Show("Do you really want to delete the Ride and all its data?", "Delete Ride From Database", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                // conn and reader declared outside try block for visibility in finally block
                //SqlConnection conn = null;
                SqlDataReader reader = null;

                int returnValue;

                try
                {
                    // instantiate and open connection
                    //conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""\\mac\home\documents\visual studio 2015\Projects\CyclingLogApplication\CyclingLogApplication\CyclingLogDatabase.mdf"";Integrated Security=True");
                    sqlConnection.Open();

                    // 1. declare command object with parameter
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM Table_Ride_Information WHERE @Id=[Id]", sqlConnection))
                    {
                        // 2. define parameters used in command object
                        SqlParameter param = new SqlParameter
                        {
                            ParameterName = "@Id",
                            Value = rideRecordID
                        };

                        // 3. add new parameter to command object
                        cmd.Parameters.Add(param);

                        // get data stream
                        reader = cmd.ExecuteReader();
                    }

                    // write each record
                    while (reader.Read())
                    {
                        //Console.WriteLine("{0}, {1}", reader["field1"], reader["field2"]);
                        //MessageBox.Show(String.Format("{0}", reader[0]));
                        //Console.WriteLine(String.Format("{0}", reader[0]));
                        string temp = reader[0].ToString();

                        if (temp.Equals(""))
                        {
                            returnValue = 0;
                        }
                        else
                        {
                            returnValue = int.Parse(temp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("[ERROR]: Exception while trying to remove a Ride year from the database. rideRecordID:" + rideRecordID + " : " + ex.Message.ToString());
                }
                finally
                {
                    // close reader
                    if (reader != null)
                    {
                        reader.Close();
                    }

                    // close connection
                    if (sqlConnection != null)
                    {
                        sqlConnection.Close();
                    }
                }
            }
        }

        private void NumericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (formLoad == 0)
            {
                NumericUpDown num = (NumericUpDown)sender;
                if (Convert.ToInt32(num.Text) > num.Value)
                {
                    //MessageBox.Show("Value decreased");
                    GetRideData(dtpRideDate.Text, Convert.ToInt16(numericUpDown2.Value));
                }
                else
                {
                    //MessageBox.Show("Value increased");
                    GetRideData(dtpRideDate.Text, Convert.ToInt16(numericUpDown2.Value));
                }
            }
        }

        private void DtpTimeRideDataEntry_ValueChanged(object sender, EventArgs e)
        {

        }

        private void Chk1RideDataEntry_Click(object sender, EventArgs e)
        {
            DateTimePicker1_ValueChanged(sender, e);
        }
    }
}
