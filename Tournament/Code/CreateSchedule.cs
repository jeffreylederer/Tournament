using System.Collections.Generic;
using System.IO;
using System.Web;



namespace Tournament.Code
{
    public class CreateSchedule
    {
        private const int BYE = -1;

        public List<CalculatedMatch> NoByes(int numberofWeeks, int numberOfTeams)
        {
            var numberOfRinks = numberOfTeams / 2;
            var matches = new List<CalculatedMatch>();

            int[] leftside = new int[numberOfRinks];
            int[] rightside = new int[numberOfRinks];
            
            for (int r = 0; r < numberOfRinks; r++)
            {
                leftside[r] = r;
                rightside[r] = numberOfTeams - r - 1;
                matches.Add(new CalculatedMatch()
                {
                    Week = 0,
                    Rink = r,
                    Team1 = r,
                    Team2 = numberOfTeams - r - 1
                });
            }

            for (int w = 1; w < numberofWeeks; w++)
            {
                int remainder = ShiftRight(leftside);
                int other = ShiftLeft(rightside, remainder);
                leftside[1] = other;
                for (int r = 0; r < numberOfRinks; r++)
                {
                    var left = leftside[r];
                    var right = rightside[r];
                    matches.Add(new CalculatedMatch()
                    {
                        Week = w,
                        Rink = (r + w*2) % numberOfRinks,
                        Team1 = left < right? left: right,
                        Team2 = left < right ? right : left
                    });
                }
            }
            return matches;
        }

        public List<CalculatedMatch> Byes(int numberofWeeks, int numberOfTeams)
        {
            var teamCount = numberOfTeams + numberOfTeams % 2;
            var numberOfRinks = teamCount / 2;
            var matches = new List<CalculatedMatch>();

            int[] leftside = new int[numberOfRinks];
            int[] rightside = new int[numberOfRinks];

            leftside[0] = 0;
            rightside[0] = teamCount - 1;

            // do first week bye
            matches.Add(new CalculatedMatch()
            {
                Week = 0,
                Rink = -1,
                Team1 = 0,
                Team2 = 0
            });


            // do first week
            for (int r = 1; r < numberOfRinks; r++)
            {
                leftside[r] = r;
                rightside[r] = teamCount - r - 1;
                matches.Add(new CalculatedMatch()
                {
                    Week = 0,
                    Rink = r - 1,
                    Team1 = r,
                    Team2 = teamCount - r - 1
                });
            }

            for (int w = 1; w < numberofWeeks; w++)
            {
                int remainder = ShiftRight(leftside);
                int other = ShiftLeft(rightside, remainder);
                leftside[1] = other;
                int rink = 0;
                for (int r = 0; r < numberOfRinks; r++)
                {
                    var left = leftside[r];
                    var right = rightside[r];
                    var match = new CalculatedMatch()
                    {
                        Week = w,
                        Rink = r,
                        Team1 = left < right ? left : right,
                        Team2 = left < right ? right : left
                    };
                    if (match.Team2 == teamCount - 1)
                    {
                        match.Team2 = match.Team1;
                        match.Rink = -1;
                        System.Diagnostics.Trace.WriteLine($"Bye {match.Team1}");
                    }
                    else
                    {
                        match.Rink = (rink + 2 * w) % (numberOfRinks - 1);
                        rink++;
                    }
                    System.Diagnostics.Trace.WriteLine($"Rink {match.Rink}, T1={match.Team1}, T2={match.Team2}");
                    matches.Add(match);
                }
            }
            matches.Sort((a, b) => (a.Week * 100 + a.Rink + 1).CompareTo(b.Week * 100 + b.Rink + 1));
            return matches;
        }

        private int ShiftRight(int[] leftside)
        {
            int remainder = leftside[leftside.Length - 1];
            for (int i = leftside.Length-2; i > 0; i--)
            {
                leftside[i+1] = leftside[i];
            }
            return remainder;
        }

        private int ShiftLeft(int[] rightside, int remainder)
        {
            int other = rightside[0];
            for (int i = 0; i < rightside.Length - 1; i++)
            {
                rightside[i] = rightside[i+1];
            }
            rightside[rightside.Length - 1] = remainder;
            return other;
        }

        public static List<CalculatedMatch> ReadMatches(Stream fs)
        {
            var matches = new List<CalculatedMatch>();
            
            using (var reader = new StreamReader(fs))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var items = line.Split(new char[] { ',' });
                    var match = new CalculatedMatch()
                    {
                        Week = int.Parse(items[0])-1,
                        Rink = int.Parse(items[1])-1,
                        Team1 = int.Parse(items[2])-1,
                        Team2 = int.Parse(items[3])-1
                    };
                    matches.Add(match);
                }
            }
            return matches;
        }
    }

    public class CalculatedMatch
    {
        public int Week;
        public int Rink;
        public int Team1;
        public int Team2;

    }
}