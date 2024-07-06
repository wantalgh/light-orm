
CREATE TABLE Trainee (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT  NOT NULL,
    Name      NVARCHAR(50)    NOT NULL,
    AlterName NVARCHAR(50)    NULL,
    BirthDate DATETIME        NOT NULL,
    Score     DOUBLE          NULL,
    Allowed   BOOLEAN         NOT NULL,
    Checked   BOOLEAN         NULL
);


CREATE TABLE Trainee2 (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT  NOT NULL,
    Name      NVARCHAR(50)    NOT NULL,
    AlterName NVARCHAR(50)    NULL,
    BirthDate DATETIME        NOT NULL,
    Score     DOUBLE          NULL,
    Allowed   BOOLEAN         NOT NULL,
    Checked   BOOLEAN         NULL
);


CREATE TABLE Trainee3 (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT  NOT NULL,
    Name        NVARCHAR(50)    NOT NULL,
    Alter_Name  NVARCHAR(50)    NULL,
    Birth_Date  DATETIME        NOT NULL
);