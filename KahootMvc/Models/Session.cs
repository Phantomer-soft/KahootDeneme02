namespace KahootMvc.Models
{
    public class Session
    {
        public Guid Id { get; set; }
        public int PinCode { get; set; }
        public Guid CurrentQuestionId { get; set; }
        public Question CurrentQuestion { get; set; }
        public DateTime? CurrentQuestionStartedAt { get; set; }
        
        public Quiz Quiz { get; set; }

    }
}
