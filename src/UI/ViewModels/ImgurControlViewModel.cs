using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DataAccess.Responses;
using Logic;
using Logic.Handlers;
using UI.UserControls;
using UI.ViewModels.Base;

namespace UI.ViewModels
{
    /// <summary>
    /// Viewmodel for <see cref="ImgurControl"/>
    /// </summary>
    public class ImgurControlViewModel : BaseControlProperties
    {
        private readonly IHandler<ImgurHandler.ImgurFilter, IApiCollection<IApiImage>> _downloader;
        
        public ImgurControlViewModel(IHandler<ImgurHandler.ImgurFilter, IApiCollection<IApiImage>> downloader)
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

            //Remove trailing slash
            Source = Source.Trim('/');

            Log = new ThreadsafeObservableStringCollection();
            Log.Add("Contacting server and parsing responses");
            ProgressMaxValue = int.MaxValue;

            var content = await _downloader.ParseSource(Source);
            if (content?.GetImages() == null)
            {
                Log.Add("Unable to contact server");
                ProgressMaxValue = 0;
            }
            else
            {
                ProgressMaxValue = content.GetImages().Count();
                try
                {
                    await
                        _downloader.FetchContent(content, TargetFolder, ResolutionFilter,
                            Log = new ThreadsafeObservableStringCollection());
                }
                catch (Exception e)
                {
                    Log.Add("Unhandled error occurred during downloading. Aborting.");
                    var message = "Unhandled error occurred during download.\n" +
                                  "Downloading has been aborted.\n\n" +
                                  "To get the the error-message and stacktrace\n" +
                                  "copied to your clipboard, click 'OK'.\n" +
                                  "If you want to report this error, please provide\n" +
                                  "the error message in an issue on\n" +
                                  "Github.com/Jameak/ImageDownloader";
                    var result = MessageBox.Show(message, "ImageDownloader", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        Clipboard.SetText(e.ToString());
                        MessageBox.Show("Error message copied to clipboard", "ImageDownloader", MessageBoxButton.OK);
                    }
                }
            }

            IsIdle = true;
        }
    }
}
