using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGGStats.Model
{
    class Stats
    {
        public Player Player { get; set; }
        public int NbPlays { get; set; }

        public List<KeyValuePair<string, int>> PositionRating { get; set; }

        //TODO : Should be more generic but for now I'm lazy....
        public int NbFirst {get; set;}
        public double NbFirstPercent { get; set; }
        public int NbSecond { get; set; }
        public double NbSecondPercent { get; set; }
        public int NbThird { get; set; }
        public double NbThirdPercent { get; set; }
        public int NbFourth { get; set; }
        public double NbFourthPercent { get; set; }
        public int NbFifth { get; set; }
        public double NbFifthPercent { get; set; }
        public int NbSixth { get; set; }
        public double NbSixthPercent { get; set; }
        public int NbSeventh { get; set; }
        public double NbSeventhPercent { get; set; }
        public int NbEigth { get; set; }
        public double NbEigthPercent { get; set; }
        public int NbUndefined { get; set; }
        public double NbUndefinedPercent { get; set; }

        public Stats()
        {
            Player = new Player();

            NbFirst = 0;
            NbFirstPercent = 0;
            NbSecond = 0;
            NbSecondPercent = 0;
            NbThird = 0;
            NbThirdPercent = 0;
            NbFourth = 0;
            NbFourthPercent = 0;
            NbFifth = 0;
            NbFifthPercent = 0;
            NbSixth = 0;
            NbSixthPercent = 0;
            NbSeventh = 0;
            NbSeventhPercent = 0;
            NbEigth = 0;
            NbEigthPercent = 0;
            NbUndefined = 0;
            NbUndefinedPercent = 0;

            //TODO : To refactor...
            PositionRating = new List<KeyValuePair<string, int>>();

            NbPlays = 0;
        }
    }
}
