using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace KahootMvc.Models
{
    public class Question
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = default!;
        public int Point { get; set; }
        public int Time { get; set; }
        public int Order { get; set; }
        [ForeignKey(nameof(QuizId))]
        public Guid QuizId { get; set; }
        [JsonIgnore] // BUNA BAK EGER HATA CIKARSA KALDIR BUNU 
        public Quiz Quiz { get; set; } = default!;
        public ICollection<Answer> Answers { get; set; }=default!;
    }
}
