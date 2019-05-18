using Leagues.Models;
using Leagues.Reports;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;


namespace Leagues.Code
{
    public static  class GenerateSchedule
    {
        public static string Tuesday()
        {
            var topLine = new StringBuilder();
            using (var db = new LeaguesEntities())
            {
                var tline = new StringBuilder();
                var teams = db.TuesdayTeams.Count();
                
                var rinks = teams / 2;

                tline.Append("WK,Date,GRN,DIR,Bound,");
                for (var rink =0;rink<rinks;rink++)
                {
                    tline.Append($"{rink},");
                }

                var tline1= tline.ToString();
                topLine.Append(tline1.Substring(0, tline1.Length - 1) + "\n");
                
                var weeks = db.TuesdaySchedules.SortBy("Id");
                foreach (var week in weeks)
                {
                    var matches = db.TuesdayMatches.Where(x => x.GameDate == week.id).OrderBy(x => x.Rink);
                    var weekLine = new StringBuilder();
                    string grn=string.Empty;
                    string dir = string.Empty;
                    string bound= string.Empty;
                    switch ((week.id-1) % 9)
                    {
                        case 0:
                            grn = "Luba";
                            dir = "N-S";
                            bound = "Red";
                            break;
                        case 1:
                            grn = "Phillips";
                            dir = "E-W";
                            bound = "White";
                            break;
                        case 2:
                            grn = "Luba";
                            dir = "E-W";
                            bound = "Yellow";
                            break;
                        case 3:
                            grn = "Phillips";
                            dir = "N-S";
                            bound = "Red";
                            break;
                        case 4:
                            grn = "Luba";
                            dir = "N-S";
                            bound = "White";
                            break;
                        case 5:
                            grn = "Phillips";
                            dir = "E-W";
                            bound = "Yellow";
                            break;
                        case 6:
                            grn = "Luba";
                            dir = "E-W";
                            bound = "Red";
                            break;
                        case 7:
                            grn = "Phillips";
                            dir = "N-S";
                            bound = "White";
                            break;
                        case 8:
                            grn = "Luba";
                            dir = "N-S";
                            bound = "Yellow";
                            break;
                    }
                    var date = $"{week.GameDate.Month}/{week.GameDate.Day}";
                    weekLine.Append($"{week.id},{date},{grn},{dir},{bound},");
                    foreach (var match in matches)
                    {
                        if(match.Rink != -1)
                            weekLine.Append($"{match.Team1}-{match.Team2},");
                    }
                    var wline = weekLine.ToString();
                    
                    topLine.Append(wline.Substring(0, wline.Length-1)+"\n");
                }
            }
            return topLine.ToString();
        }

        public static string Wednesday()
        {
            var topLine = new StringBuilder();
            using (var db = new LeaguesEntities())
            {
                var teams = db.WednesdayTeams.Count();
              
                var rinks = teams / 2;

                var tline = new StringBuilder();
                tline.Append("WK,Date,GRN,DIR,Bound,");
                for (var rink = 0; rink < rinks; rink++)
                {
                    tline.Append($"{rink},");
                }

                var tline1 = tline.ToString();
                topLine.Append(tline1.Substring(0, tline1.Length - 1) + "\n");


                var weeks = db.WednesdaySchedules.SortBy("Id");
                foreach (var week in weeks)
                {
                    var matches = db.WednesdayMatches.Where(x => x.GameDate == week.id).OrderBy(x => x.Rink);
                    var weekLine = new StringBuilder();
                    string grn = string.Empty;
                    string dir = string.Empty;
                    string bound = string.Empty;
                    switch ((week.id - 1) % 9)
                    {
                        case 1:
                            grn = "Luba";
                            dir = "N-S";
                            bound = "Red";
                            break;
                        case 2:
                            grn = "Phillips";
                            dir = "E-W";
                            bound = "White";
                            break;
                        case 3:
                            grn = "Luba";
                            dir = "E-W";
                            bound = "Yellow";
                            break;
                        case 4:
                            grn = "Phillips";
                            dir = "N-S";
                            bound = "Red";
                            break;
                        case 5:
                            grn = "Luba";
                            dir = "N-S";
                            bound = "White";
                            break;
                        case 6:
                            grn = "Phillips";
                            dir = "E-W";
                            bound = "Yellow";
                            break;
                        case 7:
                            grn = "Luba";
                            dir = "E-W";
                            bound = "Red";
                            break;
                        case 8:
                            grn = "Phillips";
                            dir = "N-S";
                            bound = "White";
                            break;
                        case 0:
                            grn = "Luba";
                            dir = "N-S";
                            bound = "Yellow";
                            break;
                    }
                    var date = $"{week.GameDate.Month}/{week.GameDate.Day}";
                    weekLine.Append($"{week.id},{date},{grn},{dir},{bound},");
                    foreach (var match in matches)
                    {
                        if (match.Rink != -1)
                            weekLine.Append($"{match.Team1}-{match.Team2},");
                    }
                    var wline = weekLine.ToString();
                    
                    topLine.Append(wline.Substring(0, wline.Length - 1) + "\n");
                }
            }
            return topLine.ToString();
        }

    }
}