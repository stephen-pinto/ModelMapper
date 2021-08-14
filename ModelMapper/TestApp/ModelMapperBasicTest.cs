using ModelMapper;
using System;
using System.Collections.Generic;
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

            ModelMapperBasic<ModelOne, ModelTwo> umapper = new ModelMapperBasic<ModelOne, ModelTwo>();
            umapper.Add(f => f.A, t => t.M);
            umapper.Add(f => "hello there", t => t.N);
            //umapper.Add(f => f.C.ToString(), t => t.N);
            umapper.Add(f => f.C, t => t.O);
            umapper.Add(f => f.E, t => t.Q);
            umapper.CopyChanges(x, y);

            Console.WriteLine($"After changes: \nY:\n{y}\n");
        }
    }
}
