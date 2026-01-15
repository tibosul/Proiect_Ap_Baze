USE VolunteerSystem;
GO

-- 1. Add IsPresent to Applications
IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'IsPresent' AND Object_ID = Object_ID(N'Applications'))
BEGIN
    PRINT 'Adding IsPresent column to Applications...';
    ALTER TABLE dbo.Applications ADD IsPresent BIT NOT NULL CONSTRAINT DF_Applications_IsPresent DEFAULT 0;
END
GO

-- 2. Fix Feedback Table (Rename Date or Add CreatedAt)
-- If 'Date' exists, rename it. If not, add CreatedAt.
IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'Date' AND Object_ID = Object_ID(N'Feedbacks'))
BEGIN
    PRINT 'Renaming Date column to CreatedAt in Feedbacks...';
    EXEC sp_rename 'dbo.Feedbacks.Date', 'CreatedAt', 'COLUMN';
END
ELSE IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'CreatedAt' AND Object_ID = Object_ID(N'Feedbacks'))
BEGIN
    PRINT 'Adding CreatedAt column to Feedbacks...';
    ALTER TABLE dbo.Feedbacks ADD CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Feedbacks_CreatedAt DEFAULT SYSUTCDATETIME();
END
GO
