using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Transactions;
using WT.Data.DbExtension;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WT.Data.DbExtensionTest
{
    [TestClass]
    public class TestDbCommandExtension
    {
        [TestMethod]
        public void TestExecuteModels()
        {
            //测试简单情景：模型属性与数据列完全对应。
            var staff = DataSourceFactory.DataSource.ExecuteModels<Staff>();

            //测试按范围查询数据
            var pagedStaff = DataSourceFactory.DataSource.ExecuteModels<Staff>(skip: 1, take: 2);

            //测试稍复杂些的情景：指定简单查询条件
            var staffabc = DataSourceFactory.DataSource.ExecuteModels<Staff>(null, new { Name = "abc", EnglishName = "def" });

            //测试稍复杂些的情景：使用到类名与表名不一致的实体。
            var staff1 = DataSourceFactory.DataSource.ExecuteModels<Staff2>(null, new { Name = "abc", EnglishName = "def" }, "Staff");

            //测试稍复杂些的情景：手动指定SQL进行查询
            var stafff2 = DataSourceFactory.DataSource.ExecuteModels<Staff>("SELECT * FROM [Staff] WHERE [EnglishName] IS NULL");

            //测试更复杂些的情景：手动指定SQL配合参数进行查询。
            var staff3 = DataSourceFactory.DataSource.ExecuteModels<Staff2>("SELECT TOP 2 * FROM [Staff] WHERE Name LIKE @SelectName", new { SelectName = "%a%" });

            //测试复杂的情景：模型属性与数据列不一一对应，且使得复杂查询条件。
            var sqlTextBuilder = new StringBuilder();
            sqlTextBuilder.AppendLine("SELECT [Id]");
            sqlTextBuilder.AppendLine("      ,[Id2]            AS 'Code'");
            sqlTextBuilder.AppendLine("      ,[Name]");
            sqlTextBuilder.AppendLine("      ,[EnglishName]    AS 'OtherName'");
            sqlTextBuilder.AppendLine("      ,[Salary]");
            sqlTextBuilder.AppendLine("      ,[EntryDate]");
            sqlTextBuilder.AppendLine("      ,[Tax]");
            sqlTextBuilder.AppendLine("      ,[QuitDate]       AS 'LeaveDate'");
            sqlTextBuilder.AppendLine("  FROM [Staff]");
            sqlTextBuilder.AppendLine(" WHERE ([Id] != @Id OR Id2 IS NULL) AND LEN([Name]) = @NameLength");

            var sqlText = sqlTextBuilder.ToString();
            var parameter = new {Id = Guid.Empty, NameLength = 3};
            var staffModel = DataSourceFactory.DataSource.ExecuteModels<StaffModel>(sqlText, parameter);

            //通过存储过程读取模型
            var staffff = DataSourceFactory.DataSource.ExecuteModels<Staff2>(sql: "sp_querystaffbyname", parameter: new {Name = "a"}, commandType: CommandType.StoredProcedure);
        }

        [TestMethod]
        public void TestExecuteObject()
        {
            //查询一个int类型结果
            var count = DataSourceFactory.DataSource.ExecuteObject<int>("SELECT COUNT(*) FROM [Staff]");

            //查询一个Guid类型结果
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append(" DECLARE @Id UNIQUEIDENTIFIER");
            sqlBuilder.Append(" SET @Id = NEWID()");
            sqlBuilder.Append(" INSERT INTO [Staff] ([Id], [Name] ,[Salary], [EntryDate]) VALUES (@Id , 'aaa', 20, GETDATE())");
            sqlBuilder.Append(" SELECT @Id");
            var identity = DataSourceFactory.DataSource.ExecuteObject<Guid>(sqlBuilder.ToString());
        }

        [TestMethod]
        public void TestInsertModel()
        {
            //可以使用TransactionScope启用事务
            using (var transaction = new TransactionScope())
            {
                //测试简单情景自动插入模型，模型类型名与表名对应，属性名与列名对应。
                var staff = new Staff {Id = Guid.NewGuid(), EntryDate = DateTime.Now, Name = "abc", Salary = 10};
                var isSuccess = DataSourceFactory.DataSource.InsertModel(staff);

                //测试复杂一些的情况，手动指定插入操作的表名
                var staff2 = new Staff2 {Id = Guid.NewGuid(), EntryDate = DateTime.Now, Name = "def", Salary = 10};
                var isSuccess2 = DataSourceFactory.DataSource.InsertModel(staff2, "Staff");

                //测试复杂情景自动插入模型，模型类型名与表名不对应，属性名与列名不对应。
                var staffModel = new StaffModel {Id = Guid.NewGuid(), Code = new Guid(), EntryDate = DateTime.Now, LeaveDate = DateTime.Now, Name = "abc", OtherName = "def", Salary = 100, Tax = 200};
                var parameterColumnMap = new Dictionary<string, string> {{"Code", "Id2"}, {"OtherName", "EnglishName"}, {"LeaveDate", "QuitDate"}};
                var isSuccess3 = DataSourceFactory.DataSource.InsertModel(staffModel, "Staff", parameterColumnMap);

            
                transaction.Complete();
            }
        }

        /// <summary>
        /// 测试删除模型
        /// </summary>
        [TestMethod]
        public void TestDeleteModel()
        {
            //无条件删除指定表的所有数据。(即不带Where子句的Delete语句，慎用。)
            var isSuccess = DataSourceFactory.DataSource.DeleteModel("Staff");

            //删除指定表的指定简单条件的数据
            var isSuccess2 = DataSourceFactory.DataSource.DeleteModel("Staff", new { EnglishName = "abc" });
        }

        [TestMethod]
        public void TestExecuteNone()
        {
            //测试使用直接执行手写SQL
            DataSourceFactory.DataSource.ExecuteNone("TRUNCATE TABLE [Staff]");

            //测试执行存储过程
            DataSourceFactory.DataSource.ExecuteNone("dbo.sp_deletestaffbyid", new { Id = Guid.Empty }, CommandType.StoredProcedure);
        }

        [TestMethod]
        public void TestUpdateModel()
        {
            //清空数据库后先插入一条记录，做为测试Update操作的目标。
            DataSourceFactory.DataSource.DeleteModel("Staff");
            var staff = new Staff {Id = Guid.NewGuid(), EntryDate = DateTime.Now, Name = "abc", Salary = 10};
            DataSourceFactory.DataSource.InsertModel(staff);


            //测试最简单场景：不带任何条件无脑更新实体。(不带条件调用方法等于不带Where条件的Update语句，非常危险。)            
            var isSuccess = DataSourceFactory.DataSource.UpdateModel(staff);

            //测试简单场景：带Id限定条件的更新
            var isSuccess2 = DataSourceFactory.DataSource.UpdateModel(staff, new { Id = staff.Id });

            //测试场景：实体类名与表名不一致，手动指定表名
            var staff2 = new Staff2 {Id = staff.Id, EntryDate = DateTime.Now, Name = "abc", Salary = 10};
            var isSuccess3 = DataSourceFactory.DataSource.UpdateModel(staff2, new { Id = staff.Id }, "Staff");

            //测试场景：只要更新Id为staffId的Staff的Name和EnglishName列，其它列不更新。
            var isSuccess4 = DataSourceFactory.DataSource.UpdateModel(new { Name = "nnn", EnglishName = (string)null }, new { Id = staff.Id }, "Staff");

            //测试场景：实体类名与表名不对应，属性名与列名也不一致
            var staffModel = new StaffModel {Code = new Guid(), EntryDate = DateTime.Now, LeaveDate = DateTime.Now, Name = "abc", OtherName = "def", Salary = 100, Tax = 200};
            var parameterColumnMap = new Dictionary<string, string> {{"Code", "Id2"}, {"OtherName", "EnglishName"}, {"LeaveDate", "QuitDate"}};
            var isSuccess5 = DataSourceFactory.DataSource.UpdateModel(staffModel, new { Id = staff.Id }, "Staff", parameterColumnMap);
        }

        [TestMethod]
        public void TestInsertOrUpdateModel()
        {
            //清空数据库以方便测试。
            DataSourceFactory.DataSource.DeleteModel("Staff");

            //测试最简单场景：不带任何条件无脑操作实体。(如果数据库中的实体对应表没有记录则插入一条，否则更新所有记录。更新所有记录通常会很危险，慎用。)            
            var staff = new Staff {Id = Guid.NewGuid(), EntryDate = DateTime.Now, Name = "abc", Salary = 10};
            var isSuccess = DataSourceFactory.DataSource.InsertOrUpdateModel(staff);

            //测试简单场景：带Id限定条件的操作。(如果存在指定的记录则更新这条记录，否则插入一条记录)
            var isSuccess2 = DataSourceFactory.DataSource.InsertOrUpdateModel(staff, new { Id = staff.Id });

            //测试场景：实体类名与表名不一致，手动指定表名
            var staff2 = new Staff2 {Id = staff.Id, EntryDate = DateTime.Now, Name = "abc", Salary = 10};
            var isSuccess3 = DataSourceFactory.DataSource.InsertOrUpdateModel(staff2, new { Id = staff.Id, Name = "abc" }, "Staff");

            //测试场景：只要操作Id为staffId的Staff的一部分列，忽略其它列。(注意不要忽略不可为null的列，否则插入数据时会出错)
            var isSuccess4 = DataSourceFactory.DataSource.InsertOrUpdateModel(new { Id = Guid.NewGuid(), Name = "abc", Salary = 1, EntryDate = DateTime.Now }, new { Id = staff.Id }, "Staff");

            //测试场景：实体类名与表名不对应，属性名与列名也不一致
            var staffModel = new StaffModel {Id = Guid.NewGuid(), Code = Guid.NewGuid(), EntryDate = DateTime.Now, LeaveDate = DateTime.Now, Name = "abc", OtherName = "def", Salary = 100, Tax = 200};
            var parameterColumnMap = new Dictionary<string, string> {{"Code", "Id2"}, {"OtherName", "EnglishName"}, {"LeaveDate", "QuitDate"}};
            var isSuccess5 = DataSourceFactory.DataSource.InsertOrUpdateModel(staffModel, new { Id = staff.Id, Name = "abc" }, "Staff", parameterColumnMap);
        }

        [TestMethod]
        public void TestExecuteDataSet()
        {
            //插入两条记录以方便测试
            using (var transaction = new TransactionScope())
            {
                DataSourceFactory.DataSource.ExecuteNone("INSERT INTO [Staff] ([Id], [Name] ,[Salary], [EntryDate]) VALUES (@Id , 'aaa', 20, GETDATE())", new { Id = Guid.NewGuid() });
                DataSourceFactory.DataSource.ExecuteNone("INSERT INTO [Staff] ([Id], [Name] ,[Salary], [EntryDate]) VALUES (@Id , 'bbb', 40, GETDATE())", new { Id = Guid.NewGuid() });
                
                transaction.Complete();
            }

            var dataSet = DataSourceFactory.DataSource.ExecuteDataSet("SELECT * FROM Staff S1 SELECT * FROM Staff S2");
        }
    }
}
