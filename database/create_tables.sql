-- Manual SQL Server schema for Employee Leave Management System.
-- EF Core migrations are the recommended way to create/update the database.

IF DB_ID(N'LeaveManagementDb') IS NULL
BEGIN
    CREATE DATABASE LeaveManagementDb;
END
GO

USE LeaveManagementDb;
GO

IF OBJECT_ID(N'dbo.LeaveRequests', N'U') IS NOT NULL DROP TABLE dbo.LeaveRequests;
IF OBJECT_ID(N'dbo.LeaveTypes', N'U') IS NOT NULL DROP TABLE dbo.LeaveTypes;
IF OBJECT_ID(N'dbo.Employees', N'U') IS NOT NULL DROP TABLE dbo.Employees;
GO

CREATE TABLE dbo.Employees (
    EmployeeID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Employees PRIMARY KEY,
    FirstName nvarchar(50) NOT NULL,
    LastName nvarchar(50) NOT NULL,
    Email nvarchar(100) NOT NULL,
    Department nvarchar(50) NOT NULL,
    IsActive bit NOT NULL CONSTRAINT DF_Employees_IsActive DEFAULT 1,
    CONSTRAINT UQ_Employees_Email UNIQUE (Email)
);
GO

CREATE TABLE dbo.LeaveTypes (
    LeaveTypeID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_LeaveTypes PRIMARY KEY,
    LeaveName nvarchar(50) NOT NULL,
    CONSTRAINT UQ_LeaveTypes_LeaveName UNIQUE (LeaveName)
);
GO

CREATE TABLE dbo.LeaveRequests (
    RequestID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_LeaveRequests PRIMARY KEY,
    EmployeeID int NOT NULL,
    LeaveTypeID int NOT NULL,
    FromDate date NOT NULL,
    ToDate date NOT NULL,
    NumberOfDays int NOT NULL,
    Reason nvarchar(500) NOT NULL,
    Status nvarchar(20) NOT NULL CONSTRAINT DF_LeaveRequests_Status DEFAULT N'Pending',
    ManagerComments nvarchar(500) NULL,
    CreatedDate datetime2 NOT NULL CONSTRAINT DF_LeaveRequests_CreatedDate DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_LeaveRequests_Employees_EmployeeID FOREIGN KEY (EmployeeID) REFERENCES dbo.Employees(EmployeeID),
    CONSTRAINT FK_LeaveRequests_LeaveTypes_LeaveTypeID FOREIGN KEY (LeaveTypeID) REFERENCES dbo.LeaveTypes(LeaveTypeID),
    CONSTRAINT CK_LeaveRequests_DateRange CHECK (ToDate >= FromDate),
    CONSTRAINT CK_LeaveRequests_Status CHECK (Status IN (N'Pending', N'Approved', N'Rejected'))
);
GO

CREATE INDEX IX_LeaveRequests_EmployeeID ON dbo.LeaveRequests(EmployeeID);
CREATE INDEX IX_LeaveRequests_LeaveTypeID ON dbo.LeaveRequests(LeaveTypeID);
GO

SET IDENTITY_INSERT dbo.LeaveTypes ON;
INSERT INTO dbo.LeaveTypes (LeaveTypeID, LeaveName) VALUES
    (1, N'Annual Leave'),
    (2, N'Sick Leave'),
    (3, N'Unpaid Leave');
SET IDENTITY_INSERT dbo.LeaveTypes OFF;
GO
