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

        public ICommand SelectSourceFolder { get; set; }

        public LocalControlViewModel(IHandler<LocalHandler.LocalFilter, LocalDirectory> downloader)
        {
            _downloader = downloader;
        }

        public async void StartDownload()
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
                await _downloader.FetchContent(content, TargetFolder, ResolutionFilter, Log);
            }

            IsIdle = true;
        }
    }
}
