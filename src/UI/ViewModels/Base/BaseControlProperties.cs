using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DataAccess.Helpers;

namespace UI.ViewModels.Base
{
    /// <summary>
    /// Provides the properties that many of the programs <see cref="UserControl"/>s share
    /// </summary>
    public abstract class BaseControlProperties : BaseViewModel
    {
        protected string _source;
        protected string _target;
        protected int? _heightMin;
        protected int? _heightMax;
        protected int? _widthMin;
        protected int? _widthMax;
        protected bool _isIdle = true;
        protected ThreadsafeObservableStringCollection _log = new ThreadsafeObservableStringCollection();
        protected int _progressMaxValue = 1;
        protected string _aspectRatio;

        protected int? _parsedAspectVal1;
        protected int? _parsedAspectVal2;

        #region XAML binding properties
        public string Source { get { return _source; } set { if (_source != value) { _source = value; OnPropertyChanged(); Download?.OnCanExecuteChanged(this); } } }

        public string TargetFolder { get { return _target; } set { if (_target != value) { _target = value; OnPropertyChanged(); Download?.OnCanExecuteChanged(this); } } }

        public int? HeightMin { get { return _heightMin; } set { if (_heightMin != value) { _heightMin = value; OnPropertyChanged(); } } }

        public int? HeightMax { get { return _heightMax; } set { if (_heightMax != value) { _heightMax = value; OnPropertyChanged(); } } }

        public int? WidthMin { get { return _widthMin; } set { if (_widthMin != value) { _widthMin = value; OnPropertyChanged(); } } }

        public int? WidthMax { get { return _widthMax; } set { if (_widthMax != value) { _widthMax = value; OnPropertyChanged(); } } }

        public bool IsIdle { get { return _isIdle; } set { if (_isIdle != value) { _isIdle = value; OnPropertyChanged(); } } }
        
        public ThreadsafeObservableStringCollection Log { get { return _log; } set { if (_log != value) { _log = value; OnPropertyChanged(); } } }

        public int ProgressMaxValue { get { return _progressMaxValue; } set { if (_progressMaxValue != value) { _progressMaxValue = value; OnPropertyChanged(); } } }

        public string AspectRatio { get { return _aspectRatio; } set { if (_aspectRatio != value) { _aspectRatio = value; OnPropertyChanged(); } } }

        public ICommand SelectFolder { get; set; }
        public RelayCommand Download { get; set; }
        public RelayCommand ShowLog { get; set; }
        #endregion

        /// <summary>
        /// Provides a mechanism for deciding whether the given height
        /// and width satisfies the specified restrictions.
        /// </summary>
        /// <returns>Returns true if the restrictions are satisfied</returns>
        protected virtual bool ResolutionFilter(int height, int width, Tuple<int,int> aspectRatio)
        {
            var validHeightMax = true;
            var validHeightMin = true;
            var validWidthMax = true;
            var validWidthMin = true;
            var validAspectRatio = true;

            if (HeightMax != null && height > HeightMax.Value)
            {
                validHeightMax = false;
            }

            if (HeightMin != null && height < HeightMin.Value)
            {
                validHeightMin = false;
            }

            if (WidthMax != null && width > WidthMax.Value)
            {
                validWidthMax = false;
            }

            if (WidthMin != null && width < WidthMin.Value)
            {
                validWidthMin = false;
            }

            if (_parsedAspectVal1 != null && _parsedAspectVal2 != null)
            {
                validAspectRatio = _parsedAspectVal1.Value == aspectRatio.Item1 && _parsedAspectVal2.Value == aspectRatio.Item2;
            }

            return validHeightMax && validHeightMin && validWidthMin && validWidthMax && validAspectRatio;
        }

        protected virtual bool TryParseAspectRatio()
        {
            _parsedAspectVal1 = null;
            _parsedAspectVal2 = null;

            if (!string.IsNullOrWhiteSpace(AspectRatio))
            {
                var regex = new Regex("^[0-9]{1,9}:[0-9]{1,9}$");
                if (!regex.IsMatch(AspectRatio))
                {
                    return false;
                }

                var vals = AspectRatio.Split(':');
                //Reduce the users aspect ratio so we can accurately compare them to the image aspects. e.g. 32:18 will be reduced to 16:9
                var reducedRatio = ImageHelper.GetAspectRatio(int.Parse(vals[0]), int.Parse(vals[1]));

                _parsedAspectVal1 = reducedRatio.Item1;
                _parsedAspectVal2 = reducedRatio.Item2;
            }

            return true;
        }
    }
}
