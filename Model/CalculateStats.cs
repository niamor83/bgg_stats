using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGGStats.Model
{
    class CalculateStats
    {

        private Plays BGGPlays { get; set; }

        //public List<string> PlayersName {get; set;}

        public List<Stats> Stats { get; private set; }

        public CalculateStats(Plays plays)
        {
            BGGPlays = plays;
            Stats = new List<Stats>();
            //PlayersName = new List<string>();

            GeneratePlayerStats();
        }

        private void GeneratePlayerStats(){
            Stats currentStat;
            foreach(Play play in BGGPlays.AllPlays){
                foreach (Play.RatingPlayer playerRating in play.Result)
                {
                        // TODO : Not correct due to Dictionnary (possibilit to have multiple value for one rating). To be updated!!!
                        //TODO Take care of the coupe Nickname + username to take into account to avoid duplicates
                    if (!Stats.Exists(s => s.Player.Nickname.Equals(playerRating.Player.Nickname)))
                        {
                            Stats.Add(currentStat = new Stats() { Player = new Player() { Nickname = playerRating.Player.Nickname, Username = playerRating.Player.Username } });
                        }
                        else
                        {
                            currentStat = Stats.Single(s => s.Player.Nickname.Equals(playerRating.Player.Nickname));
                        }
                        currentStat.NbPlays++;

                        //Add result!
                        switch (playerRating.Rating)
                        {
                            case 1:
                                currentStat.NbFirst++;
                                currentStat.NbFirstPercent = (double)currentStat.NbFirst / currentStat.NbPlays;
                                break;
                            default:
                                break;
                        }

                        //TODO : Recalculate each time.. Should be better
                        currentStat.NbFirstPercent = (double)currentStat.NbFirst / currentStat.NbPlays;

                }
            }
            Stats = Stats.OrderBy(s => s.Player.Nickname).ToList();
            
        }

    }
}
