using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Pronto
{
    public class Website : IPageContainer, IReadOnlyWebsite
    {
        public Website(XElement website)
        {
            Title = website.Attribute("title").Value;

            var pageElements = website.Elements("page");
            pages = (from p in pageElements select new Page(this, p)).ToList();

            var contentElements = website.Elements("content");
            contents = contentElements.ToDictionary(c => c.Attribute("id").Value, c => c.Value);
        }

        Website(string title, IEnumerable<Page> pages)
        {
            Title = title;
            this.pages = pages.ToList();
            contents = new Dictionary<string, string>();
        }
        
        List<Page> pages;
        Dictionary<string, string> contents;

        public string Title { get; set; }

        public int Level
        {
            get { return -1; }
        }

        public Page FindPage(string path)
        {
            var names = path.Split('/');
            var name = names[0];
            var page = pages.FirstOrDefault(p => p.Name == name);
            if (page == null)
            {
                return FindPageWithinHomePage(names);
            }
            else
            {
                return page.FindPage(names.Skip(1));
            }
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

        public IReadOnlyPage FindCurrentPageAtLevel(int level, IReadOnlyPage currentPage)
        {
            if (level < 1) throw new ArgumentOutOfRangeException("level", level, "level must be greater than zero.");
            foreach (var page in this)
            {
                if (page == currentPage) return page;

                var p = FindCurrentPageAtLevel(1, level - 1, page, currentPage);
                if (p != null) return p;
            }
            return null;
        }

        IReadOnlyPage FindCurrentPageAtLevel(int currentLevel, int wantedLevel, IReadOnlyPage container, IReadOnlyPage currentPage)
        {
            foreach (var page in container)
            {
                if (page == currentPage)
                {
                    return (IReadOnlyPage)container;
                }
                if (page.Contains(currentPage))
                {
                    if (currentLevel == wantedLevel)
                    {
                        return page;
                    }
                    else
                    {
                        return FindCurrentPageAtLevel(currentLevel++, wantedLevel, page, currentPage);
                    }
                }
            }
            return null;
        }

        IReadOnlyPage IReadOnlyWebsite.FindPage(string path)
        {
            return FindPage(path);
        }

        Page FindPageWithinHomePage(string[] names)
        {
            var homePage = HomePage();
            if (homePage != null)
            {
                return homePage.FindPage(names);
            }
            else
            {
                return null;
            }
        }

        Page HomePage()
        {
            return pages.FirstOrDefault(p => p.Name.Length == 0);
        }

        public void Save(string filename)
        {
            var document = new XDocument(SerializeAsXElement());
            document.Save(filename);
        }

        XElement SerializeAsXElement()
        {
            return new XElement("website",
                new XAttribute("title", Title),
                
                from kvp in contents
                select new XElement("content", new XAttribute("id", kvp.Key), new XCData(kvp.Value)),

                from page in pages
                select page.SaveAsXElement()
            );
        }

        public List<Page> Pages
        {
            get { return pages; }
        }

        public string Path
        {
            get { return ""; }
        }

        public string GetContent(string id)
        {
            string content;
            if (contents.TryGetValue(id, out content))
                return content;
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
                throw new Exception("Invalid XHTML.", ex);
            }
            contents[id] = content;
        }

        public Website Reorganise(XDocument xml, ref string referrerPath)
        {
            var website = CloneWithoutChildPages();
            RebuildPages(xml.Root, website, null, ref referrerPath);
            return website;
        }

        Website CloneWithoutChildPages()
        {
            var website = new Website(Title, Enumerable.Empty<Page>());
            foreach (var kvp in contents)
            {
                website.contents[kvp.Key] = kvp.Value;
            }
            return website;
        }

        void RebuildPages(XElement parentElement, IPageContainer newPageContainer, Page parent, ref string referrerPath)
        {
            foreach (var pageElement in parentElement.Elements("page"))
            {
                var path = pageElement.Attribute("path").Value;
                var oldPage = FindPage(path);
                if (oldPage == null) throw new ArgumentException("Page not found at path: " + path);
                var newPage = oldPage.CloneWithoutChildPages(newPageContainer);
                newPageContainer.Pages.Add(newPage);
                RebuildPages(pageElement, newPage, newPage, ref referrerPath);

                if (referrerPath == oldPage.Path)
                {
                    referrerPath = newPage.Path;
                }
            }
        }

        IEnumerator<IReadOnlyPage> IEnumerable<IReadOnlyPage>.GetEnumerator()
        {
            foreach (var page in pages)
            {
                yield return page;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<IReadOnlyPage>)this).GetEnumerator();
        }
    }
}
