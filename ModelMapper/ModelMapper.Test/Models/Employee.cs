using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelMapper.Test.Models
{
    public class Employee
    {
        public int Id { get; set; }

        public string CompanyName { get; set; }

        public Person Person { get; set; }
    }
}
