USE VolunteerSystem;
GO

-- 1. Add Points column to Opportunities if missing
IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'Points' AND Object_ID = Object_ID(N'Opportunities'))
BEGIN
    PRINT 'Adding Points column to Opportunities table...';
    ALTER TABLE dbo.Opportunities ADD Points INT NOT NULL CONSTRAINT DF_Opportunities_Points DEFAULT 10;
END
ELSE
BEGIN
    PRINT 'Points column already exists in Opportunities.';
END
GO

-- 2. Create PointsTransactions table if missing
IF OBJECT_ID(N'dbo.PointsTransactions', N'U') IS NULL
BEGIN
    PRINT 'Creating PointsTransactions table...';
    CREATE TABLE dbo.PointsTransactions (
        Id INT IDENTITY(1,1) CONSTRAINT PK_PointsTransactions PRIMARY KEY,
        VolunteerId INT NOT NULL,
        EventId INT NULL,
        Points INT NOT NULL,
        Reason INT NOT NULL, -- 0 CompletedEvent, 1 Bonus, 2 Penalty
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_PointsTransactions_CreatedAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT FK_PointsTransactions_Volunteer FOREIGN KEY (VolunteerId) REFERENCES dbo.Users(Id),
        CONSTRAINT FK_PointsTransactions_Event FOREIGN KEY (EventId) REFERENCES dbo.Events(Id) ON DELETE SET NULL
    );
    CREATE INDEX IX_PointsTransactions_VolunteerId ON dbo.PointsTransactions(VolunteerId);
END
ELSE
BEGIN
    PRINT 'PointsTransactions table already exists.';
END
GO
