using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Model
{
    public class AssignRole
    {
   
        public int UserRoleID { get; set; }

      
        public int user_id { get; set; }

   
        public int role_id { get; set; }
 

    }
}
