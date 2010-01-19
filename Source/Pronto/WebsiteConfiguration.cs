using System.IO;
namespace Pronto
{
    public class WebsiteConfiguration
    {
        public WebsiteConfiguration(string websiteXmlFilename, string themeName, string templateDirectory, string emptyContentAdminInstruction)
        {
            WebsiteXmlFilename = websiteXmlFilename;
            ThemeName = themeName;
            TemplateDirectory = templateDirectory;
            EmptyContentAdminInstruction = emptyContentAdminInstruction;
        }

        public string WebsiteXmlFilename { get; private set; }
        public string ThemeName { get; set; }
        public string TemplateDirectory { get; private set; }
        public string EmptyContentAdminInstruction { get; set; }
    }
}
