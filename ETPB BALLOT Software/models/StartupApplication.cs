using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ETPB_BALLOT_Software.models
{
    class StartupApplication
    {
        private static readonly Mutex Mutex = new Mutex(false, "ETPB Ballot Software"); // application identifier and ETPB Ballot Software can be any name. 

        private static MainWindow mainWindow = null;

        [STAThread]
        static void Main()
        {
            if (Mutex.WaitOne(TimeSpan.Zero, true))
            {
                App app = new App();
                mainWindow = new MainWindow();
                app.Run(mainWindow);
                Mutex.ReleaseMutex();
            }
            else
            {
                try
                {
                    MainWindow.ShowToFront("ETPB Ballot Software"); // ETPB Ballot Software is TItle of mainwindow
                }
                catch (Exception ex)
                {

                    string msg = ex.Message;
                }
            }

        }
    }
}
