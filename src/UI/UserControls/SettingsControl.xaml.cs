using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataAccess;
using Microsoft.Extensions.DependencyInjection;
using UI.ViewModels;
using UI.ViewModels.Base;

namespace UI.UserControls
{
    public partial class SettingsControl : UserControl
    {
        public ISettingsManager Settings { private get; set; } = SettingsAccess.GetInstance();
        private SettingsControlViewModel _vm;

        public SettingsControl()
        {
            InitializeComponent();

            _vm = (Application.Current as App).Container.GetService<SettingsControlViewModel>();

            _vm.SaveSettings = new RelayCommand(o =>
            {
                if (Settings.GetImgurClientID() != _vm.ImgurSetting)
                {
                    MessageBox.Show("Changes to the imgur client-id require a restart to take effect.", "ImageDownloader");
                }

                Settings.SetImgurClientID(_vm.ImgurSetting);
                Settings.SetFallbackHeight(_vm.FallbackImageDimensionsForNonsupportedImageTypes_Height);
                Settings.SetFallbackWidth(_vm.FallbackImageDimensionsForNonsupportedImageTypes_Width);

                var stringcollection = new StringCollection();
                stringcollection.AddRange(_vm.SupportedExtensions.ToLower().Split(','));
                Settings.SetSupportedExtensions(stringcollection);

                Settings.Save();
                _vm.LoadSettings.Execute(null);
            });

            DataContext = _vm;
        }

        public void RefreshLimits()
        {
            _vm.RefreshLimits?.Execute(null);
        }

        public void LoadSettings()
        {
            _vm.LoadSettings?.Execute(null);
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

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Dimensions_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Width.Text)) Width.Text = "0";
            if (string.IsNullOrWhiteSpace(Height.Text)) Height.Text = "0";
        }
    }
}
