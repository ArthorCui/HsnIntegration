using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace UnitTest
{
    public class DictionaryTester
    {
        Dictionary<LocationIDModelName, string> DeviceLocIdModels;
        Dictionary<string, string> DeviceLocIdModelsFixed;

        [Test]
        public void test_try_get_value()
        {
            DeviceLocIdModels = new Dictionary<LocationIDModelName, string>();
            DeviceLocIdModelsFixed = new Dictionary<string, string>();

            var model = new LocationIDModelName();
            model.LocationID = "FUT9001";
            model.DeviceStatusCode = "In Stock";
            model.ModelName = "STB-DTH1";
            model.GetHashCode();
            var model2 = new LocationIDModelName();
            model2.LocationID = "FUT9001";
            model2.DeviceStatusCode = "In Stock";
            model2.ModelName = "STB-DTH1";

            var expectFilePath =
                @"\\IC01\L_01_Import\BuildList_FUT9001_STB-DTH1.a7afa5af-fc31-435f-b498-b35076ca708c.txt";
            //DeviceLocIdModels.Add(model, expectFilePath);
            DeviceLocIdModelsFixed.Add(model.ToString(), expectFilePath);

            string result;
            //DeviceLocIdModels.TryGetValue(model2, out result);
            DeviceLocIdModelsFixed.TryGetValue(model2.ToString(), out result);
            Assert.AreEqual(expectFilePath, result);
        }


    }

    public class LocationIDModelName
    {
        public string LocationID;
        public string ModelName;
        public string DeviceStatusCode;

        public override string ToString()
        {
            return string.Format("{0}_{1}_{2}", LocationID, ModelName, DeviceStatusCode);
        }
    }
}
