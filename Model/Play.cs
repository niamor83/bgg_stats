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
        public string Comments { get; set; }

        public List<RatingPlayer> Result { get; set; }


        //TODO Should be refactor, Should not mix different business rules in one class...
        public string CurrentUser { set; private get; }

        public int CurrentUserRating 
        {
            get { return Result.Where(r => r.Player.Nickname == CurrentUser).Select(r => r.Rating).FirstOrDefault(); }
        }

        public Play()
        {
            Result = new List<RatingPlayer>();
        }
    }
}
