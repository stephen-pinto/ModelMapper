using ModelMapper;
using ModelMapper.Unary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApp.Models;

namespace TestApp
{
    class ModelMapperUnaryTest : IRunner
    {
        public void Run()
        {
            ModelOne x = new ModelOne()
            {
                A = 5,
                B = "dasdasd",
                C = false,
                D = 12.1521,
                E = 11.10M
            };

            ModelOne y = new ModelOne() { B = "fasss" };

            Console.WriteLine($"Before changes: \nX:\n{x} \nY:\n{y}\n");

            ModelMapperUnary<ModelOne> umapper = new ModelMapperUnary<ModelOne>();
            umapper.Add(o => o.A);
            umapper.Add(o => o.C);
            umapper.Add(o => o.D);
            umapper.CopyChanges(x, y);

            Console.WriteLine($"Before changes: \nX:\n{x} \nY:\n{y}\n");
        }
    }
}
