using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UI
{
    /// <summary>
    /// Provides a thread-safe wrapper of ObservableCollection that is still observable.
    /// Additionally, the contents of the wrapped ObservableCollection are exposed by ToString()
    /// for easy viewing in text-elements.
    /// </summary>
    public class ThreadsafeObservableStringCollection : ICollection<string>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly object _sync = new object();
        private readonly ObservableCollection<string> _collection = new ObservableCollection<string>();

        private StringBuilder _builder = new StringBuilder();

        public ThreadsafeObservableStringCollection()
        {
            //Propagate events from the observable collection
            _collection.CollectionChanged += (sender, args) => CollectionChanged?.Invoke(sender, args);
        }

        public override string ToString()
        {
            lock (_sync)
            {
                if(_builder == null) PopulateStringBuilder();

                return _builder.ToString();
            }
        }

        private void PopulateStringBuilder()
        {
            _builder = new StringBuilder();
            foreach (var item in _collection)
            {
                _builder.AppendLine(item);
            }
        }

        private void AddToBuilder(string item)
        {
            if (_builder == null)
            {
                PopulateStringBuilder();
            }
            else
            {
                _builder.AppendLine(item);
            }
        }

        #region Events
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ICollection<>

        public IEnumerator<string> GetEnumerator()
        {
            lock (_sync)
            {
                return new List<string>(_collection).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_sync)
            {
                return new List<string>(_collection).GetEnumerator();
            }
        }

        public void Add(string item)
        {
            lock (_sync)
            {
                _collection.Add(item);

                AddToBuilder(item);
                OnPropertyChanged(nameof(Count));
            }
        }

        public void Clear()
        {
            lock (_sync)
            {
                _collection.Clear();

                _builder = null;
                OnPropertyChanged(nameof(Count));
            }
        }

        public bool Contains(string item)
        {
            lock (_sync)
            {
                return _collection.Contains(item);
            }
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            lock (_sync)
            {
                _collection.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(string item)
        {
            lock (_sync)
            {
                var val = _collection.Remove(item);

                _builder = null;
                OnPropertyChanged(nameof(Count));
                return val;
            }
        }

        public int Count => _collection.Count;

        public bool IsReadOnly => false;

        #endregion
    }
}