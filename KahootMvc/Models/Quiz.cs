using System.ComponentModel.DataAnnotations.Schema;

namespace KahootMvc.Models
{
    public class Quiz
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int PinCode { get; set; }
        public ICollection<Session> Sessions { get; set; }
        public ICollection<Question> Questions { get; set; }
    }
}
