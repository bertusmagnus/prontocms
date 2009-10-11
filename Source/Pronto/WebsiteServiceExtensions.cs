using System;

namespace Pronto
{
    public static class WebsiteServiceExtensions
    {
        public static void UpdatePage(this IWebsiteService websiteService, string path, Action<Page> update)
        {
            if (websiteService == null) throw new ArgumentNullException("websiteService");
            if (update == null) throw new ArgumentNullException("update");

            using (var writer = websiteService.CreateWriter())
            {
                update(writer.Resource.FindPage(path));
            }
        }
    }
}
