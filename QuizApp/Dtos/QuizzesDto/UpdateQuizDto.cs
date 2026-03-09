using KahootMvc.Dtos.QuestionsDto;

namespace KahootMvc.Dtos.QuizzesDto;

public class UpdateQuizDto
{
    public Guid QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }

    public List<UpdateQuestionDto> Questions { get; set; } = new();
}