using UDBA.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDBA
{
    /// <summary>
    /// Class with DataReader extensions
    /// </summary>
    public static class DataReaderExtension
    {

        #region Get methods
        /// <summary>
        /// Get datareader value
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="dr">Datareader</param>
        /// <param name="column">Column name</param>
        /// <returns>Returns the datareader value</returns>
        /// <url>https://www.extensionmethod.net/csharp/idatareader/get</url>
        public static T Get<T>(this IDataReader dr, string column)
        {
            return dr.Get(column, default(T));
        }

        /// <summary>
        /// Get datareader value
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="dr">Datareader</param>
        /// <param name="columnIndex">Column index</param>
        /// <returns>Returns the datareader value</returns>
        /// <url>https://www.extensionmethod.net/csharp/idatareader/get</url>
        public static T Get<T>(this IDataReader dr, int columnIndex)
        {
            return dr.Get(columnIndex, default(T));
        }

        /// <summary>
        /// Get datareader value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr">DataReader</param>
        /// <param name="columnIndex">Column indexs</param>
        /// <param name="defaultValue">Default value when data is null</param>
        /// <returns>Returns the datareader value or the default value</returns>
        /// <url>https://www.extensionmethod.net/csharp/idatareader/get</url>
        public static T Get<T>(this IDataReader dr, string column, T defaultValue)
        {
            try
            {
                int ordinal = dr.GetOrdinal(column);

                object value = dr[ordinal];

                if (dr.IsDBNull(ordinal))
                {
                    value = defaultValue;
                }

                return (T)Convert.ChangeType(value, typeof(T));

            }
            catch (Exception e)
            {
                throw new DataReaderParseFieldException($"Error when converting attribute values: [{column}] for type [{typeof(T)}]", e);
            }
        }

        /// <summary>
        /// Get datareader value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr">DataReader</param>
        /// <param name="column">Column name</param>
        /// <param name="defaultValue">Default value when data is null</param>
        /// <returns>Returns the datareader value or the default value</returns>
        /// <url>https://www.extensionmethod.net/csharp/idatareader/get</url>
        public static T Get<T>(this IDataReader dr, int columnIndex, T defaultValue)
        {
            try
            {
                int ordinal = columnIndex;

                object value = dr[ordinal];

                if (dr.IsDBNull(ordinal))
                {
                    value = defaultValue;
                }

                return (T)Convert.ChangeType(value, typeof(T));

            }
            catch (Exception e)
            {
                throw new DataReaderParseFieldException($"Error when converting attribute values: [{columnIndex}] for type [{typeof(T)}]", e);
            }
        }

        #endregion

        #region Columns methods

        /// <summary>
        /// Checks if a column exists in the DataReader
        /// </summary>
        /// <param name="dr">DataReader</param>
        /// <param name="ColumnName">Name of the column to find</param>
        /// <returns>Returns true if the column exists in the DataReader, else returns false</returns>
        /// <url>https://www.extensionmethod.net/csharp/idatareader/columnexists</url>
        public static Boolean ColumnExists(this IDataReader dr, String ColumnName)
        {
            for (Int32 i = 0; i < dr.FieldCount; i++)
                if (dr.GetName(i).Equals(ColumnName, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        #endregion

    }
}
