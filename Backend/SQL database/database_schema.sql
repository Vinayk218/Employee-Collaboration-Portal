-- Drop and recreate the database
DROP DATABASE IF EXISTS EmployeeCollaboration;
GO
CREATE DATABASE EmployeeCollaboration;
GO
USE EmployeeCollaboration;
GO

-- Drop tables if they exist
IF OBJECT_ID('dbo.Requests', 'U') IS NOT NULL DROP TABLE dbo.Requests;
IF OBJECT_ID('dbo.Tasks', 'U') IS NOT NULL DROP TABLE dbo.Tasks;
IF OBJECT_ID('dbo.Announcements', 'U') IS NOT NULL DROP TABLE dbo.Announcements;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;

-- Create Users table with validations
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,

    FullName NVARCHAR(100) NOT NULL
        CONSTRAINT CK_Users_FullName_Length CHECK (LEN(FullName) >= 3),

    Email NVARCHAR(100) NOT NULL UNIQUE
        CONSTRAINT CK_Users_Email_Format CHECK (Email LIKE '%@%._%'),

    Department NVARCHAR(100) NOT NULL
        CONSTRAINT CK_Users_Department_Valid CHECK (Department IN ('IT', 'HR', 'Finance')),

    Role NVARCHAR(100) NOT NULL
        CONSTRAINT CK_Users_Role_Valid CHECK (Role IN ('Admin', 'Manager', 'Employee')),

    Password NVARCHAR(100) NOT NULL
        CONSTRAINT CK_Users_Password_Strength CHECK (
            LEN(Password) >= 6 AND
            Password LIKE '%[0-9]%' AND
            Password LIKE '%[A-Z]%' AND
            Password LIKE '%[a-z]%'
        ),

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- Create Requests table
CREATE TABLE Requests (
    RequestId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId) ON DELETE CASCADE,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Priority NVARCHAR(20) NOT NULL
        CONSTRAINT CK_Requests_Priority_Valid CHECK (Priority IN ('Low', 'Medium', 'High')),
    Status NVARCHAR(50) NOT NULL
        CONSTRAINT CK_Requests_Status_Valid CHECK (Status IN ('Open', 'In Progress', 'Completed', 'Resolved', 'Pending Approval')),
    SubmissionDate DATETIME DEFAULT GETDATE()
);

-- Create Tasks table
CREATE TABLE Tasks (
    TaskId INT IDENTITY(0,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId) ON DELETE CASCADE,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    DueDate DATETIME NOT NULL
        CONSTRAINT CK_Tasks_DueDate_Valid CHECK (DueDate >= GETDATE())
);

-- Create Announcements table
CREATE TABLE Announcements (
    AnnouncementId INT  PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Reset identity seeds
DBCC CHECKIDENT ('Users', RESEED, 0);
DBCC CHECKIDENT ('Requests', RESEED, 0);
DBCC CHECKIDENT ('Tasks', RESEED, 0);
DBCC CHECKIDENT ('Announcements', RESEED, 0);


-- Insert Sample data
INSERT INTO Users (FullName, Email, Department, Role, Password, CreatedAt) VALUES
('Sai Chandra', 'sai@gmail.com', 'IT', 'Manager', 'Sai123', GETDATE()),
('Vinay Reddy', 'vinay.reddy@gmail.com', 'HR', 'Employee', 'Vinay123', GETDATE()),
('Veera Kumar', 'veera.kumar@gmail.com', 'Finance', 'Admin', 'Veera123', GETDATE()),
('Sonal Jain', 'sonal.jain@gmail.com', 'IT', 'Employee', 'Sonal123', GETDATE()),
('Vanashree M', 'vanashree.m@gmail.com', 'HR', 'Manager', 'Vana123', GETDATE()),
('Raj Shetty', 'raj.shetty@gmail.com', 'Finance', 'Employee', 'Raj123', GETDATE()),
('Ravi Rao', 'ravi.rao@gmail.com', 'IT', 'Admin', 'Ravi123', GETDATE()),
('Neha Verma', 'neha.verma@gmail.com', 'Finance', 'Manager', 'Neha123', GETDATE()),
('Amit Desai', 'amit.desai@gmail.com', 'IT', 'Employee', 'Amit123', GETDATE());


INSERT INTO Requests (UserId, Title, Description, Priority, Status) VALUES
(2, 'Laptop not working', 'My system is not booting up since morning.', 'High', 'Open'),
(3, 'New Joinee Access', 'Need to set up access for new HR intern.', 'Medium', 'In Progress'),
(6, 'Budget Approval', 'Requesting approval for Q2 expense report.', 'High', 'Pending Approval'),
(3, 'Website Banner Update', 'Update homepage banner for July campaign.', 'Low', 'Completed'),
(2, 'Printer not responding', 'Admin printer not working in Block B.', 'Medium', 'Open'),
(5, 'Software Installation', 'Need Adobe XD installed on my system.', 'Low', 'Completed'),
(3, 'Leave Management Issue', 'Employee leave balance not reflecting.', 'Medium', 'In Progress'),
(7, 'Report Automation', 'Want to automate monthly report generation.', 'High', 'Open'),
(4, 'Blog Review Request', 'Need review on SEO-optimized blog draft.', 'Low', 'Completed'),
(5, 'Database Access', 'Require read-only access to prod DB.', 'High', 'Open');



INSERT INTO Tasks (UserId, Title, Description, DueDate) VALUES
(5, 'Fix Login Bug', 'Resolve session timeout issue on login.', DATEADD(DAY, 2, GETDATE())),
(2, 'Conduct Orientation', 'Welcome new hires and explain policies.', DATEADD(DAY, 1, GETDATE())),
(3, 'Prepare Tax Docs', 'Compile and verify tax deduction details.', DATEADD(DAY, 5, GETDATE())),
(4, 'Write July Newsletter', 'Prepare draft for internal email blast.', DATEADD(DAY, 3, GETDATE())),
(5, 'Inventory Check', 'Verify all office supplies and restock.', DATEADD(DAY, 7, GETDATE())),
(6, 'Test Feature X', 'Perform regression testing on release v2.1.', DATEADD(DAY, 2, GETDATE())),
(7, 'Candidate Follow-up', 'Schedule call with selected candidate.', DATEADD(DAY, 1, GETDATE())),
(3, 'Budget Summary PPT', 'Prepare summary for Q1 spend analysis.', DATEADD(DAY, 4, GETDATE())),
(2, 'Social Post Calendar', 'Plan social posts for next 4 weeks.', DATEADD(DAY, 3, GETDATE())),
(4, 'API Integration', 'Integrate external payment gateway.', DATEADD(DAY, 6, GETDATE()));


INSERT INTO Announcements (AnnouncementId,Title, Message) VALUES
(1,'Maintenance Downtime', 'Server maintenance scheduled on Saturday from 10 PM to 2 AM.'),
(2,'New HR Policies Updated', 'Please review the updated HR policies on the portal.'),
(3,'Employee of the Month', 'Congratulations to Veera for being the Employee of the Month!'),
(4,'Team Outing Planned', 'Marketing and IT teams will have a joint team outing on 15th July.'),
(5,'COVID Guidelines Reminder', 'Mask is mandatory inside premises. Maintain hygiene and social distancing.');


select * from Users;
select * from Requests;
select * from Tasks;
select * from Announcements;

drop table Users;
drop table Tasks;
drop table Requests;
drop table Announcements;

ALTER TABLE Requests
DROP CONSTRAINT FK_Requests_Users;

