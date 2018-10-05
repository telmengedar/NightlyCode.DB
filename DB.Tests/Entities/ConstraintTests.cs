﻿using Microsoft.Data.Sqlite;
using NightlyCode.DB.Clients;
using NightlyCode.DB.Entities;
using NightlyCode.DB.Entities.Operations;
using NightlyCode.DB.Tests.Models;
using NUnit.Framework;

namespace NightlyCode.DB.Tests.Entities
{

    [TestFixture]
    public class ConstraintTests
    {

        [Test]
        public void MultiUniqueDoesntAffectSingleColumns()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.Model<ValueModel>().Unique(u => u.Integer, u => u.String);
            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.Insert<ValueModel>()
                .Columns(u => u.Integer, u => u.String, u => u.Single, u => u.Double)
                .Values(0, "String1", 0.0f, 0.0).Execute();
            entitymanager.Insert<ValueModel>()
                .Columns(u => u.Integer, u => u.String, u => u.Single, u => u.Double)
                .Values(0, "String2", 0.0f, 0.0).Execute();
            entitymanager.Insert<ValueModel>()
                .Columns(u => u.Integer, u => u.String, u => u.Single, u => u.Double)
                .Values(0, "String3", 0.0f, 0.0).Execute();

            Assert.AreEqual(3, entitymanager.Load<ValueModel>(DBFunction.Count).ExecuteScalar<int>());
        }

        [Test]
        public void MultiUniqueFailsOnDoubleInsert()
        {
            IDBClient dbclient = TestData.CreateDatabaseAccess();
            EntityManager entitymanager = new EntityManager(dbclient);

            entitymanager.Model<ValueModel>().Unique(u => u.Integer, u => u.String);
            entitymanager.UpdateSchema<ValueModel>();

            entitymanager.Insert<ValueModel>()
                .Columns(u => u.Integer, u => u.String, u => u.Single, u => u.Double)
                .Values(0, "String1", 0.0f, 0.0).Execute();
            Assert.Throws<SqliteException>(() =>
            {
                entitymanager.Insert<ValueModel>()
                    .Columns(u => u.Integer, u => u.String, u => u.Single, u => u.Double)
                    .Values(0, "String1", 0.0f, 0.0).Execute();
            });
        }
    }
}