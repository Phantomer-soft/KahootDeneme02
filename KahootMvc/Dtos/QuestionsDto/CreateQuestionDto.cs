using KahootMvc.Dtos.Answers;
using System.ComponentModel.DataAnnotations;

namespace KahootMvc.Dtos.QuestionsDto
{
    public class CreateQuestionDto
    {
        [Required(ErrorMessage = "Soru metni zorunludur")]
        [StringLength(1000, MinimumLength = 5)]
        public string Text { get; set; }

        [Range(1, 1000, ErrorMessage = "Puan 1-1000 arasında olmalıdır")]
        public int Point { get; set; } = 10;

        [Required(ErrorMessage = "Süre zorunludur")]
        [Range(1, 300, ErrorMessage = "Süre 1-300 saniye arasında olmalıdır")]
        public int Time { get; set; } = 20;
        

        [Required(ErrorMessage = "Doğru cevap seçilmelidir")]
        [Range(0, int.MaxValue)]
        public int CorrectAnswerIndex { get; set; }

        [Required]
        [MinLength(2, ErrorMessage = "En az 2 şık olmalıdır")]
        public List<CreateAnswerDto> Answers { get; set; } = new();

    }
}
