namespace ExSystemProject.DTOS
{
    public class TrackDTO
    {
        public int? track_id { get; set; }

        public string? track_name { get; set; } = null!;

        public int? track_duration { get; set; }

        public int? track_intake { get; set; }

        public bool? is_active { get; set; }

        public int? branch_id { get; set; }
        public string? branch_name { get; set; }
    }
}
