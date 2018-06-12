using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
    public partial class RedditControl : UserControl
    {
        private RedditControlViewModel _vm;

        public RedditControl()
        {
            InitializeComponent();
            _vm = (Application.Current as App).Container.GetService<RedditControlViewModel>();

            //Radiobutton command
            _vm.PullCommand = new RelayCommand(o =>
            {
                if (_vm.PullParam == "top")
                {
                    RadioTopAll.IsEnabled = true;
                    RadioTopYear.IsEnabled = true;
                    RadioTopMonth.IsEnabled = true;
                    RadioTopWeek.IsEnabled = true;
                    RadioTopDay.IsEnabled = true;
                    RadioTopHour.IsEnabled = true;
                }
                else
                {
                    RadioTopAll.IsEnabled = false;
                    RadioTopYear.IsEnabled = false;
                    RadioTopMonth.IsEnabled = false;
                    RadioTopWeek.IsEnabled = false;
                    RadioTopDay.IsEnabled = false;
                    RadioTopHour.IsEnabled = false;
                }
            });

            //Checkboxes
            _vm.CustomQueryCommand = new RelayCommand(o =>
            {
                if (_vm.CustomQuery)
                {
                    RadioPullMain.IsEnabled = false;
                    RadioPullTop.IsEnabled = false;
                    RadioPullControversial.IsEnabled = false;
                    RadioPullRising.IsEnabled = false;
                    RadioPullNew.IsEnabled = false;

                    RadioTopAll.IsEnabled = false;
                    RadioTopYear.IsEnabled = false;
                    RadioTopMonth.IsEnabled = false;
                    RadioTopWeek.IsEnabled = false;
                    RadioTopDay.IsEnabled = false;
                    RadioTopHour.IsEnabled = false;
                }
                else
                {
                    RadioPullMain.IsEnabled = true;
                    RadioPullTop.IsEnabled = true;
                    RadioPullControversial.IsEnabled = true;
                    RadioPullRising.IsEnabled = true;
                    RadioPullNew.IsEnabled = true;
                    _vm.PullCommand.Execute(null);
                }
            });

            //Buttons
            _vm.Download = new RelayCommand(o => _vm.StartDownload(),                
                o => !string.IsNullOrWhiteSpace(_vm.Source) && !string.IsNullOrWhiteSpace(_vm.TargetFolder)); //Download button is only enabled when both a source and target have been chosen.

            _vm.ShowLog = SharedEventHandlingLogic.CreateLogCommand(_vm, this, "Reddit");

            _vm.SelectFolder = new RelayCommand(o =>
            {
                var dialog = new FolderBrowserDialog();
                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    _vm.TargetFolder = dialog.SelectedPath;
                }
            });
            
            _vm.PullCommand.Execute(null);

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

        private void PostAmount_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PostAmount.Text)) PostAmount.Text = "0";
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
