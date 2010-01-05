namespace Pronto
{
    public class WebsiteConfiguration
    {
        public WebsiteConfiguration(string websiteXmlFilename, string themeDirectory, string templateDirectory, string emptyContentAdminInstruction)
        {
            WebsiteXmlFilename = websiteXmlFilename;
            ThemeDirectory = themeDirectory;
            TemplateDirectory = templateDirectory;
            EmptyContentAdminInstruction = emptyContentAdminInstruction;
        }

        public string WebsiteXmlFilename { get; private set; }
        public string ThemeDirectory { get; private set; }
        public string TemplateDirectory { get; private set; }
        public string EmptyContentAdminInstruction { get; set; }
    }
}
