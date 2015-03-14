using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BGGStats.Helper;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BGGStats.Model
{
    class Plays
    {

        public const string ALL_YEARS = "All";
        private static int id = 0;

        public enum DateRange
        {
            Month,
            Year
        }


        public string CurrentPlayerUsername { get; set; }
        public string CurrentPlayerNickname { get; set; }
        public ObservableCollection<Play> AllPlays { get; private set; }

        public ObservableCollection<Play> AllPlaysByYear { get; set; }

        public ObservableCollection<string> Years { get; private set; }
        public int TotalPlays { get; private set; }

        public ObservableCollection<KeyValuePair<string, int>> LocationCounts { get; set; }
        public ObservableCollection<KeyValuePair<string, int>> GameCounts { get; set; }

        private ObservableCollection<KeyValuePair<string, int>> _gamesByDateRange;

        public Plays()
        {
            AllPlaysByYear = new ObservableCollection<Play>();
            AllPlays = new ObservableCollection<Play>();
            LocationCounts = new ObservableCollection<KeyValuePair<string, int>>();
            GameCounts = new ObservableCollection<KeyValuePair<string, int>>();
            Years = new ObservableCollection<string>();
            _gamesByDateRange = new ObservableCollection<KeyValuePair<string, int>>();
        }

        public void LoadPlays(XmlDocument xmlPlays)
        {
            foreach (XmlNode play in xmlPlays.SelectNodes("//plays/play"))
            {
                //Not optimal but not a big performance issue..
                for (int i = 0; i < Int32.Parse(play.TextAttribute("quantity")); i++)
                {
                    AllPlays.Add(LoadPlay(play));
                    Plays.id++;
                }
            }
            TotalPlays = AllPlaysByYear.Count;
        }

        private Play LoadPlay(XmlNode xmlPlay)
        {
            Play play = new Play();
            play.Id = Plays.id.ToString();
            play.BGGId = xmlPlay.TextAttribute("id");
            play.Game = xmlPlay.SelectSingleNode("item").Attributes["name"].InnerText;
            play.Location = xmlPlay.TextAttribute("location").Trim();
            play.EditLink = String.Format(Resources.EditPlay, play.BGGId);
            play.Comments = xmlPlay.SelectSingleNode("comments") == null ? String.Empty : xmlPlay.SelectSingleNode("comments").InnerText;
            play.Date = DateTime.Parse(xmlPlay.TextAttribute("date")); //Assume that there is always a date...            

            bool hasAtLeastOnePlayer = false;
            bool hasAtLeastOneZero = false;
            bool currentRatingIsZero;

            foreach (XmlNode playerOrTeam in xmlPlay.SelectNodes("players/player"))
            {
                //Manage team separated by "/" (e.g. Cooperative games or "Team games" -> Time's up)
                foreach (string playerName in playerOrTeam.TextAttribute("name").Split('/'))
                {
                    hasAtLeastOnePlayer = LoadPlayer(playerOrTeam, play, playerName, out currentRatingIsZero);
                    hasAtLeastOneZero = hasAtLeastOneZero || currentRatingIsZero;
                }
            }

            //If empty, the current game has to be added in the total of the player
            if (!hasAtLeastOnePlayer)
            {
                play.Result.Add(new Play.RatingPlayer() { Rating = 0, Player = new Player() { Nickname = CurrentPlayerNickname, Username = CurrentPlayerUsername } });
            }

            //If there are some rating "0", but them in last position to be consistent in the "order"            
            if (hasAtLeastOneZero && play.Result.Count(p => p.Rating == 0) < play.Result.Count())
            {
                int lastRating = play.Result.Max(r => r.Rating) + 1;
                foreach (var lastPlayResult in play.Result.Where(r => r.Rating == 0))
                {
                    lastPlayResult.Rating = lastRating;
                }
            }

            AddOrIncrementLocationCounts(LocationCounts, play.Location);
            AddOrIncrementLocationCounts(GameCounts, play.Game);

            return play;
        }

        private void AddOrIncrementLocationCounts(ObservableCollection<KeyValuePair<string, int>> locationCounts, string location)
        {
            //TODO remove count which is not efficient
            if (locationCounts.Count(k => k.Key.Equals(location, StringComparison.CurrentCultureIgnoreCase)) > 0)
            {
                //Assume that there is only on entry...
                int oldValue = locationCounts.Single(k => k.Key.Equals(location, StringComparison.CurrentCultureIgnoreCase)).Value;
                locationCounts.Remove(locationCounts.Single(k => k.Key.Equals(location, StringComparison.CurrentCultureIgnoreCase)));
                locationCounts.Add(new KeyValuePair<string, int>(location, ++oldValue));
            }
            else
            {
                locationCounts.Add(new KeyValuePair<string, int>(location, 1));
            }
        }

        public void ResetPlays()
        {
            AllPlaysByYear.Clear();
        }

        private bool LoadPlayer(XmlNode players, Play play, string playerName, out bool ratingZero)
        {
            string ratingStr;
            int rating;
            string name;
            string username;
            string score;

            ratingStr = players.TextAttribute("rating");
            rating = Int32.TryParse(ratingStr, out rating) ? rating : -1;

            //Means he wins => rating = 1
            if (players.TextAttribute("win") == "1")
                rating = 1;

            ratingZero = (rating == 0);

            name = playerName.Trim();
            username = players.TextAttribute("username");
            score = players.TextAttribute("score");

            play.Result.Add(new Play.RatingPlayer() { Rating = rating, Score = score, Player = new Player() { Nickname = name, Username = username } });

            //return true means that at least one player exists!
            return true;
        }

        public int GetHIndex()
        {
            int counter = 0;
            foreach (var item in GameCounts.OrderByDescending(p => p.Value))
            {
                if (item.Value > counter)
                    counter++;
                else
                    break;
            }
            return counter;
        }

        //TODO : Should be calculated directly...
        public void PopulateYears()
        {
            //Get all Years when everything is loaded 
            Years.Add(ALL_YEARS);
            foreach (var year in AllPlays.GroupBy(p => p.Date.Year).Select(p => p.Key))
            {
                Years.Add(year.ToString());
            }
        }

        public void FilterByYear(string year)
        {
            if (year == ALL_YEARS)
            {
                AllPlaysByYear = new ObservableCollection<Play>(AllPlays);
            }
            else
            {
                AllPlaysByYear = new ObservableCollection<Play>(AllPlays.Where(p => p.Date.Year.Equals(Int32.Parse(year)))); //Assume that there are only numbers!
            }

            CalculGamesAndLocationCounts();
        }

        private void CalculGamesAndLocationCounts()
        {
            LocationCounts.Clear();
            GameCounts.Clear();

            foreach (var play in AllPlaysByYear)
            {
                AddOrIncrementLocationCounts(LocationCounts, play.Location);
                AddOrIncrementLocationCounts(GameCounts, play.Game);
            }
        }

        public ObservableCollection<KeyValuePair<string, int>> GetGamesByDateRange(DateRange dateRange, string year = ALL_YEARS)
        {
            _gamesByDateRange.Clear();

            switch (dateRange)
            {
                case DateRange.Month:
                    List<string> allYears;
                    if (year == ALL_YEARS)
                        allYears = Years.OrderBy(y => y).Take(Years.Count - 1).ToList();
                    else
                        allYears = new List<string>() { year };

                    foreach (var currentYear in allYears)
                    {
                        for (int month = 1; month <= 12; month++)
                        {
                            _gamesByDateRange.AddOrUpdate(String.Concat(month, " ", currentYear), AllPlays.Count(p => (p.Date.Year.Equals(Int32.Parse(currentYear)) && p.Date.Month.Equals(month))));
                        }
                    }
                    break;
                case DateRange.Year:
                    foreach (var currentYear in Years.OrderBy(y => y).Take(Years.Count - 1))
                    {
                        _gamesByDateRange.AddOrUpdate(currentYear, AllPlays.Count(p => p.Date.Year.Equals(Int32.Parse(currentYear))));
                    }
                    break;
                default:
                    break;
            }
            return _gamesByDateRange;
        }
    }
}
