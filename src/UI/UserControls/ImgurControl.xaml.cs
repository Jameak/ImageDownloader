using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.Win32;
using UI.ViewModels;
using UI.ViewModels.Base;
using UI.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
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
            _vm.Download = new RelayCommand(o => _vm.StartDownload(),
                o => !string.IsNullOrWhiteSpace(_vm.Source) && !string.IsNullOrWhiteSpace(_vm.TargetFolder)); //Download button is only enabled when both a source and target have been chosen.

            _vm.ShowLog = SharedEventHandlingLogic.CreateLogCommand(_vm, this, "Imgur");

            _vm.SelectFolder = new RelayCommand(o =>
            {
                var dialog = new FolderBrowserDialog();
                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    _vm.TargetFolder = dialog.SelectedPath;
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
