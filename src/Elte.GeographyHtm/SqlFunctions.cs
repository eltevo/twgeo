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
        protected const string PointTableDefinition = "[i] int, [x] float, [y] float, [z] float, [ra] float, [dec] float";
        protected const string PointFillMethodName = "FillPoint";

        protected const string RangeTableDefinition = "[i] int, [htmIDStart] bigint, [htmIDEnd] bigint, [partial] bit";
        protected const string RangeFillMethodName = "FillRange";

        [SqlFunction(Name = "FromLonLat",
            DataAccess = DataAccessKind.None, IsDeterministic = true, IsPrecise = false)]
        public static SqlInt64 FromEq(SqlDouble lon, SqlDouble lat)
        {
            Trixel t = new Trixel(lon.Value, lat.Value);
            return t;
        }

        [SqlFunction(Name = "GetCenter",
            TableDefinition = PointTableDefinition, FillRowMethodName = PointFillMethodName,
            DataAccess = DataAccessKind.None, IsDeterministic = true, IsPrecise = false)]
        public static IEnumerable GetCenter(SqlInt64 htmID)
        {
            Trixel t = htmID;
            return Index(0, t.GetCenter());
        }

        [SqlFunction(Name = "GetCorners",
            TableDefinition = PointTableDefinition, FillRowMethodName = PointFillMethodName,
            DataAccess = DataAccessKind.None, IsDeterministic = true, IsPrecise = false)]
        public static IEnumerable GetCorners(SqlInt64 htmID)
        {
            Trixel t = htmID;
            Point v0, v1, v2;

            t.GetCorners(out v0, out v1, out v2);

            return Index(new Point[] { v0, v1, v2 });
        }

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

        protected static IndexedValue<T>[] Index<T>(int index, T value)
        {
            IndexedValue<T>[] res = new IndexedValue<T>[1];

            res[0] = new IndexedValue<T>(index, value);

            return res;
        }

        public static void FillRange(object obj, out SqlInt32 i, out SqlInt64 lo, out SqlInt64 hi, out SqlBoolean partial)
        {
            IndexedValue<Range> r = (IndexedValue<Range>)obj;

            i = new SqlInt32(r.Index);
            lo = new SqlInt64((Int64)r.Value.Lo.HtmID);
            hi = new SqlInt64((Int64)r.Value.Hi.HtmID);
            partial = new SqlBoolean(r.Value.Markup != Markup.Inner);
        }
    }
}