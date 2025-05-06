using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExSystemProject.ViewModels
{
    public class CourseListViewModel
    {
        public IEnumerable<CourseNewDTO> Courses { get; set; }  // Changed from CourseDTO to CourseNewDTO
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public bool? IsActive { get; set; }
        public int? BranchId { get; set; }
        public int? TrackId { get; set; }
        public SelectList Branches { get; set; }
        public SelectList Tracks { get; set; }
    }
}
