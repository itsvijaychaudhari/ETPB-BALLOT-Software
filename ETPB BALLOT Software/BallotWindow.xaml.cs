
using Com.Cdac.Gist.Translit;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
//using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CandidateDetails;

using System.Text.RegularExpressions;
using SelectPdf;


using ETPB_BALLOT_Software.models;

using Microsoft.Win32;
using System.Drawing;
using System.Windows.Media;
using System.Runtime.InteropServices;

namespace ETPB_BALLOT_Software
{
    /// <summary>
    /// Interaction logic for BallotWindow.xaml
    /// </summary>
    public partial class BallotWindow : Window
    {
        SQLiteConnection sqlite_conn = new SQLiteConnection();
        SQLiteCommand sqlite_cmd;
        TransliterationProvider transProvider = MainWindow.transProvider;
        LanguageCode langCode = new LanguageCode();
        List<string> fonts = new List<string>();
        string selectedfont = string.Empty;
        string Locale = null;
        int BallotId;
        byte[] CandPhoto = null;
        byte[] NotaPhoto = null;
        int detailballotId;
        public string strConstType;
        public string lang_Official;

        

        CandidateDetails.BallotCandidateList ballotCandidateList = new BallotCandidateList();
        Dictionary<int, CandidateRecord> TTfileDict = new Dictionary<int, CandidateRecord>();


        public BallotWindow(BallotData ballotdata)
        {
            InitializeComponent();
            lblYear.Content = Convert.ToInt32(System.DateTime.Now.Year).ToString();
            nameConstituency.Content = ballotdata.constituency;
            lblState.Content = ballotdata.state;
          
            BallotId = ballotdata.BallotID;
            strConstType = ballotdata.constituency;
            lang_Official = ballotdata.language1;

            string st = ballotdata.state;
            string stt = null;
            string regularExpressionPattern = @"\((.*?)\)";
            Regex re = new Regex(regularExpressionPattern);

            foreach (Match m in re.Matches(st))
            {
                stt = m.Value;
            }
            string stateID = stt.TrimStart('(').TrimEnd(')');

            Locale = langCode.LanguageDict[stateID];

            //Dictionary<string, string> LangCode = langCode.LanguageDict;
            //foreach (var item in LangCode)
            //{
            //    if (item.Key.ToString() == stateID)
            //    {
            //        Locale = item.Value.ToString();
            //    }

            //}

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
                dgCandidate.Columns[1].Visibility = Visibility.Hidden;
                dgCandidate.Columns[3].Visibility = Visibility.Hidden;
            }
            GenerateGrid();
            btn_Update.IsEnabled = false;

            foreach (System.Windows.Media.FontFamily fontFamily in Fonts.SystemFontFamilies)
            {
                cmbfont.Items.Add(fontFamily.Source);

            }
            cmbfont.SelectedItem = "SakalBharati";
           
            //  cmbfont.ItemsSource = fonts;
          

            // Last record should be NOTA........................... After NOTA disable Submit button
            sqlite_conn = SqLite.OpenSQLLiteConnection(sqlite_conn);
            SQLiteDataAdapter Sqa = new SQLiteDataAdapter("select * from BALLOTDETAILS where BALLOTID= '" + BallotId + "' ", sqlite_conn);
            DataTable dt = new DataTable();
            Sqa.Fill(dt);  // fill the dataset
            
            if (dt.Rows.Count > 0)
            {
                DataRow lastRow = dt.Rows[dt.Rows.Count - 1];

                if (lastRow["ISNOTA"] != DBNull.Value &&  Convert.ToInt32(lastRow["ISNOTA"]) == 1 )
                {
                    chkNotaBox.IsChecked = true;
                   
                    chkNotaBox.IsEnabled = false;
                    btn_Submit.IsEnabled = false;
                    btn_reset.IsEnabled = false;
                }
            }
         


        }

        private void GenerateGrid()  // retrieves all record from BALLOTDETAILS table and binds to datagrid
        {
            var list = new List<CandidateRecord>();

            sqlite_conn = SqLite.OpenSQLLiteConnection(sqlite_conn);
            SQLiteDataAdapter Sqa = new SQLiteDataAdapter("select * from BALLOTDETAILS where BALLOTID= '" + BallotId + "' ", sqlite_conn);
            DataTable dt = new DataTable();
            Sqa.Fill(dt);  // fill the dataset
          
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new CandidateRecord()
                {
                    ISNOTA = Convert.ToInt32(row["ISNOTA"]) ,
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
            SqLite.CloseSQLLiteConnection(sqlite_conn);

        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {

            string filename = null;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();  // Create OpenFileDialog

            //dlg.Filter = "Images (*.BMP;*.JPG;*.GIF,*.PNG,*.TIFF)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF|" + "All files (*.*)|*.*";
            dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg | PNG Files (*.png)|*.png ";
            dlg.Title = "Select a file";

            Nullable<bool> result = dlg.ShowDialog();  // Display OpenFileDialog by calling ShowDialog method

            if (result == true)
            {
                filename = dlg.FileName;

                CandPhoto = File.ReadAllBytes(filename);
                //  string base64PhotoString = Convert.ToBase64String(CandPhoto);

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
            var textbox = sender as TextBox;
            if (Locale == "")
            {

            }
            else
            {
                if (textbox.Name == "txt_EnglishName")
                {
                    txt_RegionalName.Text = transProvider.Transliterate(textbox.Text.ToString(), Locale, TransliterationHints.NAME);
                }
                if (textbox.Name == "txt_EnglishPartyName")
                {
                    txt_RegionalPartyName.Text = transProvider.Transliterate(textbox.Text.ToString(), Locale, TransliterationHints.NAME);
                }
            }

        }

        private void btn_Submit_Click(object sender, RoutedEventArgs e)
        {
            string errorMessage = IsEmptyOrNotMethod();

            if (!String.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show("Please fill following fields.....\n" + errorMessage);
            }
            else
            {
                int DetailBallotIdToInsert = GetDetailBallotId();
                int CandidateNoToInsert = GetCandidateNo();

                sqlite_conn = SqLite.OpenSQLLiteConnection(sqlite_conn);
                sqlite_cmd = sqlite_conn.CreateCommand();

                string commandstring = "INSERT INTO BALLOTDETAILS (DETAILBALLOTID,BALLOTID ,CANDIDATESLNO,CANDIDATENAMEENG,CANDIDATENAMEOL,PARTYAFFILIATIONENG,PARTYAFFILIATIONOL,CANDIDATEPHOTO,ISNOTA ) VALUES (@DETAILBALLOTID ,@BALLOTID ,@CANDIDATESLNO,@CANDIDATENAMEENG,@CANDIDATENAMEOL,@PARTYAFFILIATIONENG,@PARTYAFFILIATIONOL,@CANDIDATEPHOTO,@ISNOTA)";

                if (lang_Official == "English")
                {
                    #region For English
                    if (chkNotaBox.IsChecked == true)
                    {
                        //Try to add NOTA as 1st record --vijay
                        if (CandidateNoToInsert == 1)
                        {
                            System.Windows.Forms.MessageBox.Show("You cannot at NOTA as 1st record..!", "Warning");
                            
                        }
                        else
                        {
                            
                            using (sqlite_cmd = new SQLiteCommand(commandstring, sqlite_conn))
                            {
                                sqlite_cmd.Parameters.AddWithValue("DETAILBALLOTID", DetailBallotIdToInsert);
                                sqlite_cmd.Parameters.AddWithValue("BALLOTID", BallotId);
                                sqlite_cmd.Parameters.AddWithValue("CANDIDATESLNO", CandidateNoToInsert);
                                sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEENG", txt_EnglishName.Text.ToString().Replace("'", "''"));  // replace method in case ' in name eg. D'souza
                                sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEOL", null);
                                sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONENG", null);
                                sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONOL", null);
                                sqlite_cmd.Parameters.AddWithValue("CANDIDATEPHOTO", NotaPhoto);
                                sqlite_cmd.Parameters.AddWithValue("ISNOTA", 1);

                                int result = sqlite_cmd.ExecuteNonQuery();
                                if (result > 0)
                                {
                                    MessageBox.Show("Ballot Details inserted");
                                    // --vijay
                                    txt_EnglishName.Text = "";
                                    txt_RegionalName.Text = "";
                                    //-----------------------
                                    GenerateGrid();
                                  //  Reset();
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
                            sqlite_cmd.Parameters.AddWithValue("DETAILBALLOTID", DetailBallotIdToInsert);
                            sqlite_cmd.Parameters.AddWithValue("BALLOTID", BallotId);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATESLNO", CandidateNoToInsert);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEENG", txt_EnglishName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEOL", null);
                            sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONENG", txt_EnglishPartyName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONOL", null);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATEPHOTO", null);
                            sqlite_cmd.Parameters.AddWithValue("ISNOTA", 0);   

                            int result = sqlite_cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Ballot Details inserted");
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
                            sqlite_cmd.Parameters.AddWithValue("DETAILBALLOTID", DetailBallotIdToInsert);
                            sqlite_cmd.Parameters.AddWithValue("BALLOTID", BallotId);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATESLNO", CandidateNoToInsert);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEENG", txt_EnglishName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEOL", null);
                            sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONENG", txt_EnglishPartyName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONOL", null);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATEPHOTO", CandPhoto);
                            sqlite_cmd.Parameters.AddWithValue("ISNOTA", 0);

                            int result = sqlite_cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Ballot Details inserted");
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
                        if (CandidateNoToInsert == 1)
                        {
                            System.Windows.Forms.MessageBox.Show("You cannot at NOTA as 1st record..!","Warning");
                           
                        }
                        else
                        {
                           
                            using (sqlite_cmd = new SQLiteCommand(commandstring, sqlite_conn))
                            {
                                sqlite_cmd.Parameters.AddWithValue("DETAILBALLOTID", DetailBallotIdToInsert);
                                sqlite_cmd.Parameters.AddWithValue("BALLOTID", BallotId);
                                sqlite_cmd.Parameters.AddWithValue("CANDIDATESLNO", CandidateNoToInsert);
                                sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEENG", txt_EnglishName.Text.ToString().Replace("'", "''"));  // replace method in case ' in name eg. D'souza
                                sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEOL", txt_RegionalName.Text.ToString().Replace("'", "''"));
                                sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONENG", txt_EnglishPartyName.Text.ToString().Replace("'", "''"));
                                sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONOL", txt_RegionalPartyName.Text.ToString().Replace("'", "''"));
                                sqlite_cmd.Parameters.AddWithValue("CANDIDATEPHOTO", NotaPhoto);
                                sqlite_cmd.Parameters.AddWithValue("ISNOTA", 1);

                                int result = sqlite_cmd.ExecuteNonQuery();
                                if (result > 0)
                                {
                                    MessageBox.Show("Ballot Details inserted");
                                    // --vijay
                                    txt_EnglishName.Text = "";
                                    txt_RegionalName.Text = "";
                                    //-----------------------
                                    GenerateGrid();
                                   // Reset();
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
                            sqlite_cmd.Parameters.AddWithValue("DETAILBALLOTID", DetailBallotIdToInsert);
                            sqlite_cmd.Parameters.AddWithValue("BALLOTID", BallotId);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATESLNO", CandidateNoToInsert);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEENG", txt_EnglishName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEOL", txt_RegionalName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONENG", txt_EnglishPartyName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONOL", txt_RegionalPartyName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATEPHOTO", null);
                            sqlite_cmd.Parameters.AddWithValue("ISNOTA", 0);

                            int result = sqlite_cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Ballot Details inserted");
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
                            sqlite_cmd.Parameters.AddWithValue("DETAILBALLOTID", DetailBallotIdToInsert);
                            sqlite_cmd.Parameters.AddWithValue("BALLOTID", BallotId);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATESLNO", CandidateNoToInsert);
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEENG", txt_EnglishName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATENAMEOL", txt_RegionalName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONENG", txt_EnglishPartyName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("PARTYAFFILIATIONOL", txt_RegionalPartyName.Text.ToString().Replace("'", "''"));
                            sqlite_cmd.Parameters.AddWithValue("CANDIDATEPHOTO", CandPhoto);
                            sqlite_cmd.Parameters.AddWithValue("ISNOTA", 0);

                            int result = sqlite_cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Ballot Details inserted");
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

                // Last record should be NOTA........................... After NOTA disable Submit button
                if (chkNotaBox.IsChecked == true)
                {
                    chkNotaBox.IsEnabled = false;
                    btn_Submit.IsEnabled = false;
                    btn_reset.IsEnabled = false;
                }
                else
                {
                    Reset();
                }


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
            if (n > 0)
            {
                MessageBox.Show("Record Deleted");
                GenerateGrid();
            }
            SqLite.CloseSQLLiteConnection(sqlite_conn);
        }

        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;


        }

        private byte[] ImageToBytes(JpegBitmapEncoder encoder, ImageSource imageIn)
        {
            byte[] bytes = null;
            var bitmapSource = imageIn as BitmapSource;

            if (bitmapSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }

            return bytes;

        }

        private void btn_Update_Click(object sender, RoutedEventArgs e)
        {
            JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder();
            byte[] imageToUpdate = ImageToBytes(jpegEncoder, photo_img.Source);


            sqlite_conn = SqLite.OpenSQLLiteConnection(sqlite_conn);
            sqlite_cmd = sqlite_conn.CreateCommand();

            string commandstring = "UPDATE BALLOTDETAILS SET CANDIDATENAMEENG = @CANDIDATENAMEENG, CANDIDATENAMEOL = @CANDIDATENAMEOL, PARTYAFFILIATIONENG = @PARTYAFFILIATIONENG, PARTYAFFILIATIONOL = @PARTYAFFILIATIONOL, CANDIDATEPHOTO = @CANDIDATEPHOTO where DETAILBALLOTID = @DETAILBALLOTID";

            using (sqlite_cmd = new SQLiteCommand(commandstring, sqlite_conn))
            {

                sqlite_cmd.Parameters.AddWithValue("@CANDIDATENAMEENG", txt_EnglishName.Text.ToString());
                sqlite_cmd.Parameters.AddWithValue("@CANDIDATENAMEOL", txt_RegionalName.Text.ToString());
                sqlite_cmd.Parameters.AddWithValue("@PARTYAFFILIATIONENG", txt_EnglishPartyName.Text.ToString());
                sqlite_cmd.Parameters.AddWithValue("@PARTYAFFILIATIONOL", txt_RegionalPartyName.Text.ToString());
                sqlite_cmd.Parameters.AddWithValue("@CANDIDATEPHOTO", imageToUpdate);
                sqlite_cmd.Parameters.AddWithValue("@DETAILBALLOTID", detailballotId);

                int result = sqlite_cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    MessageBox.Show("Ballot Details Updated");
                    GenerateGrid();
                    Reset();
                }

                else
                {
                    MessageBox.Show("Ballot Details Update failed");
                }


            }



            sqlite_cmd.Dispose();
            SqLite.CloseSQLLiteConnection(sqlite_conn);
            btn_Submit.IsEnabled = true;
            btn_Update.IsEnabled = false;
        }

        private void Reset()  // clears all controls except datagrid on main window
        {
            txt_EnglishName.Text = String.Empty;
            txt_RegionalName.Text = String.Empty;
            txt_EnglishPartyName.Text = String.Empty;
            txt_RegionalPartyName.Text = String.Empty;

            chkNotaBox.IsChecked = false;
            chkNoPhoto.IsChecked = false;
            photo_img.Source = null;
           
            // CandPhoto = null;
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
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
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
            foreach (var candidate in TTfileDict)
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
                var TemplateInstance = new BallotTemplatePC();
                TemplateInstance.Session = new Dictionary<string, object>();
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
                var TemplateInstanceAC = new BallotTemplateAC();
                TemplateInstanceAC.Session = new Dictionary<string, object>();
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
            var TemplateInstance1 = new IntentionallyBlankPage();
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
            SelectPdf.PdfPage pdfPage = (SelectPdf.PdfPage)doc1.Pages[0];
            for (int i = 0; i < doc.Pages.Count; i++)
            {
                if (i % 2 == 1)
                {
                    doc.InsertPage(i, pdfPage);
                }
            }


            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "pdf";
            saveFileDialog.Filter = "Pdf File|*.pdf";
            saveFileDialog.Title = "Save Ballot";
            saveFileDialog.FileName = "Ballot";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


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


        private void Oncheck(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            if (checkbox.Name == "chkNotaBox")
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

            if (checkbox.Name == "chkNoPhoto")
            {
                photo_img.Source = null;
                btnBrowse.IsEnabled = false;
            }

            if (checkbox.Name == "chkfont")
            {
                cmbfont.IsEnabled =true;
            }
        }

        private void OnUncheck(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            if (checkbox.Name == "chkNotaBox")
            {
                photo_img.Source = null;
                btnBrowse.IsEnabled = true;
                txt_EnglishPartyName.IsEnabled = true;
                txt_RegionalPartyName.IsEnabled = true;

            }

            if (checkbox.Name == "chkNoPhoto")
            {
                btnBrowse.IsEnabled = true;
            }
            if (checkbox.Name == "chkfont")
            {
                cmbfont.IsEnabled = false;
            }

        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (textbox.Name == "txt_EnglishName")
            {
                if (Regex.Match(textbox.Text, @"^(?:([a-zA-Z0' ])(?!\1\1))+$").Success != true)
                {
                    MessageBox.Show("Enter valid data");
                    textbox.Text = String.Empty;
                }
            }
            if (textbox.Name == "txt_RegionalName")
            {
                if (Regex.Match(textbox.Text, @"^[^a-zA-Z~!@#$%^&*;:?`~><,\(\)_+=\[\]\{\}\|\.\-\'\/]{1,50}$").Success != true)
                {
                    MessageBox.Show("Enter valid data");
                    textbox.Text = String.Empty;
                }
            }
            if (textbox.Name == "txt_EnglishPartyName")
            {
                if (Regex.Match(textbox.Text, @"^(?:([a-zA-Z0' ])(?!\1\1))+$").Success != true)
                {
                    MessageBox.Show("Enter valid data");
                    textbox.Text = String.Empty;
                }
            }
            if (textbox.Name == "txt_RegionalPartyName")
            {
                if (Regex.Match(textbox.Text, @"^[^a-zA-Z~!@#$%^&*;:?`~><,\(\)_+=\[\]\{\}\|\.\-\'\/]{1,50}$").Success != true)
                {
                    MessageBox.Show("Enter valid data");
                    textbox.Text = String.Empty;
                }
            }
        }

        private string IsEmptyOrNotMethod()
        {
            string errorMsg = String.Empty;

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

            if (lang_Official != "English" && chkNotaBox.IsChecked == true )
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
            txt_EnglishName.Text = candidateRecord.CandidateNameENG;
            txt_RegionalName.Text = candidateRecord.CandidateNameOL;
            txt_EnglishPartyName.Text = candidateRecord.PartyNameENG;
            txt_RegionalPartyName.Text = candidateRecord.PartyNameOL;
            photo_img.Source = LoadImage(candidateRecord.CandidatePhoto);
            photo_img.Stretch = Stretch.UniformToFill;
        }

        private void btn_delete_Click(object sender, RoutedEventArgs e)
        {
            CandidateRecord candidateRecord = ((FrameworkElement)sender).DataContext as CandidateRecord;
            detailballotId = candidateRecord.DetailBallotID;
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to permanently delete this record?", "Delete record", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                if (candidateRecord.PartyNameENG=="" && candidateRecord.PartyNameOL=="")
                {
                    // --vijay
                    txt_EnglishName.Text = "";
                    txt_RegionalName.Text = "";
                    chkNotaBox.IsChecked = false;
                    chkNotaBox.IsEnabled = true;
                    //-----------------
                    btn_Submit.IsEnabled = true;
                    btn_reset.IsEnabled = true;
                }
                deleteRow(detailballotId);
            }
            else if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }
        }

        private void cmbfont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            selectedfont = cmbfont.SelectedItem.ToString();
            txt_RegionalName.FontFamily = new System.Windows.Media.FontFamily(selectedfont);
            txt_RegionalPartyName.FontFamily = new System.Windows.Media.FontFamily(selectedfont);
            dgCandidate.FontFamily = new System.Windows.Media.FontFamily(selectedfont);
           
        }

        //byte[] bitmap = GetYourImage();

        //    using(Image image = Image.FromStream(new MemoryStream(bitmap)))
        //    {
        //        image.Save("output.jpg", ImageFormat.Jpeg);  // Or Png
        //    }



    }
}
