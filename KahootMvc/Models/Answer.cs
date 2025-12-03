namespace KahootMvc.Models
{
    public class Answer
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public int AnswerOrder { get; set; }
        public bool IsCorrect { get; set; } 
        public Question Question { get; set; }
    }
}
