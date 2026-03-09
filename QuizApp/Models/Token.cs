namespace KahootMvc.Models;

public class Token
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool IsUsed { get; set; } =  false;
    public string TokenHash { get; set; } =  string.Empty;
}