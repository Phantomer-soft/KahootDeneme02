using System.ComponentModel.DataAnnotations.Schema;

namespace KahootMvc.Models
{
    public class SessionUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public int Point { get; set; } = 0;
        public string ? Username { get; set; } = string.Empty;
        public string ? ConnectionId { get; set; } = string.Empty;

    }
}
