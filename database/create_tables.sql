-- Manual SQL Server schema for Employee Leave Management System.
-- EF Core migrations are the recommended way to create/update the database.

IF OBJECT_ID(N'dbo.Employees', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Employees (
        EmployeeID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Employees PRIMARY KEY,
        FirstName nvarchar(50) NOT NULL,
        LastName nvarchar(50) NOT NULL,
        Email nvarchar(100) NOT NULL,
        Department nvarchar(50) NOT NULL,
        Role nvarchar(20) NOT NULL CONSTRAINT DF_Employees_Role DEFAULT N'Employee',
        PasswordHash nvarchar(max) NOT NULL CONSTRAINT DF_Employees_PasswordHash DEFAULT N'',
        PasswordSalt nvarchar(max) NOT NULL CONSTRAINT DF_Employees_PasswordSalt DEFAULT N'',
        IsActive bit NOT NULL CONSTRAINT DF_Employees_IsActive DEFAULT 1,
        CONSTRAINT AK_Employees_Email UNIQUE (Email),
        CONSTRAINT CK_Employees_Role CHECK (Role IN (N'Employee', N'Manager'))
    );
END;
GO

IF OBJECT_ID(N'dbo.Employees', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.Employees', N'Role') IS NULL
        ALTER TABLE dbo.Employees ADD Role nvarchar(20) NOT NULL CONSTRAINT DF_Employees_Role DEFAULT N'Employee';

    IF COL_LENGTH(N'dbo.Employees', N'PasswordHash') IS NULL
        ALTER TABLE dbo.Employees ADD PasswordHash nvarchar(max) NOT NULL CONSTRAINT DF_Employees_PasswordHash DEFAULT N'';

    IF COL_LENGTH(N'dbo.Employees', N'PasswordSalt') IS NULL
        ALTER TABLE dbo.Employees ADD PasswordSalt nvarchar(max) NOT NULL CONSTRAINT DF_Employees_PasswordSalt DEFAULT N'';

    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Employees_Role' AND parent_object_id = OBJECT_ID(N'dbo.Employees'))
        ALTER TABLE dbo.Employees ADD CONSTRAINT CK_Employees_Role CHECK (Role IN (N'Employee', N'Manager'));
END;
GO

IF OBJECT_ID(N'dbo.LeaveTypes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveTypes (
        LeaveTypeID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_LeaveTypes PRIMARY KEY,
        LeaveName nvarchar(50) NOT NULL,
        CONSTRAINT AK_LeaveTypes_LeaveName UNIQUE (LeaveName)
    );
END;
GO

IF OBJECT_ID(N'dbo.LeaveRequests', N'U') IS NULL
BEGIN
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
        CreatedDate datetime2 NOT NULL CONSTRAINT DF_LeaveRequests_CreatedDate DEFAULT GETUTCDATE(),
        CONSTRAINT FK_LeaveRequests_Employees_EmployeeID FOREIGN KEY (EmployeeID) REFERENCES dbo.Employees(EmployeeID) ON DELETE NO ACTION,
        CONSTRAINT FK_LeaveRequests_LeaveTypes_LeaveTypeID FOREIGN KEY (LeaveTypeID) REFERENCES dbo.LeaveTypes(LeaveTypeID) ON DELETE NO ACTION,
        CONSTRAINT CK_LeaveRequests_DateRange CHECK (ToDate >= FromDate),
        CONSTRAINT CK_LeaveRequests_Status CHECK (Status IN (N'Pending', N'Approved', N'Rejected'))
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LeaveRequests_EmployeeID' AND object_id = OBJECT_ID(N'dbo.LeaveRequests'))
    CREATE INDEX IX_LeaveRequests_EmployeeID ON dbo.LeaveRequests(EmployeeID);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LeaveRequests_LeaveTypeID' AND object_id = OBJECT_ID(N'dbo.LeaveRequests'))
    CREATE INDEX IX_LeaveRequests_LeaveTypeID ON dbo.LeaveRequests(LeaveTypeID);
GO

SET IDENTITY_INSERT dbo.LeaveTypes ON;

IF NOT EXISTS (SELECT 1 FROM dbo.LeaveTypes WHERE LeaveTypeID = 1 OR LeaveName = N'Annual Leave')
    INSERT INTO dbo.LeaveTypes (LeaveTypeID, LeaveName) VALUES (1, N'Annual Leave');

IF NOT EXISTS (SELECT 1 FROM dbo.LeaveTypes WHERE LeaveTypeID = 2 OR LeaveName = N'Sick Leave')
    INSERT INTO dbo.LeaveTypes (LeaveTypeID, LeaveName) VALUES (2, N'Sick Leave');

IF NOT EXISTS (SELECT 1 FROM dbo.LeaveTypes WHERE LeaveTypeID = 3 OR LeaveName = N'Unpaid Leave')
    INSERT INTO dbo.LeaveTypes (LeaveTypeID, LeaveName) VALUES (3, N'Unpaid Leave');

SET IDENTITY_INSERT dbo.LeaveTypes OFF;
GO
