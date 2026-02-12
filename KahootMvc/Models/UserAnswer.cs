namespace KahootMvc.Models;

public class UserAnswer
{
    public Guid Id { get; set; }
    public Guid SessionUserId { get; set; }
    public Guid QuestionId { get; set; }
    public string Answer { get; set; }
    public bool IsCorrect { get; set; }
    public DateTime AnsweredAt { get; set; }
}