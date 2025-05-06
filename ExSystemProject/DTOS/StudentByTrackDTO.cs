namespace ExSystemProject.DTOS
{
    public class StudentByTrackDTO
    {
        public int Studentid { get; set; }
        public string track_id { get; set; }
        public string userid { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public bool isActive { get; set; }
    }
}
