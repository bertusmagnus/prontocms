using System;
using System.Xml.Linq;
using Xunit;

namespace Pronto
{
    public class WebsiteTests
    {
        public WebsiteTests()
        {
            website = new Website(XElement.Parse(
@"<website title=""My Website"">
    <content id="""">default data</content>
    <content id=""foo"">foo data</content>
    <page name="""" title="""" template=""template.htm"">
        <page name=""child"" title=""Child"" template=""template.htm""/>
    </page>
    <page name=""page-1"" title=""Page 1"" template=""template.htm""/>
    <page name=""page-2"" title=""Page 2"" template=""template.htm"">
        <page name=""page-3"" title=""Page 3"" template=""template.htm""/>
    </page>
</website>"));
        }

        Website website;

        [Fact]
        public void Title_is_My_Website()
        {
            Assert.Equal("My Website", website.Title);
        }

        [Fact]
        public void Has_3_pages()
        {
            Assert.Equal(3, website.Pages.Count);
        }

        [Fact]
        public void Default_content_is_default_data()
        {
            Assert.Equal("default data", website.GetContent(""));
        }

        [Fact]
        public void Foo_content_is_foo_data()
        {
            Assert.Equal("foo data", website.GetContent("foo"));
        }

        [Fact]
        public void Missing_content_is_empty_string()
        {
            Assert.Equal("", website.GetContent("missing"));
        }

        [Fact]
        public void Path_is_empty()
        {
            Assert.Equal("", website.Path);
        }

        [Fact]
        public void Can_find_page_3()
        {
            Assert.NotNull(website.FindPage("page-2/page-3"));
        }

        [Fact]
        public void Can_find_page_foo()
        {
            Assert.Null(website.FindPage("page-foo"));
        }

        [Fact]
        public void Can_find_child_within_home_page()
        {
            Assert.NotNull(website.FindPage("child"));
        }

        [Fact]
        public void Reorganise_creates_new_website_and_sets_moved_path()
        {
            string path = "page-2/page-3";
            var newWebsite = website.Reorganise(new XDocument(new XElement("website",
                new XElement("page", new XAttribute("path", "")),
                new XElement("page", new XAttribute("path", "child")),
                new XElement("page", new XAttribute("path", "page-1")),
                new XElement("page", new XAttribute("path", "page-2")),
                new XElement("page", new XAttribute("path", "page-2/page-3"))
            )), ref path);

            Assert.Equal(5, newWebsite.Pages.Count);
            Assert.Equal("page-3", path);
        }

        [Fact]
        public void Reorganise_throws_when_invalid_page_path_given()
        {
            string path = "page-2/page-3";
            Assert.Throws<ArgumentException>(() => website.Reorganise(new XDocument(new XElement("website",
                new XElement("page", new XAttribute("path", "")),
                new XElement("page", new XAttribute("path", "child")),
                new XElement("page", new XAttribute("path", "page-1")),
                new XElement("page", new XAttribute("path", "page-2")),
                new XElement("page", new XAttribute("path", "page-error"))
            )), ref path));
        }

        [Fact]
        public void Can_find_page_at_level()
        {
            var data = new Website(XElement.Parse(
@"<website title=""My Website"">
    <page name="""" title="""" template=""template.htm""/>
    <page name=""page-1"" title=""Page 1"" template=""template.htm""/>
    <page name=""page-2"" title=""Page 2"" template=""template.htm"">
        <page name=""page-3"" title=""Page 3"" template=""template.htm""/>
        <page name=""page-4"" title=""Page 4"" template=""template.htm"">
            <page name=""page-4-1"" title=""Page 4-1"" template=""template.htm""/>
            <page name=""page-4-2"" title=""Page 4-2"" template=""template.htm""/>
        </page>
        <page name=""page-5"" title=""Page 5"" template=""template.htm""/>
    </page>
</website>"));

            Assert.Same(data.Pages[2].Pages[1], data.FindCurrentPageAtLevel(2, data.Pages[2].Pages[1].Pages[1]));
        }
    }
}
