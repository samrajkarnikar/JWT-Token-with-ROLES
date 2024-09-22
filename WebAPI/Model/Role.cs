using System.ComponentModel.DataAnnotations;

namespace WebAPI.Model
{
    public class Role
    {
        [Key]
        public int role_id { get; set; }
        public string role_name { get; set; }

        public IList<UserRole> UserRoles { get; set; }

    }
}
