using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Exceptions
{
    /// <summary>
    /// Signifies that an invalid Client-ID has been used to carry out a request.
    /// </summary>
    public class InvalidClientIDException : Exception
    {
        public InvalidClientIDException(string msg) : base(msg)
        {

        }

        public InvalidClientIDException() : base()
        {

        }
    }
}
