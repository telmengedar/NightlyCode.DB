using System.Collections.Generic;
using System.Data;
using System.Linq;
using NightlyCode.DB.Clients;
using NightlyCode.DB.Entities.Operations;
using Converter = NightlyCode.DB.Extern.Converter;

namespace NightlyCode.DB.Entities {
    /// <summary>
    /// a prepared load values operation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PreparedLoadValuesOperation<T> {
        readonly IDBClient dbclient;
        readonly PreparedOperation operation;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbclient"></param>
        /// <param name="statement"></param>
        public PreparedLoadValuesOperation(IDBClient dbclient, PreparedOperation statement) {
            this.dbclient = dbclient;
            operation = statement;
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <returns></returns>
        public DataTable Execute() {
            return dbclient.Query(operation.CommandText, operation.Parameters.Select(p => p.Value).ToArray());
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns></returns>
        public TScalar ExecuteScalar<TScalar>() {
            return Converter.Convert<TScalar>(dbclient.Scalar(operation.CommandText, operation.Parameters.Select(p => p.Value).ToArray()), true);
        }

        public override string ToString() {
            return operation.CommandText;
        }

        public IEnumerable<TScalar> ExecuteSet<TScalar>() {
            foreach(object value in dbclient.Set(operation.CommandText, operation.Parameters.Select(p => p.Value).ToArray()))
                yield return Converter.Convert<TScalar>(value, true);
        }
    }
}