using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WT.Data.DbExtensionTest
{
    /// <summary>
    /// 对应Staff表，但类名与表名不一致，部分属性名与列名也不一致。
    /// </summary>
    public class StaffModel
    {
        /// <summary>对应Id列</summary>
        public Guid Id { get; set; }

        /// <summary>对应Id2列</summary>
        public Guid? Code { get; set; }

        /// <summary>对应Name列</summary>
        public string Name { get; set; }

        /// <summary>对应EnglishName列</summary>
        public string OtherName { get; set; }

        /// <summary>对应Salary列</summary>
        public int Salary { get; set; }

        /// <summary>对应Id列</summary>
        public DateTime EntryDate { get; set; }

        /// <summary>对应Tax列</summary>
        public int? Tax { get; set; }

        /// <summary>对应QuitDate列</summary>
        public DateTime? LeaveDate { get; set; }
    }
}
