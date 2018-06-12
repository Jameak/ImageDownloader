using System.Windows;
using UI.ViewModels.Base;

namespace UI.Windows
{
    public partial class LogWindow : Window
    {
        private BaseControlProperties _vm;
        private bool _stopWarning;

        public LogWindow(BaseControlProperties baseControlProperties, string header)
        {
            InitializeComponent();
            _vm = baseControlProperties;
            Title = $"Log: {header}";
            DataContext = _vm;

            WarnIfLargeLog();
            Log.TextChanged += (sender, args) => WarnIfLargeLog();
        }

        private void WarnIfLargeLog()
        {
            if (_stopWarning) return;

            if (_vm.Log.Count > 10000)
            {
                var messagebox = MessageBox.Show(
                    "The log has a large number of entries and displaying the log entries may therefore take a while. The program will appear to be frozen during this time (but active downloads will continue in the background).\n\nAre you sure you want to keep the log open?",
                    "Warning: Large log.", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                switch (messagebox)
                {
                    case MessageBoxResult.Yes:
                    case MessageBoxResult.OK:
                        break;
                    case MessageBoxResult.None:
                    case MessageBoxResult.Cancel:
                    case MessageBoxResult.No:
                    default:
                        Close();
                        break;
                }

                _stopWarning = true;
            }
        }
    }
}
