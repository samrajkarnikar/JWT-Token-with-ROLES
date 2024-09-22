using System.ComponentModel.DataAnnotations;

namespace WebAPI.Model
{
    public class User
    {
        [Key]
        public int? user_id { get; set; } 

        [Required]
        public string? first_name { get; set; } 

        [Required]
        public string? last_name { get; set; }

        [Required]

        public string? email { get; set; }

        [Required]
        [MinLength(5)]
        public byte[] passwordHash { get; set; }

        public byte[] passwordSalt { get; set; }

        [Required]

        public bool? isActive { get; set; }

        public IList<UserRole> UserRoles { get; set; }

    }
}
