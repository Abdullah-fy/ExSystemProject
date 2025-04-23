using Microsoft.AspNetCore.Mvc;

namespace ExSystemProject.DTOS
{
    public class MStudentDTO
    {
        public int id{ get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public bool? Isactive { get; set; }
         public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? Image{ get; set; }
        public string? TrackName { get; set; }
    }
}
