using KahootMvc.Dtos.QuestionsDto;
using System.ComponentModel.DataAnnotations;

namespace KahootMvc.Dtos.QuizzesDto
{
    public class CreateQuizDto
    {
        [Required(ErrorMessage = "Quiz başlığı zorunludur")]
        [StringLength(200, MinimumLength = 3)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required(ErrorMessage = "En az 1 soru eklemelisiniz")]
        [MinLength(1, ErrorMessage = "En az 1 soru eklemelisiniz")]
        public List<CreateQuestionDto> Questions { get; set; } = new();

    }
}
