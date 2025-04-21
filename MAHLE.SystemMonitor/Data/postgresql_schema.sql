-- Create database
CREATE DATABASE MahleMonitoring;

\c MahleMonitoring

-- Create tables
CREATE TABLE "WorkerStatuses" (
    "Id" SERIAL PRIMARY KEY,
    "MachineName" VARCHAR(255) NOT NULL,
    "IsRunning" BOOLEAN NOT NULL,
    "LastUpdated" TIMESTAMP NOT NULL
);

CREATE TABLE "DiskSpaces" (
    "Id" SERIAL PRIMARY KEY,
    "MachineName" VARCHAR(255) NOT NULL,
    "DriveName" VARCHAR(50) NOT NULL,
    "TotalSpaceBytes" BIGINT NOT NULL,
    "UsedSpaceBytes" BIGINT NOT NULL,
    "CollectedAt" TIMESTAMP NOT NULL
);

CREATE TABLE "ServiceStatuses" (
    "Id" SERIAL PRIMARY KEY,
    "MachineName" VARCHAR(255) NOT NULL,
    "ServiceName" VARCHAR(255) NOT NULL,
    "IsRunning" BOOLEAN NOT NULL,
    "CheckedAt" TIMESTAMP NOT NULL
);

CREATE TABLE "BackupInfos" (
    "Id" SERIAL PRIMARY KEY,
    "MachineName" VARCHAR(255) NOT NULL,
    "DatabaseName" VARCHAR(255) NOT NULL,
    "BackupPath" VARCHAR(1024) NOT NULL,
    "SizeBytes" BIGINT NOT NULL,
    "BackupDate" TIMESTAMP NOT NULL,
    "CollectedAt" TIMESTAMP NOT NULL
);

CREATE TABLE "PingStatuses" (
    "Id" SERIAL PRIMARY KEY,
    "SourceMachineName" VARCHAR(255) NOT NULL,
    "TargetMachineName" VARCHAR(255) NOT NULL,
    "TargetIpAddress" VARCHAR(45) NOT NULL,
    "IsReachable" BOOLEAN NOT NULL,
    "ResponseTimeMs" BIGINT NOT NULL,
    "CheckedAt" TIMESTAMP NOT NULL
);

CREATE TABLE "SystemParameters" (
    "Id" SERIAL PRIMARY KEY,
    "MachineName" VARCHAR(255) NOT NULL,
    "ParameterKey" VARCHAR(255) NOT NULL,
    "ParameterValue" VARCHAR(1024) NOT NULL,
    "Description" VARCHAR(1024),
    "LastModified" TIMESTAMP NOT NULL
);

-- Create indexes
CREATE INDEX "IX_WorkerStatuses_MachineName" ON "WorkerStatuses"("MachineName");
CREATE INDEX "IX_DiskSpaces_MachineName_DriveName" ON "DiskSpaces"("MachineName", "DriveName");
CREATE INDEX "IX_ServiceStatuses_MachineName_ServiceName" ON "ServiceStatuses"("MachineName", "ServiceName");
CREATE INDEX "IX_BackupInfos_MachineName_DatabaseName" ON "BackupInfos"("MachineName", "DatabaseName");
CREATE INDEX "IX_PingStatuses_SourceMachineName_TargetMachineName" ON "PingStatuses"("SourceMachineName", "TargetMachineName");
CREATE UNIQUE INDEX "IX_SystemParameters_MachineName_ParameterKey" ON "SystemParameters"("MachineName", "ParameterKey");

-- Sample system parameters
INSERT INTO "SystemParameters" ("MachineName", "ParameterKey", "ParameterValue", "Description", "LastModified")
VALUES
    ('YOUR_MACHINE_NAME', 'Service:SQLServer', 'MSSQLSERVER', 'SQL Server service to monitor', CURRENT_TIMESTAMP),
    ('YOUR_MACHINE_NAME', 'Backup:M2S', '/path/to/backups', 'M2S database backup location', CURRENT_TIMESTAMP),
    ('YOUR_MACHINE_NAME', 'PingTarget:PC1', '192.168.1.100', 'First tethered PC to monitor', CURRENT_TIMESTAMP),
    ('YOUR_MACHINE_NAME', 'PingTarget:PC2', '192.168.1.101', 'Second tethered PC to monitor', CURRENT_TIMESTAMP);