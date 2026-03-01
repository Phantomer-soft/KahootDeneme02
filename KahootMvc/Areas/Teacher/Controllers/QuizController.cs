
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
                    var categoryId = Guid.Parse(dto.Category);
                    var quiz = new Quiz
                    {
                        Id = Guid.NewGuid(),
                        UserId = Guid.Parse(UserId), // Headerdan gelen user id
                        Title = dto.Title,
                        Description = dto.Description,
                        IsActive = true,
                        CategoryId = categoryId,
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
                .Where(x => x.UserId == userGuid && x.IsActive)
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

        [HttpPut]
public async Task<IActionResult> UpdateQuiz(
    [FromBody] UpdateQuizDto dto,
    [FromHeader(Name = "Userid")] string userId)
{
    var userGuid = Guid.Parse(userId);

    var quiz = await _context.Quizzes
        .Include(q => q.Questions)
            .ThenInclude(q => q.Answers)
        .FirstOrDefaultAsync(q => q.Id == dto.QuizId && q.UserId == userGuid);

    if (quiz == null)
        return NotFound();

    quiz.Title = dto.Title;
    quiz.Description = dto.Description;
    quiz.CategoryId = dto.CategoryId;

    var incomingQuestionIds = dto.Questions
        .Where(q => q.QuestionId.HasValue)
        .Select(q => q.QuestionId!.Value)
        .ToList();

    var removedQuestions = quiz.Questions
        .Where(q => !incomingQuestionIds.Contains(q.Id))
        .ToList();

    _context.Questions.RemoveRange(removedQuestions);

    foreach (var qDto in dto.Questions)
    {
        Question question;

        if (qDto.QuestionId.HasValue)
        {
            question = quiz.Questions.First(q => q.Id == qDto.QuestionId.Value);
            question.Text = qDto.Text;
            question.Time = qDto.Time;
            question.Point = qDto.Point;
        }
        else
        {
            question = new Question
            {
                Id = Guid.NewGuid(),
                QuizId = quiz.Id,
                Text = qDto.Text,
                Time = qDto.Time,
                Point = qDto.Point,
                Order = quiz.Questions.Count+1, 
                Answers = new List<Answer>()
            };

            quiz.Questions.Add(question);
            _context.Questions.Add(question);
        }

        var incomingAnswerIds = qDto.Answers
            .Where(a => a.AnswerId.HasValue)
            .Select(a => a.AnswerId!.Value)
            .ToList();

        var removedAnswers = question.Answers
            .Where(a => !incomingAnswerIds.Contains(a.Id))
            .ToList();

        _context.Answers.RemoveRange(removedAnswers);

        foreach (var aDto in qDto.Answers)
        {
            if (aDto.AnswerId.HasValue)
            {
                var answer = question.Answers
                    .First(a => a.Id == aDto.AnswerId.Value);

                answer.Text = aDto.Text;
                answer.IsCorrect = aDto.IsCorrect;
            }
            else
            {
                var qguid =  Guid.NewGuid();
                var answer = new Answer()
                {
                    Id = qguid,
                    Text = aDto.Text,
                    IsCorrect =  aDto.IsCorrect,
                    AnswerOrder = question.Answers.Count
                };
                _context.Answers.Add(answer);
                question.Answers.Add(answer);
            }
        }
    }
    await _context.SaveChangesAsync();
    return Ok();
}
        [HttpDelete] // Silme işlemi için HttpDelete daha uygundur
        public async Task<IActionResult> DeleteQuiz([FromHeader] string userId, [FromQuery] string quizId) // quizId'nin nereden geleceği netleştirildi
        {
            var userid = Guid.Parse(userId);
            var user = _context.Users.FirstOrDefault(u => u.Id == userid);
    
            // quizId null kontrolü eklenmeli veya Guid/Int dönüşümü yapılmalı
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q=> q.Id == Guid.Parse(quizId));

            if (quiz == null) return NotFound(); // Quiz bulunamadıysa hata dönmeli

            if (quiz.UserId == user.Id)
            {
                quiz.IsActive = false;
                _context.Quizzes.Update(quiz);
                await _context.SaveChangesAsync();  
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}

