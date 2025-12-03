using AutoMapper;
using KahootMvc.Dtos.UsersDto;
using KahootMvc.Models;

namespace KahootMvc.Mapping
{
    public class GeneralMapping : Profile
    {
        public GeneralMapping() 
        {
            CreateMap<User, SignInUserDto>().ReverseMap(); // Kayıt olan kullanıcı türünü ve kullanıcı türünü mapledim
            CreateMap<User, LoginUserDto>().ReverseMap();
        }
    }
}
