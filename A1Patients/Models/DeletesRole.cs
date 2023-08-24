using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace A1Patients.Models
{
    public class DeletesRole
    {
        public DeletesRole()
        {
            Users = new List<string>();
        }
        public string RoleId { get; set; }
        [Required(ErrorMessage = "role name is mandatory!")]
        public string RoleName { get; set; }
        public List<string> Users { get; set; }
    }
}
