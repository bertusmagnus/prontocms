using System.Web.Mvc;

namespace Pronto
{
    public class AuthorizeAdminAttribute : AuthorizeAttribute
    {
        public AuthorizeAdminAttribute()
        {
            Roles = "admin";
        }
    }
}
