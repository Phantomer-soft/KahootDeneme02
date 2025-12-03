
using AutoMapper;
using KahootMvc.AppContext;
using KahootMvc.Dtos.QuizzesDto;
using KahootMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace KahootMvc.Areas.Teacher.Controllers
{
 
    [Area("Teacher")]
    [Route("teacher/[controller]/[action]")]
    // [Authorize(Roles = "Teacher, Admin")] // Yetkilendirme kuralı
    public class QuizController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public QuizController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }



        // Aktif quiz listesini göstermek için
        public IActionResult Index =>
            // Burada veritabanından öğretmene ait quizler çekilecek ve View'e gönderilecek.
            View();




        // Yeni quiz oluşturma formunu göstermek için
        [HttpGet]
        public IActionResult CreateQuiz()
        {
            // ...
            return Redirect("CreateQuiz.html");
        }



        [HttpPost]
        public async Task<IActionResult> CreateQuiz([FromBody]CreateQuizDto dto)
        {
            if (!ModelState.IsValid)
            {
                // Geçersiz giriş hataları varsa formu tekrar göster
                return BadRequest(new { message="Geçersiz giriş lütfen tüm alanları kontrol edin."});
            }
            else
                try
                {
                    // Random PinCode oluştur 6 hane
                    var pinCode = CreatePinCode();

                    var quiz = new Quiz
                    {
                        Id = Guid.NewGuid(),
                        Title = dto.Title,
                        Description = dto.Description,
                        IsActive = true,
                        PinCode = pinCode,
                        Questions = new List<Question>()// Sorular için liste
                    };

                    // Soruları ekle
                    int questionOrder = 1;
                    foreach (var questionDto in dto.Questions)
                    {
                        // Kategoriyi bul veya oluştur
                        var category = await _context.Categories
                            .FirstOrDefaultAsync(c => c.Title == questionDto.CategoryTitle);

                        if (category == null)//Bulamazsan yeni kategori oluştur
                        {
                            category = new Category
                            {
                                Id = Guid.NewGuid(),
                                Title = questionDto.CategoryTitle
                            };
                            await _context.Categories.AddAsync(category);
                        }

                        // Soru oluşturma
                        var question = new Question
                        {
                            Id = Guid.NewGuid(),
                            Text = questionDto.Text,
                            Point = questionDto.Point,
                            Time = questionDto.Time,
                            Order = questionOrder++,
                            QuizId = quiz.Id,
                            CategoryId = category.Id,
                            Answers = new List<Answer>()
                        };

                        // Cevapları ekleme
                        int answerOrder = 0;
                        foreach (var answerDto in questionDto.Answers)
                        {
                            var answer = new Answer
                            {
                                Id = Guid.NewGuid(),
                                Text = answerDto.Text,
                                AnswerOrder = answerOrder,
                                IsCorrect = answerOrder == questionDto.CorrectAnswerIndex,
                                Question = question
                            };
                            question.Answers.Add(answer);
                            answerOrder++;
                            //Her döngüde cevap sırasını 1 arttır 
                        }

                        quiz.Questions.Add  (question);
                    }

                    // Veritabanına kaydet
                    await _context.Quizzes.AddAsync(quiz);
                    await _context.SaveChangesAsync();
                    return Ok(new { PinCode = pinCode });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message= $"Quiz kaydedilirken bir hata oluştu : {ex.Message}" });
                }
        }

        [HttpGet]
        public IActionResult GetQuizzes()
        {
            var quizzes = _context.Quizzes
                .Include(q => q.Questions)
                .OrderByDescending(q => q.Id)
                .ToList();
            return View(quizzes);
        }


        public int CreatePinCode()
        {
            Random random = new Random();
            int maxAttempts = 100; // Sonsuz döngüyü önlemek için maksimum deneme
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                var pinCode = random.Next(100000, 1000000);
                if (CheckPinCode(pinCode))
                {
                    _context.PinCodes.Add(new PinCode(pinCode));//Uygun pin kodunu pin kodu tablosuna kayıt
                    _context.SaveChanges();
                    return pinCode;
                }
                    

                attempts++;
            }
            throw new InvalidOperationException("Uygun PIN kodu bulunamadı");// Umarım gerek kalmaz ama 
        }

        public bool CheckPinCode(int pincode)
        {
            var isUsed = _context.PinCodes.FirstOrDefault(x => x.Pin == pincode);
            return isUsed == null; // Kullanılmamışsa true
        }
    }
}

