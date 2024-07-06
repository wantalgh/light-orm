
CREATE TABLE [Staff] (
    [Id]        UNIQUEIDENTIFIER NOT NULL,
    [AlterId]   UNIQUEIDENTIFIER NULL,
    [Name]      NVARCHAR (50)    NOT NULL,
    [AlterName] NVARCHAR (50)    NULL,
    [EntryDate] DATETIME2 (7)    NOT NULL,
    [QuitDate]  DATETIME2 (7)    NULL,
    [Degree]    INT              NOT NULL,
    [Balance]   INT              NULL,
    [Salary]    DECIMAL (18)     NOT NULL,
    [Allowance] DECIMAL (18)     NULL,
    [Allowed]   BIT              NOT NULL,
    [Checked]   BIT              NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


CREATE TABLE [Staff2] (
    [Id]        UNIQUEIDENTIFIER NOT NULL,
    [AlterId]   UNIQUEIDENTIFIER NULL,
    [Name]      NVARCHAR (50)    NOT NULL,
    [AlterName] NVARCHAR (50)    NULL,
    [EntryDate] DATETIME2 (7)    NOT NULL,
    [QuitDate]  DATETIME2 (7)    NULL,
    [Degree]    INT              NOT NULL,
    [Balance]   INT              NULL,
    [Salary]    DECIMAL (18)     NOT NULL,
    [Allowance] DECIMAL (18)     NULL,
    [Allowed]   BIT              NOT NULL,
    [Checked]   BIT              NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


CREATE TABLE [Staff3] (
    [Id]         UNIQUEIDENTIFIER NOT NULL,
    [Alter_Id]   UNIQUEIDENTIFIER NULL,
    [Name]       NVARCHAR (50)    NOT NULL,
    [Alter_Name] NVARCHAR (50)    NULL,
    [Entry_Date] DATETIME2 (7)    NOT NULL,
    [Quit_Date]  DATETIME2 (7)    NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


CREATE PROCEDURE [sp_deletestaffbyid]
	@Id Uniqueidentifier
AS
BEGIN
	DELETE FROM [dbo].[Staff3]
		  WHERE [Id] = @Id
END


CREATE PROCEDURE [sp_querystaffbyname]
	@Name NVARCHAR(10) 
AS
BEGIN
	SELECT [Id]
		  ,[AlterId]
		  ,[Name]
		  ,[AlterName]
		  ,[EntryDate]
		  ,[QuitDate]
		  ,[Degree]
		  ,[Balance]
		  ,[Salary]
		  ,[Allowance]
		  ,[Allowed]
		  ,[Checked]
	  FROM [dbo].[Staff]
	 WHERE [Name] = @Name OR [AlterName] = @Name
END