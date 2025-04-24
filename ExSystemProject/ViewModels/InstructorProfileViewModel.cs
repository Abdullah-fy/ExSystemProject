using System.ComponentModel.DataAnnotations.Schema;
using ExSystemProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExSystemProject.ViewModels
{
    public class InstructorProfileViewModel
    {
        public string userName { get; set; }
        public string email { get; set; }
        public string? image { get; set; }
        public IFormFile? imageFile { get; set; }   
        public decimal salary { get; set; }
        public int trackId {  get; set; }
         public virtual Track? Tracks { get; set; } = null!;
          public virtual List<Course> courses { get; set; } = new List<Course>();

    }
}
