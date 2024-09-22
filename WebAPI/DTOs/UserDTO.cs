using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using WebAPI.Model;

namespace WebAPI.DTOs
{
    public class UserDTO
    {
        [Key]
        public int? user_id { get; set; }

        public string? first_name { get; set; }

        public string? last_name { get; set; }

        public string email { get; set; } = string.Empty;
        
        public string Password { get; set; } = string.Empty;

        public bool isActive { get; set; }
       // public IList<UserRole> UserRoles { get; set; }

    }
}
