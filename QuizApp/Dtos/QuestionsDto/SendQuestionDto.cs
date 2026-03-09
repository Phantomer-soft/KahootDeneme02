using KahootMvc.Dtos.Answers;

namespace KahootMvc.Dtos.QuestionsDto;

public class SendQuestionDto
{
    public string text { get; set; } = string.Empty;
    public int point { get; set; }
    public int time { get; set; }
    public int order { get; set; }
    public List<SendAnswerDto> answers { get; set; } = new List<SendAnswerDto>();
}