namespace KahootMvc.Models
{
    public enum Roles //ENUM KULLANDIM Kİ İLERİDE EKLEME ÇIKARMA YAPMAK İSTERSEM TEK DEĞİŞTİRMEM GEREKEN YER BURASI OLSUN 
    {
        admin=0,
        teacher=1,
        student=2, 
    }
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Roles Role { get; set; } = Roles.student;
        public ICollection<Quiz> Quizzes { get; set; }
    }
}
