using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipalExtension // a static class cannot be instantiated
    {
        public static string GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value; //uniqueName
        }
        
        public static int GetUserId(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value); //uniqueName
        }
    }
}