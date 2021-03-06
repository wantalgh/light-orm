USE [master]
GO
/****** Object:  Database [DbExtensionTest]    Script Date: 2014/7/10 9:32:37 ******/
CREATE DATABASE [DbExtensionTest] ON  PRIMARY 
( NAME = N'DbExtensionTest', FILENAME = N'E:\数据库\DbExtensionTest\DbExtensionTest.mdf' , SIZE = 3072KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'DbExtensionTest_log', FILENAME = N'E:\数据库\DbExtensionTest\DbExtensionTest_log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [DbExtensionTest] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [DbExtensionTest].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [DbExtensionTest] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [DbExtensionTest] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [DbExtensionTest] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [DbExtensionTest] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [DbExtensionTest] SET ARITHABORT OFF 
GO
ALTER DATABASE [DbExtensionTest] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [DbExtensionTest] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [DbExtensionTest] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [DbExtensionTest] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [DbExtensionTest] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [DbExtensionTest] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [DbExtensionTest] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [DbExtensionTest] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [DbExtensionTest] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [DbExtensionTest] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [DbExtensionTest] SET  DISABLE_BROKER 
GO
ALTER DATABASE [DbExtensionTest] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [DbExtensionTest] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [DbExtensionTest] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [DbExtensionTest] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [DbExtensionTest] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [DbExtensionTest] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [DbExtensionTest] SET RECOVERY FULL 
GO
ALTER DATABASE [DbExtensionTest] SET  MULTI_USER 
GO
ALTER DATABASE [DbExtensionTest] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [DbExtensionTest] SET DB_CHAINING OFF 
GO
EXEC sys.sp_db_vardecimal_storage_format N'DbExtensionTest', N'ON'
GO
USE [DbExtensionTest]
GO
/****** Object:  StoredProcedure [dbo].[sp_deletestaffbyid]    Script Date: 2014/7/10 9:32:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_deletestaffbyid]
	-- Add the parameters for the stored procedure here
	@Id Uniqueidentifier
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DELETE FROM [dbo].[Staff]
		  WHERE [Id] = @Id

END

GO
/****** Object:  StoredProcedure [dbo].[sp_querystaffbyname]    Script Date: 2014/7/10 9:32:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_querystaffbyname]
	-- Add the parameters for the stored procedure here
	@Name NVARCHAR(50) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT [Id]
		  ,[Id2]
		  ,[Name]
		  ,[EnglishName]
		  ,[Salary]
		  ,[EntryDate]
		  ,[Tax]
		  ,[QuitDate]
	  FROM [dbo].[Staff]
	 WHERE [Name] = @Name
END

GO
/****** Object:  Table [dbo].[Staff]    Script Date: 2014/7/10 9:32:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Staff](
	[Id] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[Id2] [uniqueidentifier] NULL,
	[Name] [nvarchar](100) NOT NULL,
	[EnglishName] [nvarchar](100) NULL,
	[Salary] [int] NOT NULL,
	[EntryDate] [datetime2](7) NOT NULL,
	[Tax] [int] NULL,
	[QuitDate] [datetime2](7) NULL,
 CONSTRAINT [PK_Staff] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[Staff] ADD  CONSTRAINT [DF_Staff_Id]  DEFAULT (newid()) FOR [Id]
GO
USE [master]
GO
ALTER DATABASE [DbExtensionTest] SET  READ_WRITE 
GO
