﻿using System;
using System.Collections.Generic;

#if UNITY
using GoorooMania.Unity.DB.Entities;
#endif

namespace NightlyCode.Database.Entities {

    /// <summary>
    /// collection of custom db converters
    /// </summary>
    public static class DBConverterCollection {
#if UNITY
        static readonly Dictionary<Type, DBConverter> converters = new Dictionary<Type, DBConverter>();
#else
        static readonly Dictionary<Type, Tuple<Type, Func<object, object>, Func<object, object>>> converters = new Dictionary<Type, Tuple<Type, Func<object, object>, Func<object, object>>>();
#endif

        /// <summary>
        /// determines whether the collection contains a converter for the type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool ContainsConverter(Type type) {
            return converters.ContainsKey(type);
        }

        /// <summary>
        /// converts the value to a db value
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object ToDBValue(Type type, object value) {
#if UNITY
            return converters[type].ObjectToDB(value);
#else
            return converters[type].Item2(value);
#endif
        }

        /// <summary>
        /// converts the value from a db value
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object FromDBValue(Type type, object value) {
#if UNITY
            return converters[type].DBToObject(value);
#else
            return converters[type].Item3(value);
#endif
        }

        /// <summary>
        /// get type to be used in database
        /// </summary>
        /// <param name="propertyType">type to be analysed</param>
        /// <returns>type to be used when storing data in database</returns>
        public static Type GetDBType(Type propertyType) {
#if UNITY
            return converters[propertyType].DBType;
#else
            return converters[propertyType].Item1;
#endif
        }
    }
}