using System.ComponentModel.DataAnnotations;



namespace A1Patients.Models
{
    public class CreatesRole
    {
        [Required]

        public string Role { get; set; }
        public object Username { get; internal set; }
        public object Email { get; internal set; }
    }
}
