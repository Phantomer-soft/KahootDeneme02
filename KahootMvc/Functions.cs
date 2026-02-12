using System.Security.Cryptography;
using System.Text;
using KahootMvc.AppContext;
using KahootMvc.Models;

namespace KahootMvc;

public class Functions
{
    public readonly AppDbContext _context;
    public Functions(AppDbContext context)
    {
        _context = context;
    }
    public string TokenHasher(string token)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(token));
            
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
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