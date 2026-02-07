using AutoMapper;
using KahootMvc.AppContext;
using KahootMvc.Dtos.UsersDto;
using KahootMvc.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace KahootMvc.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UserController(AppDbContext context, IMapper mapper) 
        {
            _mapper = mapper;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("KayitOl")]
        public IActionResult KayitOl([FromBody] SignInUserDto signuser)
        {
            var sonucusername = _context.Users.FirstOrDefault(x => x.Username == signuser.Username);
            var sonucemail = _context.Users.FirstOrDefault(x => x.Email == signuser.Email);

            if (sonucemail == null && sonucusername == null)
            {
                var newuser = _mapper.Map<User>(signuser);
                _context.Users.Add(newuser);
                _context.SaveChanges();
                    // AJAX bunu success:true olarak görecek
                    return Json(new
                    {
                        success = true,
                        redirectUrl = Url.Action("Index", "User")
                    });
            }
            else if (sonucusername != null)
                return Json(new { success = false, message = "Bu kullanıcı adı kullanımda" });

            else if (sonucemail != null)
                return Json(new { success = false, message = "Bu e mail kullanımda" });

            else
                return Json(new { success = false, message = "Kayıt işlemi başarısız" });
        }


        [HttpPost("GirisYap")]
        public IActionResult GirisYap([FromForm] LoginUserDto loginuser)
        {
            if (loginuser != null)
            {
               var user = _context.Users.FirstOrDefault(x=> x.Username==loginuser.Email||x.Email==loginuser.Email);
                if (user != null)
                {
                    if(user.Password == loginuser.Password)
                        return Json(new { 
                            success = true, 
                            redirectUrl = "ogretmen.html",
                            userid = user.Id.ToString()
                        });
                    else
                        return Json(new { success = false, message = "Email/kullanıcı adı veya şifre hatalı." });
                }
                else
                {
                    return Json(new { success = false, message = "Email/kullanıcı adı veya şifre hatalı." });
                }
            }
            return Json(new { success = false, message = "Email/kullanıcı adı veya şifre hatalı." });
        }



    }
}
