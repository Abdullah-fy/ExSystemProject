using System.ComponentModel.DataAnnotations.Schema;
using ExSystemProject.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExSystemProject.ViewModels
{
    public class StudentViewModel
    {
        public string? username { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? Image { get; set; }
        public string? password { get; set; }
        public string? TrackId { get; set; }
        [ForeignKey("TrackId")]
        public List<SelectListItem> tracks { get; set; }
    }
}

