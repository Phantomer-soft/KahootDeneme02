using System.ComponentModel.DataAnnotations.Schema;

namespace KahootMvc.Models;

public class SessionLeaderboard
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public Guid SessionId { get; set; }
    public List<SessionUser> Users { get; set; } =  new List<SessionUser>();
}