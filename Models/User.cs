namespace TH1_SOA.Models
{
    public class User
    {
        public int IdUser { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // Chuỗi Hash/Base64 từ client
        public string? Token { get; set; }
    }
}
