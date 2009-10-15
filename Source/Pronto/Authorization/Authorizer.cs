using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Pronto.Authorization
{
    public class Authorizer : IReadOnlyAuthorizer
    {
        public Authorizer(XElement root)
        {
            admins = root.Elements("admin").Select(e => new Admin { Id = e.Attribute("id").Value, Name = e.Attribute("name").Value }).ToList();
            tokens = root.Elements("token").Select(e => e.Value).ToList();
        }

        List<Admin> admins;
        List<string> tokens;
        static Random rnd = new Random();

        public IEnumerable<Admin> Admins
        {
            get
            {
                return admins;   
            }
        }

        public bool IsUserAdmin(string id)
        {
            return admins.Any(a => a.Id == id);
        }

        public string CreateToken()
        {
            var s = string.Join("", Enumerable.Range(0, 12).Select(i => rnd.Next(0, 10).ToString()).ToArray());
            var token = s.Insert(9, "-").Insert(6, "-").Insert(3, "-");
            tokens.Add(token);
            return token;
        }

        public void AddAdmin(string id, string name, string token)
        {
            if (id == null) throw new ArgumentNullException("id");
            name = (name ?? "").Trim();
            if (name.Length == 0) throw new Exception("Please enter your name.");
            if (IsUserAdmin(id)) throw new Exception("The admin user " + name + "(" + id + ") already exists.");
            if (!tokens.Remove(token)) throw new Exception("Invalid sign-up number.");

            var admin = new Admin { Id = id, Name = name };
            admins.Add(admin);
        }

        public void AddAdmin(string id, string name)
        {
            if (id == null) throw new ArgumentNullException("id");
            name = (name ?? "").Trim();
            if (name.Length == 0) throw new Exception("Please enter your name.");
            if (IsUserAdmin(id)) throw new Exception("The admin user " + name + "(" + id + ") already exists.");
            if (admins.Count > 0) throw new Exception("Already added first admin user.");

            var admin = new Admin { Id = id, Name = name };
            
            admins.Add(admin);
        }

        public void DeleteAdmin(string id)
        {
            var admin = admins.FirstOrDefault(a => a.Id == id);
            if (admin != null) admins.Remove(admin);
        }

        internal void Save(string filename)
        {
            var doc = new XDocument(
                new XElement("authorization",
                    from admin in admins
                    select new XElement("admin", new XAttribute("id", admin.Id), new XAttribute("name", admin.Name)),

                    from token in tokens
                    select new XElement("token", token)
                )
            );
            doc.Save(filename);
        }
    }
}
