﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NightlyCode.Database.Clients;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Expressions;
using NightlyCode.Database.Entities.Operations.Fields;
using NightlyCode.Database.Entities.Operations.Prepared;

namespace NightlyCode.Database.Entities.Operations {

    /// <summary>
    /// operation used to load entities
    /// </summary>
    public class LoadEntitiesOperation<T> {
        readonly IDBClient dbclient;
        readonly Func<Type, EntityDescriptor> descriptorgetter;
        OrderByCriteria[] orderbycriterias;
        IDBField[] groupbycriterias;
        readonly List<JoinOperation> joinoperations = new List<JoinOperation>();
 
        /// <summary>
        /// creates a new <see cref="LoadEntitiesOperation{T}"/>
        /// </summary>
        /// <param name="origin"></param>
        internal LoadEntitiesOperation(LoadEntitiesOperation<T> origin)
            : this(origin.dbclient, origin.descriptorgetter) {
            orderbycriterias = origin.orderbycriterias;
            groupbycriterias = origin.groupbycriterias;
            joinoperations = origin.joinoperations;
            LimitStatement = origin.LimitStatement;
            Criterias = origin.Criterias;
            Havings = origin.Havings;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbclient"> </param>
        /// <param name="descriptorgetter"></param>
        public LoadEntitiesOperation(IDBClient dbclient, Func<Type, EntityDescriptor> descriptorgetter) {
            this.descriptorgetter = descriptorgetter;
            this.dbclient = dbclient;
        }

        /// <summary>
        /// result limit
        /// </summary>
        protected LimitField LimitStatement { get; set; }

        /// <summary>
        /// operations to join
        /// </summary>
        protected List<JoinOperation> JoinOperations => joinoperations;

        /// <summary>
        /// criterias to use when loading
        /// </summary>
        protected Expression Criterias { get; set; }

        /// <summary>
        /// having criterias
        /// </summary>
        protected Expression Havings { get; set; }

        /// <summary>
        /// loads entities using the operation
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> Execute() {
            return Prepare().Execute();
        }

        /// <summary>
        /// prepares the operation for execution
        /// </summary>
        /// <returns></returns>
        public PreparedLoadEntitiesOperation<T> Prepare() {
            OperationPreparator preparator = new OperationPreparator();
            preparator.AppendText("SELECT");

            string columnindicator = dbclient.DBInfo.ColumnIndicator;

            EntityDescriptor descriptor = descriptorgetter(typeof(T));
            preparator.AppendText(string.Join(", ", descriptor.Columns.Select(c => $"{columnindicator}{c.Name}{columnindicator}")));
            preparator.AppendText("FROM").AppendText(descriptor.TableName);

            if(joinoperations.Count>0) {
                foreach(JoinOperation operation in joinoperations) {
                    preparator.AppendText("INNER JOIN").AppendText(descriptorgetter(operation.JoinType).TableName);
                    if (!string.IsNullOrEmpty(operation.Alias))
                        preparator.AppendText("AS").AppendText(operation.Alias);
                    preparator.AppendText("ON");
                    CriteriaVisitor.GetCriteriaText(operation.Criterias, descriptorgetter, dbclient.DBInfo, preparator, new Tuple<Type, string>(operation.JoinType, operation.Alias));
                }
            }

            if(Criterias != null) {
                preparator.AppendText("WHERE");
                CriteriaVisitor.GetCriteriaText(Criterias, descriptorgetter, dbclient.DBInfo, preparator);
            }

            bool flag = true;
            if(groupbycriterias != null) {
                preparator.AppendText("GROUP BY");

                foreach(IDBField criteria in groupbycriterias) {
                    if(flag) flag = false;
                    else preparator.AppendText(",");
                    dbclient.DBInfo.Append(criteria, preparator, descriptorgetter);
                }
                
            }

            flag = true;
            if (orderbycriterias != null)
            {
                preparator.AppendText("ORDER BY");

                foreach (OrderByCriteria criteria in orderbycriterias)
                {
                    if (flag) flag = false;
                    else preparator.AppendText(",");
                    dbclient.DBInfo.Append(criteria.Field, preparator, descriptorgetter);

                    if (!criteria.Ascending)
                        preparator.AppendText("DESC");
                }
            }

            if (Havings != null) {
                preparator.AppendText("HAVING");
                CriteriaVisitor.GetCriteriaText(Havings, descriptorgetter, dbclient.DBInfo, preparator);
            }

            if (!ReferenceEquals(LimitStatement, null)) {
                dbclient.DBInfo.Append(LimitStatement, preparator, descriptorgetter);
            }

            return preparator.GetLoadEntitiesOperation<T>(dbclient, descriptor);
        }

        /// <summary>
        /// specifies criterias for the operation
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public LoadEntitiesOperation<T> Where(Expression<Func<T, bool>> criterias) {
            Criterias = criterias;
            return this;
        }

        /// <summary>
        /// specifies criterias for the operation
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public LoadEntitiesOperation<T> Where(Expression criterias)
        {
            Criterias = criterias;
            return this;
        }

        /// <summary>
        /// specifies having criterias for the operation
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public LoadEntitiesOperation<T> Having(Expression<Predicate<T>> criterias) {
            Havings = criterias;
            return this;
        }

        /// <summary>
        /// specifies an order
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public LoadEntitiesOperation<T> OrderBy(params OrderByCriteria[] fields) {
            if(fields.Length == 0)
                throw new InvalidOperationException("at least one criteria has to be specified");

            orderbycriterias = fields;
            return this;
        }

        /// <summary>
        /// groups the results by the specified fields
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public LoadEntitiesOperation<T> GroupBy(params IDBField[] fields) {
            if(fields.Length==0)
                throw new InvalidOperationException("at least one group criteria has to be specified");

            groupbycriterias = fields;
            return this;
        }

        /// <summary>
        /// specifies a limited number of rows to return
        /// </summary>
        /// <param name="limit">number of rows to return</param>
        public LoadEntitiesOperation<T> Limit(long limit) {
            if (ReferenceEquals(LimitStatement, null))
                LimitStatement = new LimitField();
            LimitStatement.Limit = limit;
            return this;
        }

        /// <summary>
        /// specifies an offset from which on to return result rows
        /// </summary>
        /// <param name="offset">number of rows to skip</param>
        public LoadEntitiesOperation<T> Offset(long offset)
        {
            if (ReferenceEquals(LimitStatement, null))
                LimitStatement = new LimitField();
            LimitStatement.Offset = offset;
            return this;
        }

        /// <summary>
        /// joins another type to the operation
        /// </summary>
        /// <typeparam name="TJoin">entity to join</typeparam>
        /// <param name="criteria">predicate for criteria</param>
        /// <returns></returns>
        public LoadEntitiesOperation<T, TJoin> Join<TJoin>(Expression<Func<T, TJoin, bool>> criteria) {
            joinoperations.Add(new JoinOperation(typeof(TJoin), criteria, $"j{joinoperations.Count}"));
            return new LoadEntitiesOperation<T, TJoin>(this);
        }
    }

    /// <summary>
    /// operation used to load entities based on a join operation
    /// </summary>
    /// <typeparam name="TLoad"></typeparam>
    /// <typeparam name="TJoin"></typeparam>
    public class LoadEntitiesOperation<TLoad, TJoin> : LoadEntitiesOperation<TLoad> {

        internal LoadEntitiesOperation(LoadEntitiesOperation<TLoad> origin)
            : base(origin) {
        }

        /// <summary>
        /// specifies criterias for the operation
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public LoadEntitiesOperation<TLoad, TJoin> Where(Expression<Func<TLoad, TJoin, bool>> criterias) {
            Criterias = criterias;
            return this;
        }

        /// <summary>
        /// specifies an order
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public new LoadEntitiesOperation<TLoad, TJoin> OrderBy(params OrderByCriteria[] fields) {
            base.OrderBy(fields);
            return this;
        }

        /// <summary>
        /// groups the results by the specified fields
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public new LoadEntitiesOperation<TLoad, TJoin> GroupBy(params IDBField[] fields) {
            base.GroupBy(fields);
            return this;
        }

        /// <summary>
        /// specifies a limited number of rows to return
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public LoadEntitiesOperation<TLoad, TJoin> Limit(int limit) {
            base.Limit(limit);
            return this;
        }

        /// <summary>
        /// specifies having criterias for the operation
        /// </summary>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public LoadEntitiesOperation<TLoad, TJoin> Having(Expression<Func<TLoad, TJoin, bool>> criterias) {
            Havings = criterias;
            return this;
        }

    }
}