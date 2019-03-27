using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CandidateDetails;
using ETPB_BALLOT_Software.models;
using Microsoft.Win32;
using SelectPdf;

namespace ETPB_BALLOT_Software
{
    /// <summary>
    /// Interaction logic for PreviewWindow.xaml
    /// </summary>
    public partial class PreviewWindow : Window
    {

        private Dictionary<int, CandidateRecord> TTfileDict = new Dictionary<int, CandidateRecord>();
        private CandidateDetails.BallotCandidateList ballotCandidateList = new BallotCandidateList();
        private SQLiteConnection sqlite_conn = new SQLiteConnection();
        private SQLiteCommand sqlite_cmd;
        public PreviewWindow()
        {
            InitializeComponent();
            DataContext = CandidateRecordList.CandidateRecords;
        }
        
        private void Update_Click(object sender, RoutedEventArgs e)
        {
            int count =CandidateRecordList.CandidateRecords.Select(x => x.CandidateNO).Distinct().Count();
            if (count < CandidateRecordList.CandidateRecords.Count)
            {
                System.Windows.Forms.MessageBox.Show("Duplicate Candidate Serial Number according to form7A Found \n Please correct and update again...!");
            }
            else
            {
                int result = 0;
                foreach (CandidateRecord candidateRecord in CandidateRecordList.CandidateRecords)
                {
                    SQLiteConnection sqlite_conn = SqLite.OpenSQLLiteConnection(new SQLiteConnection());
                    SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand();

                    string command = "UPDATE BALLOTDETAILS SET CANDIDATESLNO = @CANDIDATESLNO where DETAILBALLOTID = @DETAILBALLOTID";
                    using (sqlite_cmd = new SQLiteCommand(command, sqlite_conn))
                    {
                        sqlite_cmd.Parameters.AddWithValue("@CANDIDATESLNO", candidateRecord.CandidateNO);
                        sqlite_cmd.Parameters.AddWithValue("@DETAILBALLOTID", candidateRecord.DetailBallotID);

                        result = sqlite_cmd.ExecuteNonQuery();

                    }
                    sqlite_cmd.Dispose();
                    SqLite.CloseSQLLiteConnection(sqlite_conn);
                }
                if (result > 0)
                {
                    System.Windows.Forms.MessageBox.Show("Record Update Successfully..!");
                    btnUpdate.IsEnabled = false;
                    btnSave.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("Failed to update serial number", "Error");
                }

            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SelectPdf.GlobalProperties.LicenseKey = "xu335vTz9+b19PL05vf+6Pbm9ffo9/To/////w== ";
            TTfileDict.Clear();
            sqlite_conn = SqLite.OpenSQLLiteConnection(sqlite_conn);
            sqlite_cmd = sqlite_conn.CreateCommand();
            string commandstring = "SELECT * FROM BALLOTDETAILS  WHERE BALLOTID = " + BallotWindow.BallotId + " ORDER BY CANDIDATESLNO";
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
            if (BallotWindow.strConstType == "Parliamentary")
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
                TemplateInstance.Session.Add("FontFamily", BallotWindow.selectedfont);
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
                TemplateInstanceAC.Session.Add("FontFamily", BallotWindow.selectedfont);
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
        }


        private string ConvertImage(string filePath)
        {
            byte[] arr1 = null;
            arr1 = File.ReadAllBytes(filePath);

            // Convert byte[] to Base64 String
            string base64String = Convert.ToBase64String(arr1);

            return base64String;
        }

        private void Onload(object sender, RoutedEventArgs e)
        {
            if (BallotWindow.isChanged)
            {
                btnSave.IsEnabled = false;
            }
            else
            {
                btnUpdate.IsEnabled = false;
            }

        }
    }
}
