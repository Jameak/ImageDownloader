using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Viewmodel for <see cref="RedditControl"/>
    /// </summary>
    public class RedditControlViewModel : BaseControlProperties
    {
        private readonly IHandler<RedditHandler.RedditFilter, RedditListing> _downloader;
        private string _pullParam = "";
        private string _topParam = "all";
        private bool _includeNSFW;
        private bool _skipAlbums;
        private bool _customQuery;
        private int _postsToGet = 25;
        private bool _saveAlbumInNestedFolder;

        #region XAML binding properties
        public ICommand PullCommand { get; set; }
        public ICommand CustomQueryCommand { get; set; }
        public ICommand SkipAlbumsCommand { get; set; }

        public string PullParam { get => _pullParam; set { if (_pullParam != value) { _pullParam = value; OnPropertyChanged(); } } }

        public string TopParam { get => _topParam; set { if (_topParam != value) { _topParam = value; OnPropertyChanged(); } } }

        public bool IncludeNSFW { get => _includeNSFW; set { if (_includeNSFW != value) { _includeNSFW = value; OnPropertyChanged(); } } }

        public bool SkipAlbums { get => _skipAlbums; set { if (_skipAlbums != value) { _skipAlbums = value; OnPropertyChanged(); } } }
        
        public bool CustomQuery { get => _customQuery; set { if (_customQuery != value) { _customQuery = value; OnPropertyChanged(); } } }
        
        public int PostsToGet { get => _postsToGet; set { if (_postsToGet != value) { _postsToGet = value; OnPropertyChanged(); } } }

        public bool SaveAlbumInNestedFolder { get => _saveAlbumInNestedFolder; set { if (_saveAlbumInNestedFolder != value) { _saveAlbumInNestedFolder = value; OnPropertyChanged(); } } }
        #endregion

        public RedditControlViewModel(IHandler<RedditHandler.RedditFilter, RedditListing> downloader)
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
            RedditListing content;
            Log = new ThreadsafeObservableStringCollection();
            Log.Add("Contacting server and parsing responses");
            ProgressMaxValue = int.MaxValue;

            if (CustomQuery)
            {
                content = await _downloader.ParseSource(Source, !SkipAlbums, PostsToGet);
            }
            else
            {
                //Remove trailing slash
                Source = Source.Trim('/');

                var request = Source.Split('/').Last(); //Remove the unneeded info if the user has typed in full url, instead of just the subreddit.

                var modifier = PullParam != "top" ? $"/{PullParam}/.json" : $"/top/.json?sort=top&t={TopParam}";
                var query = request + modifier;
                content = await _downloader.ParseSource(query, !SkipAlbums, PostsToGet);
            }

            if (content?.GetImages() == null)
            {
                Log.Add("Unable to enumerate files");
                ProgressMaxValue = 0;
            }
            else
            {
                ProgressMaxValue = content.GetImages().Count();
                await
                    _downloader.FetchContent(content, TargetFolder, Filter, Log, SaveAlbumInNestedFolder);
            }

            IsIdle = true;
        }

        /// <summary>
        /// Provides a filter for deciding whether an image should be included or excluded.
        /// <see cref="RedditHandler.RedditFilter"/>
        /// </summary>
        private bool Filter(int height, int width, bool isNSFW, bool isAlbum, Tuple<int, int> aspectRatio)
        {
            if (!IncludeNSFW && isNSFW) return false;
            if (SkipAlbums && isAlbum) return false;

            return ResolutionFilter(height, width, aspectRatio);
        }
    }
}
