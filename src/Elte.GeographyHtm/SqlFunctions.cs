using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using Microsoft.SqlServer.Server;

namespace Elte.GeographyHtm
{
    public partial class UserDefinedFunctions
    {
        protected const string RangeTableDefinition = "[i] int, [htmIDStart] bigint, [htmIDEnd] bigint, [partial] bit";
        protected const string RangeFillMethodName = "FillRange";

        [SqlFunction(Name = "CoverGeography",
                TableDefinition = RangeTableDefinition, FillRowMethodName = RangeFillMethodName,
                DataAccess = DataAccessKind.None, IsDeterministic = true, IsPrecise = false)]
        public static IEnumerable Cover(SqlGeography geo)
        {
            return CoverImpl(geo);
        }

        private static IndexedValue<Range>[] CoverImpl(SqlGeography geo)
        {
            try
            {
                var cb = new CoverBuilder(geo);
                cb.Execute();

                return Index(cb.GetRanges());
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected static IndexedValue<T>[] Index<T>(IList<T> values)
        {
            IndexedValue<T>[] res = new IndexedValue<T>[values.Count];

            for (int i = 0; i < values.Count; i++)
            {
                res[i] = new IndexedValue<T>(i, values[i]);
            }

            return res;
        }
    }
}