using ModelMapper;
using ModelMapper.Basic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApp.Models;

namespace TestApp
{
    public class ModelMapperBasicTest : IRunner
    {
        public void Run()
        {
            Test1();
            Test2();
        }

        private void Test1()
        {
            Stopwatch stopwatch = new Stopwatch();

            ModelOne x = new ModelOne()
            {
                A = 5,
                B = "dasdasd",
                C = false,
                D = 12.1521,
                E = 11.10M
            };

            ModelTwo y = new ModelTwo()
            {
                N = "fasss"
            };

            Console.WriteLine($"Before changes: \nX:\n{x} \nY:\n{y}\n");
            stopwatch.Start();

            IModelMapper<ModelOne, ModelTwo> umapper = ModelMapperFactory.GetModelMapper<ModelOne, ModelTwo>(ModelDataSize.Small);

            umapper.Add(f => f.A, t => t.M);
            //umapper.Add(f => "hello there", t => t.N);
            umapper.Add(() => "hello there", t => t.N);
            //umapper.Add(f => f.C.ToString(), t => t.N);
            umapper.Add(f => f.C, t => t.O);
            umapper.Add(f => f.D, t => t.P);
            umapper.Add(f => f.E, t => t.Q);
            umapper.Build();

            for (int i = 0; i < 7000; i++)
                umapper.CopyChanges(x, y);

            stopwatch.Stop();
            Console.WriteLine($"\nAfter changes: \nY:\n{y}\n in {stopwatch.Elapsed.TotalSeconds} secs\n\n");
        }

        private void Test2()
        {
            Stopwatch stopwatch = new Stopwatch();

            ModelOne x = new ModelOne()
            {
                A = 5,
                B = "dasdasd",
                C = false,
                D = 12.1521,
                E = 11.10M
            };

            ModelTwo y = new ModelTwo()
            {
                N = "fasss"
            };

            Console.WriteLine($"Before changes: \nX:\n{x} \nY:\n{y}\n");
            stopwatch.Start();

            IModelMapper<ModelOne, ModelTwo> umapper = ModelMapperFactory.GetModelMapper<ModelOne, ModelTwo>(ModelDataSize.Large);

            umapper.Add(f => f.A, t => t.M)
                .Add(f => "Hello there", t => t.N)
                .Add(f => f.C, t => t.O)
                .Add(f => f.D, t => t.P)
                .Add(f => f.E, t => t.Q)
                .Build();

            for (int i = 0; i < 7000; i++)
                umapper.CopyChanges(x, y);

            stopwatch.Stop();
            Console.WriteLine($"\nAfter changes: \nY:\n{y}\n in {stopwatch.Elapsed.TotalSeconds} secs\n\n");
        }
    }
}
