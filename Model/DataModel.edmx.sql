
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 09/22/2023 14:46:06
-- Generated from EDMX file: C:\Users\97254\Desktop\EasySchool\Model\DataModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [school];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Classrooms_ToRooms]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Classes] DROP CONSTRAINT [FK_Classrooms_ToRooms];
GO
IF OBJECT_ID(N'[dbo].[FK_Events_ToClasses]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Events] DROP CONSTRAINT [FK_Events_ToClasses];
GO
IF OBJECT_ID(N'[dbo].[FK_Events_ToPersons]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Events] DROP CONSTRAINT [FK_Events_ToPersons];
GO
IF OBJECT_ID(N'[dbo].[FK_Events_ToRecipientPersons]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Events] DROP CONSTRAINT [FK_Events_ToRecipientPersons];
GO
IF OBJECT_ID(N'[dbo].[FK_Grades_ToTeachers]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Grades] DROP CONSTRAINT [FK_Grades_ToTeachers];
GO
IF OBJECT_ID(N'[dbo].[FK_Lessons_ToClasses]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Lessons] DROP CONSTRAINT [FK_Lessons_ToClasses];
GO
IF OBJECT_ID(N'[dbo].[FK_Lessons_ToCourses]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Lessons] DROP CONSTRAINT [FK_Lessons_ToCourses];
GO
IF OBJECT_ID(N'[dbo].[FK_Lessons_ToRooms]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Lessons] DROP CONSTRAINT [FK_Lessons_ToRooms];
GO
IF OBJECT_ID(N'[dbo].[FK_Lessons_ToTeachers]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Lessons] DROP CONSTRAINT [FK_Lessons_ToTeachers];
GO
IF OBJECT_ID(N'[dbo].[FK_Messages_ToClasses]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Messages] DROP CONSTRAINT [FK_Messages_ToClasses];
GO
IF OBJECT_ID(N'[dbo].[FK_Messages_ToPersons]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Messages] DROP CONSTRAINT [FK_Messages_ToPersons];
GO
IF OBJECT_ID(N'[dbo].[FK_Messages_ToPersonsRecipient]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Messages] DROP CONSTRAINT [FK_Messages_ToPersonsRecipient];
GO
IF OBJECT_ID(N'[dbo].[FK_Persons_ToUsers]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Persons] DROP CONSTRAINT [FK_Persons_ToUsers];
GO
IF OBJECT_ID(N'[dbo].[FK_Scores_ToCourses]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Grades] DROP CONSTRAINT [FK_Scores_ToCourses];
GO
IF OBJECT_ID(N'[dbo].[FK_Scores_ToStudents]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Grades] DROP CONSTRAINT [FK_Scores_ToStudents];
GO
IF OBJECT_ID(N'[dbo].[FK_Students_ToClasses]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Students] DROP CONSTRAINT [FK_Students_ToClasses];
GO
IF OBJECT_ID(N'[dbo].[FK_Students_ToParentPersons]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Students] DROP CONSTRAINT [FK_Students_ToParentPersons];
GO
IF OBJECT_ID(N'[dbo].[FK_Students_ToPersons]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Students] DROP CONSTRAINT [FK_Students_ToPersons];
GO
IF OBJECT_ID(N'[dbo].[FK_Teachers_ToClasses]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Teachers] DROP CONSTRAINT [FK_Teachers_ToClasses];
GO
IF OBJECT_ID(N'[dbo].[FK_Teachers_ToCourses_1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Teachers] DROP CONSTRAINT [FK_Teachers_ToCourses_1];
GO
IF OBJECT_ID(N'[dbo].[FK_Teachers_ToCourses_2]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Teachers] DROP CONSTRAINT [FK_Teachers_ToCourses_2];
GO
IF OBJECT_ID(N'[dbo].[FK_Teachers_ToCourses_3]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Teachers] DROP CONSTRAINT [FK_Teachers_ToCourses_3];
GO
IF OBJECT_ID(N'[dbo].[FK_Teachers_ToCourses_4]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Teachers] DROP CONSTRAINT [FK_Teachers_ToCourses_4];
GO
IF OBJECT_ID(N'[dbo].[FK_Teachers_ToPersons]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Teachers] DROP CONSTRAINT [FK_Teachers_ToPersons];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Classes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Classes];
GO
IF OBJECT_ID(N'[dbo].[Courses]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Courses];
GO
IF OBJECT_ID(N'[dbo].[Events]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Events];
GO
IF OBJECT_ID(N'[dbo].[Grades]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Grades];
GO
IF OBJECT_ID(N'[dbo].[Lessons]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Lessons];
GO
IF OBJECT_ID(N'[dbo].[Messages]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Messages];
GO
IF OBJECT_ID(N'[dbo].[Persons]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Persons];
GO
IF OBJECT_ID(N'[dbo].[Rooms]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Rooms];
GO
IF OBJECT_ID(N'[dbo].[SchoolInfo]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SchoolInfo];
GO
IF OBJECT_ID(N'[dbo].[Students]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Students];
GO
IF OBJECT_ID(N'[dbo].[Teachers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Teachers];
GO
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Classes'
CREATE TABLE [dbo].[Classes] (
    [classID] int IDENTITY(1,1) NOT NULL,
    [roomID] int  NULL,
    [className] nvarchar(16)  NULL
);
GO

-- Creating table 'Courses'
CREATE TABLE [dbo].[Courses] (
    [courseID] int IDENTITY(1,1) NOT NULL,
    [courseName] nvarchar(20)  NOT NULL,
    [isHomeroomTeacherOnly] bit  NOT NULL
);
GO

-- Creating table 'Lessons'
CREATE TABLE [dbo].[Lessons] (
    [lessonID] int IDENTITY(1,1) NOT NULL,
    [teacherID] int  NOT NULL,
    [courseID] int  NOT NULL,
    [classID] int  NOT NULL,
    [roomID] int  NULL,
    [firstLessonDay] tinyint  NOT NULL,
    [firstLessonHour] tinyint  NOT NULL,
    [secondLessonDay] tinyint  NULL,
    [secondLessonHour] tinyint  NULL,
    [thirdLessonDay] tinyint  NULL,
    [thirdLessonHour] tinyint  NULL,
    [fourthLessonDay] tinyint  NULL,
    [fourthLessonHour] tinyint  NULL
);
GO

-- Creating table 'Messages'
CREATE TABLE [dbo].[Messages] (
    [messageID] int IDENTITY(1,1) NOT NULL,
    [senderID] int  NULL,
    [recipientID] int  NULL,
    [recipientClassID] int  NULL,
    [forAllManagement] bit  NOT NULL,
    [forAllTeachers] bit  NOT NULL,
    [forAllStudents] bit  NOT NULL,
    [forEveryone] bit  NOT NULL,
    [data] nvarchar(350)  NOT NULL,
    [date] datetime  NOT NULL,
    [title] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'Persons'
CREATE TABLE [dbo].[Persons] (
    [personID] int IDENTITY(1,1) NOT NULL,
    [userID] int  NOT NULL,
    [firstName] nvarchar(20)  NOT NULL,
    [lastName] nvarchar(20)  NOT NULL,
    [email] nvarchar(50)  NULL,
    [phoneNumber] char(12)  NOT NULL,
    [birthdate] datetime  NULL,
    [isStudent] bit  NOT NULL,
    [isTeacher] bit  NOT NULL,
    [isSecretary] bit  NOT NULL,
    [isPrincipal] bit  NOT NULL,
    [isParent] bit  NOT NULL
);
GO

-- Creating table 'Rooms'
CREATE TABLE [dbo].[Rooms] (
    [roomID] int IDENTITY(1,1) NOT NULL,
    [roomName] nvarchar(16)  NULL
);
GO

-- Creating table 'SchoolInfo'
CREATE TABLE [dbo].[SchoolInfo] (
    [key] nvarchar(30)  NOT NULL,
    [value] nvarchar(250)  NOT NULL
);
GO

-- Creating table 'Students'
CREATE TABLE [dbo].[Students] (
    [studentID] int  NOT NULL,
    [classID] int  NULL,
    [absencesCounter] int  NOT NULL,
    [parentID] int  NULL
);
GO

-- Creating table 'Teachers'
CREATE TABLE [dbo].[Teachers] (
    [teacherID] int  NOT NULL,
    [classID] int  NULL,
    [firstCourseID] int  NULL,
    [secondCourseID] int  NULL,
    [thirdCourseID] int  NULL,
    [fourthCourseID] int  NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [userID] int IDENTITY(1,1) NOT NULL,
    [username] nvarchar(16)  NOT NULL,
    [password] nvarchar(16)  NOT NULL,
    [isDisabled] bit  NOT NULL,
    [hasToChangePassword] bit  NOT NULL
);
GO

-- Creating table 'Events'
CREATE TABLE [dbo].[Events] (
    [eventID] int IDENTITY(1,1) NOT NULL,
    [submitterID] int  NOT NULL,
    [recipientID] int  NULL,
    [recipientClassID] int  NULL,
    [name] nvarchar(50)  NOT NULL,
    [description] nvarchar(250)  NULL,
    [eventDate] datetime  NOT NULL,
    [location] nvarchar(50)  NULL
);
GO

-- Creating table 'Grades'
CREATE TABLE [dbo].[Grades] (
    [studentID] int  NOT NULL,
    [courseID] int  NOT NULL,
    [teacherID] int  NOT NULL,
    [score] tinyint  NOT NULL,
    [notes] nvarchar(150)  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [classID] in table 'Classes'
ALTER TABLE [dbo].[Classes]
ADD CONSTRAINT [PK_Classes]
    PRIMARY KEY CLUSTERED ([classID] ASC);
GO

-- Creating primary key on [courseID] in table 'Courses'
ALTER TABLE [dbo].[Courses]
ADD CONSTRAINT [PK_Courses]
    PRIMARY KEY CLUSTERED ([courseID] ASC);
GO

-- Creating primary key on [lessonID] in table 'Lessons'
ALTER TABLE [dbo].[Lessons]
ADD CONSTRAINT [PK_Lessons]
    PRIMARY KEY CLUSTERED ([lessonID] ASC);
GO

-- Creating primary key on [messageID] in table 'Messages'
ALTER TABLE [dbo].[Messages]
ADD CONSTRAINT [PK_Messages]
    PRIMARY KEY CLUSTERED ([messageID] ASC);
GO

-- Creating primary key on [personID] in table 'Persons'
ALTER TABLE [dbo].[Persons]
ADD CONSTRAINT [PK_Persons]
    PRIMARY KEY CLUSTERED ([personID] ASC);
GO

-- Creating primary key on [roomID] in table 'Rooms'
ALTER TABLE [dbo].[Rooms]
ADD CONSTRAINT [PK_Rooms]
    PRIMARY KEY CLUSTERED ([roomID] ASC);
GO

-- Creating primary key on [key] in table 'SchoolInfo'
ALTER TABLE [dbo].[SchoolInfo]
ADD CONSTRAINT [PK_SchoolInfo]
    PRIMARY KEY CLUSTERED ([key] ASC);
GO

-- Creating primary key on [studentID] in table 'Students'
ALTER TABLE [dbo].[Students]
ADD CONSTRAINT [PK_Students]
    PRIMARY KEY CLUSTERED ([studentID] ASC);
GO

-- Creating primary key on [teacherID] in table 'Teachers'
ALTER TABLE [dbo].[Teachers]
ADD CONSTRAINT [PK_Teachers]
    PRIMARY KEY CLUSTERED ([teacherID] ASC);
GO

-- Creating primary key on [userID] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([userID] ASC);
GO

-- Creating primary key on [eventID] in table 'Events'
ALTER TABLE [dbo].[Events]
ADD CONSTRAINT [PK_Events]
    PRIMARY KEY CLUSTERED ([eventID] ASC);
GO

-- Creating primary key on [studentID], [courseID] in table 'Grades'
ALTER TABLE [dbo].[Grades]
ADD CONSTRAINT [PK_Grades]
    PRIMARY KEY CLUSTERED ([studentID], [courseID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [roomID] in table 'Classes'
ALTER TABLE [dbo].[Classes]
ADD CONSTRAINT [FK_Classes_ToRooms]
    FOREIGN KEY ([roomID])
    REFERENCES [dbo].[Rooms]
        ([roomID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Classes_ToRooms'
CREATE INDEX [IX_FK_Classes_ToRooms]
ON [dbo].[Classes]
    ([roomID]);
GO

-- Creating foreign key on [classID] in table 'Lessons'
ALTER TABLE [dbo].[Lessons]
ADD CONSTRAINT [FK_Lessons_ToClasses]
    FOREIGN KEY ([classID])
    REFERENCES [dbo].[Classes]
        ([classID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Lessons_ToClasses'
CREATE INDEX [IX_FK_Lessons_ToClasses]
ON [dbo].[Lessons]
    ([classID]);
GO

-- Creating foreign key on [recipientClassID] in table 'Messages'
ALTER TABLE [dbo].[Messages]
ADD CONSTRAINT [FK_Messages_ToClasses]
    FOREIGN KEY ([recipientClassID])
    REFERENCES [dbo].[Classes]
        ([classID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Messages_ToClasses'
CREATE INDEX [IX_FK_Messages_ToClasses]
ON [dbo].[Messages]
    ([recipientClassID]);
GO

-- Creating foreign key on [classID] in table 'Students'
ALTER TABLE [dbo].[Students]
ADD CONSTRAINT [FK_Students_ToClasses]
    FOREIGN KEY ([classID])
    REFERENCES [dbo].[Classes]
        ([classID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Students_ToClasses'
CREATE INDEX [IX_FK_Students_ToClasses]
ON [dbo].[Students]
    ([classID]);
GO

-- Creating foreign key on [classID] in table 'Teachers'
ALTER TABLE [dbo].[Teachers]
ADD CONSTRAINT [FK_Teachers_ToClasses]
    FOREIGN KEY ([classID])
    REFERENCES [dbo].[Classes]
        ([classID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Teachers_ToClasses'
CREATE INDEX [IX_FK_Teachers_ToClasses]
ON [dbo].[Teachers]
    ([classID]);
GO

-- Creating foreign key on [courseID] in table 'Lessons'
ALTER TABLE [dbo].[Lessons]
ADD CONSTRAINT [FK_Lessons_ToCourses]
    FOREIGN KEY ([courseID])
    REFERENCES [dbo].[Courses]
        ([courseID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Lessons_ToCourses'
CREATE INDEX [IX_FK_Lessons_ToCourses]
ON [dbo].[Lessons]
    ([courseID]);
GO

-- Creating foreign key on [firstCourseID] in table 'Teachers'
ALTER TABLE [dbo].[Teachers]
ADD CONSTRAINT [FK_Teachers_ToCourses_1]
    FOREIGN KEY ([firstCourseID])
    REFERENCES [dbo].[Courses]
        ([courseID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Teachers_ToCourses_1'
CREATE INDEX [IX_FK_Teachers_ToCourses_1]
ON [dbo].[Teachers]
    ([firstCourseID]);
GO

-- Creating foreign key on [secondCourseID] in table 'Teachers'
ALTER TABLE [dbo].[Teachers]
ADD CONSTRAINT [FK_Teachers_ToCourses_2]
    FOREIGN KEY ([secondCourseID])
    REFERENCES [dbo].[Courses]
        ([courseID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Teachers_ToCourses_2'
CREATE INDEX [IX_FK_Teachers_ToCourses_2]
ON [dbo].[Teachers]
    ([secondCourseID]);
GO

-- Creating foreign key on [thirdCourseID] in table 'Teachers'
ALTER TABLE [dbo].[Teachers]
ADD CONSTRAINT [FK_Teachers_ToCourses_3]
    FOREIGN KEY ([thirdCourseID])
    REFERENCES [dbo].[Courses]
        ([courseID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Teachers_ToCourses_3'
CREATE INDEX [IX_FK_Teachers_ToCourses_3]
ON [dbo].[Teachers]
    ([thirdCourseID]);
GO

-- Creating foreign key on [fourthCourseID] in table 'Teachers'
ALTER TABLE [dbo].[Teachers]
ADD CONSTRAINT [FK_Teachers_ToCourses_4]
    FOREIGN KEY ([fourthCourseID])
    REFERENCES [dbo].[Courses]
        ([courseID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Teachers_ToCourses_4'
CREATE INDEX [IX_FK_Teachers_ToCourses_4]
ON [dbo].[Teachers]
    ([fourthCourseID]);
GO

-- Creating foreign key on [roomID] in table 'Lessons'
ALTER TABLE [dbo].[Lessons]
ADD CONSTRAINT [FK_Lessons_ToRooms]
    FOREIGN KEY ([roomID])
    REFERENCES [dbo].[Rooms]
        ([roomID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Lessons_ToRooms'
CREATE INDEX [IX_FK_Lessons_ToRooms]
ON [dbo].[Lessons]
    ([roomID]);
GO

-- Creating foreign key on [teacherID] in table 'Lessons'
ALTER TABLE [dbo].[Lessons]
ADD CONSTRAINT [FK_Lessons_ToTeachers]
    FOREIGN KEY ([teacherID])
    REFERENCES [dbo].[Teachers]
        ([teacherID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Lessons_ToTeachers'
CREATE INDEX [IX_FK_Lessons_ToTeachers]
ON [dbo].[Lessons]
    ([teacherID]);
GO

-- Creating foreign key on [senderID] in table 'Messages'
ALTER TABLE [dbo].[Messages]
ADD CONSTRAINT [FK_Messages_ToPersons]
    FOREIGN KEY ([senderID])
    REFERENCES [dbo].[Persons]
        ([personID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Messages_ToPersons'
CREATE INDEX [IX_FK_Messages_ToPersons]
ON [dbo].[Messages]
    ([senderID]);
GO

-- Creating foreign key on [recipientID] in table 'Messages'
ALTER TABLE [dbo].[Messages]
ADD CONSTRAINT [FK_Messages_ToPersonsRecipient]
    FOREIGN KEY ([recipientID])
    REFERENCES [dbo].[Persons]
        ([personID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Messages_ToPersonsRecipient'
CREATE INDEX [IX_FK_Messages_ToPersonsRecipient]
ON [dbo].[Messages]
    ([recipientID]);
GO

-- Creating foreign key on [userID] in table 'Persons'
ALTER TABLE [dbo].[Persons]
ADD CONSTRAINT [FK_Persons_ToUsers]
    FOREIGN KEY ([userID])
    REFERENCES [dbo].[Users]
        ([userID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Persons_ToUsers'
CREATE INDEX [IX_FK_Persons_ToUsers]
ON [dbo].[Persons]
    ([userID]);
GO

-- Creating foreign key on [parentID] in table 'Students'
ALTER TABLE [dbo].[Students]
ADD CONSTRAINT [FK_Students_ToParentPersons]
    FOREIGN KEY ([parentID])
    REFERENCES [dbo].[Persons]
        ([personID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Students_ToParentPersons'
CREATE INDEX [IX_FK_Students_ToParentPersons]
ON [dbo].[Students]
    ([parentID]);
GO

-- Creating foreign key on [studentID] in table 'Students'
ALTER TABLE [dbo].[Students]
ADD CONSTRAINT [FK_Students_ToPersons]
    FOREIGN KEY ([studentID])
    REFERENCES [dbo].[Persons]
        ([personID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [teacherID] in table 'Teachers'
ALTER TABLE [dbo].[Teachers]
ADD CONSTRAINT [FK_Teachers_ToPersons]
    FOREIGN KEY ([teacherID])
    REFERENCES [dbo].[Persons]
        ([personID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [recipientClassID] in table 'Events'
ALTER TABLE [dbo].[Events]
ADD CONSTRAINT [FK_Events_ToClasses]
    FOREIGN KEY ([recipientClassID])
    REFERENCES [dbo].[Classes]
        ([classID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Events_ToClasses'
CREATE INDEX [IX_FK_Events_ToClasses]
ON [dbo].[Events]
    ([recipientClassID]);
GO

-- Creating foreign key on [submitterID] in table 'Events'
ALTER TABLE [dbo].[Events]
ADD CONSTRAINT [FK_Events_ToPersons]
    FOREIGN KEY ([submitterID])
    REFERENCES [dbo].[Persons]
        ([personID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Events_ToPersons'
CREATE INDEX [IX_FK_Events_ToPersons]
ON [dbo].[Events]
    ([submitterID]);
GO

-- Creating foreign key on [recipientID] in table 'Events'
ALTER TABLE [dbo].[Events]
ADD CONSTRAINT [FK_Events_ToRecipientPersons]
    FOREIGN KEY ([recipientID])
    REFERENCES [dbo].[Persons]
        ([personID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Events_ToRecipientPersons'
CREATE INDEX [IX_FK_Events_ToRecipientPersons]
ON [dbo].[Events]
    ([recipientID]);
GO

-- Creating foreign key on [courseID] in table 'Grades'
ALTER TABLE [dbo].[Grades]
ADD CONSTRAINT [FK_Scores_ToCourses]
    FOREIGN KEY ([courseID])
    REFERENCES [dbo].[Courses]
        ([courseID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Scores_ToCourses'
CREATE INDEX [IX_FK_Scores_ToCourses]
ON [dbo].[Grades]
    ([courseID]);
GO

-- Creating foreign key on [teacherID] in table 'Grades'
ALTER TABLE [dbo].[Grades]
ADD CONSTRAINT [FK_Grades_ToTeachers]
    FOREIGN KEY ([teacherID])
    REFERENCES [dbo].[Teachers]
        ([teacherID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Grades_ToTeachers'
CREATE INDEX [IX_FK_Grades_ToTeachers]
ON [dbo].[Grades]
    ([teacherID]);
GO

-- Creating foreign key on [studentID] in table 'Grades'
ALTER TABLE [dbo].[Grades]
ADD CONSTRAINT [FK_Scores_ToStudents]
    FOREIGN KEY ([studentID])
    REFERENCES [dbo].[Students]
        ([studentID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------