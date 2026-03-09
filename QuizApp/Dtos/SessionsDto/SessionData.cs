using KahootMvc.Models;

namespace KahootMvc.Dtos.SessionsDto;

public class SessionData
{
    public Guid SessionId { get; set; }
    public List<Question> Questions { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public Dictionary<string, Guid> Participants { get; set; } // ConnectionId -> UserId

}