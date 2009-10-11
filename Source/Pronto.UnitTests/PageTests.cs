using System;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace Pronto
{
    public class Page_with_root_page_container
    {
        public Page_with_root_page_container()
        {
            pageContainer = new FakePageContainer();
            page = new Page(pageContainer, "test", "Test", "template.htm", true, "test");
            pageContainer.Pages.Add(page);
        }

        Page page;
        FakePageContainer pageContainer;

        [Fact]
        public void NavigationText_is_Test()
        {
            Assert.Equal("Test", page.NavigationText);
        }

        [Fact]
        public void Description_is_test()
        {
            Assert.Equal("test", page.Description);
        }

        [Fact]
        public void Path_is_test()
        {
            Assert.Equal("test", page.Path);
        }

        [Fact]
        public void Changing_Name_changes_Path()
        {
            page.Name = "foo";
            Assert.Equal("foo", page.Path);
        }

        [Fact]
        public void Setting_null_Name_throws()
        {
            Assert.Throws<ArgumentNullException>(() => page.Name = null);
        }

        [Fact]
        public void Setting_Name_with_forward_slash_throws()
        {
            Assert.Throws<ArgumentException>(() => page.Name = "foo/bar");
        }

        [Fact]
        public void Setting_Name_with_question_mark_throws()
        {
            Assert.Throws<ArgumentException>(() => page.Name = "foo?bar");
        }

        [Fact]
        public void Delete_removes_page_from_container()
        {
            page.Delete();
            Assert.DoesNotContain(page, pageContainer.Pages);
        }

        [Fact]
        public void Get_content_that_does_not_exist_returns_empty()
        {
            Assert.Equal("", page.GetContent("foo"));
        }

        [Fact]
        public void Set_content_can_be_retrieved()
        {
            page.SetContent("foo", "bar");
            Assert.Equal("bar", page.GetContent("foo"));
        }

        [Fact]
        public void Setting_invalid_HTML_content_throws()
        {
            Assert.Throws<ArgumentException>(() => page.SetContent("foo", "<p>invalid"));
        }

        [Fact]
        public void AddNewSiblingPage_inserts_page_into_container()
        {
            var sibling = page.AddNewSiblingPage("Sibling", "template.htm", true, "");
            Assert.Contains(sibling, pageContainer.Pages);
        }

        [Fact]
        public void AddNewSiblingPage_inserts_page_into_container_after_current_page()
        {
            var sibling = page.AddNewSiblingPage("Sibling", "template.htm", true, "");
            Assert.Equal(sibling, pageContainer.Pages[1]);
        }

        [Fact]
        public void AddNewSiblingPage_inserts_page_with_url_safe_name()
        {
            var sibling = page.AddNewSiblingPage("Sibling Foo Bar / Test", "template.htm", true, "");
            Assert.Equal("sibling-foo-bar-test", sibling.Name);
        }

        [Fact]
        public void AddNewSiblingPage_new_page_content_is_heading_title()
        {
            var sibling = page.AddNewSiblingPage("Sibling", "template.htm", true, "");
            Assert.Equal("<h1>Sibling</h1>", sibling.GetContent(""));
        }

        [Fact]
        public void AddNewSiblingPage_new_page_content_is_HTML_encoded_heading_title()
        {
            var sibling = page.AddNewSiblingPage("Sibling < foo & bar", "template.htm", true, "");
            Assert.Equal("<h1>Sibling &lt; foo &amp; bar</h1>", sibling.GetContent(""));
        }

        [Fact]
        public void FindPage_with_no_names_returns_current_page()
        {
            Assert.Equal(page, page.FindPage(Enumerable.Empty<string>()));
        }
    }

    public class Page_with_container_path_not_empty
    {
        public Page_with_container_path_not_empty()
	    {
            page = new Page(new FakePageContainer { Path = "home" }, "test", "Test", "template.htm", true, "");
	    }

        Page page;

        [Fact]
        public void Path_is_home_test()
        {
            Assert.Equal("home/test", page.Path);
        }
    }

    public class Page_with_empty_Title
    {
        public Page_with_empty_Title()
        {
            page = new Page(new FakePageContainer(), "", "", "template.htm", true, "");
        }

        Page page;

        [Fact]
        public void NavigationText_is_Home()
        {
            Assert.Equal("Home", page.NavigationText);
        }
    }

    public class Page_with_3_child_pages
    {
        public Page_with_3_child_pages()
        {
            page = new Page(new FakePageContainer(), "", "", "template.htm", true, "");
            page.Pages.Add(childPage1 = new Page(page, "page-1", "Page 1", "template.htm", true, ""));
            page.Pages.Add(new Page(page, "page-2", "Page 2", "template.htm", true, ""));
            page.Pages.Add(new Page(page, "page-3", "Page 3", "template.htm", true, ""));
        }

        Page page, childPage1;

        [Fact]
        public void FindPage_page_1_returns_first_child_page()
        {
            Assert.Equal(page.Pages[0], page.FindPage(new[] { "page-1" }));
        }

        [Fact]
        public void FindPage_page_3_returns_third_child_page()
        {
            Assert.Equal(page.Pages[2], page.FindPage(new[] { "page-3" }));
        }

        [Fact]
        public void FindPage_null_throws()
        {
            Assert.Throws<ArgumentNullException>(() => page.FindPage(null));
        }

        [Fact]
        public void FindPage_page_4_returns_null()
        {
            Assert.Null(page.FindPage(new[] { "page-4"} ));
        }

        [Fact]
        public void Contains_page1_returns_true()
        {
            Assert.True(page.Contains(childPage1));
        }

        [Fact]
        public void Does_not_contain_a_different_page()
        {
            Assert.False(page.Contains(new Page(new FakePageContainer(), "", "", "template.htm", false, "")));
        }

        [Fact]
        public void Adding_a_sibling_with_same_name_as_target_page_throws()
        {
            Assert.Throws<ArgumentException>(() => childPage1.AddNewSiblingPage("Page 1", "template.htm", true, ""));
        }

        [Fact]
        public void Adding_a_sibling_with_same_name_as_another_throws()
        {
            Assert.Throws<ArgumentException>(() => childPage1.AddNewSiblingPage("Page 2", "template.htm", true, ""));
        }
    }

    public class Nested_pages
    {
        public Nested_pages()
        {
            page1 = new Page(new FakePageContainer(), "", "", "template.htm", true, "");
            page1.Pages.Add(page2 = new Page(page1, "sub1", "Sub 1", "template.htm", true, ""));
            page2.Pages.Add(page3 = new Page(page2, "sub2", "Sub 1", "template.htm", true, ""));
        }

        Page page1, page2, page3;

        [Fact]
        public void FindPage_sub1_sub2_returns_page3()
        {
            Assert.Equal(page3, page1.FindPage(new[] { "sub1", "sub2" }));
        }

        [Fact]
        public void page1_contains_page3()
        {
            Assert.True(page1.Contains(page3));
        }
    }

    public class Page_created_from_XElement_with_navigation_attribute_set_to_false
    {
        public Page_created_from_XElement_with_navigation_attribute_set_to_false()
        {
            page = new Page(new FakePageContainer(), new XElement("page",
                new XAttribute("name", "page-1"),
                new XAttribute("title", "Page 1"),
                new XAttribute("template", "template.htm"),
                new XAttribute("navigation", "false"),
                new XElement("content", new XAttribute("id", ""), "data")
            ));
        }

        Page page;

        [Fact]
        public void Navigation_is_false_when_attribute_is_false()
        {
            Assert.False(page.Navigation);
        }

        [Fact]
        public void Default_content_is_data()
        {
            Assert.Equal("data", page.GetContent(""));
        }

        [Fact]
        public void Description_is_empty_string()
        {
            Assert.Equal("", page.Description);
        }
    }

    public class Page_created_from_XElement_without_navigation_attribute
    {
        public Page_created_from_XElement_without_navigation_attribute()
        {
            page = new Page(new FakePageContainer(), new XElement("page",
                new XAttribute("name", "page-1"),
                new XAttribute("title", "Page 1"),
                new XAttribute("template", "template.htm")
            ));
        }

        Page page;

        [Fact]
        public void Name_is_page_1()
        {
            Assert.Equal("page-1", page.Name);
        }

        [Fact]
        public void Title_is_Page_1()
        {
            Assert.Equal("Page 1", page.Title);
        }

        [Fact]
        public void Template_is_template_htm()
        {
            Assert.Equal("template.htm", page.Template);
        }

        [Fact]
        public void Navigation_is_true_when_attribute_is_missing()
        {
            Assert.True(page.Navigation);
        }
    }

    public class Page_created_from_XElement_with_description_attribute
    {
        public Page_created_from_XElement_with_description_attribute()
        {
            page = new Page(new FakePageContainer(), new XElement("page",
                new XAttribute("name", "page-1"),
                new XAttribute("title", "Page 1"),
                new XAttribute("template", "template.htm"),
                new XAttribute("description", "test"),
                new XElement("content", new XAttribute("id", ""), "data")
            ));
        }

        Page page;

        [Fact]
        public void Description_is_test()
        {
            Assert.Equal("test", page.Description);
        }
    }

    public class Page_create_with_nested_pages_in_XElement
    {
        public Page_create_with_nested_pages_in_XElement()
        {
            page = new Page(new FakePageContainer(), new XElement("page",
                new XAttribute("name", "page-1"),
                new XAttribute("title", "Page 1"),
                new XAttribute("template", "template.htm"),
                new XElement("page",
                    new XAttribute("name", "page-2"),
                    new XAttribute("title", "Page 2"),
                    new XAttribute("template", "template.htm")
                ),
                new XElement("page",
                    new XAttribute("name", "page-3"),
                    new XAttribute("title", "Page 3"),
                    new XAttribute("template", "template.htm")
                )
            ));
        }

        Page page;

        [Fact]
        public void page_has_2_children()
        {
            Assert.Equal(2, page.Pages.Count);
        }
    }

}
