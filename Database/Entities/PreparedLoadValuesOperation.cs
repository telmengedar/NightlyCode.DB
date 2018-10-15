using System;
using System.Collections.Generic;
using System.Linq;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Operations;
using Converter = NightlyCode.Database.Extern.Converter;

namespace NightlyCode.Database.Entities {
    /// <summary>
    /// a prepared load values operation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PreparedLoadValuesOperation<T> {
        readonly IDBClient dbclient;
        readonly PreparedOperation operation;

        /// <summary>
        /// creates a new <see cref="PreparedLoadValuesOperation{T}"/>
        /// </summary>
        /// <param name="dbclient">database access</param>
        /// <param name="statement">prepared statement</param>
        public PreparedLoadValuesOperation(IDBClient dbclient, PreparedOperation statement) {
            this.dbclient = dbclient;
            operation = statement;
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <returns></returns>
        public Clients.Tables.DataTable Execute(Transaction transaction)
        {
            return dbclient.Query(transaction, operation.CommandText, operation.Parameters);
        }

        /// <summary>
        /// executes the statement
        /// </summary>
        /// <returns></returns>
        public Clients.Tables.DataTable Execute() {
            return dbclient.Query(operation.CommandText, operation.Parameters);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns></returns>
        public TScalar ExecuteScalar<TScalar>(Transaction transaction)
        {
            return Converter.Convert<TScalar>(dbclient.Scalar(transaction, operation.CommandText, operation.Parameters), true);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns></returns>
        public TScalar ExecuteScalar<TScalar>() {
            return Converter.Convert<TScalar>(dbclient.Scalar(operation.CommandText, operation.Parameters), true);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns></returns>
        public TScalar ExecuteScalar<TScalar>(Transaction transaction, params object[] parameters) {
            if (parameters == null || parameters.Length == 0)
                parameters = operation.Parameters;
            return Converter.Convert<TScalar>(dbclient.Scalar(transaction, operation.CommandText, parameters), true);
        }

        /// <summary>
        /// executes the statement returning a scalar
        /// </summary>
        /// <returns></returns>
        public TScalar ExecuteScalar<TScalar>(params object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                parameters = operation.Parameters;

            return Converter.Convert<TScalar>(dbclient.Scalar(operation.CommandText, parameters), true);
        }

        /// <inheritdoc/>
        public override string ToString() {
            return operation.CommandText;
        }

        public IEnumerable<TScalar> ExecuteSet<TScalar>(Transaction transaction, params object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                parameters = operation.Parameters;

            foreach (object value in dbclient.Set(transaction, operation.CommandText, parameters))
                yield return Converter.Convert<TScalar>(value, true);
        }

        public IEnumerable<TScalar> ExecuteSet<TScalar>(params object[] parameters) {
            if (parameters == null || parameters.Length == 0)
                parameters = operation.Parameters;

            foreach (object value in dbclient.Set(operation.CommandText, parameters))
                yield return Converter.Convert<TScalar>(value, true);
        }

        /// <summary>
        /// executes a query and stores the result in a custom result type
        /// </summary>
        /// <typeparam name="TType">type of result</typeparam>
        /// <param name="assignments">action used to assign values</param>
        /// <returns>enumeration of result types</returns>
        public IEnumerable<TType> ExecuteType<TType>(Action<Clients.Tables.DataRow, TType> assignments)
            where TType : new()
        {
            Clients.Tables.DataTable table = Execute();
            foreach(Clients.Tables.DataRow row in table.Rows) {
                TType type = new TType();
                assignments(row, type);
                yield return type;
            }
        }
    }
}