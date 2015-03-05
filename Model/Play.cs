using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGGStats.Model
{
    class Play
    {

        public class RatingPlayer
        {
            public int Rating {get; set;}
            public Player Player {get; set;}
            public string Score { get; set;}
        }

        public string Id { get; set; }
        public string BGGId { get; set; }
        public string Game { get; set; }
        public string EditLink { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }

        public List<RatingPlayer> Result { get; set; }

        public Play()
        {
            Result = new List<RatingPlayer>();
        }

    }
}
