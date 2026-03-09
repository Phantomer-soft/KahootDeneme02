namespace KahootMvc.Dtos.Answers;

public class UpdateAnswerDto
{
    public Guid? AnswerId { get; set; }   // null → yeni cevap
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}