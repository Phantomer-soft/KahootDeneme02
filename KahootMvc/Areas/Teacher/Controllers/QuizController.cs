
using AutoMapper;
using AutoMapper.QueryableExtensions;
using KahootMvc.AppContext;
using KahootMvc.Dtos.QuizzesDto;
using KahootMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

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
            var UserId = Request.Headers["UserId"]; // headerda gelen user id yi aldım 
            if (!ModelState.IsValid)
            {
                // Geçersiz giriş hataları varsa formu tekrar göster
                return BadRequest(new { message="Geçersiz giriş lütfen tüm alanları kontrol edin."});
            }
            else
                try
                {
                    var category = _context.Categories.FirstOrDefault(c => c.Title == dto.Category);
                    if (category == null)
                    {
                        try
                        {
                            Category newcategory = new()
                            {
                                Id =  Guid.NewGuid(),
                                Title = dto.Category,
                            };
                            _context.Categories.Add(newcategory);
                            _context.SaveChanges();
                            category = newcategory;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("kategori olusturulamadi ");
                        }
                    }
                    var quiz = new Quiz
                    {
                        Id = Guid.NewGuid(),
                        UserId = Guid.Parse(UserId), // Headerdan gelen user id
                        Title = dto.Title,
                        Description = dto.Description,
                        IsActive = true,
                        CategoryId = category.Id,
                        Questions = new List<Question>()// Sorular için liste
                    };

                    // Soruları ekle
                    int questionOrder = 1;
                    foreach (var questionDto in dto.Questions)
                    {
                       
                        // Soru oluşturma
                        var question = new Question
                        {
                            Id = Guid.NewGuid(),
                            Text = questionDto.Text,
                            Point = questionDto.Point,
                            Time = questionDto.Time,
                            Order = questionOrder++,
                            QuizId = quiz.Id,
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
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message= $"Quiz kaydedilirken bir hata oluştu : {ex.Message}" });
                }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetQuizInfoDto>>> GetQuizzes(string userid)
        {
            var userGuid = Guid.Parse(userid);
            var list = await _context.Quizzes
                .Where(x => x.UserId == userGuid)
                .Select(x => new GetQuizInfoDto
                {
                    QuizId = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    QuestionCount = x.Questions.Count
                })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet]
        public async Task<ActionResult> GetQuiz(string id)
        {
            try
            {
                var quizId = Guid.Parse(id);
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                    .FirstAsync(q => q.Id == quizId);
                return Ok(quiz);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null) return NotFound();

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            return NoContent();
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

