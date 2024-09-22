using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Model
{
    public class UserRole
    {
        [Key]
        public int UserRoleID { get; set; }

        [ForeignKey("user_id")]
        public int user_id { get; set; }

        [ForeignKey("role_id")]
        public int role_id { get; set; }
        public User? Users { get; set; }
        public Role? Roles { get; set; }

    }
}
