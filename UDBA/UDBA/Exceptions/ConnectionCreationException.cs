using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDBA.Exceptions
{
    class ConnectionCreationException : Exception
    {
        public ConnectionCreationException(string message) : base(message)
        {
        }

        public ConnectionCreationException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }
}
