﻿using System;
using System.Collections.Generic;
using System.IO;



namespace RoundRobin
{
    /// <summary>
    /// Console program to work out the algorithm for two division league
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Out.Write("Number of Teams >");
            var val = Console.In.ReadLine();
            int numberOfTeams = int.Parse(val);
            Console.Out.Write("Number of Weeks >");
            val = Console.In.ReadLine();
            int weeks = int.Parse(val);
            Console.Out.Write("Directory to write file >");
            var path = Console.In.ReadLine();
            var list = new List<CalculatedMatch>();
            var list1 = new List<CalculatedMatch>();
            if (numberOfTeams % 4 == 0)
            {
                list = NoByes(numberOfTeams / 2);
            }
            else
            {
                list = Byes(numberOfTeams / 2);
             }

           
            // create matches for teams in division 2 using matches in division 1
            var numberOfRinks = numberOfTeams / 4;
            foreach (var match in list)
            {
                if (match.Rink > -1)
                { 
                    var match1 = new CalculatedMatch()
                    {
                        Week = match.Week,
                        Rink = match.Rink + numberOfRinks,
                        Team1 = match.Team1 + numberOfTeams / 2,
                        Team2 = match.Team2 + numberOfTeams / 2
                    };
                    list1.Add(match1);
                }
                
            }

            //file in the rest of the schedule with inter divisional matches
            var newList = list.FindAll(x => x.Rink == -1);
            foreach (var item in newList)
            {
                int index = list.IndexOf(item);
                var item1 = list[index];
                item1.Rink = numberOfTeams / 2 - 1;
                item1.Team2 += numberOfTeams / 2;
                list[index] = item1;
            }
            for (int w = numberOfTeams/2-1 ; w < weeks; w++)
            {
                for (int i = 0; i < numberOfTeams / 2; i++)
                {
                    int team2 = (i + w+1);
                    if (team2 >= numberOfTeams)
                        team2 = team2 - numberOfTeams / 2;
                    var match1 = new CalculatedMatch()
                    {
                        Week = w,
                        Rink = i,
                        Team1 = i,
                        Team2 =team2
                    };
                    list1.Add(match1);
                }
            }
            foreach (var item in list1)
                list.Add(item);

            // write out the results to file
            list.Sort((a,b)=>(a.Week*100+a.Rink).CompareTo(b.Week*100+b.Rink));
            using (var stream = new StreamWriter($"{path}\\Division{numberOfTeams}.txt", false))
            {
                TextWriter writer = stream;
                foreach (var item in list)
                {
                    writer.WriteLine($"{item.Week+1},{item.Rink + 1},{item.Team1 + 1},{item.Team2 + 1}");
                }
                
            }
        }

        /// <summary>
        /// This is called when there is an even number of teams in the league. It uses a round robin algoritm to generate the schedule.
        /// </summary>
        /// <param name="numberofWeeks">number of weeks for this league</param>
        /// <param name="numberOfTeams">number of teams in this league</param>
        /// <returns>Generates a list of matches. Each list has a week number, rink number, team 1 number and team 2 number.</returns>
        private static List<CalculatedMatch> NoByes(int numberOfTeams)
        {
            var numberofWeeks = numberOfTeams - 1;
            var numberOfRinks = numberOfTeams / 2;
            var matches = new List<CalculatedMatch>();

            int[] leftside = new int[numberOfRinks];
            int[] rightside = new int[numberOfRinks];

            for (int r = 0; r < numberOfRinks; r++)
            {
                leftside[r] = r;
                rightside[r] = numberOfTeams - r - 1;
                var match = new CalculatedMatch()
                {
                    Week = 0,
                    Rink = r,
                    Team1 = r,
                    Team2 = numberOfTeams - r - 1
                };
                matches.Add(match);
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
                    var match =  new CalculatedMatch()
                    {
                        Week = w,
                        Rink = (r + w * 2) % numberOfRinks,
                        Team1 = left < right ? left : right,
                        Team2 = left < right ? right : left
                    };
                    matches.Add(match);
                   
                }
            }
            
            return matches;
        }

        /// <summary>
        /// This is called when an these is an odd number of teams in the league. It uses a round robin algoritm to generate the schedule.
        /// </summary>
        /// <param name="numberofWeeks">number of weeks for this league</param>
        /// <param name="numberOfTeams">number of teams in this league</param>
        /// <returns>Generates a list of matches. Each list has a week number, rink number, team 1 number and team 2 number. 
        /// One entry per week will have a rink of -1 which is the bye team for the week.
        /// </returns>
        private static List<CalculatedMatch> Byes(int numberOfTeams)
        {
            var teamCount = numberOfTeams + numberOfTeams % 2;
            var numberOfRinks = teamCount / 2;
            var matches = new List<CalculatedMatch>();
            int numberofWeeks = numberOfTeams - 1;

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

        private static int ShiftRight(int[] leftside)
        {
            int remainder = leftside[leftside.Length - 1];
            for (int i = leftside.Length - 2; i > 0; i--)
            {
                leftside[i + 1] = leftside[i];
            }
            return remainder;
        }

        private static int ShiftLeft(int[] rightside, int remainder)
        {
            int other = rightside[0];
            for (int i = 0; i < rightside.Length - 1; i++)
            {
                rightside[i] = rightside[i + 1];
            }
            rightside[rightside.Length - 1] = remainder;
            return other;
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
