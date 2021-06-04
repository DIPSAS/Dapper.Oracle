using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dapper.Oracle.Util
{
    internal static class ConnectionExtensions
    {
        private static Dictionary<Type, Action<IDbConnection, string>> setters =
            new Dictionary<Type, Action<IDbConnection, string>>();

        public static T DownCastConnection<T>(this T connection) where T : IDbConnection
        {
            return DownCaster<T>.DownCast(connection);
        }

        public static T DownCastCommand<T>(this T command) where T : IDbCommand
        {
            return DownCaster<T>.DownCast(command);
        }


    }

    internal static class DownCaster<T>
    {
        private static Dictionary<Type, Tuple<bool, Func<T, T>>> dictionary =
            new Dictionary<Type, Tuple<bool, Func<T, T>>>();

        internal static T DownCast(T obj)
        {
            return GetInner(obj);
        }

        private static T GetInner(T obj)
        {
            if (obj == null)
            {
                return default(T);
            }

            var type = obj.GetType();
            if (dictionary.TryGetValue(type, out var info))
            {
                if (info.Item1 == false)
                {
                    return obj;
                }

                return GetInner(info.Item2(obj));
            }

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(pi => pi.CanRead && typeof(T).IsAssignableFrom(pi.PropertyType)).ToArray();

            if (properties.Length > 1)
            {
                throw new Exception($"More than one inner property of type {typeof(T)}");
            }

            if (properties.Length == 0)
            {
                dictionary[type] = new Tuple<bool, Func<T, T>>(false, null);
                return obj;
            }

            var property = properties[0];

            dictionary[type] =
                new Tuple<bool, Func<T, T>>(true, new PropertyWrapper<T, T>(property.Name, type).CreateGetter());

            return GetInner(dictionary[type].Item2(obj));
        }

    }

    internal class PropertyWrapper<TObject, TValue>
    {
        private readonly string _propertyName;
        private readonly Type _objectType;

        public PropertyWrapper(string propertyName, Type objectType)
        {
            _propertyName = propertyName;
            _objectType = objectType;
        }

        public Func<TObject, TValue> CreateGetter()
        {
            var inputVariable = Expression.Parameter(typeof(TObject));
            if (typeof(TObject) != _objectType)
            {
                var converted = Expression.Convert(inputVariable, _objectType);
                var retreiver = Expression.Property(converted, _objectType.GetProperty(_propertyName));
                return Expression.Lambda<Func<TObject, TValue>>(retreiver, inputVariable).Compile();
            }
            else
            {
                var retreiver = Expression.Property(inputVariable, _objectType.GetProperty(_propertyName));
                return Expression.Lambda<Func<TObject, TValue>>(retreiver, inputVariable).Compile();
            }
        }

        public Action<TObject, TValue> CreateSetter()
        {
            var inputVariable = Expression.Parameter(typeof(TObject));
            var inputVariable2 = Expression.Parameter(typeof(TValue));

            var convertExpression = Expression.Convert(inputVariable, _objectType);

            var expression = Expression.Assign(
                Expression.PropertyOrField(convertExpression, _propertyName),
                inputVariable2);

            return Expression.Lambda<Action<TObject, TValue>>(expression, inputVariable, inputVariable2).Compile();
        }
    }
}
