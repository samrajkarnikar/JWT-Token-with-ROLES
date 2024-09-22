using AutoMapper;
using WebAPI.DTOs;
using WebAPI.Model;



namespace WebAPI.Helper
{
    public class MappingProfiles: Profile
    {
        public MappingProfiles()
        {
            //Mapping between RegisterInputModel and user Model
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<User, Login>().ReverseMap();
            CreateMap<UserRole, AssignRole>().ReverseMap();
            //CreateMap<RegisterInputModel, User>().ReverseMap();
        }
    }
}
