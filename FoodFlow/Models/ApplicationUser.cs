using Microsoft.AspNetCore.Identity;

namespace FoodFlow.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
