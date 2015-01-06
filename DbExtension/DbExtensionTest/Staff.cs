using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WT.Data.DbExtensionTest
{
    /// <summary>
    /// 对应Staff表
    /// </summary>
    public class Staff
    {
        /// <summary>对应Id列</summary>
        public Guid Id { get; set; }

        /// <summary>对应Id2列</summary>
        public Guid? Id2 { get; set; }

        /// <summary>对应Name列</summary>
        public string Name { get; set; }

        /// <summary>对应EnglishName列</summary>
        public string EnglishName { get; set; }

        /// <summary>对应Salary列</summary>
        public int Salary { get; set; }

        /// <summary>对应Id列</summary>
        public DateTime EntryDate { get; set; }

        /// <summary>对应Tax列</summary>
        public int? Tax { get; set; }

        /// <summary>对应QuitDate列</summary>
        public DateTime? QuitDate { get; set; }
    }
}
