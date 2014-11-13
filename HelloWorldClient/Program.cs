using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HelloWorld;

namespace HelloWorldClient
{
    class Program
    {
        static void Main(string[] args)
        {
            HelloWorldServiceClient client = new HelloWorldServiceClient();
            Console.WriteLine(client.Greet(new Person { Name = "Roger" }));
            client.Close();
            Console.ReadLine();
        }
    }
}
