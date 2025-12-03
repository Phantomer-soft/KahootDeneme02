using System.ComponentModel.DataAnnotations;

namespace KahootMvc.Dtos.Answers
{
    public class CreateAnswerDto
    {
        [Required(ErrorMessage = "Cevap metni zorunludur")]
        [StringLength(500, MinimumLength = 1)]
        public string Text { get; set; }
    }
}
