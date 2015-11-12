using System;
using System.Data;
using System.Data.SqlTypes;
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
    }
}
