using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Oracle.Expressions
{
    internal class ObjectEnumWrapper<TObject, TEnumType> : ObjectWrapper<TObject, TEnumType>
    {
        private readonly string _enumType;
        private readonly string _propertyName;
        private readonly Type _objectType;


        public ObjectEnumWrapper(string enumType, string propertyName, Type objectType) : base(propertyName, objectType)
        {
            _enumType = enumType;
            _propertyName = propertyName;
            _objectType = objectType;
        }

        protected override Func<TObject, TEnumType> CreateGetter()
        {
            Type enumType = _objectType.Assembly.GetType($"{_objectType.Namespace}.{_enumType}");

            var inputVariable = Expression.Parameter(typeof(TObject));
            var converted = Expression.Convert(inputVariable, _objectType);
            var retreiver = Expression.Property(converted, _propertyName);
            var intValue = Expression.Convert(retreiver, typeof(int));
            var returnValue = Expression.Convert(intValue, typeof(TEnumType));

            return Expression.Lambda<Func<TObject, TEnumType>>(returnValue, inputVariable).Compile();
        }

        protected override Action<TObject, TEnumType> CreateSetter()
        {
            Type enumType = _objectType.Assembly.GetType($"{_objectType.Namespace}.{_enumType}");

            var inputVariable = Expression.Parameter(typeof(TObject));
            var inputVariable2 = Expression.Parameter(typeof(TEnumType));

            var intValue = Expression.Convert(inputVariable2, typeof(int));

            var enumValue = Expression.Convert(intValue, enumType);
            var convertExpression = Expression.Convert(inputVariable, _objectType);

            var expression = Expression.Assign(
                Expression.PropertyOrField(convertExpression, _propertyName),
                enumValue);

            return Expression.Lambda<Action<TObject, TEnumType>>(expression, inputVariable, inputVariable2).Compile();
        }
    }


    public class ObjectWrapper<TObject, TValue>
    {
        private readonly string _propertyName;
        private readonly Type _objectType;

        private Action<TObject, TValue> _setter;
        private Func<TObject, TValue> _getter;

        public ObjectWrapper(string propertyName, Type objectType)
        {
            _propertyName = propertyName;
            _objectType = objectType;
        }

        public void SetValue(TObject command, TValue value)
        {
            if (_setter == null)
            {
                _setter = CreateSetter();
            }

            _setter(command, value);

        }

        public TValue GetValue(TObject obj)
        {
            if (_getter == null)
            {
                _getter = CreateGetter();
            }

            return _getter(obj);
        }

        protected virtual Func<TObject, TValue> CreateGetter()
        {
            var inputVariable = Expression.Parameter(typeof(TObject));
            var retreiver = Expression.Property(inputVariable, typeof(TObject).GetProperty(_propertyName));
            return Expression.Lambda<Func<TObject, TValue>>(retreiver, inputVariable).Compile();
        }


        protected virtual Action<TObject, TValue> CreateSetter()
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
