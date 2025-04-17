namespace ExSystemProject.DTOS
{
    public class TrackDTO
    {
        public int? TrackId { get; set; }

        public string? TrackName { get; set; } = null!;

        public int? TrackDuration { get; set; }

        public int? TrackIntake { get; set; }

        public bool? IsActive { get; set; }

        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
    }
}
