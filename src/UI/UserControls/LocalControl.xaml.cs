using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UI.ViewModels;
using UI.ViewModels.Base;
using Application = System.Windows.Application;
using UserControl = System.Windows.Controls.UserControl;

namespace UI.UserControls
{
    public partial class LocalControl : UserControl
    {
        private LocalControlViewModel _vm;

        public LocalControl()
        {
            InitializeComponent();

            _vm = (Application.Current as App).Container.GetService<LocalControlViewModel>();
            _vm.Download = new RelayCommand(o => _vm.StartDownload(),                
                o => !string.IsNullOrWhiteSpace(_vm.Source) && !string.IsNullOrWhiteSpace(_vm.TargetFolder)); //Download button is only enabled when both a source and target have been chosen.

            _vm.ShowLog = SharedEventHandlingLogic.CreateLogCommand(_vm, this, "Local files");

            _vm.SelectFolder = new RelayCommand(o =>
            {
                var dialog = new FolderBrowserDialog();
                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    _vm.TargetFolder = dialog.SelectedPath;
                }
            });

            _vm.SelectSourceFolder = new RelayCommand(o =>
            {
                var dialog = new FolderBrowserDialog();
                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    _vm.Source = dialog.SelectedPath;
                }
            });

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

        /// <summary>
        /// See <see cref="SharedEventHandlingLogic.InputValidation_ConstrainToAspectRatio(object,TextCompositionEventArgs)"/>
        /// </summary>
        private void AspectRatioInputValidation(object sender, TextCompositionEventArgs e)
        {
            SharedEventHandlingLogic.InputValidation_ConstrainToAspectRatio(sender, e);
        }

        /// <summary>
        /// See <see cref="SharedEventHandlingLogic.InputValidationOnPaste_ConstrainToAspectRatio(object,DataObjectPastingEventArgs)"/>
        /// </summary>
        private void AspectRatioInputValidationOnPaste(object sender, DataObjectPastingEventArgs e)
        {
            SharedEventHandlingLogic.InputValidationOnPaste_ConstrainToAspectRatio(sender, e);
        }
    }
}
