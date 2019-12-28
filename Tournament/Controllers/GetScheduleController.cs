using System.Collections.Generic;
using System.Data.Entity;
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
        public ActionResult GenerateReport()
        {
            var topLine = new StringBuilder();

            var leagueid = (int)HttpContext.Session["leagueid"];
            var teamsize = (int) HttpContext.Session["teamsize"];
            using (var db = new TournamentEntities())
            {
                topLine.AppendLine("<table class='table table-striped table-sm'>");

                var teams = db.Teams.Where(x => x.Leagueid == leagueid).OrderBy(x=>x.TeamNo).ToList();
                

                var rinks = teams.Count / 2;
                topLine.AppendLine("<thead class='thead dark'>");
                topLine.AppendLine("<th>WK</th>");
                topLine.AppendLine("<th>Date</th>");
                topLine.AppendLine("<th>GRN</th>");
                topLine.AppendLine("<th>DIR</th>");
                topLine.AppendLine("<th>Bound</th>");
                if (db.Matches.Where(x => x.Rink == -1).Any())
                    topLine.AppendLine("<th>Bye</th>");

                for (var rink = 0; rink < rinks; rink++)
                {
                    topLine.AppendLine($"<th>{rink + 1}</th>");
                }
                topLine.AppendLine("</thead>");

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


                var weeks = db.Schedules.Where(x => x.Leagueid == leagueid).OrderBy(x => x.WeekNumber);

                int i = teamsize - 1;
                 foreach (Schedule week in weeks)
                 {
                    topLine.AppendLine("<tr>");
                    var matches = db.Matches.Where(x => x.WeekId == week.id).OrderBy(x => x.Rink).ToList();
                    var matchesByes = db.Matches.Where(x => x.Rink == -1 && x.WeekId == week.id).ToList();
                     

                    int index = i % RinkList.Count();
                    i++;
                    var rinklist = RinkList[index];
                    
                    topLine.AppendLine($"<td>{week.WeekNumber}</td>");
                    topLine.AppendLine($"<td>'{week.GameDate.Month}/{week.GameDate.Day}'</td>");
                    topLine.AppendLine($"<td>{rinklist.Green}</td>");
                    topLine.AppendLine($"<td>{rinklist.Direction}</td>");
                    topLine.AppendLine($"<td>{rinklist.Boundary}</td>");
                     if (matchesByes.Any())
                         topLine.AppendLine($"<td>{matchesByes.First().Team.TeamNo}</td>");
                    foreach (var match in matches)
                    {
                        if (match.Rink != -1)
                            topLine.AppendLine($"<td>'{match.Team.TeamNo}-{match.Team1.TeamNo}'</td>");
                        
                    }
                     topLine.AppendLine("</tr>");
                }
                topLine.AppendLine("</table>");
                ViewBag.Report = topLine.ToString();
            }
            return View();
        }


    }

    internal class Rink
    {
        public string Green { get; set; }
        public string Direction { get; set; }
        public string Boundary { get; set; }
    }
}