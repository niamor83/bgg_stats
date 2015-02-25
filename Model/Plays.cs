using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BGGStats.Helper;

namespace BGGStats.Model
{
    class Plays
    {
        private static int id = 0;
        public string CurrentPlayerUsername { get; set; }
        public string CurrentPlayerNickname { get; set; }
        public List<Play> AllPlays { get; private set;  }

        public int TotalPlays { get; private set; }

        public Plays()
        {
            AllPlays = new List<Play>();
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

            string ratingStr;
            int rating;
            string name;
            string username;
            bool hasAtLeastOnePlayer = false;

            foreach (XmlNode players in xmlPlay.SelectNodes("players/player"))
            {
                ratingStr = players.TextAttribute("rating");
                rating = Int32.TryParse(ratingStr, out rating) ? rating : -1;

                //Means he wins!
                if (players.TextAttribute("win") == "1") 
                rating = 1;

                name = players.TextAttribute("name"); 
                username = players.TextAttribute("username"); 

                play.Result.Add(new Play.RatingPlayer() { Rating = rating, Player = new Player() { Nickname = name, Username = username } });

                hasAtLeastOnePlayer = true;
            }

            //If empty, the current game has to be added in the total of the player
            if (!hasAtLeastOnePlayer)
            {
                play.Result.Add(new Play.RatingPlayer() { Rating = 0, Player = new Player() { Nickname = CurrentPlayerNickname, Username = CurrentPlayerUsername } });
            }

            return play;
        }

        public void ResetPlays()
        {
            AllPlays.Clear();
        }



    }
}
