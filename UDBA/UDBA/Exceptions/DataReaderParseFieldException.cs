using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDBA.Exceptions
{
    public class DataReaderParseFieldException : Exception
    {
        public DataReaderParseFieldException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }
}
