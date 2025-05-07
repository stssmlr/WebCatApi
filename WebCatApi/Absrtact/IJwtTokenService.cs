using WebCatApi.Data.Entities;

namespace WebCatApi.Absrtact
{
    public interface IJwtTokenService
    {
        Task<string> CreateTokenAsync(UserEntity user);
    }
}
