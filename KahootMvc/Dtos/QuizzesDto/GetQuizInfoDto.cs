using KahootMvc.AppContext;

namespace KahootMvc.Dtos.QuizzesDto
{
    public class GetQuizInfoDto
    {
        public Guid QuizId { get; set; }
        public string Title{ get; set; }
        public string Description { get; set; }
        public int QuestionCount { get; set; }
    }
   
}
