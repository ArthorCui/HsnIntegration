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
            HelloWorldService client = new HelloWorldService();
            Console.ReadLine();
            Console.WriteLine(client.Greet(new Person { Name = "Roger" }));
            Console.WriteLine(client.GetData(1));
        }
    }
}
