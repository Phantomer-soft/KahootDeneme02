using AutoMapper;
using KahootMvc.Dtos.Answers;
using KahootMvc.Dtos.QuizzesDto;
using KahootMvc.Dtos.SessionsDto;
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

            CreateMap<Quiz, GetQuizInfoDto>().ReverseMap();
            CreateMap<Quiz,CreateQuizDto>().ReverseMap();
            CreateMap<Answer, CreateAnswerDto>().ReverseMap();
        }
    }
}
