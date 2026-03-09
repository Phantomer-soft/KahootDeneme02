using KahootMvc.Dtos.Answers;

namespace KahootMvc.Dtos.QuestionsDto;

public class UpdateQuestionDto
{
    public Guid? QuestionId { get; set; }   // null → yeni soru
    public string Text { get; set; } = string.Empty;
    public int Time { get; set; }
    public int Point { get; set; }

    public List<UpdateAnswerDto> Answers { get; set; } = new();
}