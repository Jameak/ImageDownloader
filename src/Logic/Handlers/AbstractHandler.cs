using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Responses;

namespace Logic.Handlers
{
    public abstract class AbstractHandler<T,K> : IHandler<T,K> where T : class where K : IApiCollection<IApiImage>
    {
        /// <summary>
        /// Ensure that the typeparam T given by the consumer is a delegate.
        /// </summary>
        static AbstractHandler()
        {
            if (!typeof(T).IsSubclassOf(typeof(Delegate)))
            {
                throw new InvalidOperationException(typeof(T).Name + " must be a delegate type");
            }
        }

        public abstract Task<K> ParseSource(string source, bool allowNestedCollections = true, int? amount = null);
        public abstract Task FetchContent(K parsedSource, string targetFolder, T filter, ICollection<string> outputLog);
    }
}
