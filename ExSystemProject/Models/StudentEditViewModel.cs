using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExSystemProject.ViewModels
{
    public class StudentEditViewModel
    {
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [StringLength(1)]
        public string Gender { get; set; }

        public int? TrackId { get; set; }

        public bool IsActive { get; set; }

        public List<SelectListItem> Tracks { get; set; }
    }
}
