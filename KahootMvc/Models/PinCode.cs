namespace KahootMvc.Models
{
    public class PinCode
    {       

        public PinCode(int pin)
        {
             Pin = pin;
        }

        public int Id { get; set; }
        public int Pin { get; set; }

    }
}
