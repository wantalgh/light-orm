using Microsoft.Data.SqlClient;
using System.Data;
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


        [TestMethod]
        public void TestExecuteModels()
        {
            InitTestData();

            // Default generation and mapping rule:
            // model's type name is same as table name,
            // model's property name is same as table's column name,
            // table's column name is same as parameter name.
            // for details, see https://

            // Execute auto generated sql from Staff type
            // sql: SELECT Id, AlterId, Name ... FROM Staff
            IEnumerable<Staff> staffs1 = _sqlClient.ExecuteModels<Staff>();

            // Execute auto generated sql with a param
            // sql: SELECT Id, AlterId, Name ... FROM Staff WHERE Id = @Id;
            // param: @Id = Guid.Empty
            IEnumerable<Staff> staffs2 = _sqlClient.ExecuteModels<Staff>(parameter: new { Id = Guid.Empty });

            // Execute auto generated sql with params
            // sql: SELECT Id, AlterId, Name ... FROM Staff WHERE Degree = @Degree AND Name = @Name;
            // params: @Degree = 1, @Name = "name"
            IEnumerable<Staff> staffs3 = _sqlClient.ExecuteModels<Staff>(parameter: new { Degree = 1, Name = "name" });

            // Execute auto generated sql with ROW_NUMBER()OVER wrapper
            IEnumerable<Staff> staffs4 = _sqlClient.ExecuteModels<Staff>(skip: 0, take: 1);

            // Execute auto generated sql with specified tableName:
            // sql: SELECT Id, AlterId, Name ... FROM Staff2
            IEnumerable<Staff> staffs5 = _sqlClient.ExecuteModels<Staff>(tableName: "Staff2");

            // Execute complex auto generated sql.
            // specified tableName: Staff2
            // sql: SELECT Id, AlterId, Name ... FROM Staff2 WHERE Degree = @Degree AND Name = @Name;
            // params: @Degree = 1, @Name = "name",
            // with ROW_NUMBER()OVER wrapper
            // map results to IEnumerable<Staff>
            IEnumerable<Staff> staffs6 = _sqlClient.ExecuteModels<Staff>(tableName: "Staff2",
                parameter: new { Degree = 1, Name = "name" }, skip: 0, take: 10);

            // Execute sql, map results to IEnumerable<Staff>
            IEnumerable<Staff> staffs7 = _sqlClient.ExecuteModels<Staff>("SELECT * FROM Staff WHERE Checked IS NULL");

            // Execute sql with params: @IsAllow = true, @Level = 2
            IEnumerable<Staff> staffs8 = _sqlClient.ExecuteModels<Staff>(
                "SELECT * FROM Staff2 WHERE Allowed = @IsAllow OR Degree > @Level", new { IsAllow = true, Level = 0 });


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

            // Execute stored procedure with param: @Name = "name", map results to IEnumerable<Staff>
            IEnumerable<Staff> staffsA = _sqlClient.ExecuteModels<Staff>("sp_querystaffbyname",
                parameter: new { Name = "name" }, commandType: CommandType.StoredProcedure);
        }

        [TestMethod]
        public void TestExecuteModel()
        {
            InitTestData();

            // ExecuteModel method only return one record, equals ExecuteModels().FirstOrDefault()
            Staff staff = _sqlClient.ExecuteModel<Staff>();
        }


        [TestMethod]
        public void TestExecuteObject()
        {
            InitTestData();

            //Execute sql and return an int result
            var count = _sqlClient.ExecuteObject<int>("SELECT COUNT(*) FROM [Staff3]");

            //Execute sql and return a guid result
            var sql = """
                      DECLARE @Id UNIQUEIDENTIFIER
                      SET @Id = NEWID()
                      INSERT INTO [Staff3] ([Id], [Name], [Entry_Date]) VALUES (@Id , 'name', GETDATE())
                      SELECT @Id
                      """;
            var identity = _sqlClient.ExecuteObject<Guid>(sql);
        }


        [TestMethod]
        public void TestInsertModel()
        {
            InitTestData();

            //Insert a model, model's type name is same as table name, model's property name is same as table's column name
            var staff = new Staff
            {
                Id = Guid.NewGuid(),
                AlterId = Guid.Empty,
                Name = "name",
                EntryDate = DateTime.Now,
                QuitDate = DateTime.Now,
                Degree = 1,
                Salary = 1,
                Allowance = 1,
                Allowed = true,
                Checked = false
            };
            var count1 = _sqlClient.InsertModel(staff);

            //Insert a model, specify a different table name
            var count2 = _sqlClient.InsertModel(staff, "Staff2");

            //Insert a model, specify different columns name and table name
            var mappedStaff = new
            {
                Id = staff.Id,
                Alter_Id = staff.AlterId,
                Name = staff.Name,
                Alter_Name = staff.AlterName,
                Entry_Date = staff.EntryDate,
                Quit_Date = staff.QuitDate
            };
            var count3 = _sqlClient.InsertModel(mappedStaff, "Staff3");
        }


        [TestMethod]
        public void TestTransaction()
        {
            InitTestData();

            var total = _sqlClient.ExecuteObject<int>("SELECT COUNT(*) FROM [Staff3]");
            using (var scope = new TransactionScope())
            {
                // add one row
                var staff = new Staff()
                {
                    Id = Guid.NewGuid(),
                    AlterId = Guid.Empty,
                    Name = "name",
                    EntryDate = DateTime.Now,
                    QuitDate = DateTime.Now,
                    Degree = 1,
                    Salary = 1,
                    Allowance = 1,
                    Allowed = true,
                    Checked = false
                };
                _sqlClient.InsertModel(staff, "Staff3");

                // add another row
                var sql = """
                          DECLARE @Id UNIQUEIDENTIFIER
                          SET @Id = NEWID()
                          INSERT INTO [Staff3] ([Id], [Name], [Entry_Date]) VALUES (@Id , 'name', GETDATE())
                          SELECT @Id
                          """;
                _sqlClient.ExecuteObject<Guid>(sql);

                scope.Complete();
            }
            total = _sqlClient.ExecuteObject<int>("SELECT COUNT(*) FROM [Staff3]");
        }

        [TestMethod]
        public void TestDeleteModel()
        {
            InitTestData();

            // sql: DELETE FROM Staff3 WHERE Name = @Name
            // params: @Name = "name"
            var count1 = _sqlClient.DeleteModel("Staff3", new { Name = "name" });

            //sql: DELETE FROM Staff3
            var count2 = _sqlClient.DeleteModel("Staff3", null);
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

            var newId = Guid.NewGuid();
            
            // data model has table name: Staff
            var staff = new Staff()
            {
                Id = newId,
                AlterId = Guid.NewGuid(),
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
            // Update table Staff, with row condition: Id = newId
            var count1 = _sqlClient.UpdateModel(staff, new { Id = newId });


            // data model has no table name and three columns
            var model = new 
            {
                Alter_Id = Guid.Empty,
                Alter_Name = "alter",
                Quit_Date = DateTime.Now
            };

            // Update table Staff3 by model, will affect all rows.
            var count2 = _sqlClient.UpdateModel(model, null, "Staff3");

            // Update table Staff3 by model, with row condition: Id = Guid.Empty
            var count3 = _sqlClient.UpdateModel(model, new { Id = Guid.Empty }, "Staff3");
        }

        [TestMethod]
        public void TestInsertOrUpdateModel()
        {
            InitTestData();

            var newId = Guid.NewGuid();
            // data model has table name: Staff
            var staff = new Staff()
            {
                Id = newId,
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

            var count1 = _sqlClient.InsertOrUpdateModel(staff, new { Id = newId });

            var model = new
            {
                Alter_Id = Guid.Empty,
                Alter_Name = "alter",
                Quit_Date = DateTime.Now
            };
            // Insert or update table Staff3 by model, will affect all rows
            var count2 = _sqlClient.InsertOrUpdateModel(model, null, "Staff3");

            // Insert or update table Staff3 by model, with row condition: Id = Guid.Empty
            var count3 = _sqlClient.InsertOrUpdateModel(model, new { Id = Guid.Empty }, "Staff3");

        }
    }
}