using System.ComponentModel.DataAnnotations.Schema;

namespace KahootMvc.Models
{
    public class Question
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public int Point { get; set; }
        public int Time { get; set; }
        public int Order { get; set; }
        [ForeignKey(nameof(QuizId))]
        public Guid QuizId { get; set; }
        public Quiz Quiz { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
        public ICollection<Answer> Answers { get; set; }
    }
}
