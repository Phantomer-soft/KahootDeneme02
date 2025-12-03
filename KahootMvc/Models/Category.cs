namespace KahootMvc.Models
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Title { get; set; }=string.Empty;
        public ICollection<Question> Questions { get; set; }=new HashSet<Question>();

    }
}
