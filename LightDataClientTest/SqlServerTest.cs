using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Transactions;
using Wantalgh.LightDataClient;

namespace LightDataClientTest
{
    [TestClass]
    public class SqlServerTest
    {
        private DataClient _sqlClient = null!;

        [TestInitialize]
        public void Initialize()
        {
            var sqlConnStr =
                @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\App_Data\SqlServerTestDB.mdf;Integrated Security=True";

            _sqlClient = new DataClient(() => new SqlConnection(sqlConnStr));
        }

        [TestMethod]
        public void TestExecuteModels()
        {
            InitTestData();

            // Default generation and mapping rule:
            // model's type name is same as table name,
            // model's property name is same as table's column name,
            // for details, see https://

            // Execute auto generated sql from Staff type
            // sql: SELECT Id, AlterId, Name ... FROM Staff
            // map result to IEnumerable<Staff>
            IEnumerable<Staff> staffs1 = _sqlClient.ExecuteModels<Staff>();
            Assert.AreEqual(3, staffs1.Count());

            // Execute auto generated sql with a param
            // sql: SELECT Id, AlterId, Name ... FROM Staff WHERE Id = @Id;
            // param: @Id = Guid.Empty
            IEnumerable<Staff> staffs2 = _sqlClient.ExecuteModels<Staff>(parameter: new { Id = Guid.Empty });
            Assert.AreEqual(1, staffs2.Count());

            // Execute auto generated sql with more params
            // sql: SELECT Id, AlterId, Name ... FROM Staff WHERE Degree = @Degree AND Name = @Name;
            // params: @Degree = 1, @Name = "name"
            IEnumerable<Staff> staffs3 = _sqlClient.ExecuteModels<Staff>(parameter: new { Degree = 1, Name = "name" });
            Assert.AreEqual(1, staffs3.Count());

            // Execute auto generated sql with ROW_NUMBER()OVER wrapper
            IEnumerable<Staff> staffs4 = _sqlClient.ExecuteModels<Staff>(skip: 0, take: 1);
            Assert.AreEqual(1, staffs4.Count());

            // Execute auto generated sql with specified tableName:
            // sql: SELECT Id, AlterId, Name ... FROM Staff2
            IEnumerable<Staff> staffs5 = _sqlClient.ExecuteModels<Staff>(tableName: "Staff2");
            Assert.AreEqual(3, staffs5.Count());

            // Execute complex auto generated sql.
            // specified tableName: Staff2
            // sql: SELECT Id, AlterId, Name ... FROM Staff2 WHERE Degree = @Degree AND Name = @Name;
            // params: @Degree = 2, @Name = "name2",
            // with ROW_NUMBER()OVER wrapper
            // map results to IEnumerable<Staff>
            IEnumerable<Staff> staffs6 = _sqlClient.ExecuteModels<Staff>(tableName: "Staff2",
                parameter: new { Degree = 2, Name = "name2" }, skip: 0, take: 10);
            Assert.AreEqual(1, staffs6.Count());

            // Execute sql, map results to IEnumerable<Staff>
            IEnumerable<Staff> staffs7 = _sqlClient.ExecuteModels<Staff>("SELECT * FROM Staff WHERE Checked IS NULL");
            Assert.AreEqual(3, staffs7.Count());


            // Execute sql with params: @IsAllow = true, @Level = 0
            IEnumerable<Staff> staffs8 = _sqlClient.ExecuteModels<Staff>(
                "SELECT * FROM Staff2 WHERE Allowed = @IsAllow OR Degree > @Level", new { IsAllow = true, Level = 0 });
            Assert.AreEqual(2, staffs8.Count());


            // Execute sql for complex mapping
            // use "AS" to match column name and property name
            // param: @SearchName = "%name%"
            var sql = """
                      SELECT 
                      Id,
                      Alter_Id    AS AlterId,
                      Name,
                      Alter_Name  AS AlterName,
                      Entry_Date  AS EntryDate,
                      Quit_Date   AS QuitDate
                      FROM Staff3 
                      WHERE Name LIKE @SearchName
                      ORDER BY Name
                      """;
            IEnumerable<Staff> staffs9 = _sqlClient.ExecuteModels<Staff>(sql, new { SearchName = "%name%" });
            Assert.AreEqual(3, staffs9.Count());

            // Execute stored procedure with param: @Name = "name", map results to IEnumerable<Staff>
            IEnumerable<Staff> staffsA = _sqlClient.ExecuteModels<Staff>("sp_querystaffbyname",
                parameter: new { Name = "name" }, commandType: CommandType.StoredProcedure);
            Assert.AreEqual(3, staffsA.Count());


            // ExecuteModel() method only returns one record, equals ExecuteModels().FirstOrDefault()
            Staff staff = _sqlClient.ExecuteModel<Staff>();
            Assert.IsNotNull(staff);
        }


        [TestMethod]
        public void TestExecuteObject()
        {
            InitTestData();

            //Execute sql and return an int result
            var count = _sqlClient.ExecuteObject<int>("SELECT COUNT(*) FROM [Staff3]");
            Assert.AreEqual(3, count);

            //Execute sql and return a datetime result
            var now = _sqlClient.ExecuteObject<DateTime>("SELECT GETDATE()");
            Assert.IsNotNull(now);
        }


        [TestMethod]
        public void TestInsertModel()
        {
            InitTestData();

            //Insert a model, model's type name is table name, model's property name is table's column name
            var staff = new Staff
            {
                Id = Guid.NewGuid(),
                AlterId = Guid.Empty,
                Name = "name",
                AlterName = "alter",
                EntryDate = DateTime.Now,
                QuitDate = DateTime.Now,
                Degree = 1,
                Salary = 1,
                Allowance = 1,
                Allowed = true,
                Checked = false
            };
            var count1 = _sqlClient.InsertModel(staff);
            Assert.AreEqual(1, count1);


            //Insert a model, specify a different table name
            var count2 = _sqlClient.InsertModel(staff, "Staff2");
            Assert.AreEqual(1, count2);

            //Insert a model, specify a table name, use wrapper object to match columns name.
            var mappedModel = new
            {
                Id = staff.Id,
                Alter_Id = staff.AlterId,
                Name = staff.Name,
                Alter_Name = staff.AlterName,
                Entry_Date = staff.EntryDate,
                Quit_Date = staff.QuitDate
            };
            var count3 = _sqlClient.InsertModel(mappedModel, "Staff3");
            Assert.AreEqual(1, count3);


            // Insert a model and return auto generated identity
            var sql = """
                      DECLARE @Id UNIQUEIDENTIFIER
                      SET @Id = NEWID()
                      INSERT INTO [Staff3] ([Id], [Name], [Entry_Date]) VALUES (@Id , @Name, @Date)
                      SELECT @Id
                      """;
            var identity = _sqlClient.ExecuteObject<Guid>(sql, new { Name = "name", Date = DateTime.Now });
            Assert.IsNotNull(identity);
        }


        [TestMethod]
        public void TestTransaction()
        {
            InitTestData();

            var count1 = _sqlClient.ExecuteObject<int>("SELECT COUNT(*) FROM [Staff3]");
            using (var scope = new TransactionScope())
            {
                // Add one row
                var staff3 = new
                {
                    Id = Guid.NewGuid(),
                    Alter_Id = Guid.Empty,
                    Name = "name3",
                    Entry_Date = DateTime.Now,
                };
                _sqlClient.InsertModel(staff3, "Staff3");

                // Add another row
                var sql = "INSERT INTO [Staff3] ([Id], [Name], [Entry_Date]) VALUES (NEWID(), 'name', GETDATE())";
                _sqlClient.ExecuteNone(sql);

                scope.Complete();
            }
            var count2 = _sqlClient.ExecuteObject<int>("SELECT COUNT(*) FROM [Staff3]");
            Assert.AreEqual(2, count2 - count1);
        }

        [TestMethod]
        public void TestDeleteModel()
        {
            InitTestData();

            // Delete rows in table Staff3, with row condition: Id = @Id 
            // sql: DELETE FROM Staff3 WHERE Id = @Id
            // params: @Id = Guid.Empty
            var count1 = _sqlClient.DeleteModel("Staff3", new { Id = Guid.Empty });
            Assert.AreEqual(1, count1);

            //Delete rows in table Staff3, without row condition, will delete all rows
            //sql: DELETE FROM Staff3
            var count2 = _sqlClient.DeleteModel("Staff3", null);
            Assert.AreEqual(2, count2);
        }

        [TestMethod]
        public void TestExecuteNone()
        {
            InitTestData();

            //Execute sql
            _sqlClient.ExecuteNone("TRUNCATE TABLE [Staff3]");

            //Execute stored procedure
            _sqlClient.ExecuteNone("dbo.sp_deletestaffbyid", new { Id = Guid.Empty }, CommandType.StoredProcedure);
        }

        [TestMethod]
        public void TestUpdateModel()
        {
            InitTestData();

            // data model has table name: Staff
            var model = new Staff()
            {
                Id = Guid.Empty,
                Name = "name",
                EntryDate = DateTime.Now,
                QuitDate = DateTime.Now,
                Degree = 1,
                Balance = 1,
                Salary = 1,
                Allowance = 1,
                Allowed = true,
                Checked = false
            };
            // Update table Staff, with row condition: Id = model.Id
            var count1 = _sqlClient.UpdateModel(model, new { Id = model.Id });
            Assert.AreEqual(1, count1);

            // data model with no table name but three columns
            var model2 = new 
            {
                Alter_Id = Guid.Empty,
                Alter_Name = "alter",
                Quit_Date = DateTime.Now
            };
            // Update table Staff3 by model, without row condition, will affect all rows.
            var count2 = _sqlClient.UpdateModel(model2, null, "Staff3");
            Assert.AreEqual(3, count2);


            // Update table Staff3 by model, with row condition: Id = Guid.Empty
            var count3 = _sqlClient.UpdateModel(model2, new { Id = Guid.Empty }, "Staff3");
            Assert.AreEqual(1, count3);
        }

        [TestMethod]
        public void TestInsertOrUpdateModel()
        {
            InitTestData();

            // data model has table name: Staff
            var staff = new Staff()
            {
                Id = Guid.NewGuid(),
                AlterId = Guid.NewGuid(),
                Name = "name",
                EntryDate = DateTime.Now,
                QuitDate = DateTime.Now,
                Degree = 1,
                Salary = 1,
                Allowance = 1,
                Allowed = true,
                Checked = false
            };
            // Insert or update table Staff, with row condition: Id = staff.Id
            var count1 = _sqlClient.InsertOrUpdateModel(staff, new { Id = staff.Id });
            Assert.AreEqual(1, count1);

            // data model with no table name but some columns
            var model = new
            {
                Id = Guid.NewGuid(),
                Name = "name",
                Entry_Date = DateTime.Now
            };
            // Insert or update table Staff3 by model, with row condition: Id = model.Id
            // insert
            var count2 = _sqlClient.InsertOrUpdateModel(model, new { Id = model.Id }, "Staff3");
            Assert.AreEqual(1, count2);
            // update
            var count3 = _sqlClient.InsertOrUpdateModel(model, new { Id = model.Id }, "Staff3");
            Assert.AreEqual(1, count3);
        }

        // init test data
        private void InitTestData()
        {
            _sqlClient.ExecuteNone("TRUNCATE TABLE Staff");
            _sqlClient.ExecuteNone("INSERT INTO Staff (Id, Name, EntryDate, Degree, Salary, Allowed) VALUES ('00000000-0000-0000-0000-000000000000', 'name', GETDATE(), 0, 0, 0)");
            _sqlClient.ExecuteNone("INSERT INTO Staff (Id, Name, EntryDate, Degree, Salary, Allowed) VALUES ('11111111-1111-1111-1111-111111111111', 'name', GETDATE(), 1, 1, 1)");
            _sqlClient.ExecuteNone("INSERT INTO Staff (Id, Name, EntryDate, Degree, Salary, Allowed) VALUES ('22222222-2222-2222-2222-222222222222', 'name', GETDATE(), 2, 2, 2)");

            _sqlClient.ExecuteNone("TRUNCATE TABLE Staff2");
            _sqlClient.ExecuteNone("INSERT INTO Staff2 (Id, Name, EntryDate, Degree, Salary, Allowed) VALUES ('00000000-0000-0000-0000-000000000000', 'name2', GETDATE(), 0, 0, 0)");
            _sqlClient.ExecuteNone("INSERT INTO Staff2 (Id, Name, EntryDate, Degree, Salary, Allowed) VALUES ('11111111-1111-1111-1111-111111111111', 'name2', GETDATE(), 1, 1, 1)");
            _sqlClient.ExecuteNone("INSERT INTO Staff2 (Id, Name, EntryDate, Degree, Salary, Allowed) VALUES ('22222222-2222-2222-2222-222222222222', 'name2', GETDATE(), 2, 2, 2)");

            _sqlClient.DeleteModel("Staff3", null);
            _sqlClient.InsertModel(new { Id = Guid.Parse("00000000-0000-0000-0000-000000000000"), Name = "name3", Entry_Date = DateTime.Now }, "Staff3");
            _sqlClient.InsertModel(new { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "name3", Entry_Date = DateTime.Now }, "Staff3");
            _sqlClient.InsertModel(new { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "name3", Entry_Date = DateTime.Now }, "Staff3");
        }

    }
}