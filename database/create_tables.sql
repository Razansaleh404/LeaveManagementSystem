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
        IsActive bit NOT NULL CONSTRAINT DF_Employees_IsActive DEFAULT 1,
        CONSTRAINT AK_Employees_Email UNIQUE (Email),
        CONSTRAINT CK_Employees_Department CHECK (Department IN (N'IT', N'HR', N'Finance', N'Marketing', N'Sales', N'Operations', N'Engineering', N'Customer Support'))
    );
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

IF OBJECT_ID(N'dbo.AppUsers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppUsers (
        AppUserID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AppUsers PRIMARY KEY,
        Email nvarchar(100) NOT NULL,
        PasswordHash nvarchar(128) NOT NULL,
        Role nvarchar(20) NOT NULL,
        EmployeeID int NOT NULL,
        CONSTRAINT AK_AppUsers_Email UNIQUE (Email),
        CONSTRAINT AK_AppUsers_EmployeeID UNIQUE (EmployeeID),
        CONSTRAINT CK_AppUsers_Role CHECK (Role IN (N'Admin', N'Manager', N'Employee')),
        CONSTRAINT FK_AppUsers_Employees_EmployeeID FOREIGN KEY (EmployeeID) REFERENCES dbo.Employees(EmployeeID) ON DELETE NO ACTION
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
        CONSTRAINT FK_LeaveRequests_LeaveTypeID FOREIGN KEY (LeaveTypeID) REFERENCES dbo.LeaveTypes(LeaveTypeID) ON DELETE NO ACTION,
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

SET IDENTITY_INSERT dbo.Employees ON;

IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE EmployeeID = -1 OR Email = N'admin@demo.com')
    INSERT INTO dbo.Employees (EmployeeID, FirstName, LastName, Email, Department, IsActive) VALUES (-1, N'Admin', N'User', N'admin@demo.com', N'IT', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE EmployeeID = -2 OR Email = N'manager@demo.com')
    INSERT INTO dbo.Employees (EmployeeID, FirstName, LastName, Email, Department, IsActive) VALUES (-2, N'Manager', N'User', N'manager@demo.com', N'Operations', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE EmployeeID = -3 OR Email = N'employee@demo.com')
    INSERT INTO dbo.Employees (EmployeeID, FirstName, LastName, Email, Department, IsActive) VALUES (-3, N'Employee', N'User', N'employee@demo.com', N'Engineering', 1);

SET IDENTITY_INSERT dbo.Employees OFF;
GO

SET IDENTITY_INSERT dbo.AppUsers ON;

IF NOT EXISTS (SELECT 1 FROM dbo.AppUsers WHERE AppUserID = 1 OR Email = N'admin@demo.com')
    INSERT INTO dbo.AppUsers (AppUserID, Email, PasswordHash, Role, EmployeeID) VALUES (1, N'admin@demo.com', N'admin-demo-salt:0xJKYowDmRkZgbzycMBHGAWXJJKeGALbeBPiFWHmE8s=', N'Admin', -1);

IF NOT EXISTS (SELECT 1 FROM dbo.AppUsers WHERE AppUserID = 2 OR Email = N'manager@demo.com')
    INSERT INTO dbo.AppUsers (AppUserID, Email, PasswordHash, Role, EmployeeID) VALUES (2, N'manager@demo.com', N'manager-demo-salt:d85aZJikAQ2wmK5/hJIAbAd43LGy/RzLiT2zfUYiBmc=', N'Manager', -2);

IF NOT EXISTS (SELECT 1 FROM dbo.AppUsers WHERE AppUserID = 3 OR Email = N'employee@demo.com')
    INSERT INTO dbo.AppUsers (AppUserID, Email, PasswordHash, Role, EmployeeID) VALUES (3, N'employee@demo.com', N'employee-demo-salt:fP9vyvjXHJA08EsJkkieGeCV+OkcMSseLeG1uGkTNL0=', N'Employee', -3);

SET IDENTITY_INSERT dbo.AppUsers OFF;
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
