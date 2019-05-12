using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapper.Oracle.BulkSql
{
    internal static class OracleMapper
    {
        private static Dictionary<Type, OracleMappingType> OracleMappings = new Dictionary<Type, OracleMappingType>
        {
            {typeof(Guid), OracleMappingType.Raw},
            {typeof(string), OracleMappingType.Varchar2},
            {typeof(long), OracleMappingType.Long},
            {typeof(decimal), OracleMappingType.Decimal},
            {typeof(DateTime), OracleMappingType.Date},
            {typeof(int), OracleMappingType.Int32},
            {typeof(bool),OracleMappingType.Int16 }
        };

        public static OracleMappingType? GuessType(Type type)
        {
            if (OracleMappings.ContainsKey(type))
            {
                return OracleMappings[type];
            }

            return null;
        }

        public static OracleDynamicParameters Create<T>(IEnumerable<T> objects)
        {
            var type = typeof(T);
            var entities = objects.ToList();

            var odp = new OracleDynamicParameters();
            odp.ArrayBindCount = entities.Count;
            odp.BindByName = true;


            foreach (var pi in type.GetProperties())
            {
                if (pi.CanRead)
                {
                    var parameterName = pi.Name;
                    OracleMappingType? dbType = null;

                    Func<T, object> selector;
                    if (OracleMappings.ContainsKey(pi.PropertyType))
                    {
                        dbType = OracleMappings[pi.PropertyType];
                    }

                    if (pi.PropertyType == typeof(Guid))
                    {
                        selector = p => ((Guid)pi.GetValue(p)).ToByteArray();
                    }
                    else if (pi.PropertyType == typeof(bool))
                    {
                        selector = p => ((bool)pi.GetValue(p)) ? 1 : 0;
                    }
                    else
                    {
                        selector = p => pi.GetValue(p);
                    }

                    var values = entities.Select(selector).ToArray();
                    odp.Add(parameterName, values, dbType);
                }
            }

            return odp;
        }
    }
}
