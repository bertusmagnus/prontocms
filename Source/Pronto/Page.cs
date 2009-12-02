using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace Pronto
{
    public class Page : IPageContainer, IReadOnlyPage
    {
        public Page(IPageContainer pageContainer, string name, string title, string template, bool navigation, string description)
        {
            this.pageContainer = pageContainer;

            this.name = name;
            Title = title;
            Template = template;
            Navigation = navigation;
            Description = description;
            Pages = new List<Page>();
            contents = new Dictionary<string, string>();
            Path = BuildPath();
        }

        public Page(IPageContainer pageContainer, XElement element)
        {
            this.pageContainer = pageContainer;

            this.name = element.Attribute("name") != null ? element.Attribute("name").Value : "";
            Title = element.Attribute("title").Value;
            Template = element.Attribute("template").Value;
            Navigation = element.Attribute("navigation") == null ? true : element.Attribute("navigation").Value.Equals("true", StringComparison.OrdinalIgnoreCase);
            Description = element.Attribute("description") == null ? "" : element.Attribute("description").Value;
            Path = BuildPath();

            Pages = new List<Page>(from p in element.Elements("page")
                                   select new Page(this, p));

            contents = element.Elements("content").ToDictionary(
                e => e.Attribute("id") == null ? "" : e.Attribute("id").Value, 
                e => e.Value
            );
        }

        IPageContainer pageContainer;
        string name;
        Dictionary<string, string> contents;

        public string Name
        {
            get { return name; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Page Name cannot be null.");
                if (value.Contains('/'))
                    throw new ArgumentException("Page Name cannot contain a forward slash (/).");
                if (value.Contains('?'))
                    throw new ArgumentException("Page Name cannot contain a question mark (?).");
                if (value.Contains('#'))
                    throw new ArgumentException("Page Name cannot contain a hash (#).");

                ThrowIfAnotherPageHasName(value);
                
                name = value;
                Path = BuildPath();
            }
        }

        void ThrowIfAnotherPageHasName(string name)
        {
            if (pageContainer.Pages.Any(p => p.Name == name && p != this))
                throw new ArgumentException("There is already another page with the URL text \"" + name + "\".");
        }

        public string Title { get; set; }
        public string Template { get; set; }
        public bool Navigation { get; set; }
        public string Description { get; set; }
        public List<Page> Pages { get; set; }
        public string Path { get; private set; }

        public string NavigationText
        {
            get { return string.IsNullOrEmpty(Title) ? "Home" : Title; }
        }

        public void Delete()
        {
            pageContainer.Pages.Remove(this);
        }

        internal XElement SaveAsXElement()
        {
            return new XElement("page",
                new XAttribute("name", Name),
                new XAttribute("title", Title),
                new XAttribute("template", Template),
                (Navigation ? null : new XAttribute("navigation", "false")),
                (string.IsNullOrEmpty(Description) ? null : new XAttribute("description", Description)),
                contents.Select(kvp => new XElement("content", new XAttribute("id", kvp.Key), new XCData(kvp.Value))),
                from child in Pages
                select child.SaveAsXElement()
            );
        }

        public string GetContent(string id)
        {
            string data;
            if (contents.TryGetValue(id, out data))
                return data;
            else
                return "";
        }

        public void SetContent(string id, string content)
        {
            try
            {
                content.ParseXObjects();
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid XHTML.", ex);
            }
            contents[id] = content;
        }

        protected string GenerateNameFromTitle(string title)
        {
            return Regex.Replace(Regex.Replace(
                title.ToLowerInvariant()
                     .Replace("&", "-and-"), 
                @"[^a-z0-9_\-]", "-"
            ), "-{2,}", "-");
        }

        public Page AddNewSiblingPage(string title, string template, bool navigation, string description)
        {
            var name = GenerateNameFromTitle(title);
            if (name == Name) throw new ArgumentException("A page already has the name \"" + name + "\".");
            ThrowIfAnotherPageHasName(name);
            var page = new Page(pageContainer, name, title, template, navigation, description);
            page.SetContent("", "<h1>" + HttpUtility.HtmlEncode(title) + "</h1>");
            // Insert new page after this in parent collection.
            var index = pageContainer.Pages.IndexOf(this);
            pageContainer.Pages.Insert(index + 1, page);

            return page;
        }

        public Page FindPage(IEnumerable<string> names)
        {
            if (names == null) throw new ArgumentNullException("names");
            if (!names.Any()) return this;

            var page = Pages.FirstOrDefault(c => c.Name == names.First());
            if (page == null) return null;
            return page.FindPage(names.Skip(1));
        }

        public bool Contains(IReadOnlyPage page)
        {
            foreach (var child in Pages)
            {
                if (child == page) return true;
                if (child.Contains(page)) return true;
            }
            return false;
        }

        IReadOnlyPage IReadOnlyPage.FindPage(IEnumerable<string> names)
        {
            return FindPage(names);
        }

        bool IReadOnlyPage.Contains(IReadOnlyPage page)
        {
            foreach (IReadOnlyPage child in this)
            {
                if (child == page) return true;
                if (child.Contains(page)) return true;
            }
            return false;
        }

        internal Page CloneWithoutChildPages(IPageContainer pageContainer)
        {
            var clone = new Page(pageContainer, Name, Title, Template, Navigation, Description);
            foreach (var item in contents)
            {
                clone.contents[item.Key] = item.Value;
            }
            return clone;
        }

        string BuildPath()
        {
            return (pageContainer.Path.Length == 0 ? "" : pageContainer.Path + "/") + Name;
        }

        #region IEnumerable<IReadOnlyPage> Members

        IEnumerator<IReadOnlyPage> IEnumerable<IReadOnlyPage>.GetEnumerator()
        {
            foreach (var page in Pages)
            {
                yield return page;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<IReadOnlyPage>)this).GetEnumerator();
        }

        #endregion
    }    
}
