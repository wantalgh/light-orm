using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Wantalgh.LightDataClient;
using Wantalgh.LightDataClient.SqlDialectBuilder;

namespace LightDataClientTest
{
    [TestClass]
    public class SqliteTest
    {
        private DataClient _sqliteClient = null!;

        [TestInitialize]
        public void Initialize()
        {
            var sqlConnStr = @"Data Source=App_Data\SqliteTestDB.db;Version=3;";
            _sqliteClient = new DataClient(() => new SQLiteConnection(sqlConnStr), new Sqlite3SqlBuilder());
        }

        [TestMethod]
        public void TestExecuteModels()
        {
            InitTestData();

            // default generation and mapping rule:
            // model's type name is same as table name,
            // model's property name is same as table's column name,
            // for details, see https://

            // Execute auto generated sql from Staff type
            // sql: SELECT Id, Name, AlterName ... FROM Trainee
            // map result to IEnumerable<Staff>
            IEnumerable<Trainee> trainees1 = _sqliteClient.ExecuteModels<Trainee>();
            Assert.AreEqual(3, trainees1.Count());

            // Execute auto generated sql with a param
            // sql: SELECT Id, Name, AlterName ... FROM Trainee WHERE Id = @Id;
            // param: @Id = 0
            IEnumerable<Trainee> trainees2 = _sqliteClient.ExecuteModels<Trainee>(parameter: new { Id = 0 });
            Assert.AreEqual(1, trainees2.Count());

            // Execute auto generated sql with more params
            // sql: SELECT Id, Name, AlterName ... FROM Trainee WHERE Score = @Score AND Name = @Name;
            // params: @Score = 0, @Name = "name"
            IEnumerable<Trainee> trainees3 = _sqliteClient.ExecuteModels<Trainee>(parameter: new { Score = 0, Name = "name" });
            Assert.AreEqual(1, trainees3.Count());

            // Execute auto generated sql with LimitOffset statement
            IEnumerable<Trainee> trainees4 = _sqliteClient.ExecuteModels<Trainee>(skip: 0, take: 1);
            Assert.AreEqual(1, trainees4.Count());

            // Execute auto generated sql with specified tableName:
            // sql: SELECT Id, Name, AlterName ... FROM Trainee2
            IEnumerable<Trainee> trainees5 = _sqliteClient.ExecuteModels<Trainee>(tableName: "Trainee2");
            Assert.AreEqual(3, trainees5.Count());

            // Execute complex auto generated sql.
            // specified tableName: Trainee2
            // sql: SELECT Id, Name, AlterName ... FROM Trainee2 WHERE Score = @Score AND Name = @Name;
            // params: @Score = 2.2, @Name = "name2",
            // with LimitOffset statement
            // map results to IEnumerable<Trainee>
            IEnumerable<Trainee> trainees6 = _sqliteClient.ExecuteModels<Trainee>(tableName: "Trainee2",
                parameter: new { Score = 2.2, Name = "name2" }, skip: 0, take: 10);
            Assert.AreEqual(1, trainees6.Count());

            // Execute sql, map results to IEnumerable<Trainee>
            IEnumerable<Trainee> trainees7 = _sqliteClient.ExecuteModels<Trainee>("SELECT * FROM Trainee WHERE Checked IS NULL");
            Assert.AreEqual(3, trainees7.Count());


            // Execute sql with params: @IsAllow = true, @PassLine = 1
            IEnumerable<Trainee> trainees8 = _sqliteClient.ExecuteModels<Trainee>(
                "SELECT * FROM Trainee2 WHERE Allowed = @IsAllow OR Score > @PassLine", new { IsAllow = true, PassLine = 1 });
            Assert.AreEqual(3, trainees8.Count());


            // Execute sql for complex mapping
            // use "AS" to match column name and property name
            // param: @SearchName = "%name%"
            var sql = """
                      SELECT 
                      Id,
                      Name,
                      Alter_Name  AS AlterName,
                      Birth_Date  AS BirthDate
                      FROM Trainee3 
                      WHERE Name LIKE @SearchName
                      ORDER BY Alter_Name
                      """;
            IEnumerable<Trainee> trainees9 = _sqliteClient.ExecuteModels<Trainee>(sql, new { SearchName = "%name%" });
            Assert.AreEqual(3, trainees9.Count());


            // Execute sql and get results in DataSet
            var mSql = """
                       SELECT * FROM Trainee WHERE Name LIKE @SearchName;
                       SELECT * FROM Trainee2 WHERE Name LIKE @SearchName;
                       SELECT * FROM Trainee3 WHERE Name LIKE @SearchName;
                       """;
            DataSet traineesA = _sqliteClient.ExecuteDataSet(mSql, new { SearchName = "%name%" });
            Assert.AreEqual(3, traineesA.Tables.Count);


            // ExecuteModel() method only returns one record, equals ExecuteModels().FirstOrDefault()
            Trainee trainee = _sqliteClient.ExecuteModel<Trainee>();
            Assert.IsNotNull(trainee);
        }


        [TestMethod]
        public void TestExecuteScalar()
        {
            InitTestData();

            //Execute sql and return an long result
            var count = _sqliteClient.ExecuteScalar<long>("SELECT COUNT(*) FROM Trainee");
            Assert.AreEqual(3, count);

            //Execute sql and return a string result
            var now = _sqliteClient.ExecuteScalar<string>("SELECT datetime('now')");
            Assert.IsNotNull(now);
        }


        [TestMethod]
        public void TestInsertModel()
        {
            InitTestData();

            //Insert a model, model's type name is table name, model's property name is table's column name
            var trainee = new Trainee
            {
                Id = 4,
                Name = "name4",
                AlterName = null,
                BirthDate = default,
                Score = 4,
                Allowed = true,
                Checked = false
            };
            var count1 = _sqliteClient.InsertModel(trainee);
            Assert.AreEqual(1, count1);


            //Insert a model, specify a different table name
            var count2 = _sqliteClient.InsertModel(trainee, "Trainee2");
            Assert.AreEqual(1, count2);

            //Insert a model, specify a table name, use wrapper object to match columns name.
            var mappedModel = new
            {
                Id = trainee.Id,
                Name = trainee.Name,
                Alter_Name = trainee.AlterName,
                Birth_Date = trainee.BirthDate,
            };
            var count3 = _sqliteClient.InsertModel(mappedModel, "Trainee3");
            Assert.AreEqual(1, count3);


            // Insert a model and return auto increment id
            var sql = """
                      INSERT INTO Trainee3 (Name, Alter_Name, Birth_Date) VALUES (@Name , @Name2, @Date);
                      SELECT last_insert_rowid()
                      """;
            var id = _sqliteClient.ExecuteScalar<long>(sql, new { Name = "name", Name2 = "name2", Date = DateTime.Now.ToString("s") });
            Assert.IsNotNull(id);
        }


        [TestMethod]
        public void TestTransaction()
        {
            InitTestData();

            var count1 = _sqliteClient.ExecuteScalar<long>("SELECT COUNT(*) FROM [Trainee3]");

            var sql = """
                      BEGIN TRANSACTION;
                      
                      INSERT INTO Trainee3 (Name, Alter_Name, Birth_Date) VALUES ('name1' , 'alter1', datetime('now'));
                      INSERT INTO Trainee3 (Name, Alter_Name, Birth_Date) VALUES ('name2' , 'alter2', datetime('now'));
                      
                      COMMIT;
                      """;
            _sqliteClient.ExecuteNone(sql);

            var count2 = _sqliteClient.ExecuteScalar<long>("SELECT COUNT(*) FROM [Trainee3]");
            Assert.AreEqual(2, count2 - count1);
        }


        [TestMethod]
        public void TestDeleteModel()
        {
            InitTestData();

            // Delete rows in table Staff3, with row condition: Id = @Id 
            // sql: DELETE FROM Staff3 WHERE Id = @Id
            // params: @Id = Guid.Empty
            var count1 = _sqliteClient.DeleteModel("Trainee3", new { Id = 1 });
            Assert.AreEqual(1, count1);

            //Delete rows in table Staff3, without row condition, will delete all rows
            //sql: DELETE FROM Staff3
            var count2 = _sqliteClient.DeleteModel("Trainee3", null);
            Assert.AreEqual(2, count2);
        }


        [TestMethod]
        public void TestExecuteNone()
        {
            InitTestData();

            // Execute vacuum command
            _sqliteClient.ExecuteNone("VACUUM");
        }


        [TestMethod]
        public void TestUpdateModel()
        {
            InitTestData();

            // data model has table name: Trainee
            var model = new Trainee
            {
                Id = 1,
                Name = "name1",
                AlterName = null,
                BirthDate = default,
                Score = 1,
                Allowed = true,
                Checked = false
            };
            // Update table Trainee, with row condition: Id = model.Id
            var count1 = _sqliteClient.UpdateModel(model, new { Id = model.Id });
            Assert.AreEqual(1, count1);

            // data model with no table name but three columns
            var model2 = new
            {
                Name = "name2",
                Alter_Name = "alter",
                Birth_Date = DateTime.Now.ToString("s")
            };
            // Update table Trainee3, without row condition, will affect all rows.
            var count2 = _sqliteClient.UpdateModel(model2, null, "Trainee3");
            Assert.AreEqual(3, count2);
            // Update table Trainee3, with row condition: Id = 0
            var count3 = _sqliteClient.UpdateModel(model2, new { Id = 0 }, "Trainee3");
            Assert.AreEqual(1, count3);
        }


        [TestMethod]
        public void TestInsertOrUpdateModel()
        {
            InitTestData();

            // data model has table name: Trainee
            var trainee = new Trainee
            {
                Id = 1,
                Name = "name1",
                AlterName = null,
                BirthDate = default,
                Score = 1,
                Allowed = true,
                Checked = false
            };
            // Insert or update table Trainee, with row condition: Id = model.Id
            var count1 = _sqliteClient.InsertOrUpdateModel(trainee, new { Id = trainee.Id });
            Assert.AreEqual(1, count1);


            // data model with no table name but some columns  
            var model = new
            {
                Name = "name",
                Alter_Name = "alter",
                Birth_Date = DateTime.Now.ToString("s")
            };
            // Insert or update table Trainee3 by model, with row condition: Name = "name"
            // insert
            var count2 = _sqliteClient.InsertOrUpdateModel(model, new { Name = model.Name }, "Trainee3");
            Assert.AreEqual(1, count2);
            // update
            var count3 = _sqliteClient.InsertOrUpdateModel(model, new { Name = model.Name }, "Trainee3");
            Assert.AreEqual(1, count3);
        }


        private void InitTestData()
        {
            _sqliteClient.ExecuteNone("DELETE FROM Trainee");
            _sqliteClient.ExecuteNone("INSERT INTO Trainee (Id, Name, BirthDate, Score, Allowed) VALUES (0, 'name', datetime('now'), 0.0, true)");
            _sqliteClient.ExecuteNone("INSERT INTO Trainee (Id, Name, BirthDate, Score, Allowed) VALUES (1, 'name', datetime('now'), 1.1, true)");
            _sqliteClient.ExecuteNone("INSERT INTO Trainee (Id, Name, BirthDate, Score, Allowed) VALUES (2, 'name', datetime('now'), 2.2, true)");

            _sqliteClient.ExecuteNone("DELETE FROM Trainee2");
            _sqliteClient.ExecuteNone("INSERT INTO Trainee2 (Id, Name, BirthDate, Score, Allowed) VALUES (0, 'name2', datetime('now'), 0.0, true)");
            _sqliteClient.ExecuteNone("INSERT INTO Trainee2 (Id, Name, BirthDate, Score, Allowed) VALUES (1, 'name2', datetime('now'), 1.1, true)");
            _sqliteClient.ExecuteNone("INSERT INTO Trainee2 (Id, Name, BirthDate, Score, Allowed) VALUES (2, 'name2', datetime('now'), 2.2, true)");

            _sqliteClient.DeleteModel("Trainee3", null);
            _sqliteClient.InsertModel(new { Id = 0, Name = "name3", Birth_Date = DateTime.Now }, "Trainee3");
            _sqliteClient.InsertModel(new { Id = 1, Name = "name3", Birth_Date = DateTime.Now }, "Trainee3");
            _sqliteClient.InsertModel(new { Id = 2, Name = "name3", Birth_Date = DateTime.Now }, "Trainee3");
        }
    }
}
