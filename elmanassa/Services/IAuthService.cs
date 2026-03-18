using elmanassa.DTOs;

namespace elmanassa.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> Register(RegisterDTO model);
        Task<AuthResponseDTO> RegisterTeacher(TeacherRegisterDTO model);
        Task<AuthResponseDTO> Login(LoginDTO model);
        Task<bool> UserExists(string email);
    }
}
