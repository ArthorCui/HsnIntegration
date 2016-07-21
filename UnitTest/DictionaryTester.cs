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

        [Test]
        public void test_list_except()
        {
            var listA = new List<string>();
            var listB = new List<string>();

            listA.Add("first line...");
            listA.Add("second line...");

            listB.Add("first line...");
            listB.Add("third line...");

            Console.WriteLine("A except B");
            listA.Except(listB).ToList().ForEach(x => Console.WriteLine(x));

            Console.WriteLine("B except A");
            listB.Except(listA).ToList().ForEach(x => Console.WriteLine(x));

        }

        [Test]
        public void test_list_mismatch()
        {
            List<string> list1 = new List<string>();
            list1.Add("list2 not contain.");
            list1.Add("The Avengers");
            list1.Add("Shutter Island");
            list1.Add("Inception");
            list1.Add("The Dark Knight Rises");

            List<string> list2 = new List<string>();
            list2.Add("The Avengers changed");
            list2.Add("Shutter Island");
            list2.Add("Inception");
            list2.Add("The Dark Knight Rises changed");
            list2.Add("Parks and Recreation");
            list2.Add("Scandal");

            List<string> difference = list2.Except(list1).ToList();

            difference.ForEach(x => Console.WriteLine(x));

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
}
