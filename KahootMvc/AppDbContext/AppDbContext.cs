using KahootMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace KahootMvc.AppContext
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<PinCode> PinCodes { get; set; }
        public DbSet<LeaderBoard> LeaderBoards { get; set; }
        public DbSet<SessionUser> SessionUsers { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<SessionLeaderboard> SessionLeaderboards { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.Quiz)
                .WithMany(q => q.Sessions)
                .HasForeignKey("QuizId")
                .OnDelete(DeleteBehavior.NoAction);
     
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(q => q.Questions)
                .HasForeignKey(q => q.QuizId);
            // elle verdim de bi ara bakarim gerekirse 

            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Title = "Genel Kültür"
                },
                new Category
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Title = "Matematik"
                },
                new Category
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Title = "Tarih"
                },
                new Category
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Title = "Bilim"
                },
                new Category
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    Title = "Teknoloji"
                },
                new Category
                {
                    Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    Title = "Spor"
                },
                new Category
                {
                    Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                    Title = "Sanat"
                },
                new Category
                {
                    Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                    Title = "Diğer"
                }
            );
        }


    }
}
