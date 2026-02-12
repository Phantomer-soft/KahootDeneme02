using System.Text.RegularExpressions;
using KahootMvc.AppContext;
using KahootMvc.Dtos.Answers;
using KahootMvc.Dtos.QuestionsDto;
using KahootMvc.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KahootMvc.Hubs;


public class QuizHub : Hub
{
    private readonly AppDbContext _context;
    private readonly Functions _functions;
    public QuizHub(AppDbContext context, Functions functions)
    {
        _context = context;
        _functions = functions;
    }

    // ek gelistirme => kullanicilara settoken yapabilirim ama suan degil 
    public async Task<SessionUser> Join(int pinCode, string username) // Dönüş tipi eklendi
    {
        var sessionInfo = _context.Sessions.FirstOrDefault(s => s.PinCode == pinCode);
        if (sessionInfo is { IsEnded: false })
        {
            SessionUser sessionUser = new()
            {
                UserId = Guid.NewGuid(),
                SessionId = sessionInfo.Id,
                Point = 0,
                Username = username,
                ConnectionId = Context.ConnectionId
            };

            await Groups.AddToGroupAsync(Context.ConnectionId, $"{sessionInfo.Id}");
            
            await Clients.Caller.SendAsync("SetSessionUser", sessionUser.UserId.ToString(), sessionUser.SessionId.ToString());
            await Clients.Group($"{sessionInfo.Id}").SendAsync("UserJoined",new
            {
                Username = username,
                SessionId = sessionInfo.Id,
                ConnectionId = Context.ConnectionId
            });
            await _context.SessionUsers.AddAsync(sessionUser);
            await _context.SaveChangesAsync();

            return sessionUser; // Nesneyi return ediyoruz ki invoke eden alabilsin
        }
        else
        {
            throw new HubException("Session bulunamadi.");
        }
    }

    public async Task CreateSession(string teacherId, string quizId)
    {
        var id = Guid.Parse(teacherId);
        var teacher = _context.Users.FirstOrDefault(u => u.Id == id);
    
        if (teacher == null)
        {
            throw new HubException("Öğretmen bulunamadı.");
        }
        else
        {
            var quid = Guid.Parse(quizId);
            var teacherQuiz = _context.Quizzes.FirstOrDefault(q => q.Id == quid && q.UserId == teacher.Id && q.IsActive);
            if (teacherQuiz == null) 
                throw new HubException("Quiz bulunamadı.");
            else
            {
                string rawToken = Guid.NewGuid().ToString();
                string tokenHash = _functions.TokenHasher(rawToken);
                var code = _functions.CreatePinCode();
                var random = new Random();
                List<Question> questions = new List<Question>();

                PinCode pin = new PinCode(code);
                
                Session newSession = new()
                {
                    Id =Guid.NewGuid(),
                    CurrentQuestion = 1,
                    PinCode = code,
                    Quiz = _context.Quizzes.FirstOrDefault(q => q.Id == quid),
                    IsEnded = false,
                    
                };
                Token teacherToken = new()
                {
                    Id =Guid.NewGuid(),
                    UserId = teacher.Id,
                    IsUsed =  false,
                    TokenHash = tokenHash,
                };
                LeaderBoard leaderBoard = new()
                {
                    Id = random.Next(1111,99999),
                    SessionId = newSession.Id,
                };    
                _context.PinCodes.Add(pin);
                _context.Sessions.Add(newSession);
                _context.Tokens.Add(teacherToken);
                await _context.SaveChangesAsync(); 
                await Groups.AddToGroupAsync(Context.ConnectionId, $"{newSession.Id}");
                await Clients.Caller.SendAsync("SetTeacherToken",rawToken);
                await Clients.Caller.SendAsync("SetPinCode", code);
                await Clients.Caller.SendAsync("SetSessionId",newSession.Id);
                await Clients.Caller.SendAsync("SetTeacherId",teacherId);
                
            }
            
        }
        
    }

    public async Task StartSession(string sessionId, string teacherId, string teacherToken)
{
    var tokenHash = _functions.TokenHasher(teacherToken);
    // Token kontrolünde IsUsed mantığını ters kurmuş olabilirsin, genelde IsUsed false ise yetki verilir.
    var tokenEntry = _context.Tokens.FirstOrDefault(t => t.UserId == Guid.Parse(teacherId) 
                                                         && t.TokenHash == tokenHash 
                                                         && t.IsUsed==false);
    
    if (tokenEntry == null) throw new HubException("UNAUTHENTICATED");

    try
    {
        var session = await _context.Sessions
            .Include(s => s.Quiz)
            .FirstOrDefaultAsync(s => s.Id == Guid.Parse(sessionId));

        if (session != null && session.Quiz != null)
        {
            await SendQuestion(session.Quiz.Id, session.CurrentQuestion, session.Id);
            await Clients.Group(sessionId.ToString()).SendAsync("SessionStarted", "BASARILAR");
            session.CurrentQuestion += 1;
            await _context.SaveChangesAsync();
        }
        else { throw new HubException("OTURUM VEYA QUIZ BULUNAMADI"); }
    }
    catch (Exception e) { throw new HubException("SERVER ERROR: " + e.Message); }
}

    public override async Task OnDisconnectedAsync(Exception ex)
    {
        await Clients.Group("ROOM1")
            .SendAsync("UserLeft", Context.ConnectionId);

        await base.OnDisconnectedAsync(ex);
    }


    public async Task SendQuestion(Guid quizId, int questionOrder, Guid sessionId)
    {
        var question = await _context.Questions
            .Include(s => s.Answers)
            // 2. QuizId filtresi eklendi (Güvenlik için kritik)
            .FirstOrDefaultAsync(q => q.QuizId == quizId && q.Order == questionOrder);

        if (question == null) throw new HubException("Sıradaki soru bulunamadı.");

        var sendAnswerDtos = question.Answers.Select(answer => new SendAnswerDto()
        {
            Text = answer.Text
        }).ToList();

        var sendQuestion = new SendQuestionDto()
        {
            text = question.Text,
            point = question.Point,
            time = question.Time,
            order = question.Order,
            answers = sendAnswerDtos
        };
        
        await Clients.Group(sessionId.ToString()).SendAsync("SendQuestion", sendQuestion);
    }
    
}
