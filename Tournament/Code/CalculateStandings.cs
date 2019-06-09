using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Tournament.Models;
using Tournament.ReportFiles;


namespace Tournament.Code
{
    public static class CalculateStandings
    {

        

        public static TournamentDS.StandingDataTable Doit(int weekid, int teamsize, int leagueid)
        {
            var ds = new TournamentDS();
            var list = new List<Standing>();
            using (var db = new TournamentEntities())
            {

                foreach (var team in db.Teams.Where(x=>x.Leagueid== leagueid))
                {
                    string players = "";
                    switch (teamsize)
                    {
                        case 1:
                            players = team.Player.NickName;
                            break;
                        case 2:
                            players = $"{team.Player.NickName}, {team.Player2.NickName}";
                            break;
                        case 3:
                            players = $"{team.Player.NickName}, {team.Player1.NickName}, {team.Player2.NickName}";
                            break;
                    }
                    list.Add(new Standing()
                    {
                        TeamNumber = team.TeamNo,
                        Wins = 0,
                        Loses = 0,
                        TotalScore = 0,
                        Players = players
                    });
                }
                foreach(var week in db.Schedules.Where(x => x.id <= weekid && x.Leagueid == leagueid))
                {
                    if (week.Cancelled)
                        continue;
                    var total = 0;
                    var numMatches = 0;
                    var bye = false;
                    bool forfeit = false;
                    foreach (var match in db.Matches.Where(x => x.RoundId == week.id))
                    {
                        //team 1 wins
                        if (match.Team1Score > match.Team2Score && match.Rink != -1 && match.ForFeitId == 0)
                        {
                            var winner = list.Find(x => x.TeamNumber == match.Team.TeamNo);
                            var loser = list.Find(x => x.TeamNumber == match.Team1.TeamNo);
                            winner.Wins++;
                            loser.Loses++;
                            winner.TotalScore += Math.Min(20, match.Team1Score);
                            loser.TotalScore += Math.Min(20, match.Team2Score);
                            total += Math.Min(20, match.Team1Score);
                            numMatches++;
                        }
                        //team 2 wins
                        else if (match.Rink != -1 && match.ForFeitId == 0)
                        {
                            var winner = list.Find(x => x.TeamNumber == match.Team1.TeamNo);
                            var loser = list.Find(x => x.TeamNumber == match.Team.TeamNo);
                            winner.Wins++;
                            loser.Loses++;
                            winner.TotalScore += Math.Min(20, match.Team2Score);
                            loser.TotalScore += Math.Min(20, match.Team1Score);
                            total += Math.Min(20, match.Team2Score);
                            numMatches++;
                        }
                        // forfeit
                        else if (match.Rink != -1 && match.ForFeitId != 0)
                        {
                            var winner = list.Find(x => x.TeamNumber == (match.Team.TeamNo== match.ForFeitId? match.Team1.TeamNo: match.Team.TeamNo));
                            var loser = list.Find(x => x.TeamNumber == match.ForFeitId);
                            forfeit = true;
                            winner.Wins++;
                            loser.Loses++;
                        }
                        //bye
                        else
                        {
                            var winner = list.Find(x => x.TeamNumber == match.Team.TeamNo);
                            winner.Wins++;
                            bye = true;
                        }

                    }
                    if (bye || forfeit)
                    {

                        foreach (var match in db.Matches.Where(x => x.RoundId == week.id))
                        {
                            if (match.Rink != -1 && match.ForFeitId != 0)
                            {
                                var winner = list.Find(x => x.TeamNumber == (match.Team.TeamNo == match.ForFeitId ? match.Team1.TeamNo : match.Team.TeamNo));
                                winner.TotalScore += total / numMatches;
                            }
                            else if (match.Rink == -1)
                            {
                                var winner = list.Find(x => x.TeamNumber == match.Team.TeamNo);
                                winner.TotalScore += total / numMatches;
                            }
                        }
                    }



                }
            }
            int place = 1;
            list.Sort((a, b) => (b.Wins * 1000 + b.TotalScore).CompareTo(a.Wins * 1000 + a.TotalScore));
            foreach (var item in list)
            {
                ds.Standing.AddStandingRow(item.TeamNumber, item.Players, item.TotalScore, place++, item.Wins, item.Loses);
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