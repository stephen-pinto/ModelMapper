using Newtonsoft.Json;

namespace TestApp.Models
{
    public class ModelOne
    {
        public int A { get; set; }

        public string B { get; set; }

        public bool C { get; set; }

        public double D { get; set; }

        public decimal E { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
