using Com.Cdac.Gist.Translit;
using ETPB_BALLOT_Software.models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace ETPB_BALLOT_Software
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Single Instance Implementation
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        internal static void ShowToFront(string windowName)
        {
            IntPtr firstInstance = FindWindow(null, windowName);
            ShowWindow(firstInstance, 1);
            SetForegroundWindow(firstInstance);
        }

        #endregion

        BackgroundWorker backgroundWorker1;
        SQLiteConnection sqlite_conn = new SQLiteConnection();
        SQLiteCommand sqlite_cmd;
        int ballotIdToUpdate;
        public static TransliterationProvider transProvider = null; 
        LanguageCode langCode = new LanguageCode();
        // public static string locale = null;

        public MainWindow()
        {
            InitializeComponent();
            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.DoWork += BackgroundWorker1_DoWork;
            backgroundWorker1.RunWorkerCompleted += BackgroundWorker1_RunWorkerCompleted;
        }

        private void On_Load(object sender, RoutedEventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
            Get_All_States();
            stateCombo.SelectedIndex = 0;
            Get_All_Languages();
            language1Combo.SelectedIndex = 0;
            GetAllConstuency();
            constituencyCombo.SelectedIndex = 0;
            language2Combo.SelectedIndex = 1;
            GenerateGrid();
            btnUpdate.IsEnabled = false;
        }

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {

                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    (Action) (() => { ProcessWindow.Visibility = Visibility.Visible; }));
                transProvider = new TransliterationProvider();
            }
            catch
            {
                e.Result = false;
            }
        }

        private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null && (bool)e.Result)
            {
                System.Windows.Forms.MessageBox.Show("Error while loading tranliteration module...");
            }
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,(Action)(() => { ProcessWindow.Visibility = Visibility.Collapsed; }));
        }



        private void Get_All_States()   //populate states in state comboBox
        {
            using (StreamReader streamReader = new StreamReader(Directory.GetCurrentDirectory() + "\\JSON\\state.json"))
            {
                string StateJsonPath = streamReader.ReadToEnd();
                Dictionary<string, string> itemstates = JsonConvert.DeserializeObject<Dictionary<string, string>>(StateJsonPath);
                var list = itemstates.Keys.ToList();

                stateCombo.DisplayMemberPath = "state_display";
                stateCombo.SelectedValuePath = "ST_CODE";

                stateCombo.Items.Add(new { ST_CODE = "select", state_display = "---Select State---" });
                foreach (var item in list)
                {
                    stateCombo.Items.Add(new { ST_CODE = item, state_display = itemstates[item] + "(" + item + ")" });
                }
            }
        }



        //private void SelectState_Changed(object sender, EventArgs e)
        //{
        //    var combobox = sender as ComboBox;

        //    if (combobox.SelectedValue.ToString() == "KA")
        //    {
        //        language1Combo.SelectedIndex = 1;
        //       // language1Combo.SelectedValue = "KA";

        //    }
        //    else if (combobox.SelectedValue.ToString() == "OR")
        //    {
        //        language1Combo.SelectedIndex = 2;

        //    }
        //    else if (combobox.SelectedValue.ToString() == "PB")
        //    {
        //        language1Combo.SelectedIndex = 3;
        //    }
        //    else if (combobox.SelectedValue.ToString() == "TN")
        //    {
        //        language1Combo.SelectedIndex = 4;
        //    }
        //    else if (combobox.SelectedValue.ToString() == "KL"|| combobox.SelectedValue.ToString() == "LD")
        //    {
        //        language1Combo.SelectedIndex = 5;
        //    }
        //    else if (combobox.SelectedValue.ToString() == "UP" || combobox.SelectedValue.ToString() == "BR" || combobox.SelectedValue.ToString() == "JH" || combobox.SelectedValue.ToString() == "UK" || combobox.SelectedValue.ToString() == "MP" || combobox.SelectedValue.ToString() == "HP" || combobox.SelectedValue.ToString() == "RJ" || combobox.SelectedValue.ToString() == "DL" || combobox.SelectedValue.ToString() == "HR")
        //    {
        //        language1Combo.SelectedIndex = 6;
        //    }
        //    else if (combobox.SelectedValue.ToString() == "WB" || combobox.SelectedValue.ToString() == "TR")
        //    {
        //        language1Combo.SelectedIndex = 7;
        //    }
        //    else if (combobox.SelectedValue.ToString() == "MH")
        //    {
        //        language1Combo.SelectedIndex = 8;
        //    }
        //    else if (combobox.SelectedValue.ToString() == "NL" || combobox.SelectedValue.ToString() == "ML" || combobox.SelectedValue.ToString() == "MZ" || combobox.SelectedValue.ToString() == "AR" || combobox.SelectedValue.ToString() == "SK")
        //    {
        //        language1Combo.SelectedIndex = 9;
        //    }
        //    else if (combobox.SelectedValue.ToString() == "AS")
        //    {
        //        language1Combo.SelectedIndex = 10;
        //    }
        //    else if (combobox.SelectedValue.ToString() == "AP" || combobox.SelectedValue.ToString() == "TG")
        //    {
        //        language1Combo.SelectedIndex = 11;
        //    }
        //    else if (combobox.SelectedValue.ToString() == "GJ")
        //    {
        //        language1Combo.SelectedIndex = 12;
        //    }
        //    else if (combobox.SelectedValue.ToString() == "MN")
        //    {
        //        language1Combo.SelectedIndex = 13;
        //    }
        //    else if (combobox.SelectedValue.ToString() == "GA")
        //    {
        //        language1Combo.SelectedIndex = 14;
        //    }


        //    //if (combobox.SelectedIndex > 0)
        //    //{
        //    //    locale = language1Combo.SelectedValue.ToString();
        //    //}


        //    if (language1Combo.SelectedIndex == 9)
        //    {
        //        language2Combo.Visibility = Visibility.Collapsed;
        //        lbl_language2.Visibility = Visibility.Collapsed; 
        //    }
        //    else if (language1Combo.SelectedIndex != 9)
        //    {
        //        language2Combo.Visibility = Visibility.Visible;
        //        lbl_language2.Visibility = Visibility.Visible;
        //    }

        //}

        private void Get_All_Languages()   // populate languages in language comboBox
        {
            using (StreamReader streamReader = new StreamReader(Directory.GetCurrentDirectory() + "\\JSON\\language.json"))
            {
                string StateJsonPath = streamReader.ReadToEnd();
                Dictionary<string, string> itemstates = JsonConvert.DeserializeObject<Dictionary<string, string>>(StateJsonPath);
                var list = itemstates.Keys.ToList();

                language1Combo.DisplayMemberPath = "language_display";
                language1Combo.SelectedValuePath = "Lang_CODE";

                language1Combo.Items.Add(new { Lang_CODE = "select", language_display = "---Select Language---" });
                foreach (var item in list)
                {
                    language1Combo.Items.Add(new { Lang_CODE = item, language_display = itemstates[item] });
                }
            }

            //DataTable dt = new DataTable();
            //dt.Columns.Add("LanguageType");
            //dt.Columns.Add("LanguageTypeText");

            //DataRow row1 = dt.NewRow();
            //row1["LanguageType"] = "Kannada";
            //row1["LanguageTypeText"] = "ಕನ್ನಡ/Kannada";
            //dt.Rows.Add(row1);

            //DataRow row2 = dt.NewRow();
            //row2["LanguageType"] = "Oriya";
            //row2["LanguageTypeText"] = "ଓଡ଼ିଆ /Oriya";
            //dt.Rows.Add(row2);

            //DataRow row3 = dt.NewRow();
            //row3["LanguageType"] = "Punjabi";
            //row3["LanguageTypeText"] = "ਪੰਜਾਬੀ/Punjabi";
            //dt.Rows.Add(row3);



            //DataRow row4 = dt.NewRow();
            //row4["LanguageType"] = "Tamil";
            //row4["LanguageTypeText"] = "தமிழ்/Tamil";
            //dt.Rows.Add(row4);

            //DataRow row5 = dt.NewRow();
            //row5["LanguageType"] = "Malayalam";
            //row5["LanguageTypeText"] = "മലയാളം/Malayalam";
            //dt.Rows.Add(row5);

            //DataRow row6 = dt.NewRow();
            //row6["LanguageType"] = "Hindi";
            //row6["LanguageTypeText"] = "हिन्दी/Hindi";
            //dt.Rows.Add(row6);

            //DataRow row7 = dt.NewRow();
            //row7["LanguageType"] = "Bengali";
            //row7["LanguageTypeText"] = "বাংলা/Bengali";
            //dt.Rows.Add(row7);

            //DataRow row8 = dt.NewRow();
            //row8["LanguageType"] = "Marathi";
            //row8["LanguageTypeText"] = "मराठी/Marathi";
            //dt.Rows.Add(row8);

            //DataRow row9 = dt.NewRow();
            //row9["LanguageType"] = "English";
            //row9["LanguageTypeText"] = "English";
            //dt.Rows.Add(row9);


            //DataRow dr;
            //dr = dt.NewRow();
            //dr.ItemArray = new object[] { 0, "---Select Language---" };
            //dt.Rows.InsertAt(dr, 0);
            //language1Combo.DisplayMemberPath = "LanguageTypeText";
            //language1Combo.SelectedValuePath = "LanguageType";
            //language1Combo.ItemsSource = dt.DefaultView;

        }

        private void GetAllConstuency()  //populate constituencies in constituency comboBox
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ConstituencyType");
            dt.Columns.Add("ConstituencyText");

            DataRow row1 = dt.NewRow();
            row1["ConstituencyType"] = "A";
            row1["ConstituencyText"] = "Assembly Constituency";
            dt.Rows.Add(row1);

            DataRow row2 = dt.NewRow();
            row2["ConstituencyType"] = "P";
            row2["ConstituencyText"] = "Parlimentary Constituency";
            dt.Rows.Add(row2);

            DataRow dr = dt.NewRow();
            dr.ItemArray = new object[] { 0, "---Select Constituency---" };
            dt.Rows.InsertAt(dr, 0);
            constituencyCombo.DisplayMemberPath = "ConstituencyText";
            constituencyCombo.SelectedValuePath = "ConstituencyType";
            constituencyCombo.ItemsSource = dt.DefaultView;
        }

        private void GenerateGrid()  // retrieves all record from MASTERBALLOT table and binds to datagrid
        {
            var list = new List<BallotData>();

            // dgBallotMaster.ItemsSource = null;

            sqlite_conn = SqLite.OpenSQLLiteConnection(sqlite_conn);
            DataTable dt;
            using (SQLiteDataAdapter Sqa = new SQLiteDataAdapter("Select BALLOT_ID , " + " CASE WHEN  ELECTION_FOR ='P' then 'Parliamentary'  " + " when ELECTION_FOR ='A' then 'Assembly' " + " end ELECTION_FOR ,  " + "STATE_CODE ,LANGUAGE_1 , LANGUAGE_2 , " + " CASE WHEN IS_FINALIZED ='N'  then 'No' else 'Yes' end IS_FINALIZED " + " , FINALIZATION_DATE from " + " MASTERBALLOT", sqlite_conn))
            {
                dt = new DataTable();
                Sqa.Fill(dt);  // fill the dataset
            }
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new BallotData()
                {
                    BallotID = Convert.ToInt32(row["BALLOT_ID"]),
                    state = row["STATE_CODE"].ToString(),
                    constituency = row["ELECTION_FOR"].ToString(),
                    language1 = row["LANGUAGE_1"].ToString(),
                    language2 = row["LANGUAGE_2"].ToString(),
                    IsFinalized = row["IS_FINALIZED"].ToString(),
                    dateOfFinalization = row["FINALIZATION_DATE"].ToString()
                });

            }
            dgBallotMaster.ItemsSource = list;
            SqLite.CloseSQLLiteConnection(sqlite_conn);
        }

        private int GetBallotId() // gets incremented ballot id to insert new record in MASTERBALLOT table
        {
            sqlite_conn = SqLite.OpenSQLLiteConnection(sqlite_conn);
            string commandstring = "select  COALESCE(max(ballot_id) ,0)   from MASTERBALLOT";
            sqlite_cmd = sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = commandstring;
            object objMax = sqlite_cmd.ExecuteScalar();
            int cntMax = Convert.ToInt32(objMax) + 1;
            return cntMax;
        }

        private void btnMasterDetails_Click(object sender, RoutedEventArgs e)
        {
            if (constituencyCombo.SelectedIndex <= 0 || stateCombo.SelectedIndex <= 0 || language1Combo.SelectedIndex <= 0 || language2Combo.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select all fields.");
            }
            else
            {
                object state = stateCombo.SelectedItem?.GetType().GetProperty("state_display")?.GetValue(stateCombo.SelectedItem, null);
                // string state = null;
                string lang2 = (string)((ComboBoxItem)language2Combo.SelectedValue).Content;

                int ballotIdToInsert = GetBallotId();
                sqlite_conn = SqLite.OpenSQLLiteConnection(sqlite_conn);
                sqlite_cmd = sqlite_conn.CreateCommand();
                // string commandstring = "Insert into MASTERBALLOT " + "( BALLOT_ID ,ELECTION_FOR,STATE_CODE,LANGUAGE_1,LANGUAGE_2,IS_FINALIZED) VALUES ( " + ballotIdToInsert + ", '" + constituencyCombo.SelectedValue.ToString() + "','" + state.ToString() + "("+ stateCombo.SelectedValue.ToString()+")','" + language1Combo.SelectedValue.ToString() + "','" + lang2 + "','N')";
                string commandstring = "Insert into MASTERBALLOT " + "( BALLOT_ID ,ELECTION_FOR,STATE_CODE,LANGUAGE_1,LANGUAGE_2,IS_FINALIZED) VALUES ( " + ballotIdToInsert + ", '" + constituencyCombo.SelectedValue.ToString() + "','" + state.ToString() + "','" + language1Combo.SelectedValue.ToString() + "','" + lang2 + "','N')";


                sqlite_cmd.CommandText = commandstring;

                int result = sqlite_cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    //MessageBox.Show("Master record inserted");
                    GenerateGrid();
                    //DisableAddBallot();
                    Reset();
                }

                else
                {
                    MessageBox.Show("Master record insertion failed");
                }


                sqlite_cmd.Dispose();
                SqLite.CloseSQLLiteConnection(sqlite_conn);
                this.AddMasterBallot.Visibility = System.Windows.Visibility.Hidden;


            }


        }

        private void Reset()  // clears all controls except datagrid on main window
        {
            constituencyCombo.SelectedIndex = 0;
            // language2Combo.SelectedIndex = 0;
            language1Combo.SelectedIndex = 0;
            stateCombo.SelectedIndex = 0;
            btnMasterDetails.IsEnabled = true;
            btnUpdate.IsEnabled = false;
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            this.AddMasterBallot.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (constituencyCombo.SelectedIndex <= 0 || stateCombo.SelectedIndex <= 0 || language1Combo.SelectedIndex <= 0 || language2Combo.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select record from table to update.");
                Reset();
            }
            else
            {
                object state = stateCombo.SelectedItem?.GetType().GetProperty("state_display")?.GetValue(stateCombo.SelectedItem, null);
                string empty = "";
                string lang2 = (string)((ComboBoxItem)language2Combo.SelectedValue).Content;
                sqlite_conn = SqLite.OpenSQLLiteConnection(sqlite_conn);
                sqlite_cmd = sqlite_conn.CreateCommand();
                string commandstring = "Update MASTERBALLOT set ELECTION_FOR='" + constituencyCombo.SelectedValue.ToString() + "',STATE_CODE='" +
                                   state.ToString() + "',LANGUAGE_1='" +
                                   language1Combo.SelectedValue.ToString() + "',LANGUAGE_2='" + lang2 + "',IS_FINALIZED ='N',FINALIZATION_DATE='" + empty + "' where BALLOT_ID =" + ballotIdToUpdate;
                sqlite_cmd.CommandText = commandstring;
                int n = sqlite_cmd.ExecuteNonQuery();
                if (n > 0)
                {
                    MessageBox.Show("Master record Updated");
                    GenerateGrid();
                    Reset();
                }

                else
                {
                    MessageBox.Show("Master record Update failed");

                }

                sqlite_cmd.Dispose();
                SqLite.CloseSQLLiteConnection(sqlite_conn);

            }
            btnMasterDetails.IsEnabled = true;
            btnUpdate.IsEnabled = false;
        }

        private void hyperLink_click(object sender, RoutedEventArgs e)
        {

            this.AddMasterBallot.Visibility = System.Windows.Visibility.Visible;
        }

        private void dgBallotMaster_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)   //selected cell event to perform edit delete and add candidate details
        {
            //SelectedCellsChanged="dgBallotMaster_SelectedCellsChanged" in xaml
            string selectedColumnHeader;
            try
            {
                selectedColumnHeader = (string)dgBallotMaster.SelectedCells[0].Column.Header;   // getting header of selected cell in datgrid
            }
            catch
            {
                return;
            }
            // var currentRowIndex = dgBallotMaster.Items.IndexOf(dgBallotMaster.CurrentItem);
            BallotData ballotData = (BallotData)dgBallotMaster.CurrentItem;  // getting all ballot data as class object of selected row
                                                                             //  BallotData ballotData = ((FrameworkElement)sender).DataContext as BallotData;  // for button click event
                                                                             // code to delete record
            if (selectedColumnHeader == "Delete")
            {

                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to permanently delete this record?", "Delete record", MessageBoxButton.YesNo, MessageBoxImage.Question);
                //  MessageBoxResult messageBoxResult1 = MessageBox.Show("Are you sure you want to permanently delete this record?", "Delete record",me)
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    int ballotId = ballotData.BallotID;
                    deleteRow(ballotId);
                }
                else if (messageBoxResult == MessageBoxResult.No)
                {
                    return;
                }

            }

            // code to edit record
            if (selectedColumnHeader == "Edit")
            {
                btnUpdate.IsEnabled = true;
                btnMasterDetails.IsEnabled = false;
                if (this.AddMasterBallot.Visibility == Visibility.Hidden)
                {
                    this.AddMasterBallot.Visibility = Visibility.Visible;
                }
                ballotIdToUpdate = ballotData.BallotID;
                if (ballotData.constituency == "Assembly")
                {
                    constituencyCombo.SelectedIndex = 1;
                }
                language2Combo.SelectedIndex = 1;
                stateCombo.SelectedValue = ballotData.state;
                language1Combo.SelectedValue = ballotData.language1;

            }

            // code to open Ballot window
            if (selectedColumnHeader == "Candidate Details")
            {
                BallotWindow ballotWindow = new BallotWindow(ballotData);
                ballotWindow.ShowDialog();
            }

        }

        private void deleteRow(int ballotId)  //gets called in  dgBallotMaster_SelectedCellsChanged
        {
            sqlite_conn = SqLite.OpenSQLLiteConnection(sqlite_conn);
            string cmdDeletestring = "Delete from MASTERBALLOT where Ballot_ID = " + ballotId + " ;";
            sqlite_cmd = sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = cmdDeletestring;
            int n = sqlite_cmd.ExecuteNonQuery();
            if (n > 0)
            {
                MessageBox.Show("Record Deleted");
                GenerateGrid();
            }
            SqLite.CloseSQLLiteConnection(sqlite_conn);
        }

        private void FnFormsDatagridLoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void btn_edit_Click(object sender, RoutedEventArgs e)
        {


            BallotData ballotData = ((FrameworkElement)sender).DataContext as BallotData;
            btnUpdate.IsEnabled = true;
            btnMasterDetails.IsEnabled = false;
            if (this.AddMasterBallot.Visibility == Visibility.Hidden)
            {
                this.AddMasterBallot.Visibility = Visibility.Visible;
            }
            if (ballotData != null)
            {
                ballotIdToUpdate = ballotData.BallotID;
                if (ballotData.constituency == "Assembly")
                {
                    constituencyCombo.SelectedIndex = 1;
                }
                language2Combo.SelectedIndex = 1;
                string st = ballotData.state;
                string stt = null;
                string regularExpressionPattern = @"\((.*?)\)";
                Regex re = new Regex(regularExpressionPattern);

                foreach (Match m in re.Matches(st))
                {
                    stt = m.Value;
                }
                if (stt != null) stateCombo.SelectedValue = stt.TrimStart('(').TrimEnd(')');
                //stateCombo.SelectedValue = ballotData.state;
                language1Combo.SelectedValue = ballotData.language1;
            }
        }

        private void btn_delete_Click(object sender, RoutedEventArgs e)
        {
            BallotData ballotData = ((FrameworkElement)sender).DataContext as BallotData;
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to permanently delete this record?", "Delete record", MessageBoxButton.YesNo, MessageBoxImage.Question);
            //  MessageBoxResult messageBoxResult1 = MessageBox.Show("Are you sure you want to permanently delete this record?", "Delete record",me)
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                if (ballotData != null)
                {
                    int ballotId = ballotData.BallotID;
                    deleteRow(ballotId);
                }
            }
            else if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }
        }

        private void btn_candidate_details_Click(object sender, RoutedEventArgs e)
        {
            BallotData ballotData = ((FrameworkElement)sender).DataContext as BallotData;
            BallotWindow ballotWindow = new BallotWindow(ballotData);
            ballotWindow.ShowDialog();
        }

        private void stateCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string StateCode = stateCombo.SelectedValue.ToString();
            //Dictionary<string, string> LangCode = langCode.StateLanguageDict;
            //foreach (var item in LangCode)
            //{
            //    if (item.Key.ToString() == StateCode)
            //    {
            //        language1Combo.SelectedValue= item.Value.ToString();
            //    }

            //}
            // Added by vijay 
            if (langCode.StateLanguageDict.ContainsKey(StateCode))
                language1Combo.SelectedValue = langCode.StateLanguageDict[StateCode];
        }

        private void language1Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object state = language1Combo.SelectedItem?.GetType().GetProperty("language_display")?.GetValue(language1Combo.SelectedItem, null);
            if (state != null && state.ToString() == "English")
            {
                language2Combo.Visibility = Visibility.Hidden;
                lbl_language2.Visibility = Visibility.Hidden;
            }
            else
            {
                language2Combo.Visibility = Visibility.Visible;
                lbl_language2.Visibility = Visibility.Visible;
            }
          
        }
    }
}
