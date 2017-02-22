using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DataAccess;
using DataAccess.Exceptions;
using Logic;
using UI.UserControls;
using UI.ViewModels.Base;

namespace UI.ViewModels
{
    /// <summary>
    /// Viewmodel for <see cref="SettingsControl"/>
    /// </summary>
    public class SettingsControlViewModel : BaseViewModel
    {
        public ISettingsManager Settings { private get; set; } = SettingsAccess.GetInstance();

        private string _imgurSetting;
        private string _supportedExtensions;
        private int _fallbackImageDimensionsForNonsupportedImageTypes_Width;
        private int _fallbackImageDimensionsForNonsupportedImageTypes_Height;
        private int _maxClientLimit;
        private int _maxUserLimit;
        private int _remainingClientLimit;
        private int _remainingUserLimit;

        #region XAML binding properties
        public string ImgurSetting { get { return _imgurSetting; } set { if (_imgurSetting != value) { _imgurSetting = value; OnPropertyChanged(); } } }

        public string ImgurTooltip { get; private set; } = "A default shared Imgur ID is included for convenience, but for heavy use you should consider exchanging it with your own to avoid the shared rate-limiting. Requires restart for change to take effect.";
        
        public string SupportedExtensions { get { return _supportedExtensions; } set { if (_supportedExtensions != value) { _supportedExtensions = value; OnPropertyChanged(); } } }

        public string SupportedExtensionsTooltip { get; private set; } = "The file extensions to consider images during download and local filtering.";
        
        public int FallbackImageDimensionsForNonsupportedImageTypes_Width { get { return _fallbackImageDimensionsForNonsupportedImageTypes_Width; } set { if (_fallbackImageDimensionsForNonsupportedImageTypes_Width != value) { _fallbackImageDimensionsForNonsupportedImageTypes_Width = value; OnPropertyChanged(); } } }

        public int FallbackImageDimensionsForNonsupportedImageTypes_Height { get { return _fallbackImageDimensionsForNonsupportedImageTypes_Height; } set { if (_fallbackImageDimensionsForNonsupportedImageTypes_Height != value) { _fallbackImageDimensionsForNonsupportedImageTypes_Height = value; OnPropertyChanged(); } } }

        public string FallbackDimensionsTooltip { get; private set; } = "The default dimensions for files included by the SupportedExtensions-setting but non-supported by System.Drawing.Image which is used to find the dimensions from the raw byte-array";

        public int MaxClientLimit { get { return _maxClientLimit; } set { if (_maxClientLimit != value) { _maxClientLimit = value; OnPropertyChanged(); } } }

        public int MaxUserLimit { get { return _maxUserLimit; } set { if (_maxUserLimit != value) { _maxUserLimit = value; OnPropertyChanged(); } } }
        
        public int RemainingClientLimit { get { return _remainingClientLimit; } set { if (_remainingClientLimit != value) { _remainingClientLimit = value; OnPropertyChanged(); } } }

        public int RemainingUserLimit { get { return _remainingUserLimit; } set { if (_remainingUserLimit != value) { _remainingUserLimit = value; OnPropertyChanged(); } } }
        
        public string VersionNumber { get; private set; }

        public ICommand SaveSettings { get; set; }
        public ICommand RestoreDefaults { get; set; }
        public ICommand RefreshLimits { get; set; }
        public ICommand LoadSettings { get; set; }
        #endregion

        public SettingsControlViewModel(Ratelimiter limitHandler)
        {
            VersionNumber = Settings.GetVersionNumber();

            RefreshLimits = new RelayCommand(async o =>
            {
                try
                {
                    var info = await limitHandler.GetImgurLimitInfo();
                    MaxClientLimit = info.Item1;
                    MaxUserLimit = info.Item2;
                    RemainingClientLimit = info.Item3;
                    RemainingUserLimit = info.Item4;
                }
                catch (InvalidClientIDException)
                {
                    MessageBox.Show("The provided Imgur ID is invalid. All subsequent API calls to the Imgur will fail.\nPlease change the Imgur ID and restart the program.", "ImageDownloader - Invalid Imgur ID");
                    MaxClientLimit = -1;
                    MaxUserLimit = -1;
                    RemainingClientLimit = -1;
                    RemainingUserLimit = -1;
                }
            });

            RestoreDefaults = new RelayCommand(o =>
            {
                Settings.ResetDefaults();
                LoadSettings.Execute(null);
            });

            SaveSettings = new RelayCommand(o =>
            {
                Settings.SetImgurClientID(string.IsNullOrWhiteSpace(ImgurSetting) ? string.Empty : ImgurSetting);

                Settings.SetFallbackHeight(FallbackImageDimensionsForNonsupportedImageTypes_Height);
                Settings.SetFallbackWidth(FallbackImageDimensionsForNonsupportedImageTypes_Width);

                var stringcollection = new StringCollection();
                stringcollection.AddRange(SupportedExtensions.ToLower().Split(','));
                Settings.SetSupportedExtensions(stringcollection);

                Settings.Save();
                LoadSettings.Execute(null);
            });

            LoadSettings = new RelayCommand(o =>
            {
                ImgurSetting = Settings.GetImgurClientID();
                FallbackImageDimensionsForNonsupportedImageTypes_Height = Settings.GetFallbackHeight();
                FallbackImageDimensionsForNonsupportedImageTypes_Width = Settings.GetFallbackWidth();

                var builder = new StringBuilder();
                foreach (var ext in Settings.GetSupportedExtensions())
                {
                    builder.Append(ext);
                    builder.Append(",");
                }

                SupportedExtensions = builder.ToString().TrimEnd(',');
            });
        }
    }
}
