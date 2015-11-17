using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using Microsoft.SqlServer.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Elte.GeographyHtm
{
    [TestClass]
    public class CoverBuilderTest
    {
        /*
        [TestInitialize]
        public static void Initialize()
        {
            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
        }*/

        [TestMethod]
        public void TestMethod1()
        {
            var geo = SqlGeography.STPolyFromText(
                new SqlChars("POLYGON((-122.358 47.653, -122.348 47.649, -122.348 47.658, -122.358 47.658, -122.358 47.653))"),
                4326);

            var b = new CoverBuilder(geo);

            b.Execute();
        }

        [TestMethod]
        public void TestMethod2()
        {
            var geo = SqlGeography.STPolyFromText(
                new SqlChars("POLYGON ((18.676181793212891 47.570518493652344, 18.491588592529524 47.556591033935661, 18.491230010986385 47.520843505859489, 18.526424407959155 47.4857635498048, 18.483013153076229 47.497131347656477, 18.455368041992358 47.476963043212891, 18.421304702758789 47.492015838623047, 18.395893096924112 47.465057373046932, 18.33869934082054 47.446426391601676, 18.346893310546875 47.420501708984489, 18.303140640259073 47.40732574462902, 18.238006591797102 47.441616058349553, 18.227989196777287 47.468093872070312, 18.183134078979435 47.476436614990462, 18.18017959594755 47.450248718261832, 18.109714508056641 47.446956634521428, 18.072689056396484 47.3602676391601, 18.024063110351847 47.335891723632813, 18.064022064209269 47.317043304443473, 18.050090789795206 47.280612945556754, 18.073631286621151 47.280673980712891, 18.086099624634073 47.255584716796875, 18.145673751831112 47.263435363769531, 18.205286026001033 47.224288940429687, 18.2204265594483 47.102329254150391, 18.205686569213867 47.071762084960994, 18.237936019897461 47.027214050293082, 18.17698860168457 46.959053039550838, 18.200359344482479 46.916324615478629, 18.204830169677678 46.791446685791016, 18.248447418213175 46.7890701293947, 18.302539825439453 46.820808410644588, 18.358503341675032 46.784523010253963, 18.40987777709978 46.806972503662166, 18.42167854309082 46.759143829345817, 18.449462890625171 46.745956420898438, 18.511228561401367 46.749656677246151, 18.509906768798885 46.768817901611555, 18.555572509765852 46.784736633301009, 18.579574584960938 46.759975433349609, 18.617837905883846 46.751651763916243, 18.62474441528326 46.699848175048771, 18.644586563110465 46.692314147949162, 18.728956222534464 46.7255020141601, 18.721660614013956 46.762310028076229, 18.792432785034237 46.76958084106468, 18.871797561645678 46.853225708007869, 18.901165008545036 46.867488861084269, 18.92966461181669 46.860214233398608, 18.968112945556754 46.936042785644645, 18.970781326294116 47.037616729736442, 18.895252227783317 47.080810546875114, 18.869428634643555 47.166149139404411, 18.9049968719483 47.273445129394645, 18.867940902710188 47.2772674560548, 18.836660385132063 47.327758789062727, 18.75481033325201 47.390350341796989, 18.777360916137638 47.452465057373161, 18.70938873291027 47.498046875, 18.676181793212891 47.570518493652344))"),
                4326);

            var b = new CoverBuilder(geo)
            {
                MaxLevel = 3
            };

            b.Execute();

            var rr = b.GetRanges(true);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var t = new Trixel(13667206692864UL);
            var p = t.Parent;

            var g = SqlGeography.Point(10, 10, 4326);

            var tr = p.GetTriangle(g);
            var st = p.Split();
            var str = st[0].GetTriangle(g);
        }

#if false
        [TestMethod]
        public void TestMethod4()
        {
            var sql = @"WITH uk AS
(
       SELECT geom.Reduce(5000.0).BufferWithCurves(5000) AS map
       FROM gadm..Region 
       WHERE Type = 'Country' AND Name_0 = 'United Kingdom'
),
ire AS
(
       SELECT geom.Reduce(5000.0).BufferWithCurves(5000) AS map
       FROM gadm..Region 
       WHERE Type = 'Country' AND Name_0 = 'Ireland'
)
SELECT uk.map.STUnion(ire.map)
FROM uk, ire";

            using (var cn = new SqlConnection("data source=future1;initial catalog=gadm;integrated security=true"))
            {
                cn.Open();

                using (var cmd = new SqlCommand(sql, cn))
                {
                    using (var dr = cmd.ExecuteReader())
                    {
                        dr.Read();

                        var g = new SqlGeography();
                        g.Read(new BinaryReader(dr.GetSqlBytes(0).Stream));
                        //var g = (SqlGeography)dr.GetValue(0);

                        using (var outfile = new FileStream(@"C:\Data\dobos\project\GeographyHtm\data\ukire_reduced.bin", FileMode.Create, FileAccess.Write))
                        {
                            using (var w = new BinaryWriter(outfile))
                            {
                                g.Write(w);
                            }
                        }
                    }
                }
            }
        }
#endif

        [TestMethod]
        public void TestMethod5()
        {
            var geo = new SqlGeography();
            geo.Read(new BinaryReader(new MemoryStream(File.ReadAllBytes(@"..\..\..\..\data\ukire_reduced.bin"))));

            var b = new CoverBuilder(geo)
            {
                MaxLevel = 10,
            };

            b.Execute();

            var r = b.GetRanges(true);
        }
    }
}
