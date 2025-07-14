USE EmployeeCollaboration;
GO

-- Join Users and Requests
SELECT r.RequestId, r.Title, u.FullName, u.Department, r.Status
FROM Requests r
JOIN Users u ON r.UserId = u.UserId;

-- Total Requests per Priority
SELECT Priority, COUNT(*) AS Total
FROM Requests
GROUP BY Priority
ORDER BY Total DESC;

-- Count Requests per Department
SELECT u.Department, COUNT(*) AS RequestCount
FROM Requests r
JOIN Users u ON r.UserId = u.UserId
GROUP BY u.Department;

-- Number of Requests per User
SELECT u.FullName, COUNT(r.RequestId) AS RequestCount
FROM Users u
LEFT JOIN Requests r ON u.UserId = r.UserId
GROUP BY u.FullName
ORDER BY RequestCount DESC;

-- Total Tasks Assigned Per User
SELECT  U.FullName, U.Department, COUNT(T.TaskId) AS TotalTasks
FROM Users U
LEFT JOIN Tasks T ON U.UserId = T.UserId
GROUP BY U.FullName, U.Department
ORDER BY TotalTasks DESC;

-- Requests Submitted in the Last 7 Days with Employee Info
SELECT U.FullName, R.Title, R.Status, R.SubmissionDate
FROM Requests R
JOIN Users U ON R.UserId = U.UserId
WHERE R.SubmissionDate >= DATEADD(DAY, -7, GETDATE())
ORDER BY R.SubmissionDate DESC;

-- Users Without Any Requests
SELECT u.FullName, u.Email
FROM Users u
LEFT JOIN Requests r ON u.UserId = r.UserId
WHERE r.RequestId IS NULL;

-- Top 3 Users with Most Requests
SELECT TOP 3 u.FullName, COUNT(r.RequestId) AS TotalRequests
FROM Users u
JOIN Requests r ON u.UserId = r.UserId
GROUP BY u.FullName
ORDER BY TotalRequests DESC;

-- Get Last 3 Announcements Made
SELECT TOP 3 Title, Message, CreatedAt
FROM Announcements
ORDER BY CreatedAt DESC;

-- Number of Requests by Status and Priority 
SELECT 
    r.Status,
    COUNT(CASE WHEN r.Priority = 'High' THEN 1 END) AS HighPriority,
    COUNT(CASE WHEN r.Priority = 'Medium' THEN 1 END) AS MediumPriority,
    COUNT(CASE WHEN r.Priority = 'Low' THEN 1 END) AS LowPriority
FROM Requests r
GROUP BY r.Status;

-- Users Who Have Submitted More Than 2 Requests
SELECT u.FullName, COUNT(r.RequestId) AS TotalRequests
FROM Users u
JOIN Requests r ON u.UserId = r.UserId
GROUP BY u.FullName
HAVING COUNT(r.RequestId) > 2;

-- Users and Their Most Recent Request
SELECT u.FullName, u.Email, r.Title AS LatestRequest, r.SubmissionDate
FROM Users u
OUTER APPLY (
    SELECT TOP 1 Title, SubmissionDate
    FROM Requests
    WHERE UserId = u.UserId
    ORDER BY SubmissionDate DESC
) r;
