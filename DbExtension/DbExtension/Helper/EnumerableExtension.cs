using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WT.Data.DbExtension
{
    /// <summary>
    /// IEnumerable扩展
    /// </summary>
    static class EnumerableExtension
    {
        /// <summary>
        /// 根据IEnumerable得到ICollection
        /// </summary>
        public static ICollection<T> ToCollection<T>(this IEnumerable<T> enumerable)
        {
            ICollection<T> result;
            if (enumerable == null)
            {
                result = null;
            }
            else
            {
                result = enumerable as ICollection<T> ?? enumerable.ToList();
            }
            return result;
        }
    }
}
