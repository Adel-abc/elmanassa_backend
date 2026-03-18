using elmanassa.ApplicationDbContext;
using elmanassa.DTOs;
using elmanassa.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace elmanassa.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDTO> Register(RegisterDTO model)
        {
            try
            {
                // Check if user already exists
                if (await UserExists(model.Email))
                {
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Email is already registered"
                    };
                }

                // Create new user
                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = model.Role.ToLower(),
                    AvatarUrl = model.AvatarUrl,
                    PhoneNumber = model.PhoneNumber,
                    NationalId = model.NationalId,
                    Bio = model.Bio,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                //Create new Student
                var stu = new Student
                {
                    UserId = user.Id,
                    User = user,
                    DateOfBirth = DateTime.MinValue, // Default value, can be updated later
                    EducationLevel = "Not Specified" // Default value, can be updated later
                };

                await _context.Users.AddAsync(user);
                await _context.Students.AddAsync(stu);
                await _context.SaveChangesAsync();


                // Generate token
                var token = GenerateJwtToken(user);

                return new AuthResponseDTO
                {
                    Success = true,
                    Message = "User registered successfully",
                    Token = token,
                    UserId = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Role = user.Role
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = $"Registration failed: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponseDTO> RegisterTeacher(TeacherRegisterDTO model)
        {
            try
            {
                if (await UserExists(model.Email))
                {
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Email is already registered"
                    };
                }

                // Create user record
                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = "teacher",
                    AvatarUrl = model.AvatarUrl,
                    PhoneNumber = model.PhoneNumber,
                    NationalId = model.NationalId,
                    Bio = model.Bio,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Create teacher profile record
                var teacher = new Teacher
                {
                    UserId = user.Id,
                    User = user,
                    Bio = model.Bio,
                    Specialization = model.Specialization,
                    YearsOfExperience = model.YearsOfExperience,
                    CvUrl = model.CvUrl
                };

                await _context.Users.AddAsync(user);
                await _context.Teachers.AddAsync(teacher);
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);

                return new AuthResponseDTO
                {
                    Success = true,
                    Message = "Teacher registered successfully",
                    Token = token,
                    UserId = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Role = user.Role
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = $"Registration failed: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponseDTO> Login(LoginDTO model)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                if (!user.IsActive)
                {
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "User account is inactive"
                    };
                }

                var token = GenerateJwtToken(user);

                return new AuthResponseDTO
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    UserId = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Role = user.Role
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = $"Login failed: {ex.Message}"
                };
            }
        }

        public async Task<bool> UserExists(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiryHoursStr = jwtSettings["ExpiryInHours"] ?? "24";
            var expiryHours = int.Parse(expiryHoursStr);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiryHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
