using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HelloWorld;
using ServiceLibary;

namespace HelloWorldClient
{
    class Program
    {
        static void Main(string[] args)
        {
            HelloWorldServiceClient client = new HelloWorldServiceClient();
            Console.WriteLine(client.Greet(new Person { Name = "Roger" }));
            client.Close();

            ProductServiceClient client2 = new ProductServiceClient();
            Console.WriteLine(client2.GetPrice(new Product { Name = "potato", Price = 0.55 }));
            client2.Close();
            Console.ReadLine();
        }
    }
}
