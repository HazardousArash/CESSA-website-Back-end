using Microsoft.AspNetCore.Identity;

namespace Cessa.Domain.Entities.Auth
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public const string Admin = "Admin";
        public const string ContentCreator = "ContentCreator";
        public const string Student = "Student";
    }
}
