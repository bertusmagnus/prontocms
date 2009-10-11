namespace Pronto
{
    public class WebsiteConfiguration
    {
        public WebsiteConfiguration(string websiteXmlFilename, string themeDirectory, string templateDirectory)
        {
            WebsiteXmlFilename = websiteXmlFilename;
            ThemeDirectory = themeDirectory;
            TemplateDirectory = templateDirectory;
        }

        public string WebsiteXmlFilename { get; private set; }
        public string ThemeDirectory { get; private set; }
        public string TemplateDirectory { get; private set; }
    }
}
