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
        public int NbFirst {get; set;}
        public double NbFirstPercent { get; set; }

        public Stats()
        {
            Player = new Player();
            NbFirst = 0;
            NbFirstPercent = 0;
            NbPlays = 0;
        }
    }
}
