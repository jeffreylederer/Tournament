alter table [dbo].[Schedule]
  add [PlayOffs]   BIT  CONSTRAINT [DF_Schedule_PlayGame] DEFAULT ((0)) NOT NULL
  go


alter table [dbo].[Team]
add [DivisionId] SMALLINT   CONSTRAINT [DF_Team_Division] DEFAULT ((1)) NOT NULL
GO

alter table [dbo].[League]
add	[Divisions]   SMALLINT     CONSTRAINT [DF_League_Divisions] DEFAULT ((1)) NOT NULL,
    [PlayOffs]    BIT          CONSTRAINT [DF_League_PlayOffs] DEFAULT ((0)) NOT NULL

go

ALTER procedure [dbo].[ScheduleAllowDelete]
@leagueid int
as
	SELECT S.id
	 ,[GameDate]
      ,[Cancelled]
      ,[WeekDate]
	  ,PlayOffs
	  ,m1.cnt
  FROM [dbo].[Schedule] S
  outer apply (select count(*) as cnt from match m 
	left outer join [Schedule] S1 on S1.id = m.WeekId
	where S1.id = S.id and S1.Leagueid=@leagueid) m1
	where s.Leagueid = @leagueid
	order by [GameDate]

	go


ALTER procedure [dbo].[TeamAllowDelete]
@leagueid int
as
SELECT T.[id]
      ,m1.fullname as [skip]
      ,m2.fullname as [ViceSkip]
      ,m3.Fullname as [Lead]
      ,T.[Leagueid]
	  ,t.DivisionId as Division
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
  left outer join membership m3 on m3.id = p3.MembershipId
  where t.Leagueid=@leagueid
  order by teamno
  go

ALTER procedure [dbo].[LeagueAllowDelete]
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
	  ,L.PointsLimit
	  ,L.Divisions
	  ,L.PlayOffs
	  ,S1.cnt
  FROM [dbo].[League] L
  outer apply (select count(*) as cnt from schedule S
  left outer join League l1 on l1.id = S.Leagueid where l1.id  = l.id) S1
  order by LeagueName
  go
  