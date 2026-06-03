IF DB_ID('LeaveManagementDb') IS NULL
BEGIN
    CREATE DATABASE LeaveManagementDb;
END
GO

USE LeaveManagementDb;
GO

IF OBJECT_ID('dbo.LeaveRequests', 'U') IS NOT NULL DROP TABLE dbo.LeaveRequests;
IF OBJECT_ID('dbo.LeaveTypes', 'U') IS NOT NULL DROP TABLE dbo.LeaveTypes;
IF OBJECT_ID('dbo.Employees', 'U') IS NOT NULL DROP TABLE dbo.Employees;
GO

CREATE TABLE dbo.Employees (
    EmployeeID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Employees PRIMARY KEY,
    EmployeeCode VARCHAR(20) NOT NULL CONSTRAINT UQ_Employees_EmployeeCode UNIQUE,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL CONSTRAINT UQ_Employees_Email UNIQUE,
    Department VARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Employees_IsActive DEFAULT (1)
);
GO

CREATE TABLE dbo.LeaveTypes (
    LeaveTypeID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LeaveTypes PRIMARY KEY,
    LeaveName VARCHAR(50) NOT NULL
);
GO

CREATE TABLE dbo.LeaveRequests (
    RequestID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LeaveRequests PRIMARY KEY,
    EmployeeID INT NOT NULL,
    LeaveTypeID INT NOT NULL,
    FromDate DATE NOT NULL,
    ToDate DATE NOT NULL,
    NumberOfDays INT NOT NULL,
    Reason VARCHAR(500) NOT NULL,
    Status VARCHAR(20) NOT NULL CONSTRAINT DF_LeaveRequests_Status DEFAULT ('Pending'),
    CreatedDate DATETIME NOT NULL CONSTRAINT DF_LeaveRequests_CreatedDate DEFAULT (GETDATE()),
    ManagerComments VARCHAR(500) NULL,
    CONSTRAINT FK_LeaveRequests_Employees FOREIGN KEY (EmployeeID) REFERENCES dbo.Employees(EmployeeID),
    CONSTRAINT FK_LeaveRequests_LeaveTypes FOREIGN KEY (LeaveTypeID) REFERENCES dbo.LeaveTypes(LeaveTypeID),
    CONSTRAINT CK_LeaveRequests_Status CHECK (Status IN ('Pending', 'Approved', 'Rejected')),
    CONSTRAINT CK_LeaveRequests_DateRange CHECK (ToDate >= FromDate),
    CONSTRAINT CK_LeaveRequests_NumberOfDays CHECK (NumberOfDays >= 1)
);
GO

INSERT INTO dbo.LeaveTypes (LeaveName)
VALUES ('Annual Leave'), ('Sick Leave'), ('Unpaid Leave');
GO
