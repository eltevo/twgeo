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
        protected const string PointTableDefinition = "[i] int, [x] float, [y] float, [z] float, [lon] float, [lat] float";
        protected const string PointFillMethodName = "FillPoint";

        protected const string RangeTableDefinition = "[i] int, [htmIDStart] bigint, [htmIDEnd] bigint, [partial] bit, [geo] geography";
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
        public static IEnumerable Cover(SqlGeography geo, SqlBoolean includeIntersection)
        {
            return CoverImpl(geo, includeIntersection);
        }

        private static IndexedValue<Range>[] CoverImpl(SqlGeography geo, SqlBoolean includeIntersection)
        {
            try
            {
                var cb = new CoverBuilder(geo);
                cb.Execute();

                return Index(cb.GetRanges(includeIntersection.Value));
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected static IndexedValue<T>[] Index<T>(IList<T> values)
        {
            var res = new IndexedValue<T>[values.Count];

            for (int i = 0; i < values.Count; i++)
            {
                res[i] = new IndexedValue<T>(i, values[i]);
            }

            return res;
        }

        protected static IndexedValue<T>[] Index<T>(int index, T value)
        {
            var res = new IndexedValue<T>[1];

            res[0] = new IndexedValue<T>(index, value);

            return res;
        }

        public static void FillPoint(object obj, out SqlInt32 i, out SqlDouble x, out SqlDouble y, out SqlDouble z, out SqlDouble lon, out SqlDouble lat)
        {
            var c = (IndexedValue<Point>)obj;

            i = new SqlInt32(c.Index);
            x = new SqlDouble(c.Value.X);
            y = new SqlDouble(c.Value.Y);
            z = new SqlDouble(c.Value.Z);
            lon = new SqlDouble(c.Value.Lon);
            lat = new SqlDouble(c.Value.Lat);
        }

        public static void FillRange(object obj, out SqlInt32 i, out SqlInt64 lo, out SqlInt64 hi, out SqlBoolean partial, out SqlGeography geo)
        {
            var r = (IndexedValue<Range>)obj;

            i = new SqlInt32(r.Index);
            lo = new SqlInt64((Int64)r.Value.Lo.HtmID);
            hi = new SqlInt64((Int64)r.Value.Hi.HtmID);
            partial = new SqlBoolean(r.Value.Markup != Markup.Inner);
            geo = r.Value.Intersection;
        }
    }
}