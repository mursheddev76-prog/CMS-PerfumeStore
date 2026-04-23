using System.ComponentModel.DataAnnotations;

namespace PerfumeStore.Web.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }

        public string LoginType { get; set; } = "customer";
    }
}
