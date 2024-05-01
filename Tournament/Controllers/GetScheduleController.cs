using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using Tournament.Models;
using Tournament.ReportFiles;

namespace Tournament.Controllers
{

    // GET: GetSchedule
    public class GetScheduleController : Controller
    {
        private readonly TournamentEntities _db = new TournamentEntities();

        /// <summary>
        /// Gets the object to be serialized to XML.
        /// </summary>
        public ActionResult GenerateReport(int id)
        {
            var topLine = new StringBuilder();

           
            
            var league = _db.Leagues.Find(id);
            if (league == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            var startWeek = league.StartWeek;
            using (var db = new TournamentEntities())
            {
                topLine.AppendLine("<table class='table table-striped table-sm'>");

                var teams = db.Teams.Where(x => x.Leagueid == id).OrderBy(x => x.TeamNo).ToList();


                var rinks = teams.Count / 2;
                topLine.AppendLine("<thead class='thead dark'>");
                topLine.AppendLine("<th>WK</th>");
                topLine.AppendLine("<th>Date</th>");
                //topLine.AppendLine("<th>GRN</th>");
                //topLine.AppendLine("<th>DIR</th>");
                //topLine.AppendLine("<th>Bound</th>");
                if (rinks*2 < teams.Count)
                    topLine.AppendLine("<th>Bye</th>");

                for (var rink = 0; rink < rinks; rink++)
                {
                    topLine.AppendLine($"<th align='center'>{rink + 1}</th>");
                }
                topLine.AppendLine("</thead>");

                var rinkList = db.RinkOrders.OrderBy(x => x.id).ToList();
                


                var weeks = db.Schedules.Where(x => x.Leagueid == id).OrderBy(x => x.GameDate);

                int i = startWeek;
                int weekNumber = 1;
                foreach (Schedule week in weeks)
                {
                    topLine.AppendLine("<tr>");
                    var matches = db.Matches.Where(x => x.WeekId == week.id).OrderBy(x => x.Rink).ToList();
                    var matchesByes = db.Matches.Where(x => x.Rink == -1 && x.WeekId == week.id).ToList();


                    int index = ((i - 1) % rinkList.Count) + 1;
                    var rinklist = rinkList.Find(x => x.id == index);
                    i++;
                    topLine.AppendLine($"<td>{weekNumber++}</td>");
                    topLine.AppendLine($"<td>{week.GameDate.Month}/{week.GameDate.Day}</td>");
                    //topLine.AppendLine($"<td>{rinklist.Green}</td>");
                    //topLine.AppendLine($"<td>{rinklist.Direction}</td>");
                    //topLine.AppendLine($"<td>{rinklist.Boundary}</td>");
                    if (matchesByes.Any())
                        topLine.AppendLine($"<td>{matchesByes.First().Team.TeamNo}</td>");
                    foreach (var match in matches)
                    {
                        if (match.Rink != -1)
                            topLine.AppendLine($"<td>{match.Team.TeamNo}-{match.Team1.TeamNo}</td>");

                    }
                    topLine.AppendLine("</tr>");
                }
                topLine.AppendLine("</table>");
                ViewBag.Report = topLine.ToString();
            }
            ViewBag.id = id;
            return View();
        }

        public ActionResult ExportToExcel(int id)
        {
            DataGrid dgGrid = new DataGrid();
            var league = _db.Leagues.Find(id);
            if (league == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            string filename = $"{league.LeagueName}.xls";
            var startWeek = league.StartWeek;
            using (var db = new TournamentEntities())
            {
                

                var teams = db.Teams.Where(x => x.Leagueid == id).OrderBy(x => x.TeamNo).ToList();

                DataTable dt = new DataTable();
                var rinks = teams.Count / 2;
                dt.Columns.Add("WK", typeof(int));
                dt.Columns.Add("Date", typeof(string));
                //dt.Columns.Add("GRN", typeof(string));
                //dt.Columns.Add("DIR", typeof(string));
                //dt.Columns.Add("Bound", typeof(string));
                if (rinks * 2 < teams.Count)
                    dt.Columns.Add("Bye", typeof(int));

                for (var rink = 0; rink < rinks; rink++)
                {
                    dt.Columns.Add($"Rink {rink+1}", typeof(string));
                }

                var rinkList = db.RinkOrders.OrderBy(x => x.id).ToList();



                var weeks = db.Schedules.Where(x => x.Leagueid == id).OrderBy(x => x.GameDate);

                int i = startWeek;
                int weekNumber = 1;
                foreach (Schedule week in weeks)
                {
                    DataRow gridRow = dt.NewRow();
                    var matches = db.Matches.Where(x => x.WeekId == week.id).OrderBy(x => x.Rink).ToList();
                    var matchesByes = db.Matches.Where(x => x.Rink == -1 && x.WeekId == week.id).ToList();


                    int index = ((i - 1) % rinkList.Count) + 1;
                    var rinklist = rinkList.Find(x => x.id == index);
                    i++;
                    gridRow["WK"] = $"{weekNumber++}";
                    gridRow["Date"] = $"{week.GameDate.Month}/{week.GameDate.Day}";
                    //gridRow["GRN"] = $"{rinklist.Green}";
                    //gridRow["DIR"] = $"{rinklist.Direction}";
                    //gridRow["Bound"] = $"{rinklist.Boundary}";

                    if (matchesByes.Any())
                        gridRow["Bye"] = $"{matchesByes.First().Team.TeamNo}";
                    foreach (var match in matches)
                    {
                        if (match.Rink != -1)
                            gridRow[$"Rink {match.Rink}"] = $"{match.Team.TeamNo} vs {match.Team1.TeamNo}";

                    }
                    dt.Rows.Add(gridRow);
                }
                dgGrid.DataSource = dt;
                dgGrid.DataBind();
            }


            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            dgGrid.RenderControl(htw);

            //Write the HTML back to the browser.
            //Response.ContentType = application/vnd.ms-excel;
            Response.ClearContent();
            Response.Charset = "";
            Response.Buffer = true;
            Response.ContentType = "application/vnd.ms-excel";
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + filename);
            Response.Charset = "";
            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();
            return RedirectToAction("GenerateReport", new {id=id});
        }

        [Authorize]
        public ActionResult ScheduleReport(int id)
        {
            ViewBag.Id = id;
            var league = _db.Leagues.Find(id);
            if (league == null)
                return HttpNotFound();
            var weeks = _db.Schedules.Where(x => x.Leagueid == id).OrderBy(x => x.GameDate).ToList();
            var table = new TournamentDS.ScheduleDataTable();
            int weekNumber = 1;
            int rinks = 0;

            foreach (Schedule week in weeks)
            {
                if (!week.PlayOffs)
                {
                    var matches = _db.Matches.Where(x => x.WeekId == week.id).OrderBy(x => x.Rink).ToList();
                    var matchesByes = _db.Matches.Where(x => x.Rink == -1 && x.WeekId == week.id).ToList();
                    object[] row = new object[13];
                    row[0] = weekNumber++;
                    row[1] = $"{week.GameDate.Month}/{week.GameDate.Day}";
                    if (matchesByes.Any())
                        row[2] = matchesByes.First().Team.TeamNo.ToString();
                    int rinkNumber = 3;
                    foreach (var match in matches)
                    {
                        if (match.Rink != -1)
                        {
                            row[rinkNumber++] = $"{match.Team.TeamNo}-{match.Team1.TeamNo}";
                            rinks = Math.Max(rinks, match.Rink);
                        }

                    }
                    table.Rows.Add(row);
                }
                else
                {
                    object[] row = new object[13];
                    row[0] = weekNumber++;
                    row[1] = $"{week.GameDate.Month}/{week.GameDate.Day}";
                    for (int j = 0; j < rinks; j++)
                    {
                        row[j + 3] = "PO";
                    }
                    table.Rows.Add(row);
                }       


            }


        
            var reportViewer = new ReportViewer()
            {
                ProcessingMode = ProcessingMode.Local,
                Width = Unit.Pixel(800),
                Height = Unit.Pixel(1000),
                ShowExportControls = true
            };
            

            reportViewer.LocalReport.ReportPath = Server.MapPath("/ReportFiles/Schedule.rdlc");
           

            var teams = _db.Teams.Where(x => x.Leagueid == id).OrderBy(x=>x.TeamNo);
            var ds = new TournamentDS();
            foreach (var team in teams)
            {
                ds.Team.AddTeamRow(team.TeamNo, team.Player.Membership.FullName, team.Lead == null ? "" : team.Player2.Membership.FullName, team.ViceSkip == null ? "" : team.Player1.Membership.FullName, team.DivisionId);
            }
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Team", ds.Team.Rows));
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Schedule", table.Rows));
            var p1 = new ReportParameter("TeamSize", league.TeamSize.ToString());
            var p2 = new ReportParameter("Description", league.LeagueName);
            var p3 = new ReportParameter("Rinks", (teams.Count() / 2).ToString());
            var p4 = new ReportParameter("Divisions", league.Divisions.ToString());
            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3,p4 });
            ViewBag.ReportViewer = reportViewer;
            return View();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

    }

}