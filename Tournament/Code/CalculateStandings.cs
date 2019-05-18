using System;
using System.Collections.Generic;
using System.Linq;
using Tournament.Models;
using Tournament.ReportFiles;


namespace Tournament.Code
{
    public static class CalculateStandings
    {

        

        public static TournamentDS.StandingDataTable Doit(int weekid)
        {
            var ds = new TournamentDS();
            using (var db = new TournamentEntities())
            {
                var list = new List<Standing>();
                foreach (var team in db.Teams)
                {
                    list.Add(new Standing()
                    {
                        TeamNumber = team.id,
                        Wins = 0,
                        Loses = 0,
                        TotalScore = 0,
                        Players = team.Player.NickName + ", " + team.Player1.NickName + team.Player2.NickName
                    });
                }
                foreach (var week in db.Schedules.Where(x => x.id <= weekid))
                {
                    foreach (var match in db.Matches.Where(x => x.RoundId == week.id))
                    {
                        if (match.Team1Score > match.Team2Score && match.Rink != -1)
                        {
                            var winner = list.Find(x => x.TeamNumber == match.TeamNo1);
                            var loser = list.Find(x => x.TeamNumber == match.TeamNo2);
                            winner.Wins++;
                            loser.Loses++;
                            winner.TotalScore += Math.Min(20, match.Team1Score);

                        }
                        else if (match.Rink != -1)
                        {
                            var winner = list.Find(x => x.TeamNumber == match.TeamNo2);
                            var loser = list.Find(x => x.TeamNumber == match.TeamNo1);
                            winner.Wins++;
                            loser.Loses++;
                            winner.TotalScore += Math.Min(20, match.Team2Score);
                        }
                        else
                        {
                            var winner = list.Find(x => x.TeamNumber == match.TeamNo1);
                            winner.Wins++;
                        }
                    }
                    list.Sort((a, b) => (b.Wins * 1000 + b.TotalScore).CompareTo(a.Wins * 1000 + a.TotalScore));

                    int place = 1;
                    foreach (var item in list)
                    {
                        ds.Standing.AddStandingRow(item.TeamNumber, item.Players, item.TotalScore, place++, item.Wins, item.Loses
                        );
                    }
                }
            }
            return ds.Standing;
        }
    }


    internal class Standing
    {
        public int TeamNumber { get; set; }
        public int Wins { get; set; }
        public int Loses { get; set; }
        public int TotalScore { get; set; }
        public string Players { get; set; }
        
    }
}