using System.ComponentModel.DataAnnotations.Schema;

namespace KahootMvc.Models
{
    public class Session
    {
        public Guid Id { get; set; }
        public int PinCode { get; set; }
        public Guid ? CurrentQuestionId { get; set; }
        [ForeignKey(nameof(LeaderBoardId))]
        public Guid ? LeaderBoardId { get; set; }
        public int CurrentQuestion { get; set; } = 0;
        public DateTime? CurrentQuestionStartedAt { get; set; }
        public Guid ? QuizId { get; set; }
        public Quiz?  Quiz { get; set; }=default!;
        public bool IsEnded { get; set; }=false; // True olduğu zaman pin kodu değişecek 
    }
}
