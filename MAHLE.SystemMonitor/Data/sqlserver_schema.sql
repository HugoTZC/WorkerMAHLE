-- Create database
CREATE DATABASE MahleMonitoring;
GO

USE MahleMonitoring;
GO

-- Create tables
CREATE TABLE WorkerStatuses (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MachineName NVARCHAR(255) NOT NULL,
    IsRunning BIT NOT NULL,
    LastUpdated DATETIME2 NOT NULL
);

CREATE TABLE DiskSpaces (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MachineName NVARCHAR(255) NOT NULL,
    DriveName NVARCHAR(50) NOT NULL,
    TotalSpaceBytes BIGINT NOT NULL,
    UsedSpaceBytes BIGINT NOT NULL,
    CollectedAt DATETIME2 NOT NULL
);

CREATE TABLE ServiceStatuses (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MachineName NVARCHAR(255) NOT NULL,
    ServiceName NVARCHAR(255) NOT NULL,
    IsRunning BIT NOT NULL,
    CheckedAt DATETIME2 NOT NULL
);

CREATE TABLE BackupInfos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MachineName NVARCHAR(255) NOT NULL,
    DatabaseName NVARCHAR(255) NOT NULL,
    BackupPath NVARCHAR(1024) NOT NULL,
    SizeBytes BIGINT NOT NULL,
    BackupDate DATETIME2 NOT NULL,
    CollectedAt DATETIME2 NOT NULL
);

CREATE TABLE PingStatuses (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SourceMachineName NVARCHAR(255) NOT NULL,
    TargetMachineName NVARCHAR(255) NOT NULL,
    TargetIpAddress NVARCHAR(45) NOT NULL,
    IsReachable BIT NOT NULL,
    ResponseTimeMs BIGINT NOT NULL,
    CheckedAt DATETIME2 NOT NULL
);

CREATE TABLE SystemParameters (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MachineName NVARCHAR(255) NOT NULL,
    ParameterKey NVARCHAR(255) NOT NULL,
    ParameterValue NVARCHAR(1024) NOT NULL,
    Description NVARCHAR(1024) NULL,
    LastModified DATETIME2 NOT NULL
);

-- Create indexes
CREATE INDEX IX_WorkerStatuses_MachineName ON WorkerStatuses(MachineName);
CREATE INDEX IX_DiskSpaces_MachineName_DriveName ON DiskSpaces(MachineName, DriveName);
CREATE INDEX IX_ServiceStatuses_MachineName_ServiceName ON ServiceStatuses(MachineName, ServiceName);
CREATE INDEX IX_BackupInfos_MachineName_DatabaseName ON BackupInfos(MachineName, DatabaseName);
CREATE INDEX IX_PingStatuses_SourceMachineName_TargetMachineName ON PingStatuses(SourceMachineName, TargetMachineName);
CREATE UNIQUE INDEX IX_SystemParameters_MachineName_ParameterKey ON SystemParameters(MachineName, ParameterKey);

-- Sample system parameters
INSERT INTO SystemParameters (MachineName, ParameterKey, ParameterValue, Description, LastModified)
VALUES
    ('YOUR_MACHINE_NAME', 'Service:SQLServer', 'MSSQLSERVER', 'SQL Server service to monitor', GETUTCDATE()),
    ('YOUR_MACHINE_NAME', 'Backup:M2S', 'C:\Backups', 'M2S database backup location', GETUTCDATE()),
    ('YOUR_MACHINE_NAME', 'PingTarget:PC1', '192.168.1.100', 'First tethered PC to monitor', GETUTCDATE()),
    ('YOUR_MACHINE_NAME', 'PingTarget:PC2', '192.168.1.101', 'Second tethered PC to monitor', GETUTCDATE());