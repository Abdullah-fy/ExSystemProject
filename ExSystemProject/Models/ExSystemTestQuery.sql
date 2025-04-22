USE ExSystemTest
Create Table Branch(
	branch_id Int Primary Key identity (1,1),
	branch_name varchar(100) not null,
	location varchar(100) not null,
	isactive bit default 1
)
GO
--NOTE!!! when you delete branch you need to change track status also
Create Table Track(
	track_id int primary key Identity(10,10),
	track_name varchar(100) not null,
	track_duration int,
	track_intake int,
	is_active bit default 1,
	branch_id INT,
    CONSTRAINT FK_Track_Branch FOREIGN KEY (branch_id) REFERENCES Branch(branch_id) 
					ON DELETE CASCADE ON UPDATE CASCADE
)
--NOTE!!! when you change branch status you need to change all table under it, like instructor and student .....
GO
Create Table Users(
	userId int primary key identity(1,1),
	username varchar(100),
	isactive bit default 1,
	email varchar(50) unique,
	gender nchar(1) CHECK (gender IN ('M', 'F')),
	img varchar(100),
	Upassword varchar(100),
	role varchar(50) not null check(role in ('admin', 'instructor', 'student',	 'superadmin', 'supervisor'))
)
--NOTE!!!! when you change user status(isactive) you need to check its role and update the status in other table
GO
Create Table Instructor(
	Ins_Id Int Primary Key Identity(1,1),
	Salary Decimal(10,2),
	userId int,
	isactive bit default 1,
	track_id int,

	Foreign Key (userId) References Users(userId)
		on Delete CASCADE On Update Cascade,
	Foreign Key (track_id) References Track(track_id)
		on Delete Set Null On Update Cascade,
)
--NOTE!!! when update the instructor status update it in other tables also (Users)
-- when delete instructor make the course inactive
GO
Create Table Student(
	StudentId int primary key identity(1,1),
	track_id int,
	EnrollmentDate Date Default GetDate(),
	userId int,
	isactive bit default 1,
	Foreign Key (userId) References Users(userId)
		on Delete CASCADE On Update Cascade,
	Foreign Key (track_id) References Track(track_id)
		on Delete Set Null On Update Cascade
)
--NOTE!!! when you change status reflect this on other tables also like (Users)
GO
CREATE Table Courses(
	Crs_Id int Primary Key Identity(1,1),
	Crs_Name varchar(255) not null,
	Crs_period int,
	ins_id int ,
	isactive bit default 1,
	FOREIGN KEY (ins_id) REFERENCES Instructor(ins_id) on delete set null
)
--NOTE!!! when delete course reflect this on other tables aslo like (Exam)
GO
CREATE TABLE Student_Course
(
	CONSTRAINT PK_Student_Course PRIMARY KEY (Crs_Id, StudentId),
	Crs_Id int, 
	StudentId int,
	grade int,
	isactive BIT DEFAULT 1,
	EnrolledAt Date Default GetDate(),
	ispassed bit,
	Foreign Key (Crs_Id) References Courses(Crs_Id)
		 On Update Cascade,-- ON DELETE?
	Foreign Key (StudentId) References Student(StudentId)
		On Update Cascade-- ON DELETE?
)
--NOTE!!! foreach query you need to check on student and course status
GO
Create Table Topic(
	topic_id int primary key identity(1,1),
	topic_name varchar(100),
	descrtption varchar(255),
	Crs_id int,
	isactive bit default 1,
	Foreign Key (Crs_Id) References Courses(Crs_Id)
		on Delete CASCADE On Update Cascade
)
GO
Create Table Exam(
	exam_id int primary key identity(1,1),
	exam_name varchar(50) not null,
	startTime DATETIME,
	endTime DATETIME,
	crs_id int,
	ins_id int,	
	isactive bit default 1,
	TotalMarks int,--DRIVEN
	passedGrade int,
	CONSTRAINT CHK_Exam_Time CHECK (endTime > startTime),
	FOREIGN KEY (crs_id) REFERENCES Courses(Crs_Id) ON DELETE CASCADE,
	FOREIGN KEY (ins_id) REFERENCES Instructor(Ins_Id) ON DELETE SET NULL
)
--NOTE!!! when delete exam reflect this on other tables also, like (question)
ALTER TABLE Exam
ADD CONSTRAINT UQ_ExamName_PerCourse UNIQUE (exam_name, crs_id)
GO
Create Table Question(
	ques_id int primary key identity(1,1),
	ques_text varchar(255) not null,
	ques_type varchar(50) not null CHECK (ques_type IN ('MCQ', 'True/False')),
	exam_id int,
	ques_score int not null,
	isactive bit default 1,
	FOREIGN KEY (exam_id) REFERENCES Exam(exam_id) on delete cascade
)
--NOTE!!! when delete question reflect this on other table also, like choice
GO
Create Table Choice(
	choice_id int primary key identity(1,1),
	choice_text varchar(255) not null,
	ques_id int not null,
	is_correct BIT NOT NULL,
	FOREIGN KEY (ques_id) REFERENCES Question(ques_id) on delete cascade
)
GO
Create Table Student_Answer(
	answerid int primary key identity(1,1),
	studentid int not null,
	ques_id int,
	choice_id int NOT NULL,
	FOREIGN KEY (studentid) REFERENCES Student(studentid),
	FOREIGN KEY (ques_id) REFERENCES Question(ques_id),
	FOREIGN KEY (choice_id) REFERENCES Choice(choice_id)
)

GO
Create Table Student_Exam(
	StudentExamId int primary key identity(1,1),
	exam_id int,
	StudentId int not null,
	Score int not null,
	isactive BIT DEFAULT 1,
	pass_fail varchar(50) ,
	examination_date Date Default GetDate(),
	FOREIGN KEY (studentid) REFERENCES Student(studentid),
	CONSTRAINT unique_exam_student UNIQUE (exam_id, StudentId),
	FOREIGN KEY (exam_id) REFERENCES Exam(exam_id) on delete cascade
)

GO
-- SUPERVISOR AND ADMIN
CREATE TABLE UserAssignment (
    assignmentId int primary key identity(1,1),
    userId int not null,
    branch_id int,
	isactive BIT DEFAULT 1,
    track_id int,
	FOREIGN KEY (userId) REFERENCES Users(userId),
	FOREIGN KEY (branch_id) REFERENCES Branch(branch_id),
	FOREIGN KEY (track_id) REFERENCES Track(track_id),
		CONSTRAINT CHK_Role_Assignment CHECK (
        (branch_id IS NOT NULL AND track_id IS NULL) OR 
        (branch_id IS NOT NULL AND track_id IS NOT NULL)
    )
)
GO
---------------------------------------------------------------TRIGGERS AREA
--when update branch Status
CREATE TRIGGER trg_Branch_Status
ON Branch
AFTER UPDATE
AS
BEGIN
    IF UPDATE(isactive)
    BEGIN
        UPDATE Track
        SET is_active = 0
        WHERE branch_id IN (
            SELECT branch_id 
            FROM inserted 
            WHERE isactive = 0
        )
    END
END
GO
-- WHEN track is inactive
CREATE TRIGGER trg_Track_Status
ON Track
AFTER UPDATE
AS
BEGIN
    IF UPDATE(is_active)
    BEGIN
        UPDATE Instructor
        SET isactive = 0
        WHERE track_id IN (SELECT track_id FROM inserted WHERE is_active = 0)

        UPDATE Student
        SET isactive = 0
        WHERE track_id IN (SELECT track_id FROM inserted WHERE is_active = 0)
    END
END

GO
--when change user status (this should work in active or deactive case)
CREATE TRIGGER trg_User_Status
ON Users
AFTER UPDATE
AS
BEGIN
    IF UPDATE(isactive)
    BEGIN
        UPDATE Instructor
        SET isactive = i.isactive
        FROM inserted i
        WHERE Instructor.userId = i.userId AND i.role = 'instructor'

        UPDATE Student
        SET isactive = i.isactive
        FROM inserted i
        WHERE Student.userId = i.userId AND i.role = 'student'

		--if user change from instructor to supervisor for example
		UPDATE UserAssignment 
        SET isactive = i.isactive
        FROM UserAssignment ua
        JOIN inserted i ON ua.userId = i.userId
        WHERE i.role IN ('superadmin', 'supervisor')
    END
END
GO
--when delete instructor
CREATE OR ALTER TRIGGER trg_Instructor_Deactive
ON Instructor
AFTER UPDATE
AS
BEGIN
    IF UPDATE(isactive)
    BEGIN
        UPDATE Courses
        SET isactive = 0
        WHERE ins_id IN (
            SELECT i.Ins_Id
            FROM inserted i
            JOIN deleted d ON i.Ins_Id = d.Ins_Id
            WHERE i.isactive = 0 AND d.isactive = 1
        )
    END
END
GO
-- update exam when course change
CREATE TRIGGER trg_Course_Status 
ON Courses 
AFTER UPDATE
AS 
BEGIN
    IF UPDATE(isactive)
        UPDATE Exam 
        SET isactive = 0 
        WHERE crs_id IN (SELECT crs_id FROM inserted WHERE isactive = 0)
END
GO
-- course and topic
CREATE TRIGGER trg_Course_Topic_Status 
ON Courses 
AFTER UPDATE
AS 
BEGIN
    IF UPDATE(isactive)
        UPDATE Topic 
        SET isactive = 0 
        WHERE Crs_id IN (SELECT crs_id FROM inserted WHERE isactive = 0)
END
GO
CREATE TRIGGER trg_Course_StudentCourse_Status 
ON Courses 
AFTER UPDATE
AS 
BEGIN
    IF UPDATE(isactive)
    BEGIN
        UPDATE sc
        SET sc.isactive = 0 
        FROM Student_Course sc
        JOIN inserted i ON sc.Crs_Id = i.crs_id
        JOIN deleted d ON i.crs_id = d.crs_id
        WHERE i.isactive = 0 AND d.isactive = 1
    END
END
GO

CREATE TRIGGER trg_Student_StudentCourse_Status 
ON Student
AFTER UPDATE
AS 
BEGIN
    IF UPDATE(isactive)
    BEGIN
        UPDATE sc
        SET sc.isactive = 0 
        FROM Student_Course sc
        JOIN inserted i ON sc.StudentId = i.StudentId
        JOIN deleted d ON i.StudentId = d.StudentId
        WHERE i.isactive = 0 AND d.isactive = 1
    END
END
GO
-- triggers to updata pass or fail                           you may need to drop this one
CREATE TRIGGER trg_UpdatePassFail 
ON Student_Exam 
AFTER INSERT, UPDATE
AS 
BEGIN
    UPDATE Student_Exam 
    SET pass_fail = CASE 
        WHEN Score >= (SELECT passedGrade FROM Exam WHERE exam_id = Student_Exam.exam_id) THEN 'Pass' 
        ELSE 'Fail' 
    END
END

GO
-- Stop instructor to be student
create trigger trg_prevent_instructor_to_be_student
ON Instructor
AFTER INSERT, UPDATE
AS
BEGIN
	IF EXISTS(
		SELECT 1
		FROM inserted I
		JOIN Student S ON S.userId = I.userId
	)
	BEGIN
		RAISERROR('can not be student while being instructor', 16, 1)
		ROLLBACK TRANSACTION
		RETURN
	END
END

GO
-- prevent student to be instructor
CREATE TRIGGER trg_prevent_student_to_be_instructor
ON Student
after insert, UPDATE
AS
BEGIN
	IF EXISTS(
		SELECT 1
		FROM inserted I
		JOIN Instructor s on s.userId = I.userId
	)
	BEGIN
		RAISERROR('Can not be instructor while being Student', 16, 1)
		ROLLBACK TRANSACTION
		RETURN
	END
END


GO
-- prevent admin and supervisor to be student
CREATE TRIGGER trg_prevent_superviosor_and_admin_to_be_student
ON UserAssignment
after insert, UPDATE
AS
BEGIN
	IF EXISTS(
		SELECT 1
		FROM inserted I
		JOIN Student u on u.userId = I.userId
	)
	BEGIN
		RAISERROR('Adminstration level can not be students', 16, 1)
		ROLLBACK TRANSACTION
		RETURN
	END
END

GO
-- prevent student to be in adminstration level
CREATE TRIGGER trg_prevent_student_to_be_superviosor_or_admin
ON Student
after insert, UPDATE
AS
BEGIN
	IF EXISTS(
		SELECT 1
		FROM inserted I
		JOIN UserAssignment s on s.userId = I.userId
	)
	BEGIN
		RAISERROR('students can not be in adminstration level', 16, 1)
		ROLLBACK TRANSACTION
		RETURN
	END
END
GO
-- MAKE PASSedgrade driven and calculate it
CREATE TRIGGER trg_UpdatePassedGrade
ON Exam
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON

    UPDATE Exam
    SET passedGrade = (TotalMarks * 0.60)
    WHERE exam_id IN (SELECT exam_id FROM inserted)
END
GO
-----------------------------------------------------------------STORED PROSEDURE AREA
---------------------------------BRANCH TABLE STORED PROCEDURES
-- Create a new branch
ALTER PROCEDURE sp_CreateBranch
    @branch_name VARCHAR(100),
    @location VARCHAR(100)
AS
BEGIN
	DECLARE @branchId INT
    BEGIN TRY
        INSERT INTO Branch (branch_name, location)
        VALUES (@branch_name, @location)
        SET @branchId = SCOPE_IDENTITY()

        SELECT * FROM Branch WHERE branch_id = @branchId
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
exec sp_CreateBranch @branch_name = 'mansoura', @location = 'mansoura'
exec sp_CreateBranch @branch_name = 'smart', @location = 'smart'
exec sp_CreateBranch @branch_name = 'alex', @location = 'alex'

GO
-- Get all branches
CREATE PROCEDURE sp_GetAllBranches
    @activeBranches BIT = 1
AS
BEGIN
    IF @activeBranches = 1
        SELECT * FROM Branch WHERE isactive = 1
    ELSE IF @activeBranches = 0
        SELECT * FROM Branch WHERE isactive = 0
    ELSE
        SELECT * FROM Branch
END
exec sp_GetAllBranches
GO
-- Get branch by ID
CREATE PROCEDURE sp_GetBranchById
    @branch_id INT
AS
BEGIN
    SELECT * FROM Branch WHERE branch_id = @branch_id
END
exec sp_GetBranchById @branch_id = 1

GO
-- Update branch
CREATE PROCEDURE sp_UpdateBranch
    @branch_id INT,
    @branch_name VARCHAR(100),
    @location VARCHAR(100),
    @isactive BIT = 1
AS
BEGIN
    BEGIN TRY
        UPDATE Branch
        SET branch_name = @branch_name,
            location = @location,
            isactive = @isactive
        WHERE branch_id = @branch_id
        
        SELECT * FROM Branch WHERE branch_id = @branch_id
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
exec sp_UpdateBranch @branch_id = 1, @branch_name = 'mansoura' , @location = 'mansoura', @isactive = 1
GO
-- Delete branch (logical delete)
CREATE PROCEDURE sp_DeleteBranch
    @branch_id INT
AS
BEGIN
    BEGIN TRY
        UPDATE Branch
        SET isactive = 0
        WHERE branch_id = @branch_id
        
        SELECT * FROM Branch WHERE branch_id = @branch_id
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
exec sp_DeleteBranch @branch_id = 1

GO

------------------------------------------ TRACK TABLE STORED PROCEDURES
-- Create a new track
CREATE PROCEDURE sp_CreateTrack
    @track_name VARCHAR(100),
    @track_duration INT = NULL,
    @track_intake INT = NULL,
    @branch_id INT
AS
BEGIN
    BEGIN TRY
        INSERT INTO Track (track_name, track_duration, track_intake, branch_id)
        VALUES (@track_name, @track_duration, @track_intake, @branch_id)
        
        SELECT * FROM Track WHERE track_id = SCOPE_IDENTITY()
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
EXEC sp_CreateTrack @track_name = 'PD', @track_duration = 9, @track_intake = 45, @branch_id = 1
EXEC sp_CreateTrack @track_name = 'OS', @track_duration = 9, @track_intake = 45, @branch_id = 1
EXEC sp_CreateTrack @track_name = 'AI', @track_duration = 9, @track_intake = 45, @branch_id = 1
EXEC sp_CreateTrack @track_name = 'PD', @track_duration = 9, @track_intake = 45, @branch_id = 2
EXEC sp_CreateTrack @track_name = 'OS', @track_duration = 9, @track_intake = 45, @branch_id = 2
EXEC sp_CreateTrack @track_name = 'AI', @track_duration = 9, @track_intake = 45, @branch_id = 2
EXEC sp_CreateTrack @track_name = 'PD', @track_duration = 9, @track_intake = 45, @branch_id = 3
EXEC sp_CreateTrack @track_name = 'OS', @track_duration = 9, @track_intake = 45, @branch_id = 3
EXEC sp_CreateTrack @track_name = 'AI', @track_duration = 9, @track_intake = 45, @branch_id = 3
EXEC sp_CreateTrack @track_name = 'PD', @track_duration = 4, @track_intake = 45, @branch_id = 1
EXEC sp_CreateTrack @track_name = 'OS', @track_duration = 4, @track_intake = 45, @branch_id = 2
EXEC sp_CreateTrack @track_name = 'AI', @track_duration = 4, @track_intake = 45, @branch_id = 3
GO
-- Get all tracks active and inactvie and both
CREATE PROCEDURE sp_GetActiveAndInActiveTracks
    @activeTrackes BIT = 1
AS
BEGIN
    IF @activeTrackes = 1
        SELECT t.*, b.branch_name 
        FROM Track t
        JOIN Branch b ON t.branch_id = b.branch_id
        WHERE t.is_active = 1
    ELSE IF @activeTrackes = 0
        SELECT t.*, b.branch_name 
        FROM Track t
        JOIN Branch b ON t.branch_id = b.branch_id
        WHERE t.is_active = 0
    ELSE
        SELECT t.*, b.branch_name 
        FROM Track t
        JOIN Branch b ON t.branch_id = b.branch_id
END
EXEC sp_GetActiveAndInActiveTracks 
GO
-- Get track by ID
CREATE PROCEDURE sp_GetTrackById
    @track_id INT
AS
BEGIN
    SELECT t.*, b.branch_name 
    FROM Track t
    JOIN Branch b ON t.branch_id = b.branch_id
    WHERE t.track_id = @track_id
END
exec sp_GetTrackById @track_id = 10
GO
-- Get tracks by branch ID active and inactive and both
CREATE PROCEDURE sp_GetTracksByBranchId
    @branch_id INT,
    @activeTrackes BIT = 1
AS
BEGIN
    IF @activeTrackes = 1
        SELECT * FROM Track WHERE branch_id = @branch_id AND is_active = 1
    ELSE IF @activeTrackes = 0
        SELECT * FROM Track WHERE branch_id = @branch_id AND is_active = 0
    ELSE
        SELECT * FROM Track WHERE branch_id = @branch_id
END
exec sp_GetTracksByBranchId @branch_id = 1
GO
-- Update track
CREATE PROCEDURE sp_UpdateTrack
    @track_id INT,
    @track_name VARCHAR(100),
    @track_duration INT = NULL,
    @track_intake INT = NULL,
    @branch_id INT,
    @is_active BIT = 1
AS
BEGIN
    BEGIN TRY
        UPDATE Track
        SET track_name = @track_name,
            track_duration = @track_duration,
            track_intake = @track_intake,
            branch_id = @branch_id,
            is_active = @is_active
        WHERE track_id = @track_id
        
        SELECT * FROM Track WHERE track_id = @track_id
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
GO
-- Delete track (logical delete)
CREATE PROCEDURE sp_DeleteTrack
    @track_id INT
AS
BEGIN
    BEGIN TRY
        UPDATE Track
        SET is_active = 0
        WHERE track_id = @track_id
        
        SELECT * FROM Track WHERE track_id = @track_id
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
GO
------------------------------------------INSTRUCTOR TABLE STORED PROCEDURES
CREATE OR ALTER PROCEDURE sp_GetAllInstructorsWithBranch
    @activeInstructor BIT = NULL
AS
BEGIN
    SELECT 
        i.Ins_Id AS InsId, 
        i.Salary, 
        i.isactive, 
        i.track_id,
        i.userId,
        u.username, 
        u.email, 
        u.gender,
        u.img,
        t.track_name,
        t.track_id AS TrackId,
        b.branch_id,
        b.branch_name
    FROM 
        Instructor i
    JOIN 
        Users u ON i.userId = u.userId
    LEFT JOIN 
        Track t ON i.track_id = t.track_id
    LEFT JOIN 
        Branch b ON t.branch_id = b.branch_id
    WHERE 
        (@activeInstructor IS NULL OR i.isactive = @activeInstructor)
    ORDER BY 
        u.username;
END;
GO

CREATE OR ALTER PROCEDURE sp_GetInstructorByIdWithBranch
    @instructor_id INT
AS
BEGIN
    SELECT 
        i.Ins_Id AS InsId, 
        i.Salary, 
        i.isactive, 
        i.track_id,
        i.userId,
        u.username, 
        u.email, 
        u.gender,
        u.img,
        t.track_name,
        t.track_id,
        b.branch_id,
        b.branch_name
    FROM 
        Instructor i
    JOIN 
        Users u ON i.userId = u.userId
    LEFT JOIN 
        Track t ON i.track_id = t.track_id
    LEFT JOIN 
        Branch b ON t.branch_id = b.branch_id
    WHERE 
        i.Ins_Id = @instructor_id;
END;
GO

CREATE OR ALTER PROCEDURE sp_GetInstructorsByTrackWithBranch
    @track_id INT,
    @activeInstructor BIT = 1
AS
BEGIN
    SELECT 
        i.Ins_Id AS InsId, 
        i.Salary, 
        i.isactive, 
        i.track_id,
        i.userId,
        u.username, 
        u.email, 
        u.gender,
        t.track_name,
        t.track_id,
        b.branch_id,
        b.branch_name
    FROM 
        Instructor i
    JOIN 
        Users u ON i.userId = u.userId
    JOIN 
        Track t ON i.track_id = t.track_id
    JOIN 
        Branch b ON t.branch_id = b.branch_id
    WHERE 
        i.track_id = @track_id
        AND (@activeInstructor IS NULL OR i.isactive = @activeInstructor)
    ORDER BY 
        u.username;
END;
GO

CREATE OR ALTER PROCEDURE sp_GetInstructorsByBranchWithBranch
    @branch_id INT,
    @activeInstructor BIT = 1
AS
BEGIN
    SELECT 
        i.Ins_Id AS InsId, 
        i.Salary, 
        i.isactive, 
        i.track_id,
        i.userId,
        u.username, 
        u.email, 
        u.gender,
        t.track_name,
        t.branch_id,
        b.branch_name
    FROM 
        Instructor i
    JOIN 
        Users u ON i.userId = u.userId
    JOIN 
        Track t ON i.track_id = t.track_id
    JOIN 
        Branch b ON t.branch_id = b.branch_id
    WHERE 
        b.branch_id = @branch_id
        AND (@activeInstructor IS NULL OR i.isactive = @activeInstructor)
    ORDER BY 
        u.username;
END;
GO


-- Create a new instructor
CREATE PROCEDURE sp_CreateInstructor
    @username VARCHAR(100),
    @email VARCHAR(50),
    @gender NCHAR(1),
    @password VARCHAR(100),
    @salary DECIMAL(10,2),
    @track_id INT
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION
        
        DECLARE @userId INT
        
        INSERT INTO Users (username, email, gender, Upassword, role)
        VALUES (@username, @email, @gender, @password, 'instructor')
        
        SET @userId = SCOPE_IDENTITY()
        
        INSERT INTO Instructor (userId, Salary, track_id)
        VALUES (@userId, @salary, @track_id)
        
        SELECT i.*, u.username, u.email, u.gender 
        FROM Instructor i
        JOIN Users u ON i.userId = u.userId
        WHERE i.Ins_Id = SCOPE_IDENTITY()
        
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
exec sp_CreateInstructor @username = 'Ahmed', @email = 'ahmed@gmail.com', @gender = 'M', @password = 'ahmed!1', @salary=1000, @track_id = 10
exec sp_CreateInstructor @username = 'mahamed', @email = 'mahamed@gmail.com', @gender = 'M', @password = 'mahamed!1', @salary=10000, @track_id = 20
exec sp_CreateInstructor @username = 'samer', @email = 'samer@gmail.com', @gender = 'M', @password = 'samer!1', @salary=10000, @track_id = 30
exec sp_CreateInstructor @username = 'manar', @email = 'manar@gmail.com', @gender = 'F', @password = 'manar!1', @salary=10000, @track_id = 40
exec sp_CreateInstructor @username = 'sayed', @email = 'sayed@gmail.com', @gender = 'F', @password = 'sayed!1', @salary=10000, @track_id = 50
exec sp_CreateInstructor @username = 'ebrahim', @email = 'ebrahim@gmail.com', @gender = 'M', @password = 'ebrahim!1', @salary=1000, @track_id = 60

GO
CREATE OR ALTER PROCEDURE sp_GetAllInstructorsWithBranch
    @activeInstructor BIT = NULL
AS
BEGIN
    SELECT 
        i.Ins_Id AS InsId, 
        i.Salary, 
        i.isactive, 
        i.track_id,
        i.userId,
        u.username, 
        u.email, 
        u.gender,
        u.img,
        t.track_name,
        t.track_id AS TrackId,
        b.branch_id,
        b.branch_name
    FROM 
        Instructor i
    JOIN 
        Users u ON i.userId = u.userId
    LEFT JOIN 
        Track t ON i.track_id = t.track_id
    LEFT JOIN 
        Branch b ON t.branch_id = b.branch_id
    WHERE 
        (@activeInstructor IS NULL OR i.isactive = @activeInstructor)
    ORDER BY 
        u.username;
END;
GO

CREATE OR ALTER PROCEDURE sp_GetInstructorByIdWithBranch
    @instructor_id INT
AS
BEGIN
    SELECT 
        i.Ins_Id AS InsId, 
        i.Salary, 
        i.isactive, 
        i.track_id,
        i.userId,
        u.username, 
        u.email, 
        u.gender,
        u.img,
        t.track_name,
        t.track_id,
        b.branch_id,
        b.branch_name
    FROM 
        Instructor i
    JOIN 
        Users u ON i.userId = u.userId
    LEFT JOIN 
        Track t ON i.track_id = t.track_id
    LEFT JOIN 
        Branch b ON t.branch_id = b.branch_id
    WHERE 
        i.Ins_Id = @instructor_id;
END;
GO

CREATE OR ALTER PROCEDURE sp_GetInstructorsByTrackWithBranch
    @track_id INT,
    @activeInstructor BIT = 1
AS
BEGIN
    SELECT 
        i.Ins_Id AS InsId, 
        i.Salary, 
        i.isactive, 
        i.track_id,
        i.userId,
        u.username, 
        u.email, 
        u.gender,
        t.track_name,
        t.track_id,
        b.branch_id,
        b.branch_name
    FROM 
        Instructor i
    JOIN 
        Users u ON i.userId = u.userId
    JOIN 
        Track t ON i.track_id = t.track_id
    JOIN 
        Branch b ON t.branch_id = b.branch_id
    WHERE 
        i.track_id = @track_id
        AND (@activeInstructor IS NULL OR i.isactive = @activeInstructor)
    ORDER BY 
        u.username;
END;
GO
CREATE OR ALTER PROCEDURE sp_CreateInstructorWithValidation
    @username VARCHAR(100),
    @email VARCHAR(50),
    @gender NCHAR(1),
    @password VARCHAR(100),
    @salary DECIMAL(10,2),
    @track_id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate inputs
    IF @username IS NULL OR LEN(@username) = 0
    BEGIN
        RAISERROR('Username cannot be empty', 16, 1);
        RETURN;
    END
    
    IF @email IS NULL OR LEN(@email) = 0
    BEGIN
        RAISERROR('Email cannot be empty', 16, 1);
        RETURN;
    END
    
    -- Check if email already exists
    IF EXISTS (SELECT 1 FROM Users WHERE email = @email)
    BEGIN
        RAISERROR('Email address is already in use', 16, 1);
        RETURN;
    END
    
    -- Check if gender is valid
    IF @gender NOT IN ('M', 'F')
    BEGIN
        RAISERROR('Gender must be either M or F', 16, 1);
        RETURN;
    END
    
    -- Check if track exists and is active
    IF NOT EXISTS (SELECT 1 FROM Track WHERE track_id = @track_id AND is_active = 1)
    BEGIN
        RAISERROR('Selected track does not exist or is inactive', 16, 1);
        RETURN;
    END
    
    BEGIN TRY
        BEGIN TRANSACTION
            
        DECLARE @userId INT
            
        INSERT INTO Users (username, email, gender, Upassword, role)
        VALUES (@username, @email, @gender, @password, 'instructor')
            
        SET @userId = SCOPE_IDENTITY()
            
        INSERT INTO Instructor (userId, Salary, track_id)
        VALUES (@userId, @salary, @track_id)
            
        SELECT i.*, u.username, u.email, u.gender 
        FROM Instructor i
        JOIN Users u ON i.userId = u.userId
        WHERE i.Ins_Id = SCOPE_IDENTITY()
            
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
go

CREATE OR ALTER PROCEDURE sp_GetInstructorsByBranchWithBranch
    @branch_id INT,
    @activeInstructor BIT = 1
AS
BEGIN
    SELECT 
        i.Ins_Id AS InsId, 
        i.Salary, 
        i.isactive, 
        i.track_id,
        i.userId,
        u.username, 
        u.email, 
        u.gender,
        t.track_name,
        t.branch_id,
        b.branch_name
    FROM 
        Instructor i
    JOIN 
        Users u ON i.userId = u.userId
    JOIN 
        Track t ON i.track_id = t.track_id
    JOIN 
        Branch b ON t.branch_id = b.branch_id
    WHERE 
        b.branch_id = @branch_id
        AND (@activeInstructor IS NULL OR i.isactive = @activeInstructor)
    ORDER BY 
        u.username;
END;
GO

-- Get all instructors
CREATE PROCEDURE sp_GetAllInstructors
    @activeInstructor BIT = 1
AS
BEGIN
    IF @activeInstructor = 1
        SELECT i.*, u.username, u.email, u.gender, t.track_name
        FROM Instructor i
        JOIN Users u ON i.userId = u.userId
        LEFT JOIN Track t ON i.track_id = t.track_id
        WHERE i.isactive = 1
    ELSE IF @activeInstructor = 0
        SELECT i.*, u.username, u.email, u.gender, t.track_name
        FROM Instructor i
        JOIN Users u ON i.userId = u.userId
        LEFT JOIN Track t ON i.track_id = t.track_id
        WHERE i.isactive = 0
    ELSE
        SELECT i.*, u.username, u.email, u.gender, t.track_name
        FROM Instructor i
        JOIN Users u ON i.userId = u.userId
        LEFT JOIN Track t ON i.track_id = t.track_id
END
exec sp_GetAllInstructors
GO
-- Get instructor by ID
CREATE PROCEDURE sp_GetInstructorById
    @ins_id INT
AS
BEGIN
    SELECT i.*, u.username, u.email, u.gender, t.track_name
    FROM Instructor i
    JOIN Users u ON i.userId = u.userId
    LEFT JOIN Track t ON i.track_id = t.track_id
    WHERE i.Ins_Id = @ins_id
END
exec sp_GetInstructorById @ins_id = 1
GO
-- Get instructors by track ID
CREATE PROCEDURE sp_GetInstructorsByTrackId
    @track_id INT,
    @Getactive BIT = 1
AS
BEGIN
    IF @Getactive = 1
        SELECT i.*, u.username, u.email, u.gender
        FROM Instructor i
        JOIN Users u ON i.userId = u.userId
        WHERE i.track_id = @track_id AND i.isactive = 1
    ELSE IF @Getactive = 0
        SELECT i.*, u.username, u.email, u.gender
        FROM Instructor i
        JOIN Users u ON i.userId = u.userId
        WHERE i.track_id = @track_id AND i.isactive = 0
    ELSE
        SELECT i.*, u.username, u.email, u.gender
        FROM Instructor i
        JOIN Users u ON i.userId = u.userId
        WHERE i.track_id = @track_id
END
exec sp_GetInstructorsByTrackId @track_id = 10
GO
-- GET insturctor by branch id
CREATE PROCEDURE sp_GetInstructorsByBranchId
    @branch_id INT,
    @GetActive BIT = 1
AS
BEGIN    

    SELECT 
        i.Ins_Id, i.Salary, i.isactive AS IsActive, u.username, u.email, u.gender, t.track_name
    FROM 
        Instructor i
        JOIN Users u ON i.userId = u.userId
        JOIN Track t ON i.track_id = t.track_id
    WHERE 
        t.branch_id = @branch_id
        AND (@GetActive IS NULL OR i.isactive = @GetActive)
    ORDER BY
        u.username
END 
EXEC sp_GetInstructorsByBranchId @branch_id = 2
GO
-- Update instructor
CREATE PROCEDURE sp_UpdateInstructor
    @ins_id INT,
    @username VARCHAR(100),
    @email VARCHAR(50),
    @gender NCHAR(1),
    @salary DECIMAL(10,2),
    @track_id INT,
    @isactive BIT = 1
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION
        
        DECLARE @userId INT
        
        SELECT @userId = userId FROM Instructor WHERE Ins_Id = @ins_id
        
        UPDATE Users
        SET username = @username,
            email = @email,
            gender = @gender,
            isactive = @isactive
        WHERE userId = @userId
        
        UPDATE Instructor
        SET Salary = @salary,
            track_id = @track_id,
            isactive = @isactive
        WHERE Ins_Id = @ins_id
        
        SELECT i.*, u.username, u.email, u.gender
        FROM Instructor i
        JOIN Users u ON i.userId = u.userId
        WHERE i.Ins_Id = @ins_id
        
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
GO
-- Delete instructor (logical delete)
CREATE PROCEDURE sp_DeleteInstructor
    @ins_id INT
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION
        
        DECLARE @userId INT
        
        SELECT @userId = userId FROM Instructor WHERE Ins_Id = @ins_id
        
        UPDATE Users
        SET isactive = 0
        WHERE userId = @userId
                
        SELECT i.*, u.username, u.email
        FROM Instructor i
        JOIN Users u ON i.userId = u.userId
        WHERE i.Ins_Id = @ins_id
        
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
GO
---------------------------------STUDENT TABLE STORED PROCEDURES
-- Create a new student
CREATE PROCEDURE sp_CreateStudent
    @username VARCHAR(100),
    @email VARCHAR(50),
    @gender NCHAR(1),
    @password VARCHAR(100),
    @track_id INT = NULL
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION
        
        DECLARE @userId INT
        
        INSERT INTO Users (username, email, gender, Upassword, role)
        VALUES (@username, @email, @gender, @password, 'student')
        
        SET @userId = SCOPE_IDENTITY()
        
        INSERT INTO Student (userId, track_id)
        VALUES (@userId, @track_id)
        
        SELECT s.*, u.username, u.email, u.gender, t.track_name 
        FROM Student s
        JOIN Users u ON s.userId = u.userId
        LEFT JOIN Track t ON s.track_id = t.track_id
        WHERE s.StudentId = SCOPE_IDENTITY()
        
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
exec sp_CreateStudent @username = 'abdullah', @email='abdullah@gmail.com', @gender = 'M', @password = 'abdullah!1', @track_id = 10
exec sp_CreateStudent @username = 'mohamed', @email='mohamed@gmail.com', @gender = 'M', @password = 'mohamed!1', @track_id = 20
exec sp_CreateStudent @username = 'sara', @email='sara@gmail.com', @gender = 'M', @password = 'sara!1', @track_id = 40
exec sp_CreateStudent @username = 'omar', @email='omar@gmail.com', @gender = 'M', @password = 'omar!1', @track_id = 50
exec sp_CreateStudent @username = 'tarek', @email='tarek@gmail.com', @gender = 'M', @password = 'tarek!1', @track_id = 50
exec sp_CreateStudent @username = 'hamdy', @email='hamdy@gmail.com', @gender = 'M', @password = 'hamdy!1', @track_id = 60
exec sp_CreateStudent @username = 'reda', @email='reda@gmail.com', @gender = 'M', @password = 'reda!1', @track_id = 70

GO

CREATE OR ALTER PROCEDURE sp_GetAllStudentsWithBranch
    @activeStudent BIT = NULL
AS
BEGIN
    SELECT 
        s.*,
        u.username, u.email, u.gender,
        t.track_name, t.track_id, 
        b.branch_id, b.branch_name
    FROM 
        Student s
        JOIN Users u ON s.userId = u.userId
        LEFT JOIN Track t ON s.track_id = t.track_id
        LEFT JOIN Branch b ON t.branch_id = b.branch_id
    WHERE 
        (@activeStudent IS NULL OR s.isactive = @activeStudent)
END
go
CREATE OR ALTER PROCEDURE sp_GetStudentByIdWithBranch
    @student_id INT
AS
BEGIN
    SELECT 
        s.*,
        u.username, u.email, u.gender, u.img,
        t.track_name, t.track_id,
        b.branch_id, b.branch_name
    FROM 
        Student s
        JOIN Users u ON s.userId = u.userId
        LEFT JOIN Track t ON s.track_id = t.track_id
        LEFT JOIN Branch b ON t.branch_id = b.branch_id
    WHERE 
        s.StudentId = @student_id
END
GO

--------

CREATE OR ALTER PROCEDURE sp_GetAllStudentsWithBranch
    @activeStudent BIT = NULL
AS
BEGIN
    SELECT 
        s.*,
        u.username, u.email, u.gender,
        t.track_name, t.track_id, 
        b.branch_id, b.branch_name
    FROM 
        Student s
        JOIN Users u ON s.userId = u.userId
        LEFT JOIN Track t ON s.track_id = t.track_id
        LEFT JOIN Branch b ON t.branch_id = b.branch_id
    WHERE 
        (@activeStudent IS NULL OR s.isactive = @activeStudent)
END


go 
CREATE OR ALTER PROCEDURE sp_GetStudentsByDepartmentWithBranch
    @track_id INT,
    @activeStudent BIT = 1
AS
BEGIN
    SELECT 
        s.*,
        u.username, u.email, u.gender,
        t.track_name, t.track_id, 
        b.branch_id, b.branch_name
    FROM 
        Student s
        JOIN Users u ON s.userId = u.userId
        JOIN Track t ON s.track_id = t.track_id
        LEFT JOIN Branch b ON t.branch_id = b.branch_id
    WHERE 
        s.track_id = @track_id 
        AND (@activeStudent IS NULL OR s.isactive = @activeStudent)
END

go 
CREATE OR ALTER PROCEDURE sp_GetStudentsByBranchIdWithBranch
    @branch_id INT,
    @ActiveOnly BIT = 1
AS
BEGIN
    SELECT 
        s.StudentId, s.track_id, s.EnrollmentDate, s.userId, s.isactive AS IsActive,
        u.username, u.email, u.gender, u.img AS ProfileImage,
        t.track_name,
        b.branch_id, b.branch_name,
        COUNT(sc.Crs_Id) AS EnrolledCoursesCount
    FROM 
        Student s
        JOIN Users u ON s.userId = u.userId
        JOIN Track t ON s.track_id = t.track_id
        JOIN Branch b ON t.branch_id = b.branch_id
        LEFT JOIN Student_Course sc ON s.StudentId = sc.StudentId AND sc.isactive = 1
    WHERE 
        t.branch_id = @branch_id
        AND (@ActiveOnly IS NULL OR s.isactive = @ActiveOnly)
    GROUP BY
        s.StudentId, s.track_id, s.EnrollmentDate, s.userId, s.isactive,
        u.username, u.email, u.gender, u.img,
        t.track_name,
        b.branch_id, b.branch_name
    ORDER BY
        u.username
END
go
------
-- Get all students
CREATE PROCEDURE sp_GetAllStudents
    @activeStudent BIT = 1
AS
BEGIN
    IF @activeStudent = 0
        SELECT s.*, u.username, u.email, u.gender, t.track_name
        FROM Student s
        JOIN Users u ON s.userId = u.userId
        LEFT JOIN Track t ON s.track_id = t.track_id
        WHERE s.isactive = 0
    ELSE IF @activeStudent = 1
        SELECT s.*, u.username, u.email, u.gender, t.track_name
        FROM Student s
        JOIN Users u ON s.userId = u.userId
        LEFT JOIN Track t ON s.track_id = t.track_id
        WHERE s.isactive = 1
    ELSE
        SELECT s.*, u.username, u.email, u.gender, t.track_name
        FROM Student s
        JOIN Users u ON s.userId = u.userId
        LEFT JOIN Track t ON s.track_id = t.track_id
END
exec sp_GetAllStudents
GO
-- Get student by ID
CREATE PROCEDURE sp_GetStudentById
    @student_id INT
AS
BEGIN
    SELECT s.*, u.username, u.email,u.img, u.gender, t.track_name
    FROM Student s
    JOIN Users u ON s.userId = u.userId
    LEFT JOIN Track t ON s.track_id = t.track_id
    WHERE s.StudentId = @student_id
END
exec sp_GetStudentById @student_id = 1

GO
-- Get students by department (track) ------------------------FIRST REPORT
CREATE PROCEDURE sp_GetStudentsByDepartment
    @track_id INT,
    @activeStudent BIT = 1
AS
BEGIN
    IF @activeStudent = 1
        SELECT s.*, u.username, u.email, u.gender, t.track_name
        FROM Student s
        JOIN Users u ON s.userId = u.userId
        JOIN Track t ON s.track_id = t.track_id
        WHERE s.track_id = @track_id AND s.isactive = 1
    ELSE IF @activeStudent = 0
        SELECT s.*, u.username, u.email, u.gender, t.track_name
        FROM Student s
        JOIN Users u ON s.userId = u.userId
        JOIN Track t ON s.track_id = t.track_id
        WHERE s.track_id = @track_id AND s.isactive = 0
    ELSE 
        SELECT s.*, u.username, u.email, u.gender, t.track_name
        FROM Student s
        JOIN Users u ON s.userId = u.userId
        JOIN Track t ON s.track_id = t.track_id
        WHERE s.track_id = @track_id
END
exec sp_GetStudentsByDepartment @track_id = 10

GO
-- get by branch id
CREATE PROCEDURE sp_GetStudentsByBranchId
    @branch_id INT,
    @ActiveOnly BIT = 1
AS
BEGIN
    SELECT 
        s.StudentId, s.EnrollmentDate, s.isactive AS IsActive, u.username, u.email, u.gender, u.img AS ProfileImage, t.track_name, b.branch_name,
        COUNT(sc.Crs_Id) AS EnrolledCoursesCount
    FROM 
        Student s
        JOIN Users u ON s.userId = u.userId
        JOIN Track t ON s.track_id = t.track_id
        JOIN Branch b ON t.branch_id = b.branch_id
        LEFT JOIN Student_Course sc ON s.StudentId = sc.StudentId AND sc.isactive = 1
    WHERE 
        t.branch_id = @branch_id
        AND (@ActiveOnly IS NULL OR s.isactive = @ActiveOnly)
    GROUP BY
        s.StudentId, s.EnrollmentDate, s.isactive, u.username, u.email, u.gender, u.img, t.track_name, b.branch_name
    ORDER BY
        u.username
END
exec sp_GetStudentsByBranchId @branch_id = 1
GO
-- Update student
CREATE PROCEDURE sp_UpdateStudent
    @student_id INT,
    @username VARCHAR(100),
    @email VARCHAR(50),
    @gender NCHAR(1),
    @track_id INT = NULL,
    @isactive BIT = 1
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION
        
        DECLARE @userId INT
        
        SELECT @userId = userId FROM Student WHERE StudentId = @student_id
        
        UPDATE Users
        SET username = @username,
            email = @email,
            gender = @gender,
            isactive = @isactive
        WHERE userId = @userId
        
        UPDATE Student
        SET track_id = @track_id,
            isactive = @isactive
        WHERE StudentId = @student_id
        
        SELECT s.*, u.username, u.email, u.gender, t.track_name
        FROM Student s
        JOIN Users u ON s.userId = u.userId
        LEFT JOIN Track t ON s.track_id = t.track_id
        WHERE s.StudentId = @student_id
        
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
GO
-- Delete student (logical delete)
CREATE PROCEDURE sp_DeleteStudent
    @student_id INT
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION
        
        DECLARE @userId INT
        
        SELECT @userId = userId FROM Student WHERE StudentId = @student_id
        
        UPDATE Users
        SET isactive = 0
        WHERE userId = @userId
                
        SELECT s.*, u.username, u.email
        FROM Student s
        JOIN Users u ON s.userId = u.userId
        WHERE s.StudentId = @student_id
        
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
GO
---------------------------------------COURSES TABLE STORED PROCEDURES
-- Create a new course
CREATE PROCEDURE sp_CreateCourse
    @crs_name VARCHAR(255),
    @crs_period INT = NULL,
    @ins_id INT = NULL
AS
BEGIN
    BEGIN TRY
        INSERT INTO Courses (Crs_Name, Crs_period, ins_id)
        VALUES (@crs_name, @crs_period, @ins_id)
        
        SELECT c.*, i.Ins_Id, u.username as instructor_name
        FROM Courses c
        LEFT JOIN Instructor i ON c.ins_id = i.Ins_Id
        LEFT JOIN Users u ON i.userId = u.userId
        WHERE c.Crs_Id = SCOPE_IDENTITY()
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
exec sp_CreateCourse @crs_name = 'c#', @crs_period = 2, @ins_id = 1
exec sp_CreateCourse @crs_name = 'c++', @crs_period = 2, @ins_id = 2
exec sp_CreateCourse @crs_name = 'html', @crs_period = 2, @ins_id = 3
exec sp_CreateCourse @crs_name = 'js', @crs_period = 2, @ins_id = 4
exec sp_CreateCourse @crs_name = 'jquery', @crs_period = 2, @ins_id = 5
exec sp_CreateCourse @crs_name = 'oop', @crs_period = 2, @ins_id = 6


GO
-- Get all courses
CREATE PROCEDURE sp_GetAllCourses
    @activeCourse BIT = 1
AS
BEGIN
    IF @activeCourse = 1
        SELECT c.*, u.username as instructor_name
        FROM Courses c
        LEFT JOIN Instructor i ON c.ins_id = i.Ins_Id
        LEFT JOIN Users u ON i.userId = u.userId
        WHERE c.isactive = 1
    ELSE IF @activeCourse = 0
        SELECT c.*, u.username as instructor_name
        FROM Courses c
        LEFT JOIN Instructor i ON c.ins_id = i.Ins_Id
        LEFT JOIN Users u ON i.userId = u.userId
        WHERE c.isactive = 0
    ELSE
        SELECT c.*, u.username as instructor_name
        FROM Courses c
        LEFT JOIN Instructor i ON c.ins_id = i.Ins_Id
        LEFT JOIN Users u ON i.userId = u.userId
END
exec sp_GetAllCourses
GO
-- Get course by ID
CREATE PROCEDURE sp_GetCourseById
    @crs_id INT
AS
BEGIN
    SELECT c.*, u.username as instructor_name
    FROM Courses c
    LEFT JOIN Instructor i ON c.ins_id = i.Ins_Id
    LEFT JOIN Users u ON i.userId = u.userId
    WHERE c.Crs_Id = @crs_id
END
exec sp_GetCourseById @crs_id = 1
GO
-- Get instructor courses
CREATE PROCEDURE sp_GetInstructorCourses
    @ins_id INT,
    @active BIT = 1
AS
BEGIN
    IF @active = 1
        SELECT c.*, i.Ins_Id, u.username as instructor_name
        FROM Courses c
        JOIN Instructor i ON c.ins_id = i.Ins_Id
        JOIN Users u ON i.userId = u.userId
        WHERE i.Ins_Id = @ins_id AND c.isactive = 1
    ELSE IF @active = 0
        SELECT c.*, i.Ins_Id, u.username as instructor_name
        FROM Courses c
        JOIN Instructor i ON c.ins_id = i.Ins_Id
        JOIN Users u ON i.userId = u.userId
        WHERE i.Ins_Id = @ins_id AND c.isactive = 0
    ELSE
        SELECT c.*, i.Ins_Id, u.username as instructor_name
        FROM Courses c
        JOIN Instructor i ON c.ins_id = i.Ins_Id
        JOIN Users u ON i.userId = u.userId
        WHERE i.Ins_Id = @ins_id
END
exec sp_GetInstructorCourses @ins_id = 1
GO
-- Get course topics
CREATE PROCEDURE sp_GetCourseTopics
    @crs_id INT,
    @active BIT = 1
AS
BEGIN
    IF @active = 1
        SELECT t.*, c.Crs_Name
        FROM Topic t
        JOIN Courses c ON t.Crs_id = c.Crs_Id
        WHERE c.Crs_Id = @crs_id AND t.isactive = 1
    ELSE IF @active = 0
        SELECT t.*, c.Crs_Name
        FROM Topic t
        JOIN Courses c ON t.Crs_id = c.Crs_Id
        WHERE c.Crs_Id = @crs_id AND t.isactive = 0
    ELSE
        SELECT t.*, c.Crs_Name
        FROM Topic t
        JOIN Courses c ON t.Crs_id = c.Crs_Id
        WHERE c.Crs_Id = @crs_id
END
exec sp_GetCourseTopics @crs_id = 1
GO
-- Update course
CREATE PROCEDURE sp_UpdateCourse
    @crs_id INT,
    @crs_name VARCHAR(255),
    @crs_period INT = NULL,
    @ins_id INT = NULL,
    @isactive BIT = 1
AS
BEGIN
    BEGIN TRY
        UPDATE Courses
        SET Crs_Name = @crs_name,
            Crs_period = @crs_period,
            ins_id = @ins_id,
            isactive = @isactive
        WHERE Crs_Id = @crs_id
        
        SELECT c.*, u.username as instructor_name
        FROM Courses c
        LEFT JOIN Instructor i ON c.ins_id = i.Ins_Id
        LEFT JOIN Users u ON i.userId = u.userId
        WHERE c.Crs_Id = @crs_id
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
GO
-- Delete course (logical delete)
CREATE PROCEDURE sp_DeleteCourse
    @crs_id INT
AS
BEGIN
    BEGIN TRY
        UPDATE Courses
        SET isactive = 0
        WHERE Crs_Id = @crs_id
        
        SELECT * FROM Courses WHERE Crs_Id = @crs_id
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
GO
-----------------------------------------TOPIC TABLE STORED PROCEDURES
-- Create a new topic
CREATE PROCEDURE sp_CreateTopic
    @topic_name VARCHAR(100),
    @description VARCHAR(255) = NULL,
    @crs_id INT
AS
BEGIN
    BEGIN TRY
        INSERT INTO Topic (topic_name, descrtption, Crs_id)
        VALUES (@topic_name, @description, @crs_id)
        
        SELECT t.*, c.Crs_Name
        FROM Topic t
        JOIN Courses c ON t.Crs_id = c.Crs_Id
        WHERE t.topic_id = SCOPE_IDENTITY()
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
exec sp_CreateTopic @topic_name = 'delegete', @description = 'learn delegte', @crs_id = 1
exec sp_CreateTopic @topic_name = 'pointer', @description = 'learn pointer', @crs_id = 2
exec sp_CreateTopic @topic_name = 'structure of html', @description = 'structure of html', @crs_id = 3
exec sp_CreateTopic @topic_name = 'css', @description = 'css', @crs_id = 4


GO
-- Get all topics
CREATE PROCEDURE sp_GetAllTopics
    @activeTopic BIT = 1
AS
BEGIN
    IF @activeTopic = 1
        SELECT t.*, c.Crs_Name
        FROM Topic t
        JOIN Courses c ON t.Crs_id = c.Crs_Id
        WHERE t.isactive = 1
    ELSE IF @activeTopic = 0
        SELECT t.*, c.Crs_Name
        FROM Topic t
        JOIN Courses c ON t.Crs_id = c.Crs_Id
        WHERE t.isactive = 0
    ELSE
        SELECT t.*, c.Crs_Name
        FROM Topic t
        JOIN Courses c ON t.Crs_id = c.Crs_Id
END
exec sp_GetAllTopics
GO
-- Get topic by ID
CREATE PROCEDURE sp_GetTopicById
    @topic_id INT
AS
BEGIN
    SELECT t.*, c.Crs_Name
    FROM Topic t
    JOIN Courses c ON t.Crs_id = c.Crs_Id
    WHERE t.topic_id = @topic_id
END
exec sp_GetTopicById @topic_id = 1

GO
-- Get topics by course ID  -------------------------------FORTH REPORT
create PROCEDURE sp_GetTopicsByCourseId
    @crs_id INT,
    @Getactive BIT = 1
AS
BEGIN
    IF @Getactive = 1
        SELECT t.*, c.Crs_Name
        FROM Topic t
        JOIN Courses c ON t.Crs_id = c.Crs_Id
        WHERE t.Crs_id = @crs_id AND t.isactive = 1
    ELSE IF @Getactive = 0
        SELECT t.*, c.Crs_Name
        FROM Topic t
        JOIN Courses c ON t.Crs_id = c.Crs_Id
        WHERE t.Crs_id = @crs_id AND t.isactive = 0
    ELSE
        SELECT t.*, c.Crs_Name
        FROM Topic t
        JOIN Courses c ON t.Crs_id = c.Crs_Id
        WHERE t.Crs_id = @crs_id
END
exec sp_GetTopicsByCourseId @crs_id = 1

GO
-- Update topic
CREATE PROCEDURE sp_UpdateTopic
    @topic_id INT,
    @topic_name VARCHAR(100),
    @description VARCHAR(255) = NULL,
    @crs_id INT,
    @isactive BIT = 1
AS
BEGIN
    BEGIN TRY
        UPDATE Topic
        SET topic_name = @topic_name,
            descrtption = @description,
            Crs_id = @crs_id,
            isactive = @isactive
        WHERE topic_id = @topic_id
        
        SELECT t.*, c.Crs_Name
        FROM Topic t
        JOIN Courses c ON t.Crs_id = c.Crs_Id
        WHERE t.topic_id = @topic_id
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
GO
-- Delete topic (logical delete)
CREATE PROCEDURE sp_DeleteTopic
    @topic_id INT
AS
BEGIN
    BEGIN TRY
        UPDATE Topic
        SET isactive = 0
        WHERE topic_id = @topic_id
        
        SELECT * FROM Topic WHERE topic_id = @topic_id
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
GO
-------------------------------------------------STORED FOR REPORTS
-- Create Exam (Empty) NOT ASSIGN TO ANYONE
create PROC sp_create_blank_exam
(
    @crs_id int,
    @exam_name VARCHAR(255),
    @startTime DATETIME = NULL,
    @endTime DATETIME = NULL,
    @ins_id int = null
)
AS 
BEGIN
    IF @startTime IS NULL
        SET @startTime = DATEADD(DAY, 7, GETDATE())
    IF @endTime IS NULL
        SET @endTime = DATEADD(HOUR, 1, @startTime)

    INSERT INTO Exam (crs_id, TotalMarks, exam_name, startTime, endTime, ins_id)
        VALUES (@crs_id, 0, @exam_name, @startTime, @endTime, @ins_id)
    
    SELECT * FROM Exam WHERE exam_id = SCOPE_IDENTITY()
END
EXEC sp_create_blank_exam @crs_id = 1, @exam_name = 'c#', @startTime = '2025-4-1 09:00:00', @endTime = '2025-4-1 10:00:00', @ins_id = 1

GO
-- Generate exam (assign exam to student, insert into student_exam table)
create PROCEDURE sp_AssignExamToStudent
    @exam_id INT,
    @student_id INT
AS
BEGIN
	-- check if he took the exam
	IF EXISTS (SELECT 1 FROM Student_Exam WHERE exam_id = @exam_id AND StudentId = @student_id)
    BEGIN
        SELECT 'Student is already assigned to this exam.' AS Message
        RETURN;
    END

    INSERT INTO Student_Exam (exam_id, StudentId, Score, pass_fail, examination_date, isactive)
    VALUES (@exam_id, @student_id, 0, NULL, GETDATE(), 1);

    SELECT 'Exam assigned successfully!' AS Message
END
EXEC sp_AssignExamToStudent @exam_id = 5, @student_id = 1

GO
-- Insert MCQ Question
CREATE PROCEDURE sp_insert_ques_mcq
(
    @ques_text varchar(255),
    @choice1 varchar(255),
    @choice2 varchar(255),
    @choice3 varchar(255),
    @choice4 varchar(255),
    @ques_score int,
    @correct_choice_no int,
    @exam_id int
)
AS
BEGIN
    DECLARE @last_ques_id INT

    INSERT INTO Question (ques_text, ques_type, ques_score, exam_id) 
    VALUES (@ques_text, 'MCQ', @ques_score, @exam_id)

    SET @last_ques_id = SCOPE_IDENTITY()

    INSERT INTO Choice (ques_id, choice_text, is_correct) 
    VALUES 
        (@last_ques_id, @choice1, CASE WHEN @correct_choice_no = 1 THEN 1 ELSE 0 END),
        (@last_ques_id, @choice2, CASE WHEN @correct_choice_no = 2 THEN 1 ELSE 0 END),
        (@last_ques_id, @choice3, CASE WHEN @correct_choice_no = 3 THEN 1 ELSE 0 END),
        (@last_ques_id, @choice4, CASE WHEN @correct_choice_no = 4 THEN 1 ELSE 0 END)
    UPDATE Exam SET TotalMarks += @ques_score
        WHERE exam_id = @exam_id
        
    SELECT * FROM Question WHERE ques_id = @last_ques_id
END
GO
EXEC sp_insert_ques_mcq 
    @ques_text = 'What is the most popular programming language in the world?',
    @choice1 = 'Java',
    @choice2 = 'Python',
    @choice3 = 'C++',
    @choice4 = 'JavaScript',
    @ques_score = 5,
    @correct_choice_no = 2,
    @exam_id = 5
GO
EXEC sp_insert_ques_mcq 
    @ques_text = 'What is the most used operating system?',
    @choice1 = 'Windows',
    @choice2 = 'Linux',
    @choice3 = 'macOS',
    @choice4 = 'Android',
    @ques_score = 5,
    @correct_choice_no = 1,
    @exam_id = 5
GO
EXEC sp_insert_ques_mcq 
    @ques_text = 'What does LINQ stand for in C#?',
    @choice1 = 'Language Integrated Query',
    @choice2 = 'Logical Integrated Query',
    @choice3 = 'Linked Integrated Query',
    @choice4 = 'Linear Integrated Query',
    @ques_score = 5,
    @correct_choice_no = 1,
    @exam_id = 5
GO
EXEC sp_insert_ques_mcq 
    @ques_text = 'Which of the following is not a type of database?',
    @choice1 = 'Relational Database',
    @choice2 = 'NoSQL Database',
    @choice3 = 'Hierarchical Database',
    @choice4 = 'File System',
    @ques_score = 5,
    @correct_choice_no = 4,
    @exam_id = 5
GO
EXEC sp_insert_ques_mcq 
    @ques_text = 'What is the capital of France?',
    @choice1 = 'Berlin',
    @choice2 = 'Madrid',
    @choice3 = 'Paris',
    @choice4 = 'Rome',
    @ques_score = 5,
    @correct_choice_no = 3,
    @exam_id = 5
GO
-- Create Question True/False
CREATE PROCEDURE sp_insert_ques_tf
(  
    @ques_text varchar(255),
    @ques_score int,
    @correct_answer varchar(20),
    @exam_id int
)
AS
BEGIN
	DECLARE @last_ques_id INT

    INSERT INTO Question (ques_text, ques_type, ques_score, exam_id) 
    VALUES (@ques_text, 'True/False', @ques_score, @exam_id)

	SET @last_ques_id = SCOPE_IDENTITY()

	INSERT INTO Choice (ques_id, choice_text, is_correct) 
    VALUES 
        (@last_ques_id, 'True', CASE WHEN @correct_answer = 1 THEN 1 ELSE 0 END),
        (@last_ques_id, 'False', CASE WHEN @correct_answer = 0 THEN 1 ELSE 0 END)

    UPDATE Exam SET TotalMarks += @ques_score
        WHERE exam_id = @exam_id
        
    SELECT * FROM Question WHERE ques_id = @last_ques_id
END
GO
EXEC sp_insert_ques_tf 
    @ques_text = 'Is Java a compiled language?',
    @ques_score = 5,
    @correct_answer = 0,
    @exam_id = 5
GO
EXEC sp_insert_ques_tf 
    @ques_text = 'Is Python an interpreted language?',
    @ques_score = 5,
    @correct_answer = 1,
    @exam_id = 5
GO
EXEC sp_insert_ques_tf 
    @ques_text = 'Does C++ support object-oriented programming?',
    @ques_score = 5,
    @correct_answer = 1,
    @exam_id = 5
GO
EXEC sp_insert_ques_tf 
    @ques_text = 'Is JavaScript primarily used for server-side programming?',
    @ques_score = 5,
    @correct_answer = 0,
    @exam_id = 5
GO
EXEC sp_insert_ques_tf 
    @ques_text = 'Does SQL stand for Structured Query Language?',
    @ques_score = 5,
    @correct_answer = 1,
    @exam_id = 5
GO
-- Submit exam answer (save answers, check if choice is correct, update score)
CREATE PROCEDURE sp_SubmitExamAnswers
    @student_id INT,
    @exam_id INT,
    @ques_id INT,
    @choice_id INT
AS
BEGIN
    DECLARE @is_correct BIT, @ques_score INT;

    SELECT @is_correct = is_correct FROM Choice WHERE choice_id = @choice_id;

    SELECT @ques_score = ques_score FROM Question WHERE ques_id = @ques_id;

    INSERT INTO Student_Answer (studentid, ques_id, choice_id)
    VALUES (@student_id, @ques_id, @choice_id);

    IF @is_correct = 1
    BEGIN
        UPDATE Student_Exam
        SET Score = Score + @ques_score
        WHERE exam_id = @exam_id AND StudentId = @student_id;
    END
    SELECT 'Answer submitted successfully!' AS Message
END
Exec sp_SubmitExamAnswers @student_id = 1 , @exam_id =5, @ques_id = 1, @choice_id = 2 
Exec sp_SubmitExamAnswers @student_id = 1 , @exam_id =5, @ques_id = 2, @choice_id = 1
Exec sp_SubmitExamAnswers @student_id = 1 , @exam_id =5, @ques_id = 2, @choice_id = 1 
Exec sp_SubmitExamAnswers @student_id = 1 , @exam_id =5, @ques_id = 4, @choice_id = 4 
Exec sp_SubmitExamAnswers @student_id = 1 , @exam_id =5, @ques_id = 5, @choice_id = 3 
Exec sp_SubmitExamAnswers @student_id = 1 , @exam_id =5, @ques_id = 6, @choice_id = 2 
Exec sp_SubmitExamAnswers @student_id = 1 , @exam_id =5, @ques_id = 7, @choice_id = 1 
Exec sp_SubmitExamAnswers @student_id = 1 , @exam_id =5, @ques_id = 8, @choice_id = 1
Exec sp_SubmitExamAnswers @student_id = 1 , @exam_id =5, @ques_id = 9, @choice_id = 1
Exec sp_SubmitExamAnswers @student_id = 1 , @exam_id =5, @ques_id = 10, @choice_id = 2 
Exec sp_SubmitExamAnswers @student_id = 1 , @exam_id =5, @ques_id = 11, @choice_id = 1

GO
-- Model answer
CREATE PROCEDURE sp_ModelAnswer
    @exam_id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        q.ques_id, q.ques_text, c.choice_id, c.choice_text
    FROM  Question q INNER JOIN  Choice c 
	ON q.ques_id = c.ques_id
    WHERE  q.exam_id = @exam_id AND c.is_correct = 1
END
exec sp_ModelAnswer @exam_id = 5
GO
-- Get question by id and its choices
CREATE PROCEDURE sp_get_question_choices(
    @ques_id int
)
AS
BEGIN
    SELECT q.ques_id, q.ques_text, q.ques_type, q.ques_score, q.exam_id, ch.choice_id, ch.choice_text, ch.is_correct
    FROM Question AS q
    INNER JOIN Choice AS ch ON ch.ques_id = q.ques_id
    WHERE q.ques_id = @ques_id
END
EXEC sp_get_question_choices @ques_id = 1
GO
--get question by exam id
CREATE PROC sp_get_questions_by_exam_id(
    @exam_id int
)
AS
BEGIN
    SELECT * FROM Question
    WHERE exam_id = @exam_id
END
EXEC sp_get_questions_by_exam_id @exam_id = 5
GO
-- Update question text
CREATE PROC sp_update_question_text(
    @ques_id int,
    @ques_text varchar(500)
)
AS
BEGIN
    UPDATE Question SET ques_text = @ques_text
    WHERE ques_id = @ques_id
    
    SELECT * FROM Question WHERE ques_id = @ques_id
END
GO
-- Get All Questions
CREATE PROC sp_get_all_questions
AS
BEGIN
    SELECT * FROM Question
END
GO
-- Update question and its choices
CREATE PROCEDURE sp_update_question_and_choices
(
    @ques_id int,
    @ques_text varchar(255),
    @choice1 varchar(255),
    @choice2 varchar(255),
    @choice3 varchar(255),
    @choice4 varchar(255),
    @ques_score int,
    @correct_choice_no int
)
AS
BEGIN
    DECLARE @exam_id int
    DECLARE @old_score int

    SELECT @old_score = ques_score FROM Question WHERE ques_id = @ques_id

    UPDATE Question SET ques_text = @ques_text, ques_score = @ques_score
    WHERE ques_id = @ques_id

    SELECT @exam_id = exam_id FROM Question WHERE ques_id = @ques_id

    UPDATE Choice
        SET choice_text = @choice1,
        is_correct = CASE WHEN @correct_choice_no = 1 THEN 1 ELSE 0 END
    WHERE ques_id = @ques_id AND choice_id = 
        (SELECT MIN(choice_id) FROM Choice WHERE ques_id = @ques_id)

    UPDATE Choice
        SET choice_text = @choice2,
        is_correct = CASE WHEN @correct_choice_no = 2 THEN 1 ELSE 0 END
    WHERE ques_id = @ques_id AND choice_id = 
        (SELECT MIN(choice_id) + 1 FROM Choice WHERE ques_id = @ques_id)

    UPDATE Choice
        SET choice_text = @choice3,
        is_correct = CASE WHEN @correct_choice_no = 3 THEN 1 ELSE 0 END
    WHERE ques_id = @ques_id AND choice_id = 
        (SELECT MIN(choice_id) + 2 FROM Choice WHERE ques_id = @ques_id)

    UPDATE Choice
        SET choice_text = @choice4,
        is_correct = CASE WHEN @correct_choice_no = 4 THEN 1 ELSE 0 END
    WHERE ques_id = @ques_id AND choice_id = 
        (SELECT MIN(choice_id) + 3 FROM Choice WHERE ques_id = @ques_id)
    
    DECLARE @score int
    SET @score = @ques_score - @old_score
    
    UPDATE Exam SET TotalMarks = TotalMarks + @score
    WHERE exam_id = @exam_id

    SELECT q.ques_id, q.ques_text, q.ques_score, 
           c.choice_id, c.choice_text, c.is_correct
    FROM Question q
    INNER JOIN Choice c ON q.ques_id = c.ques_id
    WHERE q.ques_id = @ques_id
END
GO
-- DELETE QUESTION
CREATE PROC sp_delete_question
(
    @ques_id int
)
AS
BEGIN
    DECLARE @exam_id int
    DECLARE @ques_score int
    
    SELECT @exam_id = exam_id, @ques_score = ques_score 
    FROM Question 
    WHERE ques_id = @ques_id

    IF @exam_id IS NOT NULL
    BEGIN
        UPDATE Exam 
        SET TotalMarks = TotalMarks - @ques_score
        WHERE exam_id = @exam_id
    END
    
    DELETE FROM Question WHERE ques_id = @ques_id
    
    SELECT 'Question deleted successfully' AS Message
END
GO
-- Get choices by question id
CREATE PROC sp_get_choices_quesid
(
    @ques_id int
)
AS
BEGIN
    SELECT * FROM Choice WHERE ques_id = @ques_id
END
GO
-- Get exams by course id
CREATE PROC sp_get_exams_by_crsid
(
    @crs_id int
)
AS
BEGIN
    SELECT * FROM Exam WHERE crs_id = @crs_id
END
GO
-- Get Exam by id
CREATE PROC sp_get_exam_byid(
    @exam_id int
)
AS
BEGIN
    SELECT * FROM Exam WHERE exam_id = @exam_id
END
GO
-- Get all exams
CREATE PROC sp_get_all_exams
AS
BEGIN
    SELECT * FROM Exam
END
GO
-- Add question to exam
CREATE PROC sp_add_ques_to_exam
(
    @exam_id int,
    @ques_id int
)
AS
BEGIN
    DECLARE @ques_score int

    IF @exam_id IS NULL OR @ques_id IS NULL
        RETURN
    
    IF EXISTS (SELECT * FROM Exam WHERE exam_id = @exam_id)
    BEGIN
        IF (SELECT exam_id FROM Question WHERE ques_id = @ques_id) IS NULL
        BEGIN
            UPDATE Question SET exam_id = @exam_id
                WHERE ques_id = @ques_id
                
            SELECT @ques_score = ques_score FROM Question WHERE ques_id = @ques_id

            UPDATE Exam SET TotalMarks = TotalMarks + @ques_score
                WHERE exam_id = @exam_id
                
            SELECT 'Question added to exam successfully' AS Message
        END
        ELSE
            SELECT 'There is an exam already assigned to this question!' AS Message
    END
    ELSE
        SELECT 'Exam does not exist' AS Message
END
GO
--Remove question from exam...
CREATE PROC sp_remove_ques_from_exam
(
@exam_id int,
@ques_id int
)
AS
BEGIN
	DECLARE @ques_score int

		IF @exam_id IS NULL OR @ques_id IS NULL
			RETURN
    
		IF EXISTS (SELECT * FROM Exam WHERE exam_id = @exam_id)
		BEGIN
			IF (SELECT exam_id FROM Question WHERE ques_id = @ques_id) = @exam_id
			BEGIN
				SELECT @ques_score = ques_score FROM Question WHERE ques_id = @ques_id
            
				UPDATE Exam SET TotalMarks = TotalMarks - @ques_score
					WHERE exam_id = @exam_id

				UPDATE Question SET exam_id = NULL
					WHERE ques_id = @ques_id
                
				SELECT 'Question removed from exam successfully' AS Message
			END
			ELSE
				SELECT 'The question does not belong to this exam' AS Message
		END
		ELSE
			SELECT 'Exam does not exist' AS Message
	END
GO
-- ADD choice to question
CREATE PROCEDURE sp_add_choice_to_ques
(
    @ques_id INT,
    @choice VARCHAR(255),
    @is_correct INT = 0
)
AS
BEGIN
    IF (SELECT COUNT(*) FROM Choice WHERE ques_id = @ques_id) >= 4
    BEGIN
        SELECT 'Cannot add more than 4 choices to this question' AS Message
        RETURN
    END

    IF @is_correct = 1
    BEGIN
        UPDATE Choice 
        SET is_correct = 0 
        WHERE ques_id = @ques_id
    END

    INSERT INTO Choice (ques_id, choice_text, is_correct)
    VALUES (@ques_id, @choice, @is_correct)

    SELECT 'Choice added successfully' AS Message
END
GO
-- Remove choice from question
CREATE PROC sp_remove_choice_from_ques(
    @choice_id int,
    @new_correct_id int = NULL
)
AS
BEGIN
    DECLARE @ques_id INT
    DECLARE @is_correct BIT
    DECLARE @choice_count INT
    
    SELECT @ques_id = ques_id, @is_correct = is_correct 
    FROM Choice 
    WHERE choice_id = @choice_id
    
    SELECT @choice_count = COUNT(*) 
    FROM Choice 
    WHERE ques_id = @ques_id
    
    IF @choice_count <= 1
    BEGIN
        SELECT 'Cannot delete the only choice for this question' AS Message
        RETURN
    END
    
    IF @is_correct = 1
    BEGIN
        IF @new_correct_id IS NULL
        BEGIN
            SELECT 'Must specify a new correct choice when deleting the current correct choice' AS Message
            RETURN
        END
        
        DELETE FROM Choice WHERE choice_id = @choice_id
        UPDATE Choice SET is_correct = 1 WHERE choice_id = @new_correct_id
    END
    ELSE
    BEGIN
        DELETE FROM Choice WHERE choice_id = @choice_id
    END
    
    SELECT 'Choice removed successfully' AS Message
END
GO
--DELETE EXAM
CREATE PROC sp_delete_exam
(
    @exam_id int
)
AS
BEGIN
    IF EXISTS (SELECT * FROM Exam WHERE exam_id = @exam_id)
    BEGIN
        BEGIN TRANSACTION
        
        UPDATE Question SET exam_id = NULL
        WHERE exam_id = @exam_id
         
        DELETE FROM Exam WHERE exam_id = @exam_id
        
        COMMIT TRANSACTION
        
        SELECT 'Exam removed successfully' AS Message
    END
    ELSE
        SELECT 'Exam does not exist' AS Message
END
GO

-- Get exam questions and choices (to display it during exam) ------------- FIFTH REPORT
CREATE PROCEDURE sp_GetExamQuestionsAndChoices
    @exam_id INT
AS
BEGIN
    SELECT  q.ques_id, q.ques_text, q.ques_type, q.ques_score, c.choice_id, c.choice_text, c.is_correct
    FROM Question q
    JOIN Choice c ON q.ques_id = c.ques_id
    WHERE q.exam_id = @exam_id
END
EXEC sp_GetExamQuestionsAndChoices @exam_id= 5
GO
-- 4. Get exam questions with student answers (for review/grievances)
CREATE PROCEDURE sp_GetExamQuestionsWithStudentAnswers
    @exam_id INT,
    @student_id INT
AS
BEGIN
    SELECT q.ques_id, q.ques_text, q.ques_type, q.ques_score, c.choice_id, c.choice_text, sa.choice_id AS student_choice_id
    FROM Question q
    JOIN Choice c ON q.ques_id = c.ques_id
    LEFT JOIN Student_Answer sa ON q.ques_id = sa.ques_id AND sa.studentid = @student_id
    WHERE q.exam_id = @exam_id
END

GO
go
-- Gererate rondom exam, for this you need to make question and choices and start use this rondom exam generation
CREATE PROCEDURE sp_GenerateRandomExam
    @Crs_Name VARCHAR(255),
    @MCQ_Count INT,
    @TF_Count INT,
    @Ins_Id INT,
    @ExamName VARCHAR(50),
    @StartTime DATETIME,
    @EndTime DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Crs_Id INT;
    DECLARE @Exam_Id INT;
    
    -- Get course ID
    SELECT @Crs_Id = Crs_Id 
    FROM Courses 
    WHERE Crs_Name = @Crs_Name AND isactive = 1;
    
    IF @Crs_Id IS NULL
    BEGIN
        RAISERROR('Course not found or inactive', 16, 1)
        RETURN;
    END
    
    -- Verify instructor is assigned to this course
    IF NOT EXISTS (
        SELECT 1 FROM Courses 
        WHERE Crs_Id = @Crs_Id AND ins_id = @Ins_Id AND isactive = 1
    )
    BEGIN
        RAISERROR('Instructor is not assigned to this course', 16, 1)
        RETURN;
    END
    
    -- Create the exam
    INSERT INTO Exam (exam_name, startTime, endTime, crs_id, ins_id, isactive)
    VALUES (@ExamName, @StartTime, @EndTime, @Crs_Id, @Ins_Id, 1);
    
    SET @Exam_Id = SCOPE_IDENTITY()
    
    -- Insert random MCQ questions
    INSERT INTO Question (ques_text, ques_type, exam_id, ques_score, isactive)
    SELECT TOP (@MCQ_Count) 
        ques_text, ques_type, @Exam_Id, 5, 1
    FROM Question q
    JOIN Courses c ON q.exam_id IN (SELECT exam_id FROM Exam WHERE crs_id = c.Crs_Id)
    WHERE c.Crs_Id = @Crs_Id 
    AND q.ques_type = 'MCQ' 
    AND q.isactive = 1
    ORDER BY NEWID()
    
    -- Insert random True/False questions
    INSERT INTO Question (ques_text, ques_type, exam_id, ques_score, isactive)
    SELECT TOP (@TF_Count) 
        ques_text, ques_type, @Exam_Id, 5, 1
    FROM Question q
    JOIN Courses c ON q.exam_id IN (SELECT exam_id FROM Exam WHERE crs_id = c.Crs_Id)
    WHERE c.Crs_Id = @Crs_Id 
    AND q.ques_type = 'True/False' 
    AND q.isactive = 1
    ORDER BY NEWID()
     
    SELECT @Exam_Id AS GeneratedExamId
END
exec sp_GenerateRandomExam @crs_name = 'c#', @MCQ_Count = 3, @TF_Count = 2, @Ins_Id = 1, @ExamName = 'c#rondom', @startTime = '2025-04-01 10:00:00', @endTime = '2025-04-01 11:00:00'
GO
-- generate Rondom exam and assign to student
CREATE PROCEDURE sp_GenerateAndAssignExam
    @Crs_Name VARCHAR(255),
    @MCQ_Count INT,
    @TF_Count INT,
    @Ins_Id INT,
    @ExamName VARCHAR(50),
    @StartTime DATETIME,
    @EndTime DATETIME
AS
BEGIN
    SET NOCOUNT ON
    
    DECLARE @Crs_Id INT
    DECLARE @Exam_Id INT
    DECLARE @StudentCount INT = 0
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        -- Get course ID
        SELECT @Crs_Id = Crs_Id 
        FROM Courses 
        WHERE Crs_Name = @Crs_Name AND isactive = 1    
        IF @Crs_Id IS NULL
        BEGIN
            RAISERROR('Course not found or inactive', 16, 1)
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Verify instructor is assigned to this course
        IF NOT EXISTS ( SELECT 1 FROM Courses 
						WHERE Crs_Id = @Crs_Id AND ins_id = @Ins_Id AND isactive = 1
						)
        BEGIN
            RAISERROR('Instructor is not assigned to this course', 16, 1)
            ROLLBACK TRANSACTION
            RETURN
        END
        -- Create the exam
        INSERT INTO Exam (exam_name, startTime, endTime, crs_id, ins_id, isactive)
        VALUES (@ExamName, @StartTime, @EndTime, @Crs_Id, @Ins_Id, 1)
        
        SET @Exam_Id = SCOPE_IDENTITY();
        
        -- Insert random MCQ questions
        INSERT INTO Question (ques_text, ques_type, exam_id, ques_score, isactive)
        SELECT TOP (@MCQ_Count) ques_text, ques_type, @Exam_Id, 5, 1
        FROM Question q JOIN Courses c 
		ON q.exam_id IN (SELECT exam_id FROM Exam WHERE crs_id = c.Crs_Id)
        WHERE c.Crs_Id = @Crs_Id AND q.ques_type = 'MCQ' AND q.isactive = 1
        ORDER BY NEWID()
        
        -- Insert random True/False questions
        INSERT INTO Question (ques_text, ques_type, exam_id, ques_score, isactive)
        SELECT TOP (@TF_Count) ques_text, ques_type, @Exam_Id, 5, 1
        FROM Question q JOIN Courses c ON q.exam_id IN (SELECT exam_id FROM Exam WHERE crs_id = c.Crs_Id)
        WHERE c.Crs_Id = @Crs_Id AND q.ques_type = 'True/False' AND q.isactive = 1
        ORDER BY NEWID()
        
        -- Assign exam to all active students enrolled in the course
        INSERT INTO Student_Exam (exam_id, StudentId, Score, isactive, examination_date)
        SELECT @Exam_Id, sc.StudentId, 0, 1, NULL
        FROM Student_Course sc JOIN Student s 
		ON sc.StudentId = s.StudentId
        JOIN Users u 
		ON s.userId = u.userId
        WHERE sc.Crs_Id = @Crs_Id AND sc.isactive = 1 AND s.isactive = 1 AND u.isactive = 1
        
        SET @StudentCount = @@ROWCOUNT
        
        COMMIT TRANSACTION
        
        SELECT 
            @Exam_Id AS GeneratedExamId,
            @StudentCount AS StudentsAssigned,
            'Exam successfully generated and assigned to students' AS Message
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END

GO
CREATE OR ALTER PROCEDURE sp_UpdateExam
    @exam_id INT,
    @exam_name VARCHAR(50),
    @startTime DATETIME,
    @endTime DATETIME,
    @crs_id INT = NULL,
    @ins_id INT = NULL,
    @isactive BIT = NULL
AS
BEGIN
    BEGIN TRY
        -- Check if exam exists
        IF NOT EXISTS (SELECT 1 FROM Exam WHERE exam_id = @exam_id)
        BEGIN
            RAISERROR('Exam with ID %d does not exist', 16, 1, @exam_id);
            RETURN;
        END

        -- Check time constraint
        IF @endTime <= @startTime
        BEGIN
            RAISERROR('End time must be greater than start time', 16, 1);
            RETURN;
        END

        -- Get current values for parameters that weren't provided
        DECLARE @current_crs_id INT, @current_ins_id INT, @current_isactive BIT
        
        SELECT @current_crs_id = crs_id,
               @current_ins_id = ins_id,
               @current_isactive = isactive
        FROM Exam
        WHERE exam_id = @exam_id
        
        -- Update exam with provided or current values
        UPDATE Exam
        SET exam_name = @exam_name,
            startTime = @startTime,
            endTime = @endTime,
            crs_id = ISNULL(@crs_id, @current_crs_id),
            ins_id = ISNULL(@ins_id, @current_ins_id),
            isactive = ISNULL(@isactive, @current_isactive)
        WHERE exam_id = @exam_id
        
        -- Return the updated exam
        SELECT e.*, c.Crs_Name, u.username AS InstructorName
        FROM Exam e
        LEFT JOIN Courses c ON e.crs_id = c.Crs_Id
        LEFT JOIN Instructor i ON e.ins_id = i.Ins_Id
        LEFT JOIN Users u ON i.userId = u.userId
        WHERE e.exam_id = @exam_id
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_GetExamsBy_crsid
    @crs_id INT,
    @activeExamsOnly BIT = NULL -- NULL = all exams, 1 = active only, 0 = inactive only
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM Courses WHERE Crs_Id = @crs_id)
    BEGIN
        RAISERROR('Course with ID %d does not exist', 16, 1, @crs_id);
        RETURN;
    END
    
    -- Get course information
    DECLARE @CourseName VARCHAR(255)
    SELECT @CourseName = Crs_Name FROM Courses WHERE Crs_Id = @crs_id
    
    SELECT 
        e.exam_id,
        e.exam_name,
        e.startTime,
        e.endTime,
        e.crs_id,
        e.ins_id,
        e.isactive,
        e.TotalMarks,
        e.passedGrade,
        @CourseName AS CourseName,
        u.username AS InstructorName,
        (SELECT COUNT(*) FROM Question q WHERE q.exam_id = e.exam_id) AS QuestionCount,
        (SELECT COUNT(*) FROM Student_Exam se WHERE se.exam_id = e.exam_id) AS AssignedStudentCount,
        CASE
            WHEN e.startTime > GETDATE() THEN 'Upcoming'
            WHEN e.endTime < GETDATE() THEN 'Completed'
            ELSE 'Active'
        END AS ExamStatus
    FROM 
        Exam e
        LEFT JOIN Instructor i ON e.ins_id = i.Ins_Id
        LEFT JOIN Users u ON i.userId = u.userId
    WHERE 
        e.crs_id = @crs_id
        AND (@activeExamsOnly IS NULL OR e.isactive = @activeExamsOnly)
    ORDER BY
        CASE
            WHEN e.startTime > GETDATE() THEN 0 -- Upcoming exams first
            WHEN e.endTime < GETDATE() THEN 2 -- Completed exams last
            ELSE 1 -- Active exams in the middle
        END,
        e.startTime ASC
END
GO
--GET EXAM RESULT  ---------------------------------SIX REPORT
CREATE PROCEDURE sp_GetExamResults
    @ExamId INT,
    @StudentId INT = NULL
AS
BEGIN
    SET NOCOUNT ON

    IF NOT EXISTS (SELECT 1 FROM Exam WHERE exam_id = @ExamId)
    BEGIN
        RAISERROR('Exam not found', 16, 1)
        RETURN;
    END
    
    SELECT e.exam_name, e.startTime, e.endTime, c.Crs_Name, u.username AS InstructorName, e.TotalMarks, e.passedGrade
    FROM Exam e JOIN Courses c 
	ON e.crs_id = c.Crs_Id
    JOIN Instructor i 
	ON e.ins_id = i.Ins_Id
    JOIN Users u 
	ON i.userId = u.userId WHERE e.exam_id = @ExamId

    IF @StudentId IS NULL
    BEGIN
        SELECT s.StudentId, u.username AS StudentName, se.Score, se.pass_fail AS Result,
            CAST(se.Score AS FLOAT) / e.TotalMarks * 100 AS Percentage, se.examination_date
			FROM Student_Exam se JOIN Student s 
			ON se.StudentId = s.StudentId
			JOIN Users u 
			ON s.userId = u.userId
			JOIN Exam e 
			ON se.exam_id = e.exam_id
			WHERE se.exam_id = @ExamId
			AND s.isactive = 1
			ORDER BY se.Score DESC;
    END
    ELSE
    BEGIN
        SELECT 
            s.StudentId, u.username AS StudentName, se.Score, se.pass_fail AS Result,
            CAST(se.Score AS FLOAT) / e.TotalMarks * 100 AS Percentage, se.examination_date,
            q.ques_text AS Question, q.ques_type AS QuestionType, q.ques_score AS MaxScore, ch.choice_text AS StudentAnswer,
            CASE WHEN ch.is_correct = 1 THEN 'Correct' ELSE 'Incorrect' END AS IsCorrect,
            (SELECT choice_text FROM Choice WHERE ques_id = q.ques_id AND is_correct = 1) AS CorrectAnswer
			FROM Student_Exam se
			JOIN Student s ON se.StudentId = s.StudentId
			JOIN Users u ON s.userId = u.userId
			JOIN Exam e ON se.exam_id = e.exam_id
			LEFT JOIN Student_Answer sa ON s.StudentId = sa.studentid
			LEFT JOIN Question q ON sa.ques_id = q.ques_id
			LEFT JOIN Choice ch ON sa.choice_id = ch.choice_id
			WHERE se.exam_id = @ExamId
			AND s.StudentId = @StudentId
			AND s.isactive = 1
    END
END
GO
-- SECOND REPORT Enter Id Get his Grades-----------------second
EXEC sp_GetStudentGradesReport @StudentId = 1
GO
CREATE PROCEDURE sp_GetStudentGradesReport
    @StudentId INT
AS
BEGIN
    SET NOCOUNT ON
    
    IF NOT EXISTS (SELECT 1 FROM Student WHERE StudentId = @StudentId)
    BEGIN
        RAISERROR('Student with ID %d does not exist', 16, 1, @StudentId);
        RETURN;
    END
    
    DECLARE @StudentName VARCHAR(100)
    DECLARE @TrackName VARCHAR(100)
    
    SELECT @StudentName = u.username, @TrackName = t.track_name
    FROM Student s JOIN Users u 
	ON s.userId = u.userId
    LEFT JOIN Track t 
	ON s.track_id = t.track_id
    WHERE s.StudentId = @StudentId
    
    SELECT c.Crs_Name AS CourseName, sc.grade AS Grade,
        CASE 
            WHEN sc.grade IS NULL THEN 'Not Graded'
            ELSE CAST(sc.grade AS VARCHAR) + '/100 (' + CAST(sc.grade AS VARCHAR) + '%)'
        END AS GradePercentage,
        sc.ispassed AS PassStatus, sc.EnrolledAt AS EnrollmentDate,
        CASE 
            WHEN sc.ispassed = 1 THEN 'Passed'
            WHEN sc.ispassed = 0 THEN 'Failed'
            ELSE 'Not Evaluatd'
        END AS PassFailStatus, i.username AS InstructorName
    FROM Student_Course sc
        JOIN Courses c ON sc.Crs_Id = c.Crs_Id
        LEFT JOIN Instructor ins ON c.ins_id = ins.Ins_Id
        LEFT JOIN Users i ON ins.userId = i.userId
    WHERE sc.StudentId = @StudentId AND sc.isactive = 1
    ORDER BY sc.EnrolledAt DESC
    
    SELECT @StudentName AS StudentName, @StudentId AS StudentId, @TrackName AS TrackName,
        COUNT(CASE WHEN sc.ispassed = 1 THEN 1 END) AS PassedCoursesCount,
        COUNT(*) AS TotalCoursesCount,
        CAST(COUNT(CASE WHEN sc.ispassed = 1 THEN 1 END) AS FLOAT) / NULLIF(COUNT(*), 0) * 100 AS SuccessPercentage,
        AVG(CAST(ISNULL(sc.grade, 0) AS FLOAT)) AS AverageGrade
    FROM Student_Course sc
    WHERE sc.StudentId = @StudentId AND sc.isactive = 1
END
GO 

----Third report instructor id and get coursed name and number of student
CREATE PROCEDURE sp_GetInstructorCoursesWithStudentCount
    @InstructorId INT
AS
BEGIN
    SET NOCOUNT ON

    IF NOT EXISTS (SELECT 1 FROM Instructor WHERE Ins_Id = @InstructorId)
    BEGIN
        RAISERROR('Instructor does not exist', 16, 1)
        RETURN
    END
    
    DECLARE @InstructorName VARCHAR(100)
    DECLARE @TrackName VARCHAR(100)
    
    SELECT @InstructorName = u.username, @TrackName = t.track_name
    FROM Instructor i JOIN Users u 
	ON i.userId = u.userId
    LEFT JOIN Track t 
	ON i.track_id = t.track_id
    WHERE i.Ins_Id = @InstructorId

    SELECT c.Crs_Id AS CourseId, c.Crs_Name AS CourseName, c.Crs_period AS CoursePeriod,
        COUNT(sc.StudentId) AS StudentCount,
        COUNT(CASE WHEN sc.ispassed = 1 THEN 1 END) AS PassedStudents,
        COUNT(CASE WHEN sc.ispassed = 0 THEN 1 END) AS FailedStudents,
        AVG(CAST(ISNULL(sc.grade, 0) AS FLOAT)) AS AverageGrade,
        CASE 
			WHEN c.isactive = 1 THEN 'Active'
            ELSE 'Inactive'
        END AS CourseStatus
    FROM Courses c LEFT JOIN Student_Course sc 
	ON c.Crs_Id = sc.Crs_Id AND sc.isactive = 1
    WHERE c.ins_id = @InstructorId
    GROUP BY c.Crs_Id, c.Crs_Name, c.Crs_period, c.isactive
    ORDER BY c.Crs_Name
    
    SELECT @InstructorName AS InstructorName, @InstructorId AS InstructorId, @TrackName AS TrackName,
        COUNT(DISTINCT c.Crs_Id) AS TotalCourses,
        SUM(COUNT(sc.StudentId)) OVER () AS TotalStudents,
        CAST(COUNT(DISTINCT CASE WHEN c.isactive = 1 THEN c.Crs_Id END) AS VARCHAR) + ' active / ' +  
		CAST(COUNT(DISTINCT c.Crs_Id) AS VARCHAR) + ' total' AS CourseStatusSummary
    FROM Courses c
        LEFT JOIN Student_Course sc 
		ON c.Crs_Id = sc.Crs_Id AND sc.isactive = 1
    WHERE c.ins_id = @InstructorId
    GROUP BY c.Crs_Id, c.isactive
END
GO

