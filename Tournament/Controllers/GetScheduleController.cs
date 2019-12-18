using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Tournament.Models;

namespace Tournament.Controllers
{

    // GET: GetSchedule
    public class GetScheduleController : Controller
    {
        /// <summary>
        /// Gets the object to be serialized to XML.
        /// </summary>
        public FileResult ExportToExcel()
        {
            var topLine = new StringBuilder();
            var leagueid = (int)HttpContext.Session["leagueid"];
            var teamsize = (int) HttpContext.Session["teamsize"];
            using (var db = new TournamentEntities())
            {
                var tline = new StringBuilder();
                var teams = db.Teams.Where(x => x.Leagueid == leagueid).OrderBy(x=>x.TeamNo).ToList();
                

                var rinks = teams.Count / 2;

                tline.Append("WK,Date,GRN,DIR,Bound,");
                for (var rink = 0; rink < rinks; rink++)
                {
                    tline.Append($"{rink + 1},");
                }

                var RinkList = new List<Rink>()
                {
                    new Rink()
                    {
                        Green = "Luba",
                        Direction = "E-W",
                        Boundary = "Red"
                    },
                    new Rink()
                    {
                        Green = "Phillips",
                        Direction = "N-S",
                        Boundary = "Red"
                    },
                    new Rink()
                    {
                        Green = "Luba",
                        Direction = "N-S",
                        Boundary = "White"
                    },
                    new Rink()
                    {
                        Green = "Phillips",
                        Direction = "E-W",
                        Boundary = "White"
                    },
                    new Rink()
                    {
                        Green = "Luba",
                        Direction = "N-S",
                        Boundary = "Yellow"
                    },
                    new Rink()
                    {
                        Green = "Phillips",
                        Direction = "E-W",
                        Boundary = "Yellow"
                    },

                    new Rink()
                    {
                        Green = "Luba",
                        Direction = "N-S",
                        Boundary = "Red"
                    },
                    new Rink()
                    {
                        Green = "Phillips",
                        Direction = "N-S",
                        Boundary = "White"
                    },
                    new Rink()
                    {
                        Green = "Luba",
                        Direction = "E-W",
                        Boundary = "Yellow"
                    },
                    new Rink()
                    {
                        Green = "Phillips",
                        Direction = "N-S",
                        Boundary = "Yellow"
                    }
                };


                var tline1 = tline.ToString();
                topLine.Append(tline1.Substring(0, tline1.Length - 1) + "\n");

                var weeks = db.Schedules.Where(x => x.Leagueid == leagueid).OrderBy(x => x.WeekNumber);

                int i = teamsize - 1;
                 foreach (Schedule week in weeks)
                {
                    var matches = db.Matches.Where(x => x.WeekId == week.id).OrderBy(x => x.Rink);
                    var weekLine = new StringBuilder();
                    string grn = string.Empty;
                    string dir = string.Empty;
                    string bound = string.Empty;
                    int index = i % RinkList.Count();
                    i++;
                    var rinklist = RinkList[index];
                    var date = $"'{week.GameDate.Month}/{week.GameDate.Day}'";
                    weekLine.Append($"{week.WeekNumber},{date},{rinklist.Green},{rinklist.Direction},{rinklist.Boundary},");
                    foreach (var match in matches)
                    {
                        if (match.Rink != -1)
                            weekLine.Append($"'{match.Team.TeamNo}-{match.Team1.TeamNo}',");
                    }
                    var wline = weekLine.ToString();

                    topLine.Append(wline.Substring(0, wline.Length - 1) + "\n");
                }
            }
            byte[] bytes = Encoding.ASCII.GetBytes(topLine.ToString());
            return File(bytes, "application/csv", "Grid.csv");
       }
    }

    internal class Rink
    {
        public string Green { get; set; }
        public string Direction { get; set; }
        public string Boundary { get; set; }
    }
}