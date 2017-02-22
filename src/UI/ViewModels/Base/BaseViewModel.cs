using System.ComponentModel;
using System.Runtime.CompilerServices;
using UI.Properties;

namespace UI.ViewModels.Base
{
    /// <summary>
    /// Base version of a viewmodel that implements <see cref="INotifyPropertyChanged"/>
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
