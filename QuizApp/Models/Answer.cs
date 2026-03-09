using System.Text.Json.Serialization;

namespace KahootMvc.Models
{
    public class Answer
    {
      
        public Guid Id { get; set; }
        public string Text { get; set; }
        public int AnswerOrder { get; set; }
        public bool IsCorrect { get; set; } 
        [JsonIgnore] // BUNA BAK EGER HATA CIKARSA KALDIR BUNU 
        public Question Question { get; set; }
    }
}
