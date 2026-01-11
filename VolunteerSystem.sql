

/* ==========================================
   VolunteerSystem - Full Schema (SQL Server)
   From scratch (DROP + CREATE)
   ========================================== */

-- Drop & recreate database (DEV only)
IF DB_ID('VolunteerSystem') IS NOT NULL
BEGIN
    ALTER DATABASE VolunteerSystem SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE VolunteerSystem;
END
GO

CREATE DATABASE VolunteerSystem;
GO
USE VolunteerSystem;
GO

/* ==========================================
   1) USERS / ROLES / AUTH
   ========================================== */

CREATE TABLE dbo.Users (
    Id INT IDENTITY(1,1) CONSTRAINT PK_Users PRIMARY KEY,
    Email NVARCHAR(256) NOT NULL,
    PasswordHash NVARCHAR(512) NOT NULL,

    -- Account state
    Status INT NOT NULL CONSTRAINT DF_Users_Status DEFAULT 0,  -- 0 Active, 1 Suspended, 2 PendingApproval
    IsEmailConfirmed BIT NOT NULL CONSTRAINT DF_Users_IsEmailConfirmed DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
    LastLoginAt DATETIME2 NULL
);
GO

CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users(Email);
GO

CREATE TABLE dbo.Roles (
    Id INT IDENTITY(1,1) CONSTRAINT PK_Roles PRIMARY KEY,
    Name NVARCHAR(64) NOT NULL CONSTRAINT UQ_Roles_Name UNIQUE -- Volunteer, Organizer, Admin
);
GO

CREATE TABLE dbo.UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id) ON DELETE CASCADE
);
GO

-- Profiles (separate, clean)
CREATE TABLE dbo.VolunteerProfiles (
    Id INT NOT NULL CONSTRAINT PK_VolunteerProfiles PRIMARY KEY,
    FullName NVARCHAR(256) NOT NULL,
    Phone NVARCHAR(50) NULL,
    Bio NVARCHAR(MAX) NULL,
    Skills NVARCHAR(MAX) NOT NULL CONSTRAINT DF_VolunteerProfiles_Skills DEFAULT '',
    Points INT NOT NULL CONSTRAINT DF_VolunteerProfiles_Points DEFAULT 0,

    City NVARCHAR(128) NULL,
    Lat DECIMAL(9,6) NULL,
    Lng DECIMAL(9,6) NULL,

    CONSTRAINT FK_VolunteerProfiles_User FOREIGN KEY (Id) REFERENCES dbo.Users(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.OrganizerProfiles (
    Id INT NOT NULL CONSTRAINT PK_OrganizerProfiles PRIMARY KEY,
    OrganizationName NVARCHAR(256) NOT NULL,
    OrganizationDescription NVARCHAR(MAX) NULL,

    City NVARCHAR(128) NULL,
    Lat DECIMAL(9,6) NULL,
    Lng DECIMAL(9,6) NULL,

    IsVerified BIT NOT NULL CONSTRAINT DF_OrganizerProfiles_IsVerified DEFAULT 0,

    CONSTRAINT FK_OrganizerProfiles_User FOREIGN KEY (Id) REFERENCES dbo.Users(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.AdminProfiles (
    Id INT NOT NULL CONSTRAINT PK_AdminProfiles PRIMARY KEY,
    CONSTRAINT FK_AdminProfiles_User FOREIGN KEY (Id) REFERENCES dbo.Users(Id) ON DELETE CASCADE
);
GO

-- Password recovery
CREATE TABLE dbo.PasswordResetTokens (
    Id INT IDENTITY(1,1) CONSTRAINT PK_PasswordResetTokens PRIMARY KEY,
    UserId INT NOT NULL,
    TokenHash NVARCHAR(256) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    UsedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_PasswordResetTokens_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_PasswordResetTokens_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_PasswordResetTokens_UserId ON dbo.PasswordResetTokens(UserId);
GO

-- Admin audit actions
CREATE TABLE dbo.AdminActions (
    Id INT IDENTITY(1,1) CONSTRAINT PK_AdminActions PRIMARY KEY,
    AdminUserId INT NOT NULL,
    TargetUserId INT NOT NULL,
    ActionType INT NOT NULL, -- 0 Approve, 1 Suspend, 2 Unsuspend, 3 ResetPassword
    Reason NVARCHAR(512) NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AdminActions_CreatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_AdminActions_Admin FOREIGN KEY (AdminUserId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_AdminActions_Target FOREIGN KEY (TargetUserId) REFERENCES dbo.Users(Id)
);
GO
CREATE INDEX IX_AdminActions_TargetUserId ON dbo.AdminActions(TargetUserId);
GO

/* Seed roles */
INSERT INTO dbo.Roles(Name) VALUES (N'Volunteer'), (N'Organizer'), (N'Admin');
GO

/* ==========================================
   2) RECOMMENDATIONS: SKILLS / INTERESTS
   ========================================== */

CREATE TABLE dbo.Skills (
    Id INT IDENTITY(1,1) CONSTRAINT PK_Skills PRIMARY KEY,
    Name NVARCHAR(128) NOT NULL CONSTRAINT UQ_Skills_Name UNIQUE
);
GO

CREATE TABLE dbo.Interests (
    Id INT IDENTITY(1,1) CONSTRAINT PK_Interests PRIMARY KEY,
    Name NVARCHAR(128) NOT NULL CONSTRAINT UQ_Interests_Name UNIQUE
);
GO

CREATE TABLE dbo.VolunteerSkills (
    VolunteerId INT NOT NULL,
    SkillId INT NOT NULL,
    CONSTRAINT PK_VolunteerSkills PRIMARY KEY (VolunteerId, SkillId),
    CONSTRAINT FK_VolunteerSkills_Volunteer FOREIGN KEY (VolunteerId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_VolunteerSkills_Skill FOREIGN KEY (SkillId) REFERENCES dbo.Skills(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.VolunteerInterests (
    VolunteerId INT NOT NULL,
    InterestId INT NOT NULL,
    CONSTRAINT PK_VolunteerInterests PRIMARY KEY (VolunteerId, InterestId),
    CONSTRAINT FK_VolunteerInterests_Volunteer FOREIGN KEY (VolunteerId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_VolunteerInterests_Interest FOREIGN KEY (InterestId) REFERENCES dbo.Interests(Id) ON DELETE CASCADE
);
GO

/* ==========================================
   3) OPPORTUNITIES / EVENTS
   ========================================== */

CREATE TABLE dbo.Opportunities (
    Id INT IDENTITY(1,1) CONSTRAINT PK_Opportunities PRIMARY KEY,
    OrganizerId INT NOT NULL,
    Title NVARCHAR(256) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    RequiredSkills NVARCHAR(MAX) NOT NULL CONSTRAINT DF_Opportunities_RequiredSkills DEFAULT '',
    Location NVARCHAR(256) NULL,

    City NVARCHAR(128) NULL,
    Lat DECIMAL(9,6) NULL,
    Lng DECIMAL(9,6) NULL,

    TotalSlots INT NOT NULL CONSTRAINT DF_Opportunities_TotalSlots DEFAULT 0,
    ApplicationDeadline DATETIME2 NULL,

    Status INT NOT NULL CONSTRAINT DF_Opportunities_Status DEFAULT 1, -- 0 Draft, 1 Published, 2 Archived, 3 Deleted
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Opportunities_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Opportunities_Organizer FOREIGN KEY (OrganizerId) REFERENCES dbo.Users(Id)
);
GO
CREATE INDEX IX_Opportunities_OrganizerId ON dbo.Opportunities(OrganizerId);
GO

CREATE TABLE dbo.OpportunitySkills (
    OpportunityId INT NOT NULL,
    SkillId INT NOT NULL,
    CONSTRAINT PK_OpportunitySkills PRIMARY KEY (OpportunityId, SkillId),
    CONSTRAINT FK_OpportunitySkills_Opportunity FOREIGN KEY (OpportunityId) REFERENCES dbo.Opportunities(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OpportunitySkills_Skill FOREIGN KEY (SkillId) REFERENCES dbo.Skills(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.OpportunityInterests (
    OpportunityId INT NOT NULL,
    InterestId INT NOT NULL,
    CONSTRAINT PK_OpportunityInterests PRIMARY KEY (OpportunityId, InterestId),
    CONSTRAINT FK_OpportunityInterests_Opportunity FOREIGN KEY (OpportunityId) REFERENCES dbo.Opportunities(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OpportunityInterests_Interest FOREIGN KEY (InterestId) REFERENCES dbo.Interests(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.Events (
    Id INT IDENTITY(1,1) CONSTRAINT PK_Events PRIMARY KEY,
    OpportunityId INT NOT NULL,
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    MaxVolunteers INT NOT NULL CONSTRAINT DF_Events_MaxVolunteers DEFAULT 0,
    IsCompleted BIT NOT NULL CONSTRAINT DF_Events_IsCompleted DEFAULT 0,

    CONSTRAINT FK_Events_Opportunity FOREIGN KEY (OpportunityId) REFERENCES dbo.Opportunities(Id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_Events_OpportunityId ON dbo.Events(OpportunityId);
GO
CREATE INDEX IX_Events_StartAt ON dbo.Events(StartAt);
GO

/* ==========================================
   4) APPLY / APPROVE / WITHDRAW + ATTENDANCE
   ========================================== */

CREATE TABLE dbo.Applications (
    Id INT IDENTITY(1,1) CONSTRAINT PK_Applications PRIMARY KEY,
    EventId INT NOT NULL,
    VolunteerId INT NOT NULL,

    Status INT NOT NULL CONSTRAINT DF_Applications_Status DEFAULT 0, -- 0 Pending, 1 Approved, 2 Rejected, 3 Withdrawn
    AppliedAt DATETIME2 NOT NULL CONSTRAINT DF_Applications_AppliedAt DEFAULT SYSUTCDATETIME(),

    DecidedAt DATETIME2 NULL,
    DecidedByOrganizerId INT NULL,

    CONSTRAINT FK_Applications_Event FOREIGN KEY (EventId) REFERENCES dbo.Events(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Applications_Volunteer FOREIGN KEY (VolunteerId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Applications_DecidedBy FOREIGN KEY (DecidedByOrganizerId) REFERENCES dbo.Users(Id),

    CONSTRAINT UQ_Applications_Event_Volunteer UNIQUE (EventId, VolunteerId)
);
GO
CREATE INDEX IX_Applications_EventId ON dbo.Applications(EventId);
GO
CREATE INDEX IX_Applications_VolunteerId ON dbo.Applications(VolunteerId);
GO
CREATE INDEX IX_Applications_Status ON dbo.Applications(Status);
GO

CREATE TABLE dbo.EventAttendance (
    Id INT IDENTITY(1,1) CONSTRAINT PK_EventAttendance PRIMARY KEY,
    EventId INT NOT NULL,
    VolunteerId INT NOT NULL,
    Status INT NOT NULL CONSTRAINT DF_EventAttendance_Status DEFAULT 0, -- 0 Present, 1 Absent, 2 Excused
    MarkedAt DATETIME2 NOT NULL CONSTRAINT DF_EventAttendance_MarkedAt DEFAULT SYSUTCDATETIME(),
    MarkedByOrganizerId INT NOT NULL,

    CONSTRAINT FK_EventAttendance_Event FOREIGN KEY (EventId) REFERENCES dbo.Events(Id) ON DELETE CASCADE,
    CONSTRAINT FK_EventAttendance_Volunteer FOREIGN KEY (VolunteerId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_EventAttendance_MarkedBy FOREIGN KEY (MarkedByOrganizerId) REFERENCES dbo.Users(Id),

    CONSTRAINT UQ_EventAttendance_Event_Volunteer UNIQUE (EventId, VolunteerId)
);
GO

/* ==========================================
   5) TEAMS (per event) + instructions
   ========================================== */

CREATE TABLE dbo.Teams (
    Id INT IDENTITY(1,1) CONSTRAINT PK_Teams PRIMARY KEY,
    EventId INT NOT NULL,
    Name NVARCHAR(128) NOT NULL,
    Instructions NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Teams_Event FOREIGN KEY (EventId) REFERENCES dbo.Events(Id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_Teams_EventId ON dbo.Teams(EventId);
GO

CREATE TABLE dbo.TeamMembers (
    TeamId INT NOT NULL,
    VolunteerId INT NOT NULL,
    CONSTRAINT PK_TeamMembers PRIMARY KEY (TeamId, VolunteerId),
    CONSTRAINT FK_TeamMembers_Team FOREIGN KEY (TeamId) REFERENCES dbo.Teams(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TeamMembers_Volunteer FOREIGN KEY (VolunteerId) REFERENCES dbo.Users(Id)
);
GO

/* ==========================================
   6) FEEDBACK + POINTS
   ========================================== */

CREATE TABLE dbo.Feedbacks (
    Id INT IDENTITY(1,1) CONSTRAINT PK_Feedbacks PRIMARY KEY,
    EventId INT NOT NULL,
    VolunteerId INT NOT NULL,
    Rating INT NOT NULL,
    Comment NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Feedbacks_CreatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Feedbacks_Event FOREIGN KEY (EventId) REFERENCES dbo.Events(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Feedbacks_Volunteer FOREIGN KEY (VolunteerId) REFERENCES dbo.Users(Id),

    CONSTRAINT UQ_Feedbacks_Event_Volunteer UNIQUE (EventId, VolunteerId)
);
GO

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
GO
CREATE INDEX IX_PointsTransactions_VolunteerId ON dbo.PointsTransactions(VolunteerId);
GO

/* ==========================================
   7) REPORTS (generic target)
   ========================================== */

CREATE TABLE dbo.Reports (
    Id INT IDENTITY(1,1) CONSTRAINT PK_Reports PRIMARY KEY,
    ReporterId INT NOT NULL,

    TargetType INT NOT NULL, -- 0 Opportunity, 1 User, 2 Message, 3 Event
    TargetId INT NOT NULL,   -- ID in respective table
    ReasonType INT NOT NULL, -- 0 Fake, 1 Abuse, 2 Spam, 3 Other

    Description NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Reports_CreatedAt DEFAULT SYSUTCDATETIME(),
    Status INT NOT NULL CONSTRAINT DF_Reports_Status DEFAULT 0, -- 0 Open, 1 InReview, 2 Resolved, 3 Rejected

    ResolvedByAdminId INT NULL,
    ResolvedAt DATETIME2 NULL,

    CONSTRAINT FK_Reports_Reporter FOREIGN KEY (ReporterId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Reports_ResolvedBy FOREIGN KEY (ResolvedByAdminId) REFERENCES dbo.Users(Id)
);
GO
CREATE INDEX IX_Reports_Status ON dbo.Reports(Status);
GO
CREATE INDEX IX_Reports_Target ON dbo.Reports(TargetType, TargetId);
GO

/* ==========================================
   8) CHAT (conversations + messages)
   ========================================== */

CREATE TABLE dbo.ChatConversations (
    Id INT IDENTITY(1,1) CONSTRAINT PK_ChatConversations PRIMARY KEY,
    EventId INT NULL,
    OrganizerId INT NOT NULL,
    VolunteerId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ChatConversations_CreatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_ChatConversations_Event FOREIGN KEY (EventId) REFERENCES dbo.Events(Id) ON DELETE SET NULL,
    CONSTRAINT FK_ChatConversations_Organizer FOREIGN KEY (OrganizerId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_ChatConversations_Volunteer FOREIGN KEY (VolunteerId) REFERENCES dbo.Users(Id)
);
GO
CREATE INDEX IX_ChatConversations_EventId ON dbo.ChatConversations(EventId);
GO
CREATE INDEX IX_ChatConversations_OrgVol ON dbo.ChatConversations(OrganizerId, VolunteerId);
GO

CREATE TABLE dbo.ChatMessages (
    Id INT IDENTITY(1,1) CONSTRAINT PK_ChatMessages PRIMARY KEY,
    ConversationId INT NOT NULL,
    SenderId INT NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    SentAt DATETIME2 NOT NULL CONSTRAINT DF_ChatMessages_SentAt DEFAULT SYSUTCDATETIME(),
    ReadAt DATETIME2 NULL,

    CONSTRAINT FK_ChatMessages_Conversation FOREIGN KEY (ConversationId) REFERENCES dbo.ChatConversations(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ChatMessages_Sender FOREIGN KEY (SenderId) REFERENCES dbo.Users(Id)
);
GO
CREATE INDEX IX_ChatMessages_ConversationId ON dbo.ChatMessages(ConversationId);
GO

/* ==========================================
   Helpful views (optional): Calendar queries are derived from Events + Applications
   ========================================== */

/* ==========================================
   SEED DATA
   Password: Password123! -> a109e36947ad56de1dca1cc49f0ef8ac9ad9a7b1aa0df41fb3c4cb73c1ff01ea
   ========================================== */

-- 1. Admin User
INSERT INTO dbo.Users (Email, PasswordHash, IsEmailConfirmed)
VALUES (N'admin@test.com', N'a109e36947ad56de1dca1cc49f0ef8ac9ad9a7b1aa0df41fb3c4cb73c1ff01ea', 1);

DECLARE @AdminId INT = SCOPE_IDENTITY();

INSERT INTO dbo.AdminProfiles (Id) VALUES (@AdminId);
INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (@AdminId, 3); -- 3 = Admin

-- 2. Volunteer User
INSERT INTO dbo.Users (Email, PasswordHash, IsEmailConfirmed)
VALUES (N'volunteer@test.com', N'a109e36947ad56de1dca1cc49f0ef8ac9ad9a7b1aa0df41fb3c4cb73c1ff01ea', 1);

DECLARE @VolunteerId INT = SCOPE_IDENTITY();

INSERT INTO dbo.VolunteerProfiles (Id, FullName, Skills, Points)
VALUES (@VolunteerId, N'Demo Volunteer', N'Coding,Testing', 10);

INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (@VolunteerId, 1); -- 1 = Volunteer
GO

