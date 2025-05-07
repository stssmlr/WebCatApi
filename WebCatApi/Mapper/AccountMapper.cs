using AutoMapper;
using WebCatApi.Data.Entities;
using WebCatApi.Models.Account;

namespace WebCatApi.Mapper;

public class AccountMapper : Profile
{
    public AccountMapper()
    {
        CreateMap<RegisterViewModel, UserEntity>()
            .ForMember(x => x.UserName, opt => opt.MapFrom(x => x.Email));
    }
}