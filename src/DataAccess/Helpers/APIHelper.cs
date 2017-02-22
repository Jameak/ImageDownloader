using System.Collections.Generic;

namespace DataAccess.Helpers
{
    /// <summary>
    /// Helper for allowing deserialization of json responses into objects automatically
    /// without making additional models. For deserizalition-purposes only. 
    /// </summary>
    public class ApiHelper<T>
    {
        public virtual T Data { get; set; }

        public virtual ICollection<T> Children { get; set; }

        public virtual string After { get; set; }
    }
}
