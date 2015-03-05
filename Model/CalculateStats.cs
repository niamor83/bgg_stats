using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BGGStats.Helper;
using System.Collections.ObjectModel;

namespace BGGStats.Model
{
    class CalculateStats
    {

        private Plays BGGPlays { get; set; }

        //public List<string> PlayersName {get; set;}

        public ObservableCollection<Stats> Stats { get; private set; }

        public CalculateStats(Plays plays)
        {
            BGGPlays = plays;
            Stats = new ObservableCollection<Stats>();
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
                    //TODO Change count because it's not efficient...
                    if (Stats.Count(s => s.Player.Nickname.Equals(playerRating.Player.Nickname)) == 0)
                        {
                            Stats.Add(currentStat = new Stats() { Player = new Player() { Nickname = playerRating.Player.Nickname, Username = playerRating.Player.Username } });
                        }
                        else
                        {
                            currentStat = Stats.Single(s => s.Player.Nickname.Equals(playerRating.Player.Nickname));
                        }
                        currentStat.NbPlays++;

                        //TODO : Must be refactored... Awful...
                        switch (playerRating.Rating)
                        {
                            case 1:
                                currentStat.NbFirst++;
                                currentStat.NbFirstPercent = (double)currentStat.NbFirst / currentStat.NbPlays;
                                currentStat.PositionRating.AddOrUpdate("1st", currentStat.NbFirst);
                                break;
                            case 2:
                                currentStat.NbSecond++;
                                currentStat.NbSecondPercent = (double)currentStat.NbSecond / currentStat.NbPlays;
                                currentStat.PositionRating.AddOrUpdate("2nd", currentStat.NbSecond);
                                break;
                            case 3:
                                currentStat.NbThird++;
                                currentStat.NbThirdPercent = (double)currentStat.NbThird / currentStat.NbPlays;
                                currentStat.PositionRating.AddOrUpdate("3rd", currentStat.NbThird);
                                break;
                            case 4:
                                currentStat.NbFourth++;
                                currentStat.NbFourthPercent = (double)currentStat.NbFourth / currentStat.NbPlays;
                                currentStat.PositionRating.AddOrUpdate("4th", currentStat.NbFourth);
                                break;
                            case 5:
                                currentStat.NbFifth++;
                                currentStat.NbFifthPercent = (double)currentStat.NbFifth / currentStat.NbPlays;
                                currentStat.PositionRating.AddOrUpdate("5th", currentStat.NbFifth);
                                break;
                            case 6:
                                currentStat.NbSixth++;
                                currentStat.NbSixthPercent = (double)currentStat.NbSixth / currentStat.NbPlays;
                                currentStat.PositionRating.AddOrUpdate("6th", currentStat.NbSixth);
                                break;
                            case 7:
                                currentStat.NbSeventh++;
                                currentStat.NbSeventhPercent = (double)currentStat.NbSeventh / currentStat.NbPlays;
                                currentStat.PositionRating.AddOrUpdate("7th", currentStat.NbSeventh);
                                break;
                            case 8:
                                currentStat.NbEigth++;
                                currentStat.NbEigthPercent = (double)currentStat.NbEigth / currentStat.NbPlays;
                                currentStat.PositionRating.AddOrUpdate("8th", currentStat.NbEigth);
                                break;
                            default:
                                //If not found => Undefined
                                currentStat.NbEigth++;
                                currentStat.NbEigthPercent = (double)currentStat.NbEigth / currentStat.NbPlays;
                                currentStat.PositionRating.AddOrUpdate("8th", currentStat.NbEigth);
                                break;
                        }

                        //TODO : Recalculate each time.. Should be better
                        currentStat.NbFirstPercent = (double)currentStat.NbFirst / currentStat.NbPlays;
                }
            }
            Stats = new ObservableCollection<Stats>(Stats.OrderBy(s => s.Player.Nickname));
            
        }

    }
}
