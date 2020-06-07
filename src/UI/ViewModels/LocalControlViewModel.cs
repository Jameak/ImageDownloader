using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DataAccess.Responses.Impl;
using Logic;
using Logic.Handlers;
using UI.UserControls;
using UI.ViewModels.Base;

namespace UI.ViewModels
{
    /// <summary>
    /// Viewmodel for <see cref="LocalControl"/>
    /// </summary>
    public class LocalControlViewModel : BaseControlProperties
    {
        private readonly IHandler<LocalHandler.LocalFilter, LocalDirectory> _downloader;
        private bool _preserveFolderHierarchy;

        public bool PreserveFolderHierarchy { get => _preserveFolderHierarchy; set { if (_preserveFolderHierarchy != value) { _preserveFolderHierarchy = value; OnPropertyChanged(); } } }
        public ICommand SelectSourceFolder { get; set; }

        public LocalControlViewModel(IHandler<LocalHandler.LocalFilter, LocalDirectory> downloader)
        {
            _downloader = downloader;
        }

        public override async void StartDownload()
        {
            if (!TryParseAspectRatio())
            {
                MessageBox.Show("The given aspect ratio is not valid.", "ImageDownloader");
                return;
            }

            IsIdle = false;

            Log = new ThreadsafeObservableStringCollection();
            Log.Add("Enumerating files");
            ProgressMaxValue = int.MaxValue;

            var content = await _downloader.ParseSource(Source);
            if (content?.GetImages() == null)
            {
                Log.Add("Unable to enumerate files");
                ProgressMaxValue = 0;
            }
            else
            {
                ProgressMaxValue = content.GetImages().Count();
                try
                {
                    await _downloader.FetchContent(content, TargetFolder, ResolutionFilter, Log, PreserveFolderHierarchy);
                }
                catch (Exception e)
                {
                    Log.Add("Unhandled error occurred during operation. Aborting.");
                    var message = "Unhandled error occurred during operation.\n" +
                                  "Operation has been aborted.\n\n" +
                                  "To get the the error-message and stacktrace\n" +
                                  "copied to your clipboard, click 'OK'.\n" +
                                  "If you want to report this error, please provide\n" +
                                  "the error message in an issue on\n" +
                                  "Github.com/Jameak/ImageDownloader";
                    var result = MessageBox.Show(message, "ImageDownloader", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        Clipboard.SetText(e.ToString());
                        MessageBox.Show("Exception text copied to clipboard", "ImageDownloader", MessageBoxButton.OK);
                    }
                }
            }

            IsIdle = true;
        }
    }
}
