using System.ComponentModel.DataAnnotations.Schema;

namespace KahootMvc.Models
{
    public class Quiz
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CategoryId { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public bool IsActive { get; set; }
        public ICollection<Session> Sessions { get; set; } = default!;
        public ICollection<Question> Questions { get; set; } = default!;
    }
}
