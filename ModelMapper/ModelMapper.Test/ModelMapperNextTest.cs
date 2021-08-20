using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelMapper.Test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelMapper.Test
{
    [TestClass]
    public class ModelMapperNextTest
    {
        [TestMethod]
        public void SimpleMappingTest()
        {
            var dflt = new Person() { Age = 0, Name = "None" };
            Employee employee1 = new Employee() { Id = 1231, CompanyName = "X company", Person = new Person() { Age = 20, Name = "SomenameA" } };
            Employee employee2 = new Employee() { Id = 3344, CompanyName = "Z company", Person = new Person() { Age = 26, Name = "SomenameA" } };

            var modelMapper = new ModelMapperNext<Employee, Employee>();
            modelMapper.Add(o => o.Id, o => o.Id)
                .Add(o => o.Person, o => o.Person, dflt)
                .Add(o => o.CompanyName, o => o.CompanyName)
                .Build();

            modelMapper.CopyChanges(employee1, employee2);
            Assert.AreEqual(employee1.CompanyName, employee2.CompanyName);
            Assert.AreEqual(employee1.Id, employee2.Id);            
        }

        [TestMethod]
        public void TestWithDefaults()
        {
            var dflt = new Person() { Age = 0, Name = "None" };
            Employee employee = new Employee() { Id = 1231, CompanyName = "X company", Person = new Person() { Age = 20, Name = "SomenameA" } };
            Employee employee2 = new Employee() { Id = 2121, CompanyName = "Y company", Person = null};
            Employee employee3 = new Employee() { Id = 3344, CompanyName = "Z company", Person = new Person() { Age = 26, Name = "SomenameA" } };

            var modelMapper = new ModelMapperNext<Employee, Employee>();
            modelMapper.Add(o => o.Id, o => o.Id)
                .Add(o => o.Person, o => o.Person, dflt)
                .Add(o => o.CompanyName, o => o.CompanyName)
                .Build();

            var newEmployee = new Employee();
            modelMapper.CopyChanges(employee, newEmployee);
            Assert.IsNotNull(newEmployee.CompanyName);
            Assert.IsNotNull(newEmployee.Person);

            newEmployee = new Employee();
            modelMapper.CopyChanges(employee2, newEmployee);
            Assert.IsNotNull(newEmployee.CompanyName);
            Assert.IsNotNull(newEmployee.Person);
        }
    }
}
