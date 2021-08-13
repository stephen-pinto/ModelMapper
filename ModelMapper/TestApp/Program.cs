using System;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            IRunner runner = new ModelMapperBasicTest();
            runner.Run();
        }
    }
}
