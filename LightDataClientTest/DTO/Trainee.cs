using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightDataClientTest
{
    public class Trainee
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public string? AlterName { get; set; }

        public DateTime BirthDate { get; set; }

        public double Score{ get; set; }

        public bool Allowed { get; set; }

        public bool? Checked { get; set; }
    }
}
