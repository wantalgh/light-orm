using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WT.Data.DbExtension
{
    class PlSql11Builder : ISqlBuilder
    {
        public string BuildSelectSql(string tableName, IEnumerable<string> columns, IEnumerable<KeyValuePair<string, string>> keyColumnDic)
        {
            throw new NotImplementedException();
        }

        public string BuildInsertSql(string tableName, IEnumerable<KeyValuePair<string, string>> parameterColumnDic)
        {
            throw new NotImplementedException();
        }

        public string BuildUpdateSql(string tableName, IEnumerable<KeyValuePair<string, string>> parameterColumnDic, IEnumerable<KeyValuePair<string, string>> keyColumnDic)
        {
            throw new NotImplementedException();
        }

        public string BuildDeleteSql(string tableName, IEnumerable<KeyValuePair<string, string>> keyColumnDic)
        {
            throw new NotImplementedException();
        }

        public string BuildMergeSql(string tableName, IEnumerable<KeyValuePair<string, string>> parameterColumnDic, IEnumerable<KeyValuePair<string, string>> keyColumnDic)
        {
            throw new NotImplementedException();
        }


        public string DecoratePageSelectSql(string selectSql, int skip, int take)
        {
            throw new NotImplementedException();
        }
    }
}
