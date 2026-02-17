namespace KahootMvc.Dtos.SessionsDto;

public class SessionResponse
{
    public bool Success { get; set; }
    public int PinCode { get; set; }
    public string QuizTitle { get; set; }
    public int TotalQuestions { get; set; }
}