using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppRole: IdentityRole<int>
    //many to many relationship to users
    {
        public ICollection<AppUserRole> UserRoles { get; set; }
    }
}