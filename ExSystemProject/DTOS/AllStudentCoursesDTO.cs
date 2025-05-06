namespace ExSystemProject.DTOS
{
    public class AllStudentCoursesDTO
    {
        public int Crs_Id { get; set; }
        public string Crs_Name { get; set; }
        public string Description { get; set; }
        public int Crs_period { get; set; }
        public DateTime EnrolledAt { get; set; }
        public string Grade { get; set; }
    }
}
