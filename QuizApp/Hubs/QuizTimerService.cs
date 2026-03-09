using KahootMvc.AppContext;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KahootMvc.Hubs;

public class QuizTimerService
{
    private readonly IHubContext<QuizHub> _hubContext;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public QuizTimerService(IHubContext<QuizHub> hubContext, IServiceScopeFactory serviceScopeFactory)
    {
        _hubContext = hubContext;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public void StartQuestionTimer(string sessionId, int seconds)
    {
        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds + 2));
            
            await _hubContext.Clients.Group(sessionId).SendAsync("TimeUp");
            await Task.Delay(2000);


            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var leaderboard = await context.SessionUsers
                .Where(u => u.SessionId == Guid.Parse(sessionId)) 
                .OrderByDescending(u => u.Point)   
                .Select(u => new 
                { 
                   userName = u.Username, 
                    score = u.Point 
                })                                   
                .ToListAsync();
            
            await _hubContext.Clients.Group(sessionId).SendAsync("UpdateLeaderboard", leaderboard);
        });
    }
}