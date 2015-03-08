using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BGGStats.Helper;
using System.Collections.ObjectModel;

namespace BGGStats.Model
{
    class Plays
    {
        private static int id = 0;
        public string CurrentPlayerUsername { get; set; }
        public string CurrentPlayerNickname { get; set; }
        public List<Play> AllPlays { get; private set;  }

        public int TotalPlays { get; private set; }

        public ObservableCollection<KeyValuePair<string, int>> LocationCounts { get; set; }
        public ObservableCollection<KeyValuePair<string, int>> GameCounts { get; set; }

        public Plays()
        {
            AllPlays = new List<Play>();
            LocationCounts = new ObservableCollection<KeyValuePair<string, int>>();
            GameCounts = new ObservableCollection<KeyValuePair<string, int>>();
        }

        public void LoadPlays(XmlDocument xmlPlays)
        {            
            foreach (XmlNode play in xmlPlays.SelectNodes("//plays/play"))
            {
                //Not optimal but not a big performance issue..
                for (int i = 0; i < Int32.Parse(play.TextAttribute("quantity")); i++ )
                {
                    AllPlays.Add(LoadPlay(play));
                    Plays.id++;
                }   
            }
            TotalPlays = AllPlays.Count; 
        }

        private Play LoadPlay(XmlNode xmlPlay)
        {
            Play play = new Play();
            play.Id = Plays.id.ToString();
            play.BGGId = xmlPlay.TextAttribute("id");  
            play.Game = xmlPlay.SelectSingleNode("item").Attributes["name"].InnerText;
            play.Location = xmlPlay.TextAttribute("location").Trim();            
            play.Date = DateTime.Parse(xmlPlay.TextAttribute("date")); //Assume that there is always a date...
            play.EditLink = String.Format(Resources.EditPlay, play.BGGId);

            bool hasAtLeastOnePlayer = false;

            foreach (XmlNode playerOrTeam in xmlPlay.SelectNodes("players/player"))
            {
                //Manage team separated by "/" (e.g. Cooperative games or "Team games" -> Time's up)
                foreach (string playerName in playerOrTeam.TextAttribute("name").Split('/'))
                {
                    hasAtLeastOnePlayer = LoadPlayer(playerOrTeam, play, playerName);
                }                                
            }

            //If empty, the current game has to be added in the total of the player
            if (!hasAtLeastOnePlayer)
            {
                play.Result.Add(new Play.RatingPlayer() { Rating = 0, Player = new Player() { Nickname = CurrentPlayerNickname, Username = CurrentPlayerUsername} });
            }

            AddOrIncrementLocationCounts(LocationCounts, play.Location);
            AddOrIncrementLocationCounts(GameCounts, play.Game);

            return play;
        }

        private void AddOrIncrementLocationCounts(ObservableCollection<KeyValuePair<string, int>> locationCounts, string location)
        {
            //TODO remove count which is not efficient
            if (locationCounts.Count(k => k.Key.Equals(location, StringComparison.CurrentCultureIgnoreCase) ) > 0)
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
            AllPlays.Clear();
        }

        private bool LoadPlayer(XmlNode players, Play play, string playerName)
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

    }
}
