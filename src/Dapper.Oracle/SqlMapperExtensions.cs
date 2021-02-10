using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Dapper.Oracle
{
    public static class OracleTypeMapper
    {
        internal static Dictionary<Type, SqlMapper.ITypeHandler> Handlers =
            new Dictionary<Type, SqlMapper.ITypeHandler>();


        public static void AddTypeHandler(Type type, SqlMapper.ITypeHandler handler)
        {
            Handlers[type] = handler;
            SqlMapper.AddTypeHandler(type, handler);
        }

        public static void AddTypeHandler<T>(SqlMapper.ITypeHandler handler)
        {
            Handlers[typeof(T)] = handler;
            SqlMapper.AddTypeHandler(typeof(T), handler);
        }

        public static void AddTypeHandler<TType, THandler>() where THandler : SqlMapper.TypeHandler<TType>, new()
        {
            THandler handler = new THandler();
            Handlers[typeof(TType)] = handler;
            SqlMapper.AddTypeHandler(handler);
        }

        public static bool HasTypeHandler(Type type, out SqlMapper.ITypeHandler handler)
        {
            if (Handlers.ContainsKey(type))
            {
                handler = Handlers[type];
                return true;
            }
            else
            {
                handler = null;
                return false;
            }            
        }
    }
}