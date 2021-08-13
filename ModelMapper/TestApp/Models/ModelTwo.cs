using Newtonsoft.Json;

namespace TestApp.Models
{
    public class ModelTwo
    {
        public int M { get; set; }

        public string N { get; set; }

        public bool O { get; set; }

        public double P { get; set; }

        public decimal Q { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
