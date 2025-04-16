using System.ComponentModel.DataAnnotations;

namespace ExSystemProject.ViewModels
{
    public class LoginViewModel
    {
        public string Username { get; set; }
        [DataType(DataType.Password)]
        public string password { get; set; }
        public bool RememberMe { get; set; }
    }
}
