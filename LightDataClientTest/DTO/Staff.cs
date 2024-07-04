using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightDataClientTest
{
    public class Staff
    {
        public Guid Id { get; set; }

        public Guid? AlterId { get; set; }

        public string Name { get; set; } = null!;

        public string? AlterName { get; set; }

        public DateTime EntryDate { get; set; }

        public DateTime? QuitDate { get; set; }

        public int Degree { get; set; }

        public int? Balance { get; set; }

        public decimal Salary { get; set; }

        public decimal? Allowance { get; set; }

        public bool Allowed { get; set; }

        public bool? Checked { get; set; }
    }
}
