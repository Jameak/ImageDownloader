using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using UI.ViewModels;
using UI.ViewModels.Base;
using Application = System.Windows.Application;
using UserControl = System.Windows.Controls.UserControl;

namespace UI.UserControls
{
    public partial class ImgurControl : UserControl
    {
        private ImgurControlViewModel _vm;

        public ImgurControl()
        {
            InitializeComponent();

            _vm = (Application.Current as App).Container.GetService<ImgurControlViewModel>();
            _vm.Download = new RelayCommand(o =>
            {
                _vm.StartDownload();
                Separator.Visibility = Visibility.Visible;
                Log.Visibility = Visibility.Visible;
            },  //Download button is only enabled when both a source and target have been chosen.
                o => !string.IsNullOrWhiteSpace(_vm.Source) && !string.IsNullOrWhiteSpace(_vm.TargetFolder));

            _vm.SelectFolder = new RelayCommand(o =>
            {
                var dialog = new FolderBrowserDialog();
                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    _vm.TargetFolder = dialog.SelectedPath;
                }
            });

            Separator.Visibility = Visibility.Collapsed;
            Log.Visibility = Visibility.Collapsed;

            DataContext = _vm;
        }

        /// <summary>
        /// See <see cref="SharedEventHandlingLogic.InputValidation_ConstrainToInt(object,TextCompositionEventArgs)"/>
        /// </summary>
        private void InputValidation(object sender, TextCompositionEventArgs e)
        {
            SharedEventHandlingLogic.InputValidation_ConstrainToInt(sender, e);
        }

        /// <summary>
        /// See <see cref="SharedEventHandlingLogic.InputValidationOnPaste_ConstrainToInt(object,DataObjectPastingEventArgs)"/>
        /// </summary>
        private void InputValidationOnPaste(object sender, DataObjectPastingEventArgs e)
        {
            SharedEventHandlingLogic.InputValidationOnPaste_ConstrainToInt(sender, e);
        }
    }
}
