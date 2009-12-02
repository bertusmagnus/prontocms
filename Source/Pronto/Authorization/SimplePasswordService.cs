using System.IO;
using System.Web;
using System.Web.Caching;

namespace Pronto.Authorization
{
    public class SimplePasswordService : FileBasedResourceService<SimplePassword, IReadOnlySimplePassword>
    {
        public SimplePasswordService(HttpServerUtilityBase server, Cache cache)
            : base(server.MapPath("~/App_Data/simplepassword.txt"), cache)
        {
        }

        protected override SimplePassword LoadResource(string filename)
        {
            return new SimplePassword(File.ReadAllText(filename));
        }

        protected override void SaveResource(SimplePassword resource, string filename)
        {
            File.WriteAllText(filename, resource.Value);
        }
    }

    public class SimplePassword : IReadOnlySimplePassword
    {
        public SimplePassword(string password)
        {
            this.password = password;
        }

        string password;

        public bool IsCorrect(string password)
        {
            return password == this.password;
        }

        public void ChangePassword(string newPassword)
        {
            password = newPassword;
        }

        public string Value { get { return password; } }
    }

    public interface IReadOnlySimplePassword
    {
        bool IsCorrect(string password);
    }
}
