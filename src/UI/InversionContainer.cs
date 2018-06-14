using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI
{
    public class InversionContainer : IServiceProvider
    {
        private readonly Dictionary<Type, Func<IServiceProvider, dynamic>> _transients = new Dictionary<Type, Func<IServiceProvider, dynamic>>();
        private readonly Dictionary<Type, dynamic> _singletons = new Dictionary<Type, dynamic>();

        public void AddTransient<T>(Func<IServiceProvider, T> obj)
        {
            _transients.Add(typeof(T), obj as Func<IServiceProvider, dynamic>);
        }

        public void AddTransient<T, K>() where K :T, new()
        {
            AddTransient<T>(o => new K());
        }

        public void AddSingleton<T>(T obj)
        {
            _singletons.Add(typeof(T), obj);
        }

        public object GetService(Type serviceType)
        {
            if (_singletons.ContainsKey(serviceType))
            {
                _singletons.TryGetValue(serviceType, out object val);
                return val;
            }
            
            if(!_transients.TryGetValue(serviceType, out Func<IServiceProvider, dynamic> value))
            {
                throw new InvalidOperationException();
            }
            return value?.Invoke(this);
        }
    }

    public static class InversionContainerExtensions
    {
        public static T GetService<T>(this IServiceProvider serviceProvider) where T : class
        {
            return serviceProvider.GetService(typeof(T)) as T;
        }
    }
}
