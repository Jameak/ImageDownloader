using System.Windows.Controls;
using System.Windows.Input;

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

        public ICommand SelectFolder { get; set; }
        public RelayCommand Download { get; set; }
        #endregion

        /// <summary>
        /// Provides a mechanism for deciding whether the given height
        /// and width satisfies the specified restrictions.
        /// </summary>
        /// <returns>Returns true if the restrictions are satisfied</returns>
        protected virtual bool ResolutionFilter(int height, int width)
        {
            var validHeightMax = true;
            var validHeightMin = true;
            var validWidthMax = true;
            var validWidthMin = true;

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

            return validHeightMax && validHeightMin && validWidthMin && validWidthMax;
        }
    }
}
