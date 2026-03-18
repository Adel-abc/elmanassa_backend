namespace elmanassa.DTOs
{
    public class AuthResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
    }
}
