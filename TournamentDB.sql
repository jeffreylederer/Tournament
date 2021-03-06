USE [master]
GO
/****** Object:  Database [Tournament]    Script Date: 1/7/2020 11:50:42 PM ******/
CREATE DATABASE [Tournament]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Tournament', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQL\DATA\Tournament.mdf' , SIZE = 15360KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'Tournament_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQL\DATA\Tournament_log.ldf' , SIZE = 2048KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [Tournament] SET COMPATIBILITY_LEVEL = 120
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Tournament].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Tournament] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Tournament] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Tournament] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Tournament] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Tournament] SET ARITHABORT OFF 
GO
ALTER DATABASE [Tournament] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [Tournament] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Tournament] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Tournament] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Tournament] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Tournament] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Tournament] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Tournament] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Tournament] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Tournament] SET  DISABLE_BROKER 
GO
ALTER DATABASE [Tournament] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Tournament] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Tournament] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Tournament] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Tournament] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Tournament] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Tournament] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Tournament] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [Tournament] SET  MULTI_USER 
GO
ALTER DATABASE [Tournament] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Tournament] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Tournament] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Tournament] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [Tournament] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [Tournament] SET QUERY_STORE = OFF
GO
USE [Tournament]
GO
/****** Object:  Table [dbo].[League]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[League](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[LeagueName] [varchar](50) NOT NULL,
	[Active] [bit] NOT NULL,
	[TeamSize] [int] NOT NULL,
	[rowversion] [timestamp] NOT NULL,
	[TiesAllowed] [bit] NOT NULL,
	[PointsCount] [bit] NOT NULL,
	[WinPoints] [smallint] NOT NULL,
	[TiePoints] [smallint] NOT NULL,
	[ByePoints] [smallint] NOT NULL,
	[StartWeek] [int] NOT NULL,
 CONSTRAINT [PK_League] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[User]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[username] [varchar](200) NOT NULL,
	[password] [varchar](100) NOT NULL,
	[Roles] [varchar](50) NULL,
	[rowversion] [timestamp] NOT NULL,
 CONSTRAINT [PK_User_1] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserLeague]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserLeague](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[LeagueId] [int] NOT NULL,
	[Roles] [varchar](50) NULL,
	[rowversion] [timestamp] NOT NULL,
 CONSTRAINT [PK_UserLeague] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[UserLeagueView]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE View [dbo].[UserLeagueView]
as
SELECT 
      u.id
      ,g.LeagueName
	  ,u.username
      ,l.[Roles] as [league role]
	  ,u.Roles as [site role]
	  ,g.active
  FROM [User] u
  left outer join [UserLeague] l on u.id=l.userid
  left outer join League g on g.id = l.LeagueId


  -- select * from UserLeagueView
GO
/****** Object:  Table [dbo].[ELMAH_Error]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ELMAH_Error](
	[ErrorId] [uniqueidentifier] NOT NULL,
	[Application] [nvarchar](60) NOT NULL,
	[Host] [nvarchar](50) NOT NULL,
	[Type] [nvarchar](100) NOT NULL,
	[Source] [nvarchar](60) NOT NULL,
	[Message] [nvarchar](500) NOT NULL,
	[User] [nvarchar](50) NOT NULL,
	[StatusCode] [int] NOT NULL,
	[TimeUtc] [datetime] NOT NULL,
	[Sequence] [int] IDENTITY(1,1) NOT NULL,
	[AllXml] [ntext] NOT NULL,
 CONSTRAINT [PK_ELMAH_Error] PRIMARY KEY NONCLUSTERED 
(
	[ErrorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Match]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Match](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[WeekId] [int] NOT NULL,
	[Rink] [int] NOT NULL,
	[TeamNo1] [int] NOT NULL,
	[TeamNo2] [int] NULL,
	[Team1Score] [int] NOT NULL,
	[Team2Score] [int] NOT NULL,
	[rowversion] [timestamp] NOT NULL,
	[ForFeitId] [int] NOT NULL,
 CONSTRAINT [PK_Schedule] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Membership]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Membership](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [varchar](50) NOT NULL,
	[LastName] [varchar](50) NOT NULL,
	[FullName]  AS (([FirstName]+' ')+[Lastname]),
	[shortname] [varchar](25) NULL,
	[NickName]  AS (case when isnull([shortname],'')='' then [firstname] else [shortname] end),
	[rowversion] [timestamp] NOT NULL,
	[Wheelchair] [bit] NOT NULL,
 CONSTRAINT [PK_Membership] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Player]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Player](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Leagueid] [int] NOT NULL,
	[MembershipId] [int] NOT NULL,
	[rowversion] [timestamp] NOT NULL,
 CONSTRAINT [PK_Players] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RinkOrder]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RinkOrder](
	[id] [int] NOT NULL,
	[Green] [varchar](25) NOT NULL,
	[Direction] [varchar](25) NOT NULL,
	[Boundary] [varchar](25) NOT NULL,
	[rowversion] [timestamp] NOT NULL,
 CONSTRAINT [PK_RinkOrder] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Schedule]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Schedule](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[GameDate] [date] NOT NULL,
	[Leagueid] [int] NOT NULL,
	[rowversion] [timestamp] NOT NULL,
	[Cancelled] [bit] NOT NULL,
	[WeekDate]  AS (CONVERT([varchar](10),[GameDate],(101))),
 CONSTRAINT [PK_Schedule_1] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Team]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Team](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Skip] [int] NULL,
	[ViceSkip] [int] NULL,
	[Lead] [int] NULL,
	[Leagueid] [int] NOT NULL,
	[TeamNo] [int] NOT NULL,
	[rowversion] [timestamp] NOT NULL,
 CONSTRAINT [PK_] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Membership]    Script Date: 1/7/2020 11:50:42 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Membership] ON [dbo].[Membership]
(
	[LastName] ASC,
	[FirstName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Player]    Script Date: 1/7/2020 11:50:42 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Player] ON [dbo].[Player]
(
	[MembershipId] ASC,
	[Leagueid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Schedule_1]    Script Date: 1/7/2020 11:50:42 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Schedule_1] ON [dbo].[Schedule]
(
	[Leagueid] ASC,
	[GameDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_User]    Script Date: 1/7/2020 11:50:42 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_User] ON [dbo].[User]
(
	[username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_UserLeague]    Script Date: 1/7/2020 11:50:42 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserLeague] ON [dbo].[UserLeague]
(
	[UserId] ASC,
	[LeagueId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ELMAH_Error] ADD  CONSTRAINT [DF_ELMAH_Error_ErrorId]  DEFAULT (newid()) FOR [ErrorId]
GO
ALTER TABLE [dbo].[League] ADD  CONSTRAINT [DF_League_Active]  DEFAULT ((1)) FOR [Active]
GO
ALTER TABLE [dbo].[League] ADD  DEFAULT ((0)) FOR [TiesAllowed]
GO
ALTER TABLE [dbo].[League] ADD  DEFAULT ((0)) FOR [PointsCount]
GO
ALTER TABLE [dbo].[League] ADD  CONSTRAINT [DF_League_WinPoints]  DEFAULT ((1)) FOR [WinPoints]
GO
ALTER TABLE [dbo].[League] ADD  CONSTRAINT [DF_League_TiePoints]  DEFAULT ((1)) FOR [TiePoints]
GO
ALTER TABLE [dbo].[League] ADD  CONSTRAINT [DF_League_ByePoints]  DEFAULT ((1)) FOR [ByePoints]
GO
ALTER TABLE [dbo].[League] ADD  CONSTRAINT [DF_League_StartWeek]  DEFAULT ((1)) FOR [StartWeek]
GO
ALTER TABLE [dbo].[Match] ADD  CONSTRAINT [DF_Match_Team1Score]  DEFAULT ((0)) FOR [Team1Score]
GO
ALTER TABLE [dbo].[Match] ADD  CONSTRAINT [DF_Match_Team2Score]  DEFAULT ((0)) FOR [Team2Score]
GO
ALTER TABLE [dbo].[Match] ADD  CONSTRAINT [DF_Match_ForFeit]  DEFAULT ((0)) FOR [ForFeitId]
GO
ALTER TABLE [dbo].[Membership] ADD  CONSTRAINT [DF_Membership_Wheelchair]  DEFAULT ((0)) FOR [Wheelchair]
GO
ALTER TABLE [dbo].[Schedule] ADD  CONSTRAINT [DF_Schedule_Cancelled]  DEFAULT ((0)) FOR [Cancelled]
GO
ALTER TABLE [dbo].[League]  WITH CHECK ADD  CONSTRAINT [FK_League_RinkOrder] FOREIGN KEY([StartWeek])
REFERENCES [dbo].[RinkOrder] ([id])
GO
ALTER TABLE [dbo].[League] CHECK CONSTRAINT [FK_League_RinkOrder]
GO
ALTER TABLE [dbo].[Match]  WITH CHECK ADD  CONSTRAINT [FK_Match_Schedule] FOREIGN KEY([WeekId])
REFERENCES [dbo].[Schedule] ([id])
GO
ALTER TABLE [dbo].[Match] CHECK CONSTRAINT [FK_Match_Schedule]
GO
ALTER TABLE [dbo].[Match]  WITH CHECK ADD  CONSTRAINT [FK_Match_Team] FOREIGN KEY([TeamNo1])
REFERENCES [dbo].[Team] ([id])
GO
ALTER TABLE [dbo].[Match] CHECK CONSTRAINT [FK_Match_Team]
GO
ALTER TABLE [dbo].[Match]  WITH CHECK ADD  CONSTRAINT [FK_Match_Team1] FOREIGN KEY([TeamNo2])
REFERENCES [dbo].[Team] ([id])
GO
ALTER TABLE [dbo].[Match] CHECK CONSTRAINT [FK_Match_Team1]
GO
ALTER TABLE [dbo].[Player]  WITH CHECK ADD  CONSTRAINT [FK__Player__leagueid__4E88ABD4] FOREIGN KEY([Leagueid])
REFERENCES [dbo].[League] ([id])
GO
ALTER TABLE [dbo].[Player] CHECK CONSTRAINT [FK__Player__leagueid__4E88ABD4]
GO
ALTER TABLE [dbo].[Player]  WITH CHECK ADD  CONSTRAINT [FK_Player_Membership] FOREIGN KEY([MembershipId])
REFERENCES [dbo].[Membership] ([id])
GO
ALTER TABLE [dbo].[Player] CHECK CONSTRAINT [FK_Player_Membership]
GO
ALTER TABLE [dbo].[Schedule]  WITH CHECK ADD  CONSTRAINT [FK__Schedule__League__4F7CD00D] FOREIGN KEY([Leagueid])
REFERENCES [dbo].[League] ([id])
GO
ALTER TABLE [dbo].[Schedule] CHECK CONSTRAINT [FK__Schedule__League__4F7CD00D]
GO
ALTER TABLE [dbo].[Team]  WITH CHECK ADD  CONSTRAINT [FK__Players] FOREIGN KEY([Skip])
REFERENCES [dbo].[Player] ([id])
GO
ALTER TABLE [dbo].[Team] CHECK CONSTRAINT [FK__Players]
GO
ALTER TABLE [dbo].[Team]  WITH CHECK ADD  CONSTRAINT [FK__Players1] FOREIGN KEY([ViceSkip])
REFERENCES [dbo].[Player] ([id])
GO
ALTER TABLE [dbo].[Team] CHECK CONSTRAINT [FK__Players1]
GO
ALTER TABLE [dbo].[Team]  WITH CHECK ADD  CONSTRAINT [FK__Players2] FOREIGN KEY([Lead])
REFERENCES [dbo].[Player] ([id])
GO
ALTER TABLE [dbo].[Team] CHECK CONSTRAINT [FK__Players2]
GO
ALTER TABLE [dbo].[Team]  WITH CHECK ADD  CONSTRAINT [FK__Team__leagueid__4D94879B] FOREIGN KEY([Leagueid])
REFERENCES [dbo].[League] ([id])
GO
ALTER TABLE [dbo].[Team] CHECK CONSTRAINT [FK__Team__leagueid__4D94879B]
GO
ALTER TABLE [dbo].[UserLeague]  WITH CHECK ADD  CONSTRAINT [FK_UserLeague_League] FOREIGN KEY([LeagueId])
REFERENCES [dbo].[League] ([id])
GO
ALTER TABLE [dbo].[UserLeague] CHECK CONSTRAINT [FK_UserLeague_League]
GO
ALTER TABLE [dbo].[UserLeague]  WITH CHECK ADD  CONSTRAINT [FK_UserLeague_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[UserLeague] CHECK CONSTRAINT [FK_UserLeague_User]
GO
/****** Object:  StoredProcedure [dbo].[ELMAH_GetErrorsXml]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-------------------------------------------

create PROCEDURE [dbo].[ELMAH_GetErrorsXml]
(
    @Application NVARCHAR(60),
    @PageIndex INT = 0,
    @PageSize INT = 15,
    @TotalCount INT OUTPUT
)
AS 

    SET NOCOUNT ON

    DECLARE @FirstTimeUTC DATETIME
    DECLARE @FirstSequence INT
    DECLARE @StartRow INT
    DECLARE @StartRowIndex INT

    SELECT 
        @TotalCount = COUNT(1) 
    FROM 
        [ELMAH_Error]
    WHERE 
        [Application] = @Application

    -- Get the ID of the first error for the requested page

    SET @StartRowIndex = @PageIndex * @PageSize + 1

    IF @StartRowIndex <= @TotalCount
    BEGIN

        SET ROWCOUNT @StartRowIndex

        SELECT  
            @FirstTimeUTC = [TimeUtc],
            @FirstSequence = [Sequence]
        FROM 
            [ELMAH_Error]
        WHERE   
            [Application] = @Application
        ORDER BY 
            [TimeUtc] DESC, 
            [Sequence] DESC

    END
    ELSE
    BEGIN

        SET @PageSize = 0

    END

    -- Now set the row count to the requested page size and get
    -- all records below it for the pertaining application.

    SET ROWCOUNT @PageSize

    SELECT 
        errorId     = [ErrorId], 
        application = [Application],
        host        = [Host], 
        type        = [Type],
        source      = [Source],
        message     = [Message],
        [user]      = [User],
        statusCode  = [StatusCode], 
        time        = CONVERT(VARCHAR(50), [TimeUtc], 126) + 'Z'
    FROM 
        [ELMAH_Error] error
    WHERE
        [Application] = @Application
    AND
        [TimeUtc] <= @FirstTimeUTC
    AND 
        [Sequence] <= @FirstSequence
    ORDER BY
        [TimeUtc] DESC, 
        [Sequence] DESC
    FOR
        XML AUTO
GO
/****** Object:  StoredProcedure [dbo].[ELMAH_GetErrorXml]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-----------------------------------------------------

Create PROCEDURE [dbo].[ELMAH_GetErrorXml]
(
    @Application NVARCHAR(60),
    @ErrorId UNIQUEIDENTIFIER
)
AS

    SET NOCOUNT ON

    SELECT 
        [AllXml]
    FROM 
        [ELMAH_Error]
    WHERE
        [ErrorId] = @ErrorId
    AND
        [Application] = @Application
GO
/****** Object:  StoredProcedure [dbo].[ELMAH_LogError]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create PROCEDURE [dbo].[ELMAH_LogError]
(
    @ErrorId UNIQUEIDENTIFIER,
    @Application NVARCHAR(60),
    @Host NVARCHAR(30),
    @Type NVARCHAR(100),
    @Source NVARCHAR(60),
    @Message NVARCHAR(500),
    @User NVARCHAR(50),
    @AllXml NTEXT,
    @StatusCode INT,
    @TimeUtc DATETIME
)
AS

    SET NOCOUNT ON

    INSERT
    INTO
        [ELMAH_Error]
        (
            [ErrorId],
            [Application],
            [Host],
            [Type],
            [Source],
            [Message],
            [User],
            [AllXml],
            [StatusCode],
            [TimeUtc]
        )
    VALUES
        (
            @ErrorId,
            @Application,
            @Host,
            @Type,
            @Source,
            @Message,
            @User,
            @AllXml,
            @StatusCode,
            @TimeUtc
        )

GO
/****** Object:  StoredProcedure [dbo].[GetMatchAll]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[GetMatchAll]
@WeekId int
as
select L.* from (
select S.ID, M1.NickName as Skip1, m5.NickName as Vice1, M2.NickName as Lead1,  M3.NickName as Skip2, m6.NickName as Vice2,M4.NickName as Lead2, T1.TeamNo as Team1, T2.TeamNo as Team2, S.WeekDate as Date, M.Rink
from [dbo].[Match] M
inner join [dbo].[Schedule] S on S.Id = M.WeekId
inner join [dbo].[Team] T1 on M.TeamNo1=T1.id
inner join [dbo].[Team] T2 on M.TeamNo2=T2.id
left outer join [dbo].[Player] P1 on p1.id=t1.[Skip]
left outer join [dbo].[Player] P2 on p2.id=t1.Lead
left outer join [dbo].[Player] P3 on p3.id=t2.[Skip]
left outer join [dbo].[Player] P4 on p4.id=t2.Lead
left outer join [dbo].[Player] P5 on p5.id=t1.ViceSkip
left outer join [dbo].[Player] P6 on p6.id=t2.ViceSkip

left outer join [dbo].[Membership] M1 on p1.membershipid = m1.id
left outer join [dbo].[Membership] M2 on p2.membershipid = m2.id
left outer join [dbo].[Membership] M3 on p3.membershipid = m3.id
left outer join [dbo].[Membership] M4 on p4.membershipid = m4.id
left outer join [dbo].[Membership] M5 on p5.membershipid = m5.id
left outer join [dbo].[Membership] M6 on p6.membershipid = m6.id

union

select S.ID, M3.NickName as Skip1, m6.NickName as Vice1, M4.NickName as Lead1,  M1.NickName as Skip2, m5.NickName as Vice2, M2.NickName as Lead2, T2.TeamNo as Team1, T1.TeamNo as Team2, S.WeekDate as Date, M.Rink
from [dbo].[Match] M
inner join [dbo].[Schedule] S on S.Id = M.WeekId
inner join [dbo].[Team] T1 on M.TeamNo1=T1.id
inner join [dbo].[Team] T2 on M.TeamNo2=T2.id
left outer join [dbo].[Player] P1 on p1.id=t1.[Skip]
left outer join [dbo].[Player] P2 on p2.id=t1.Lead
left outer join [dbo].[Player] P3 on p3.id=t2.[Skip]
left outer join [dbo].[Player] P4 on p4.id=t2.Lead
left outer join [dbo].[Player] P5 on p5.id=t1.ViceSkip
left outer join [dbo].[Player] P6 on p6.id=t2.ViceSkip

left outer join [dbo].[Membership] M1 on p1.membershipid = m1.id
left outer join [dbo].[Membership] M2 on p2.membershipid = m2.id
left outer join [dbo].[Membership] M3 on p3.membershipid = m3.id
left outer join [dbo].[Membership] M4 on p4.membershipid = m4.id
left outer join [dbo].[Membership] M5 on p5.membershipid = m5.id
left outer join [dbo].[Membership] M6 on p6.membershipid = m6.id
) L
where l.id = @WeekId and l.Rink <> -1
order by l.Rink

-- exec [GetMatchAll] 1
GO
/****** Object:  StoredProcedure [dbo].[LeagueAllowDelete]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Script for SelectTopNRows command from SSMS  ******/
CREATE procedure [dbo].[LeagueAllowDelete]
as
SELECT L.[id]
      ,L.[LeagueName]
      ,L.[Active]
      ,L.[TeamSize]
      ,L.[rowversion]
      ,L.[TiesAllowed]
      ,L.[PointsCount]
      ,L.[WinPoints]
      ,L.[TiePoints]
      ,L.[ByePoints]
      ,L.[StartWeek]
	  ,S1.cnt
  FROM [dbo].[League] L
  outer apply (select count(*) as cnt from schedule S
  left outer join League l1 on l1.id = S.Leagueid where l1.id  = l.id) S1
  order by LeagueName
GO
/****** Object:  StoredProcedure [dbo].[MembershipAllowDelete]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Script for SelectTopNRows command from SSMS  ******/
CREATE procedure [dbo].[MembershipAllowDelete]
as
SELECT  [id]
      ,[FirstName]
      ,[LastName]
      ,[FullName]
      ,[shortname]
      ,[NickName]
      ,[Wheelchair]
	  ,P1.cnt
  FROM [dbo].[Membership] M
  outer apply (select count(*) as cnt from player p 
  left outer join membership m1 on m1.id=p.[MembershipId]
  where m1.id = m.id) P1
GO
/****** Object:  StoredProcedure [dbo].[PlayerAllowDelete]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Script for SelectTopNRows command from SSMS  ******/
CREATE procedure [dbo].[PlayerAllowDelete]
@leagueid int
as
SELECT P.[id]
	  ,T1.cnt
	  ,M.FullName
	  ,M.LastName
	  ,M.FirstName
  FROM [dbo].[Player] P
  outer apply (select count(*) as cnt  from team t 
  left outer join player P1 on p1.id = t.[skip] or p1.id = t.ViceSkip or p1.id = t.[Lead]
  where p.id = p1.id and t.Leagueid=@leagueid) T1
  left outer join membership m on m.id = p.MembershipId
  where Leagueid=@leagueid
GO
/****** Object:  StoredProcedure [dbo].[ScheduleAllowDelete]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[ScheduleAllowDelete]
@leagueid int
as
	SELECT S.id
	 ,[GameDate]
      ,[Cancelled]
      ,[WeekDate]
	  ,m1.cnt
  FROM [dbo].[Schedule] S
  outer apply (select count(*) as cnt from match m 
	left outer join [Schedule] S1 on S1.id = m.WeekId
	where S1.id = S.id and S1.Leagueid=@leagueid) m1
	where s.Leagueid = @leagueid
	order by [GameDate]
GO
/****** Object:  StoredProcedure [dbo].[TeamAllowDelete]    Script Date: 1/7/2020 11:50:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[TeamAllowDelete]
@leagueid int
as
SELECT T.[id]
      ,m1.fullname as [skip]
      ,m2.fullname as [ViceSkip]
      ,m3.Fullname as [Lead]
      ,T.[Leagueid]
      ,[TeamNo]
	  ,m5.cnt
  FROM [dbo].[Team] t
  outer apply (select count(*) as cnt from match m 
	left outer join team t1 on t1.id = m.TeamNo1 or t1.id = m.TeamNo2
	where t1.id = t.id and t1.Leagueid=@leagueid) m5
  left outer join player p1 on p1.id = t.[skip]
  left outer join player p2 on p2.id = t.ViceSkip
  left outer join player p3 on p3.id = t.[lead]
  left outer join membership m1 on m1.id = p1.MembershipId
  left outer join membership m2 on m2.id = p2.MembershipId
  left outer join membership m3 on m2.id = p3.MembershipId
  where t.Leagueid=@leagueid
  order by teamno
GO
USE [master]
GO
ALTER DATABASE [Tournament] SET  READ_WRITE 
GO
