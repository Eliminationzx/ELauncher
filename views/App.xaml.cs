using System.Threading;
using System.Windows;

namespace Launcher
{
    public partial class App
    {
        static Mutex _instanceMutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;

            _instanceMutex = new Mutex(true, "F37E84CB-B76A-49B1-A1AC-99979903087B", out createdNew);
            if (!createdNew)
            {
                MessageBox.Show("A copy of the Launcher is already running on this computer", "Attempt to run again", MessageBoxButton.OK, MessageBoxImage.Warning);
                _instanceMutex = null;
                Current.Shutdown();
                return;
            }
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_instanceMutex != null)
                _instanceMutex.ReleaseMutex();
            base.OnExit(e);
        }
    }
}
