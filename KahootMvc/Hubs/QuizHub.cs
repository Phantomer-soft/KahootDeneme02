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
    private readonly QuizTimerService _timer;
    public QuizHub(AppDbContext context, Functions functions, QuizTimerService timer)
    {
        _context = context;
        _functions = functions;
        _timer = timer;
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

            // kullaniciyi kayit oldugu oturumun liderlik tablosuna eklemis oldum 
            var sessionLeaderboard = _context.SessionLeaderboards.FirstOrDefault(s => s.SessionId == sessionUser.SessionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{sessionInfo.Id}");
            
            await Clients.Caller.SendAsync("SetSessionUser", sessionUser.UserId.ToString(), sessionUser.SessionId.ToString());
            await Clients.Group($"{sessionInfo.Id}").SendAsync("UserJoined",new
            {
                Username = username,
                SessionId = sessionInfo.Id,
                ConnectionId = Context.ConnectionId
            });
            sessionLeaderboard?.Users.Add(sessionUser);
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
                    QuizId = Guid.Parse(quizId)
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
    var tokenEntry = _context.Tokens.FirstOrDefault(t => t.UserId == Guid.Parse(teacherId) 
                                                         && t.TokenHash == tokenHash 
                                                         && t.IsUsed==false);
    
    if (tokenEntry == null) throw new HubException("UNAUTHENTICATED");

    try
    {
        var session = await _context.Sessions
            .Include(s => s.Quiz)
            .FirstOrDefaultAsync(s => s.Id == Guid.Parse(sessionId));

        if (session is { Quiz: not null })
        {
            var question = _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefault(q => q.QuizId == session.Quiz.Id && q.Order == 1);
            if(question == null)
                throw new HubException("QUESTION NOT FOUND");
            else
            {
                // suanlik idare eder bir guvenlik onlemi olarak yaptim sonra jwt ye gecerim belki 
                var rawToken = _functions.TokenHasher(Guid.NewGuid().ToString());
                SessionLeaderboard leaderboard = new()
                {
                    SessionId = session.Id,
                    Users = new List<SessionUser>()
                };
                Token token = new()
                {
                    Id =Guid.NewGuid(),
                    IsUsed = false,
                    UserId = Guid.Parse(teacherId),
                    TokenHash = _functions.TokenHasher(rawToken)
                };
                // tokeni kullanildi isaretleyip yenisini urettim 
                tokenEntry.IsUsed = true;
                await _context.Tokens.AddAsync(token);
                await SendNewQuestion(session.Id, question);
                await Clients.Group(sessionId).SendAsync("SessionStarted", "BASARILAR");
                await Clients.Caller.SendAsync("SetTeacherToken",rawToken);
                _timer.StartQuestionTimer(sessionId,question.Time);
                session.CurrentQuestionId = question.Id;
                await _context.SessionLeaderboards.AddAsync(leaderboard); 
                session.CurrentQuestionStartedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                
            }
           
        }
        else { throw new HubException("OTURUM VEYA QUIZ BULUNAMADI"); }
    }
    catch (Exception e) { throw new HubException("SERVER ERROR: " + e.Message); }
}

    public async Task NextQuestion(string sessionId, string teacherId, string teacherToken)
    {
        var tokenHash = _functions.TokenHasher(teacherToken);
        var tokenEntry = _context.Tokens
            .FirstOrDefault(t => t.UserId == Guid.Parse(teacherId) 
            && t.TokenHash == tokenHash 
            && t.IsUsed==false);
        
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.Id == Guid.Parse(sessionId)); 
        
        if (session != null)
        {
            // calsiir mi emin degilim denemeye deger 
            // sessionun suanki soru numarasinin bir sonrasini ariyor 
            var question = _context.Questions
                .Include(q=> q.Answers)
                .FirstOrDefault(q => q.QuizId == session.QuizId && q.Order == session.CurrentQuestion+1);
            if (question == null)

            {
                var leaderboard = await _context.SessionUsers
                    .Where(u => u.SessionId == Guid.Parse(sessionId)) 
                    .OrderByDescending(u => u.Point)   
                    .Select(u => new 
                    { 
                        userName = u.Username, 
                        score = u.Point 
                    })                                   
                    .ToListAsync();
            
                await Clients.Group(sessionId).SendAsync("EndSession", leaderboard);
                session.IsEnded = true;
            }
            else
            {
                // suanlik idare eder bir guvenlik onlemi olarak yaptim sonra jwt ye gecerim belki 
                var rawToken = _functions.TokenHasher(Guid.NewGuid().ToString());
                Token token = new()
                {
                    Id =Guid.NewGuid(),
                    IsUsed = false,
                    UserId = Guid.Parse(teacherId),
                    TokenHash = _functions.TokenHasher(rawToken)
                };
                // tokeni kullanildi isaretleyip yenisini urettim 
                tokenEntry.IsUsed = true;
                await _context.Tokens.AddAsync(token);
                await SendNewQuestion(session.Id, question);
                await Clients.Caller.SendAsync("SetTeacherToken",rawToken);
                session.CurrentQuestionId = question.Id;
                _timer.StartQuestionTimer(sessionId,question.Time);
                session.CurrentQuestion += 1;
                session.CurrentQuestionStartedAt = DateTime.Now;
            }
            await _context.SaveChangesAsync(); 
           
        }
        else { throw new HubException("OTURUM VEYA QUIZ BULUNAMADI"); }
    }
    
    public override async Task OnDisconnectedAsync(Exception ex)
    {
        await Clients.Group("ROOM1")
            .SendAsync("UserLeft", Context.ConnectionId);

        await base.OnDisconnectedAsync(ex);
    }
    
    private async Task SendNewQuestion(Guid sessionId,Question question)
    {
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

    public Task EndQuestion(Guid quizId)
    {
        return Task.CompletedTask;
    }
    public Task SubmitAnswer(Guid sessionId, string answerText)
    {
        var connectionId = Context.ConnectionId;
        var sender = _context.SessionUsers.FirstOrDefault(s => s.SessionId == sessionId && s.ConnectionId == connectionId);

        if (sender == null)
            throw new HubException("BU OTURUMA KAYITLI DEĞİLSİNİZ");
        else
        {
            var session = _context.Sessions
                .Include(s => s.Quiz).ThenInclude(quiz => quiz!.Questions).ThenInclude(question => question.Answers)
                .FirstOrDefault(s => s.Id == sessionId);
            if (session == null)
                throw new HubException("OYUN BULUNAMADI");
            else
            {
                // sure manipule edilebilir ona da bakacagim bir ara 
                var quiz = session.Quiz;
                if (quiz != null)
                {
                    var question = quiz.Questions.FirstOrDefault(q => q.Id == session.CurrentQuestionId);
                    var answered = question?.Answers.FirstOrDefault(a => a.Text == answerText);
    
                    if (answered is { IsCorrect: true })
                    {
                        // Session'dan başlama zamanını al
                        var questionStartTime = session.CurrentQuestionStartedAt; 
                        var answerTime = DateTime.Now;
        
                        if (questionStartTime.HasValue)
                        {
                            var elapsedTime = answerTime - questionStartTime.Value;
                            var elapsedSeconds = (int)elapsedTime.TotalSeconds;
            
                            var maxTimeSeconds = question.Time * 60;
                            var basePoint = question.Point;
                            
                            int calculatedPoint = 0;
                            if (elapsedSeconds < maxTimeSeconds)
                            {
                                calculatedPoint = CalculatePoints(basePoint, elapsedSeconds, maxTimeSeconds);
                                sender.Point += (calculatedPoint+basePoint);
                            }
                        }
                        _context.SaveChanges();
                    }
                    return Task.CompletedTask;
                }
                else
                {
                    throw new HubException("SESSION A KAYITLI OYUN BULUNAMADI");
                }
            }
        }

       
    }
    private int CalculatePoints(int basePoint, int elapsedSeconds, int maxTimeSeconds)
    {
        if (elapsedSeconds > maxTimeSeconds)
            return 0;
        double timePercentageLeft = (double)(maxTimeSeconds - elapsedSeconds) / maxTimeSeconds;
        double maxBonusRate = 0.40;
        double bonusMultiplier = timePercentageLeft * maxBonusRate; 
        int totalPoint = (int)(basePoint * (1 + bonusMultiplier));
        return totalPoint;
    }
    
}
