
using CandidateDetails;
using Com.Cdac.Gist.Translit;
using ETPB_BALLOT_Software.models;
using Microsoft.Win32;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
//using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ETPB_BALLOT_Software
{
    /// <summary>
    /// Interaction logic for BallotWindow.xaml
    /// </summary>
    public partial class BallotWindow : Window
    {
        private SQLiteConnection sqlite_conn = new SQLiteConnection();
        private SQLiteCommand sqlite_cmd;
        private TransliterationProvider transProvider = MainWindow.transProvider;
        private LanguageCode langCode = new LanguageCode();
        private List<string> fonts = new List<string>();
        public static  string selectedfont = string.Empty;
        public static bool isChanged = false;
        private string Locale = null;
        public static int BallotId;
        private byte[] CandPhoto = null;
        private byte[] NotaPhoto = null;
        private int detailballotId;
        public static string strConstType;
        public string lang_Official;
        private CandidateDetails.BallotCandidateList ballotCandidateList = new BallotCandidateList();
        private Dictionary<int, CandidateRecord> TTfileDict = new Dictionary<int, CandidateRecord>();


        public BallotWindow(BallotData ballotdata)
        {
            InitializeComponent();
            lblYear.Content = Convert.ToInt32(System.DateTime.Now.Year).ToString();
            nameConstituency.Content = ballotdata.constituency;
            lblState.Content = ballotdata.state;
            lblLanguage.Content = ballotdata.language1;

            BallotId = ballotdata.BallotID;
            strConstType = ballotdata.constituency;
            lang_Official = ballotdata.language1;


            string stateString = ballotdata.state;
            string state = null;
            string regularExpressionPattern = @"\((.*?)\)";
            Regex regex = new Regex(regularExpressionPattern);

            foreach (Match match in regex.Matches(stateString))
            {
                state = match.Value;
            }
            string stateID = state.TrimStart('(').TrimEnd(')');

            //added by vijay 
            //get appropriate locale regardless of user choice language
            //Locale = langCode.LanguageDict[stateID];

            //get user choice language locale
            Locale = langCode.LangMapedWithLocaleDict[lang_Official];
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            if (lang_Official == "English")
            {
                //txt_RegionalName.IsEnabled = false;
                //txt_RegionalPartyName.IsEnabled = false;
                txt_RegionalName.Visibility = Visibility.Hidden;
                txt_RegionalPartyName.Visibility = Visibility.Hidden;
                txt_RegionalName.Text = null;
                txt_RegionalPartyName.Text = null;
                //dgCandidate.Columns[3].Visibility = Visibility.Hidden;
                //dgCandidate.Columns[5].Visibility = Visibility.Hidden;
                dgCandidate.Columns.FirstOrDefault(x => (string)x.Header == "Candidate Name(Official)").Visibility = Visibility.Hidden;
                dgCandidate.Columns.FirstOrDefault(x => (string)x.Header == "Party Affiliation(Official)").Visibility = Visibility.Hidden;
            }
            GenerateGrid();
            btn_Update.IsEnabled = false;

            foreach (System.Windows.Media.FontFamily fontFamily in Fonts.SystemFontFamilies)
            {
                cmbfont.Items.Add(fontFamily.Source);
            }
            cmbfont.SelectedItem = "SakalBharati";

        }

        private void GenerateGrid()  // retrieves all record from BALLOTDETAILS table and binds to datagrid
        {
            List<CandidateRecord> list = new List<CandidateRecord>();
            sqlite_conn = SqLite.OpenSQLLiteConnection(sqlite_conn);
            SQLiteDataAdapter Sqa = new SQLiteDataAdapter("select * from BALLOTDETAILS where BALLOTID= '" + BallotId + "' ", sqlite_conn);
            DataTable dt = new DataTable();
            Sqa.Fill(dt);  // fill the dataset

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new CandidateRecord()
                {
                    ISNOTA = Convert.ToInt32(row["ISNOTA"]),
                    DetailBallotID = Convert.ToInt32(row["DETAILBALLOTID"]),
                    BallotID = Convert.ToInt32(row["BALLOTID"]),
                    CandidateNO = Convert.ToInt32(row["CANDIDATESLNO"]),
                    CandidateNameENG = (row["CANDIDATENAMEENG"] != DBNull.Value) ? row["CANDIDATENAMEENG"].ToString() : null,
                    CandidateNameOL = (row["CANDIDATENAMEOL"] != DBNull.Value) ? row["CANDIDATENAMEOL"].ToString() : null,
                    PartyNameENG = (row["PARTYAFFILIATIONENG"] != DBNull.Value) ? row["PARTYAFFILIATIONENG"].ToString() : null,
                    PartyNameOL = (row["PARTYAFFILIATIONOL"] != DBNull.Value) ? row["PARTYAFFILIATIONOL"].ToString() : null,
                    CandidatePhoto = (row["CANDIDATEPHOTO"] != DBNull.Value) ? (byte[])row["CANDIDATEPHOTO"] : null,
                });
            }
            dgCandidate.ItemsSource = list;
            CandidateRecordList.CandidateRecords.Clear();
            CandidateRecordList.CandidateRecords = list;

            //check id Nota is last Record
            if (dt.Rows.Count > 0)
            {
                DataRow lastRow = dt.Rows[dt.Rows.Count - 1];
                if (lastRow["ISNOTA"] != DBNull.Value && Convert.ToInt32(lastRow["ISNOTA"]) == 1)
                {
                    IfNotaDoThis();
                }
                else
                {
                    Reset();
                }
            }
            SqLite.CloseSQLLiteConnection(sqlite_conn);
        }
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {

            string filename = null;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {

                //dlg.Filter = "Images (*.BMP;*.JPG;*.GIF,*.PNG,*.TIFF)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF|" + "All files (*.*)|*.*";
                Filter = "Image Files(*.jpg; *.jpeg; *.png; *.bmp)| *.jpg; *.jpeg; *.png; *.bmp",
                Title = "Select a file"
            };  // Create OpenFileDialog

            Nullable<bool> result = dlg.ShowDialog();  // Display OpenFileDialog by calling ShowDialog method

            if (result == true)
            {
                filename = dlg.FileName;
                CandPhoto = File.ReadAllBytes(filename);
                BitmapImage bmpImage = new BitmapImage();
                bmpImage.BeginInit();
                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                bmpImage.UriSource = new Uri(filename);
                bmpImage.DecodePixelHeight = 76;
                bmpImage.DecodePixelWidth = 94;
                bmpImage.EndInit();
                photo_img.Source = bmpImage;

            }

        }

        private void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            if (Locale != "" && textbox?.Text != string.Empty)
            {
                switch (textbox?.Name)
                {
                    case "txt_EnglishName":
                        txt_RegionalName.Text = transProvider.Transliterate(textbox.Text, Locale, TransliterationHints.NAME);
                        break;
                    case "txt_EnglishPartyName":
                        txt_RegionalPartyName.Text = transProvider.Transliterate(textbox.Text, Locale, TransliterationHints.NAME);
                        break;
                }
            }
        }

        private void btn_Submit_Click(object sender, RoutedEventArgs e)
        {
            /*1.check all fields are property filled
              2.Insert record 

                  A. If State language is english set candidate and party regional name to null 
                  B. Else fill all the requird fields

                  check all the conditions like
                  C. Candidate have photo or not
                      Set candidate photo = null;
                  D. check if current record is Nota record or another record
                      After nota added Disable record submision and enable correction of serial number and save ballot button
                  
             */
            string errorMessage = IsEmptyOrNotMethod();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show("Please fill following fields.....\n" + errorMessage);
            }
            else
            {
                int detailBallotIdToInsert = GetDetailBallotId();
                int candidateNoToInsert = GetCandidateNo();

                sqlite_conn = SqLite.OpenSQLLiteConnection(sqlite_conn);
                sqlite_cmd = sqlite_conn.CreateCommand();

                string commandstring = "INSERT INTO BALLOTDETAILS (DETAILBALLOTID,BALLOTID ,CANDIDATESLNO,CANDIDATENAMEENG,CANDIDATENAMEOL,PARTYAFFILIATIONENG,PARTYAFFILIATIONOL,CANDIDATEPHOTO,ISNOTA ) VALUES (@DETAILBALLOTID ,@BALLOTID ,@CANDIDATESLNO,@CANDIDATENAMEENG,@CANDIDATENAMEOL,@PARTYAFFILIATIONENG,@PARTYAFFILIATIONOL,@CANDIDATEPHOTO,@ISNOTA)";

                if (lang_Official == "English")
                {
                    #region For English

                    if (chkNotaBox.IsChecked == true)
                    {
                        //if anyobe trying to add NOTA as 1st record --vijay
                        if (candidateNoToInsert == 1)
                        {
                            System.Windows.Forms.MessageBox.Show("You cannot at NOTA as 1st record..!", "Warning");
                            Reset();
                        }
                        else
                        {
                            using (sqlite_cmd = new SQLiteCommand(commandstring, sqlite_conn))
                            {
                                sqlite_cmd.Parameters.AddWithValue("DETAILBALLOTID", detailBallotIdToInsert);
                                sqlite_cmd.Parameters.AddWithValue("BALLOTID", BallotId);
                                sqlite_cmd.Parameters.AddWithValue("CANDIDATESLNO", candidateNoToInsert);
                                sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEENG", txt_EnglishName.Text.ToString().Replace("'", "''"));  // replace method in case ' in name eg. D'souza
                                sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEOL", null);
                                sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONENG", null);
                                sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONOL", null);
                                sqlite_cmd.Parameters.AddWithValue("CANDIDATEPHOTO", NotaPhoto);
                                sqlite_cmd.Parameters.AddWithValue("ISNOTA", 1);

                                int result = sqlite_cmd.ExecuteNonQuery();
                                if (result > 0)
                                {
                                    GenerateGrid();
                                }
                                else
                                {
                                    MessageBox.Show("Ballot Details insertion failed");
                                }

                            }
                        }
                    }
                    else if (chkNoPhoto.IsChecked == true)
                    {

                        using (sqlite_cmd = new SQLiteCommand(commandstring, sqlite_conn))
                        {
                            sqlite_cmd.Parameters.AddWithValue("DETAILBALLOTID", detailBallotIdToInsert);
                            sqlite_cmd.Parameters.AddWithValue("BALLOTID", BallotId);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATESLNO", candidateNoToInsert);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEENG", txt_EnglishName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEOL", null);
                            sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONENG", txt_EnglishPartyName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONOL", null);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATEPHOTO", null);
                            sqlite_cmd.Parameters.AddWithValue("ISNOTA", 0);

                            int result = sqlite_cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                // MessageBox.Show("Ballot Details inserted");
                                GenerateGrid();
                                // Reset();
                            }

                            else
                            {
                                MessageBox.Show("Ballot Details insertion failed");
                            }

                        }

                    }
                    else
                    {
                        using (sqlite_cmd = new SQLiteCommand(commandstring, sqlite_conn))
                        {
                            sqlite_cmd.Parameters.AddWithValue("DETAILBALLOTID", detailBallotIdToInsert);
                            sqlite_cmd.Parameters.AddWithValue("BALLOTID", BallotId);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATESLNO", candidateNoToInsert);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEENG", txt_EnglishName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEOL", null);
                            sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONENG", txt_EnglishPartyName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONOL", null);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATEPHOTO", CandPhoto);
                            sqlite_cmd.Parameters.AddWithValue("ISNOTA", 0);

                            int result = sqlite_cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                //MessageBox.Show("Ballot Details inserted");
                                GenerateGrid();
                                // Reset();
                            }

                            else
                            {
                                MessageBox.Show("Ballot Details insertion failed");
                            }

                        }
                    }

                    #endregion
                }
                else
                {
                    #region For All
                    if (chkNotaBox.IsChecked == true)
                    {
                        //Try to insert NOTA as 1st record   --vijay
                        if (candidateNoToInsert == 1)
                        {
                            System.Windows.Forms.MessageBox.Show("You cannot at NOTA as 1st record..!", "Warning");
                            Reset();
                            return;

                        }
                        else
                        {
                            using (sqlite_cmd = new SQLiteCommand(commandstring, sqlite_conn))
                            {
                                sqlite_cmd.Parameters.AddWithValue("DETAILBALLOTID", detailBallotIdToInsert);
                                sqlite_cmd.Parameters.AddWithValue("BALLOTID", BallotId);
                                sqlite_cmd.Parameters.AddWithValue("CANDIDATESLNO", candidateNoToInsert);
                                sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEENG", txt_EnglishName.Text.ToString().Replace("'", "''"));  // replace method in case ' in name eg. D'souza
                                sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEOL", txt_RegionalName.Text.ToString().Replace("'", "''"));
                                sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONENG", txt_EnglishPartyName.Text.ToString().Replace("'", "''"));
                                sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONOL", txt_RegionalPartyName.Text.ToString().Replace("'", "''"));
                                sqlite_cmd.Parameters.AddWithValue("CANDIDATEPHOTO", NotaPhoto);
                                sqlite_cmd.Parameters.AddWithValue("ISNOTA", 1);

                                int result = sqlite_cmd.ExecuteNonQuery();
                                if (result > 0)
                                {
                                    GenerateGrid();
                                }
                                else
                                {
                                    MessageBox.Show("Ballot Details insertion failed");
                                }

                            }
                        }
                    }
                    else if (chkNoPhoto.IsChecked == true)
                    {

                        using (sqlite_cmd = new SQLiteCommand(commandstring, sqlite_conn))
                        {
                            sqlite_cmd.Parameters.AddWithValue("DETAILBALLOTID", detailBallotIdToInsert);
                            sqlite_cmd.Parameters.AddWithValue("BALLOTID", BallotId);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATESLNO", candidateNoToInsert);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEENG", txt_EnglishName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEOL", txt_RegionalName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONENG", txt_EnglishPartyName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONOL", txt_RegionalPartyName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATEPHOTO", null);
                            sqlite_cmd.Parameters.AddWithValue("ISNOTA", 0);

                            int result = sqlite_cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                //MessageBox.Show("Ballot Details inserted");
                                GenerateGrid();
                                // Reset();
                            }

                            else
                            {
                                MessageBox.Show("Ballot Details insertion failed");
                            }

                        }

                    }
                    else
                    {
                        using (sqlite_cmd = new SQLiteCommand(commandstring, sqlite_conn))
                        {
                            sqlite_cmd.Parameters.AddWithValue("DETAILBALLOTID", detailBallotIdToInsert);
                            sqlite_cmd.Parameters.AddWithValue("BALLOTID", BallotId);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATESLNO", candidateNoToInsert);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEENG", txt_EnglishName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEOL", txt_RegionalName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONENG", txt_EnglishPartyName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONOL", txt_RegionalPartyName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATEPHOTO", CandPhoto);
                            sqlite_cmd.Parameters.AddWithValue("ISNOTA", 0);

                            int result = sqlite_cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                // MessageBox.Show("Ballot Details inserted");
                                GenerateGrid();
                                //  Reset();
                            }

                            else
                            {
                                MessageBox.Show("Ballot Details insertion failed");
                            }

                        }
                    }
                    #endregion
                }

                sqlite_cmd.Dispose();
                SqLite.CloseSQLLiteConnection(sqlite_conn);
            }

        }

        private int GetDetailBallotId() // gets incremented DETAILBALLOTID to insert new record in BALLOTDETAILS table
        {
            sqlite_conn = SqLite.OpenSQLLiteConnection(sqlite_conn);
            string commandstring = "select  COALESCE(max(DETAILBALLOTID),0)   from BALLOTDETAILS";
            sqlite_cmd = sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = commandstring;
            object objMax = sqlite_cmd.ExecuteScalar();
            int cntMax = Convert.ToInt32(objMax) + 1;
            return cntMax;
        }

        private int GetCandidateNo() // gets incremented CANDIDATESLNO to insert new record in BALLOTDETAILS table
        {
            sqlite_conn = SqLite.OpenSQLLiteConnection(sqlite_conn);
            string commandstring = "select  COALESCE(max(CANDIDATESLNO),0)   from BALLOTDETAILS where BALLOTID='" + BallotId + "' ";
            sqlite_cmd = sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = commandstring;
            object objMax = sqlite_cmd.ExecuteScalar();
            int cntMax = Convert.ToInt32(objMax) + 1;
            return cntMax;
        }


        private void dgCandidate_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            //SelectedCellsChanged="dgCandidate_SelectedCellsChanged" in xaml
            string selectedColumnHeader;
            try
            {
                selectedColumnHeader = (string)dgCandidate.SelectedCells[0].Column.Header;   // getting header of selected cell in datgrid
            }
            catch
            {
                return;
            }

            CandidateRecord candidateRecord = (CandidateRecord)dgCandidate.CurrentItem;  // getting all candidate data as class object of selected row

            detailballotId = candidateRecord.DetailBallotID;

            // code to delete record
            if (selectedColumnHeader == "Delete")
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to permanently delete this record?", "Delete record", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.Yes)
                {

                    deleteRow(detailballotId);
                }
                else if (messageBoxResult == MessageBoxResult.No)
                {
                    return;
                }

            }

            // code to edit record
            if (selectedColumnHeader == "Edit")
            {
                btn_Update.IsEnabled = true;
                btn_Submit.IsEnabled = false;
                txt_EnglishName.Text = candidateRecord.CandidateNameENG;
                txt_RegionalName.Text = candidateRecord.CandidateNameOL;
                txt_EnglishPartyName.Text = candidateRecord.PartyNameENG;
                txt_RegionalPartyName.Text = candidateRecord.PartyNameOL;
                photo_img.Source = LoadImage(candidateRecord.CandidatePhoto);
                photo_img.Stretch = Stretch.UniformToFill;

            }


        }

        private void deleteRow(int detailballotId)  //gets called in  dgBallotMaster_SelectedCellsChanged
        {
            sqlite_conn = SqLite.OpenSQLLiteConnection(sqlite_conn);
            string cmdDeletestring = "Delete from BALLOTDETAILS where DETAILBALLOTID = '" + detailballotId + "' ";
            sqlite_cmd = sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = cmdDeletestring;
            int n = sqlite_cmd.ExecuteNonQuery();
            if (n <= 0)
            {
                MessageBox.Show("Failed to delete Record...!");
            }
            GenerateGrid();
            SqLite.CloseSQLLiteConnection(sqlite_conn);
        }

        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
            {
                return null;
            }

            BitmapImage image = new BitmapImage();
            using (MemoryStream mem = new MemoryStream(imageData))
            {


                image.BeginInit();
                //image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.DecodePixelHeight = 76;
                image.DecodePixelWidth = 94;
                image.StreamSource = mem;
                image.EndInit();


                //mem.Position = 0;
                //image.BeginInit();
                //image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                //image.CacheOption = BitmapCacheOption.OnLoad;
                //image.UriSource = null;
                //image.StreamSource = mem;
                //image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private byte[] ImageToBytes(JpegBitmapEncoder encoder, ImageSource imageIn)
        {
            byte[] bytes = null;
            BitmapSource bitmapSource = imageIn as BitmapSource;

            if (bitmapSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (MemoryStream stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }

            return bytes;

        }



        private void Reset()  // clears all controls except datagrid on main window
        {
            //clear fields
            txt_EnglishName.Text = string.Empty;
            txt_RegionalName.Text = string.Empty;
            txt_EnglishPartyName.Text = string.Empty;
            txt_RegionalPartyName.Text = string.Empty;
            photo_img.Source = null;

            //unchecked field
            chkNotaBox.IsChecked = false;
            chkNoPhoto.IsChecked = false;

            //enabled 
            chkNotaBox.IsEnabled = true;
            btn_Submit.IsEnabled = true;
            btn_reset.IsEnabled = true;
            btnBrowse.IsEnabled = true;

            //disabled
            chkForm7a.IsEnabled = false;
            btn_Update.IsEnabled = false;
            btnSaveBallot.IsEnabled = false;
            btnPreview.IsEnabled = false;

        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo("Chrome.exe", e.Uri.AbsoluteUri));
            e.Handled = true;
        }


        //structure used for keyboard


        private void Keyboard_Appearance_Method(object sender, RoutedEventArgs e)
        {
            try
            {

                string fullpath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\C-DAC\\OT-TOOL\\OT-TYPING-TOOL\\OT TypingTool.exe";

                Process[] pname = Process.GetProcessesByName("OT TypingTool");

                if (pname.Length == 0)
                {

                    System.Diagnostics.Process.Start(fullpath);

                }

                else
                {

                    MessageBox.Show("Alredy Open in System task bar.");

                }

            }

            catch
            {

                MessageBox.Show("Install First then try.");

            }
        }

        private void FnFormsDatagridLoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (chkForm7a.IsChecked != true)
            {
                e.Row.Header = (e.Row.GetIndex() + 1).ToString();
            }
            else
            {
                e.Row.Header = null;
            }
        }



        private void btnSaveBallot_Click(object sender, RoutedEventArgs e)
        {
            TTfileDict.Clear();
            sqlite_conn = SqLite.OpenSQLLiteConnection(sqlite_conn);
            sqlite_cmd = sqlite_conn.CreateCommand();
            string commandstring = "SELECT * FROM BALLOTDETAILS  WHERE BALLOTID = " + BallotId + " ORDER BY CANDIDATESLNO";
            sqlite_cmd.CommandText = commandstring;
            SQLiteDataReader oReader = sqlite_cmd.ExecuteReader();
            while (oReader.Read())
            {
                if (string.IsNullOrEmpty(oReader["CANDIDATEPHOTO"].ToString()))
                {
                    TTfileDict.Add(Convert.ToInt32(oReader["DETAILBALLOTID"]),
                                    new CandidateRecord()
                                    {
                                        BallotID = Convert.ToInt32(oReader["BALLOTID"].ToString()),
                                        CandidateNO = Convert.ToInt32(oReader["CANDIDATESLNO"]),
                                        CandidateNameENG = oReader["CANDIDATENAMEENG"].ToString(),
                                        CandidateNameOL = oReader["CANDIDATENAMEOL"].ToString(),
                                        PartyNameENG = oReader["PARTYAFFILIATIONENG"].ToString(),
                                        PartyNameOL = oReader["PARTYAFFILIATIONOL"].ToString(),
                                        CandidatePhoto = new byte[1],
                                        ISNOTA = Convert.ToInt32(oReader["ISNOTA"]),
                                    });
                }

                else
                {
                    TTfileDict.Add(Convert.ToInt32(oReader["DETAILBALLOTID"]),
                          new CandidateRecord()
                          {
                              BallotID = Convert.ToInt32(oReader["BALLOTID"].ToString()),
                              CandidateNO = Convert.ToInt32(oReader["CANDIDATESLNO"]),
                              CandidateNameENG = oReader["CANDIDATENAMEENG"].ToString(),
                              CandidateNameOL = oReader["CANDIDATENAMEOL"].ToString(),
                              PartyNameENG = oReader["PARTYAFFILIATIONENG"].ToString(),
                              PartyNameOL = oReader["PARTYAFFILIATIONOL"].ToString(),
                              CandidatePhoto = (byte[])oReader["CANDIDATEPHOTO"],
                              // CandidatePhoto = Convert.ToBase64String(oReader["CANDIDATEPHOTO"] as byte[]),
                              ISNOTA = Convert.ToInt32(oReader["ISNOTA"]),
                          });
                }
            }

            oReader.Close();
            sqlite_cmd.Dispose();
            SqLite.CloseSQLLiteConnection(sqlite_conn);

            if (TTfileDict.Values.Count == 0)
            {
                MessageBox.Show("No Candidate  Record Found...");

                return;
            }
            string str = "";
            List<CandidateRecord> candidateLists = new List<CandidateRecord>();
            candidateLists.Clear();
            foreach (KeyValuePair<int, CandidateRecord> candidate in TTfileDict)
            {
                candidateLists.Add(candidate.Value);
            }


            ballotCandidateList.Candidate = candidateLists;

            string exeFile = (new System.Uri(Assembly.GetEntryAssembly().CodeBase)).AbsolutePath;
            string exeDir = System.IO.Path.GetDirectoryName(exeFile);

            string Directorypath = Directory.GetCurrentDirectory() + "\\images";

            string base64String1 = ConvertImage(Directorypath + "\\eci-logo.png");
            string Imagesrc = "data:image/jpg;base64," + base64String1;


            string base64String2 = null;
            string Imagesrc2 = null;
            //string Bordername = null;
            // var TemplateInstance=
            if (strConstType == "Parliamentary")
            {
                base64String2 = ConvertImage(Directorypath + "\\PC.PNG");
                Imagesrc2 = "data:image/jpg;base64," + base64String2;
                BallotTemplatePC TemplateInstance = new BallotTemplatePC
                {
                    Session = new Dictionary<string, object>()
                };
                TemplateInstance.Session.Add("TTfileDict", ballotCandidateList);
                TemplateInstance.Session.Add("Imagesrc", Imagesrc);
                TemplateInstance.Session.Add("Imagesrc2", Imagesrc2);
                TemplateInstance.Session.Add("FontFamily", selectedfont);
                TemplateInstance.Initialize();
                str = TemplateInstance.TransformText();
                // Bordername = "PC.PNG";
            }
            else
            {
                base64String2 = ConvertImage(Directorypath + "\\AC.PNG");
                Imagesrc2 = "data:image/jpg;base64," + base64String2;
                BallotTemplateAC TemplateInstanceAC = new BallotTemplateAC
                {
                    Session = new Dictionary<string, object>()
                };
                TemplateInstanceAC.Session.Add("TTfileDict", ballotCandidateList);
                TemplateInstanceAC.Session.Add("Imagesrc", Imagesrc);
                TemplateInstanceAC.Session.Add("Imagesrc2", Imagesrc2);
                TemplateInstanceAC.Session.Add("FontFamily", selectedfont);
                TemplateInstanceAC.Initialize();
                str = TemplateInstanceAC.TransformText();
                // Bordername = "AC.PNG";
            }

            // ConvertHtmlToImage(str);





            /////////////////////////////////////////////////////////////////////////////////////////
            // Intentionally blank page tt file code
            string str1 = "";
            IntentionallyBlankPage TemplateInstance1 = new IntentionallyBlankPage();
            //TemplateInstance1.Session = new Dictionary<string, object>();
            //TemplateInstance1.Session.Add(Imagesrc, Imagesrc);
            //TemplateInstance1.Initialize();
            str1 = TemplateInstance1.TransformText();
            //////////////////////////////////////////////////////////////////////////////////////


            HtmlToPdf converter = new HtmlToPdf();
            string fullPath = System.IO.Path.Combine(exeDir, "..\\..\\HtmlEngine\\Select.Html.dep");
            string filepath = System.IO.Path.GetFileName(fullPath);
            GlobalProperties.HtmlEngineFullPath = filepath;
            SelectPdf.PdfDocument doc = converter.ConvertHtmlString(str);

            SelectPdf.PdfDocument doc1 = converter.ConvertHtmlString(str1);
            SelectPdf.PdfPage pdfPage = doc1.Pages[0];
            for (int i = 0; i < doc.Pages.Count; i++)
            {
                if (i % 2 == 1)
                {
                    doc.InsertPage(i, pdfPage);
                }
            }


            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                DefaultExt = "pdf",
                Filter = "Pdf File|*.pdf",
                Title = "Save Ballot",
                FileName = "Ballot",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };


            if (saveFileDialog.ShowDialog() == true)
            {
                doc.Save(saveFileDialog.FileName);
                doc.Close();
                MessageBox.Show("File Saved...");
            }
            else
            {
                MessageBox.Show("File Not Saved...");
            }


            //doc.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Ballot.pdf");  // it generates pdf of only 4 records
            //doc.Close();
            //MessageBox.Show("File Saved On Deskstop Named Ballot.pdf");
            //System.Diagnostics.Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Ballot.pdf");


        }


        //public void ConvertHtmlToImage( string str)
        //{
        //    Bitmap m_Bitmap = new Bitmap(400, 600);
        //    PointF point = new PointF(0, 0);
        //    SizeF maxSize = new System.Drawing.SizeF(500, 500);
        //    HtmlRenderer.HtmlRender.Render(Graphics.FromImage(m_Bitmap),str,point, maxSize);

        //    //m_Bitmap.Save(@"C:\Test.png", ImageFormat.Png);
        //}




        private string ConvertImage(string filePath)
        {
            byte[] arr1 = null;
            arr1 = File.ReadAllBytes(filePath);

            // Convert byte[] to Base64 String
            string base64String = Convert.ToBase64String(arr1);

            return base64String;
        }

        private void btn_finalize_Click(object sender, RoutedEventArgs e)
        {
            sqlite_conn = SqLite.OpenSQLLiteConnection(sqlite_conn);
            sqlite_cmd = sqlite_conn.CreateCommand();
            string commandstring = " UPDATE  MASTERBALLOT SET IS_FINALIZED = 'Y',FINALIZATION_DATE = '" + DateTime.Now.Date.ToString("dd-MM-yyyy") + "'WHERE BALLOT_ID = '" + BallotId + "' ";
            sqlite_cmd.CommandText = commandstring;
            int result = sqlite_cmd.ExecuteNonQuery();
            if (result > 0)
            {
                MessageBox.Show("Ballot is Finalized");
            }
            else
            {
                MessageBox.Show("Ballot Finalization failed");
            }

            sqlite_cmd.Dispose();
            SqLite.CloseSQLLiteConnection(sqlite_conn);

        }


        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if ((bool)checkBox.IsChecked && checkBox.Name == "chkNotaBox")
            {
                btnBrowse.IsEnabled = false;
                txt_EnglishPartyName.Text = string.Empty;
                txt_RegionalPartyName.Text = string.Empty;
                txt_EnglishPartyName.IsEnabled = false;
                txt_RegionalPartyName.IsEnabled = false;
                string exeFile = (new System.Uri(Assembly.GetEntryAssembly().CodeBase)).AbsolutePath;
                string exeDir = System.IO.Path.GetDirectoryName(exeFile);
                string Directorypath = Directory.GetCurrentDirectory() + "\\images";
                string filename = Directorypath + "\\nabove.png";
                NotaPhoto = File.ReadAllBytes(filename);
                BitmapImage bmpImage = new BitmapImage();
                bmpImage.BeginInit();
                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                bmpImage.UriSource = new Uri(filename);
                bmpImage.DecodePixelHeight = 76;
                bmpImage.DecodePixelWidth = 94;
                bmpImage.EndInit();
                photo_img.Source = bmpImage;
            }
            else if ((bool)checkBox.IsChecked && checkBox.Name == "chkNoPhoto")
            {
                photo_img.Source = null;
                btnBrowse.IsEnabled = false;
            }
            else if ((bool)checkBox.IsChecked && checkBox.Name == "chkfont")
            {
                cmbfont.IsEnabled = true;
            }
            else if (!(bool)checkBox.IsChecked && checkBox.Name == "chkNotaBox")
            {
                photo_img.Source = null;
                btnBrowse.IsEnabled = true;
                txt_EnglishPartyName.IsEnabled = true;
                txt_RegionalPartyName.IsEnabled = true;
            }
            else if (!(bool)checkBox.IsChecked && checkBox.Name == "chkNoPhoto")
            {
                btnBrowse.IsEnabled = true;
            }
            else if (!(bool)checkBox.IsChecked && checkBox.Name == "chkfont")
            {
                cmbfont.IsEnabled = false;
            }

        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            if (textbox?.Text != string.Empty)
            {
                switch (textbox?.Name)
                {
                    case "txt_EnglishName":
                        if (Regex.Match(textbox.Text, @"^(?:([a-zA-Z0' ])(?!\1\1))+$").Success != true)
                        {
                            MessageBox.Show("Enter valid data");
                            textbox.Text = string.Empty;
                        }
                        break;
                    case "txt_RegionalName":
                        if (Regex.Match(textbox.Text, @"^[^a-zA-Z~!@#$%^&*;:?`~><,\(\)_+=\[\]\{\}\|\.\-\'\/]{1,50}$").Success != true)
                        {
                            MessageBox.Show("Enter valid data");
                            textbox.Text = string.Empty;
                        }
                        break;
                    case "txt_EnglishPartyName":
                        if (Regex.Match(textbox.Text, @"^(?:([a-zA-Z0' ])(?!\1\1))+$").Success != true)
                        {
                            MessageBox.Show("Enter valid data");
                            textbox.Text = string.Empty;
                        }
                        break;
                    case "txt_RegionalPartyName":
                        if (Regex.Match(textbox.Text, @"^[^a-zA-Z~!@#$%^&*;:?`~><,\(\)_+=\[\]\{\}\|\.\-\'\/]{1,50}$").Success != true)
                        {
                            MessageBox.Show("Enter valid data");
                            textbox.Text = string.Empty;
                        }
                        break;
                }
            }
        }

        private string IsEmptyOrNotMethod()
        {
            string errorMsg = string.Empty;

            if (lang_Official == "English" && chkNotaBox.IsChecked == true)
            {
                if (txt_EnglishName.Text == "")
                {
                    errorMsg += "- Name !\r\n";
                }
            }

            if (lang_Official == "English" && chkNoPhoto.IsChecked == true)
            {
                if (txt_EnglishName.Text == "")
                {
                    errorMsg += "- Name !\r\n";
                }
                if (txt_EnglishPartyName.Text == "")
                {
                    errorMsg += "- Party !\r\n";
                }

            }

            if (lang_Official == "English" && chkNotaBox.IsChecked == false && chkNoPhoto.IsChecked == false)
            {
                if (txt_EnglishName.Text == "")
                {
                    errorMsg += "- Name !\r\n";
                }
                if (txt_EnglishPartyName.Text == "")
                {
                    errorMsg += "- Party !\r\n";
                }
                if (photo_img.Source == null)
                {
                    errorMsg += "- photo !\r\n";
                }

            }

            if (lang_Official != "English" && chkNoPhoto.IsChecked == true)
            {
                if (txt_EnglishName.Text == "" || txt_RegionalName.Text == "")
                {
                    errorMsg += "- Name !\r\n";
                }
                if (txt_EnglishPartyName.Text == "" || txt_RegionalPartyName.Text == "")
                {
                    errorMsg += "- Party !\r\n";
                }

            }

            if (lang_Official != "English" && chkNotaBox.IsChecked == true)
            {
                if (txt_EnglishName.Text == "" || txt_RegionalName.Text == "")
                {
                    errorMsg += "- Name !\r\n";
                }


            }

            if (lang_Official != "English" && chkNotaBox.IsChecked == false && chkNoPhoto.IsChecked == false)
            {
                if (txt_EnglishName.Text == "" || txt_RegionalName.Text == "")
                {
                    errorMsg += "- Name !\r\n";
                }
                if (txt_EnglishPartyName.Text == "" || txt_RegionalPartyName.Text == "")
                {
                    errorMsg += "- Party !\r\n";
                }
                if (photo_img.Source == null)
                {
                    errorMsg += "- photo !\r\n";
                }

            }

            return errorMsg;
        }

        private void btn_reset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            btn_Submit.IsEnabled = true;
            btn_Update.IsEnabled = false;
        }

        private void btn_edit_Click(object sender, RoutedEventArgs e)
        {


            CandidateRecord candidateRecord = ((FrameworkElement)sender).DataContext as CandidateRecord;
            btn_Update.IsEnabled = true;
            btn_Submit.IsEnabled = false;
            //added by vijay to check current record is nota or not
            if (candidateRecord != null)
            {
                if (candidateRecord.ISNOTA == 1) // Nota record 
                {
                    detailballotId = candidateRecord.DetailBallotID;
                    txt_EnglishName.Text = candidateRecord.CandidateNameENG;
                    txt_RegionalName.Text = candidateRecord.CandidateNameOL;
                    photo_img.Source = LoadImage(candidateRecord.CandidatePhoto);
                    CandPhoto = candidateRecord.CandidatePhoto;

                    //disabled field
                    txt_EnglishPartyName.IsEnabled = false;
                    txt_RegionalPartyName.IsEnabled = false;
                }
                else
                {
                    detailballotId = candidateRecord.DetailBallotID;
                    txt_EnglishName.Text = candidateRecord.CandidateNameENG;
                    txt_RegionalName.Text = candidateRecord.CandidateNameOL;
                    txt_EnglishPartyName.Text = candidateRecord.PartyNameENG;
                    txt_RegionalPartyName.Text = candidateRecord.PartyNameOL;
                    photo_img.Source = LoadImage(candidateRecord.CandidatePhoto);
                    CandPhoto = candidateRecord.CandidatePhoto;

                    //enabled some fields
                    txt_EnglishPartyName.IsEnabled = true;
                    txt_RegionalPartyName.IsEnabled = true;
                }
            }
        }

        private void btn_Update_Click(object sender, RoutedEventArgs e)
        {
            //JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder();
            //byte[] imageToUpdate = ImageToBytes(jpegEncoder, photo_img.Source);
            sqlite_conn = SqLite.OpenSQLLiteConnection(sqlite_conn);
            sqlite_cmd = sqlite_conn.CreateCommand();

            string commandstring = "UPDATE BALLOTDETAILS SET CANDIDATENAMEENG = @CANDIDATENAMEENG, CANDIDATENAMEOL = @CANDIDATENAMEOL, PARTYAFFILIATIONENG = @PARTYAFFILIATIONENG, PARTYAFFILIATIONOL = @PARTYAFFILIATIONOL, CANDIDATEPHOTO = @CANDIDATEPHOTO where DETAILBALLOTID = @DETAILBALLOTID";
            // string commandstring = "UPDATE BALLOTDETAILS SET CANDIDATENAMEENG = :CANDIDATENAMEENG, CANDIDATENAMEOL = :CANDIDATENAMEOL, PARTYAFFILIATIONENG = :PARTYAFFILIATIONENG, PARTYAFFILIATIONOL = :PARTYAFFILIATIONOL, CANDIDATEPHOTO = :CANDIDATEPHOTO where DETAILBALLOTID = :DETAILBALLOTID";

            using (sqlite_cmd = new SQLiteCommand(commandstring, sqlite_conn))
            {
                sqlite_cmd.Parameters.AddWithValue("@CANDIDATENAMEENG", txt_EnglishName.Text.ToString());
                sqlite_cmd.Parameters.AddWithValue("@CANDIDATENAMEOL", txt_RegionalName.Text.ToString());
                sqlite_cmd.Parameters.AddWithValue("@PARTYAFFILIATIONENG", txt_EnglishPartyName.Text.ToString());
                sqlite_cmd.Parameters.AddWithValue("@PARTYAFFILIATIONOL", txt_RegionalPartyName.Text.ToString());
                //sqlite_cmd.Parameters.AddWithValue("@CANDIDATEPHOTO", imageToUpdate);
                sqlite_cmd.Parameters.AddWithValue("@CANDIDATEPHOTO", CandPhoto);
                sqlite_cmd.Parameters.AddWithValue("@DETAILBALLOTID", detailballotId);

                int result = sqlite_cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    //MessageBox.Show("Ballot Details Updated");
                    GenerateGrid();
                    //Reset();
                    //btn_Submit.IsEnabled = true;
                    //btn_Update.IsEnabled = false;
                }

                else
                {
                    MessageBox.Show("Ballot Details Update failed");
                }
            }
            sqlite_cmd.Dispose();
            SqLite.CloseSQLLiteConnection(sqlite_conn);
        }

        private void btn_delete_Click(object sender, RoutedEventArgs e)
        {
            CandidateRecord candidateRecord = ((FrameworkElement)sender).DataContext as CandidateRecord;
            detailballotId = candidateRecord.DetailBallotID;
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to permanently delete this record?", "Delete record", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                //handle in reset method
                //if (candidateRecord.PartyNameENG=="" && candidateRecord.PartyNameOL=="")
                //{
                //    // --vijay
                //    txt_EnglishName.Text = "";
                //    txt_RegionalName.Text = "";
                //    chkNotaBox.IsChecked = false;
                //    chkNotaBox.IsEnabled = true;
                //    //-----------------
                //    btn_Submit.IsEnabled = true;
                //    btn_reset.IsEnabled = true;
                //}

                deleteRow(detailballotId);
            }
        }

        private void cmbfont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            selectedfont = cmbfont.SelectedItem.ToString();
            txt_RegionalName.FontFamily = new System.Windows.Media.FontFamily(selectedfont);
            txt_RegionalPartyName.FontFamily = new System.Windows.Media.FontFamily(selectedfont);
            dgCandidate.FontFamily = new System.Windows.Media.FontFamily(selectedfont);

        }

        //added by vijay
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            if ((bool)checkBox.IsChecked)
            {
                System.Windows.Forms.MessageBox.Show("PDF will be created according to FORM7A serial number entered and, \n Verify NOTA is your last entry.", "Information");
                GenerateGrid();
                Col_SRForm7A.Visibility = Visibility.Visible;
            }
            else
            {
                Col_SRForm7A.Visibility = Visibility.Collapsed;
            }
        }

        //added by vijay
        private void IfNotaDoThis()
        {
            //disabled
            chkNotaBox.IsEnabled = false;
            btn_Submit.IsEnabled = false;
            btn_reset.IsEnabled = false;
            btnBrowse.IsEnabled = false;
            btn_Update.IsEnabled = false;

            //enabled
            chkForm7a.IsEnabled = true; //enable serial number chkbox to chnage serial number according to fomr 7a after Nota added
            btnSaveBallot.IsEnabled = true;
            btnPreview.IsEnabled = true;

            txt_EnglishName.IsEnabled = true;
            txt_RegionalName.IsEnabled = true;
            txt_EnglishPartyName.IsEnabled = true;
            txt_RegionalPartyName.IsEnabled = true;


            //Checked
            chkNotaBox.IsChecked = true;

            //clear
            txt_EnglishName.Text = string.Empty;
            txt_RegionalName.Text = string.Empty;
            txt_EnglishPartyName.Text = string.Empty;
            txt_RegionalPartyName.Text = string.Empty;
            photo_img.Source = null;
        }


        //added by vijay to change form7a serial number
        private void DgCandidate_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditingElement is TextBox)
            {
                int newSR = Convert.ToInt32((e.EditingElement as TextBox).Text);
                CandidateRecord candidateRecord = e.Row.Item as CandidateRecord;
                //CandidateRecord candidateRecord = (sender as DataGrid).CurrentItem as CandidateRecord;
                if (candidateRecord != null && newSR != candidateRecord.CandidateNO)
                {
                    //sqlite_conn = SqLite.OpenSQLLiteConnection(sqlite_conn);
                    //sqlite_cmd = sqlite_conn.CreateCommand();

                    //string command = "UPDATE BALLOTDETAILS SET CANDIDATESLNO = @CANDIDATESLNO where DETAILBALLOTID = @DETAILBALLOTID";
                    //using (sqlite_cmd = new SQLiteCommand(command, sqlite_conn))
                    //{
                    //    sqlite_cmd.Parameters.AddWithValue("@CANDIDATESLNO", newSR);
                    //    sqlite_cmd.Parameters.AddWithValue("@DETAILBALLOTID", candidateRecord.DetailBallotID);

                    //    int result = sqlite_cmd.ExecuteNonQuery();
                    //    if (result > 0)
                    //    {
                    //        string msg = $"Serial Number of {candidateRecord.CandidateNameENG} "+" "+$" {candidateRecord.CandidateNameOL} is changed from {candidateRecord.CandidateNO} "+" to "+ $"{newSR}.";
                    //        MessageBox.Show(msg,"Information");
                    //        GenerateGrid();
                    //        //Reset();
                    //        //btn_Submit.IsEnabled = true;
                    //        //btn_Update.IsEnabled = false;
                    //    }
                    //    else
                    //    {
                    //        MessageBox.Show("Failed to update serial number","Error");
                    //    }
                    //}
                    //sqlite_cmd.Dispose();
                    //SqLite.CloseSQLLiteConnection(sqlite_conn);

                    //conditions
                    // user cannot add serial number greater than or equal to nota

                    if (newSR >= CandidateRecordList.CandidateRecords.Count)
                    {
                        CandidateRecord update = CandidateRecordList.CandidateRecords
                            .Where(candidate => candidate.CandidateNO == candidateRecord.CandidateNO)
                            .Select(candidate => candidate).First();
                        (e.EditingElement as TextBox).Text = update.CandidateNO.ToString();
                    }
                    else
                    {
                        //update list
                        CandidateRecord update = CandidateRecordList.CandidateRecords.Where(candidate => candidate.CandidateNO == candidateRecord.CandidateNO).Select(candidate => candidate).First() as CandidateRecord;
                        update.CandidateNO = newSR;
                        isChanged = true;
                    }
                }
            }
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            PreviewWindow preview = new PreviewWindow();
             bool? result = preview.ShowDialog();
            GenerateGrid();
        }
    }
}
