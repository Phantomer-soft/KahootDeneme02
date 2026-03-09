using System.ComponentModel.DataAnnotations.Schema;

namespace KahootMvc.Models
{
    public class LeaderBoard
    {
        public int Id { get; set; }
        [ForeignKey(nameof(SessionId))]
        public Guid SessionId { get; set; }

        [ForeignKey(nameof(SessionLeaderboardId))]
        public int SessionLeaderboardId { get; set; }


    }
}
