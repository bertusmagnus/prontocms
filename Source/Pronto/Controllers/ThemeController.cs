using System;
using System.IO;
using System.Net;
using System.Web.Mvc;

namespace Pronto.Controllers
{
    public class ThemeController : Controller
    {
        public ThemeController(WebsiteConfiguration websiteConfiguration)
        {
            this.websiteConfiguration = websiteConfiguration;
        }

        WebsiteConfiguration websiteConfiguration;

        public ActionResult GetFile(string path)
        {
            if (string.IsNullOrEmpty(path) || path.Contains(".."))
            {
                Response.StatusCode = 404;
                return new EmptyResult();
            }

            var filename = Path.Combine(websiteConfiguration.ThemeDirectory, path);
            if (System.IO.File.Exists(filename))
            {
                return FileIfModified(filename);
            }
            else
            {
                Response.StatusCode = 404;
                return new EmptyResult();
            }
        }

        ActionResult FileIfModified(string filename)
        {
            var modified = System.IO.File.GetLastWriteTimeUtc(filename);
            if (HasFileChanged(modified))
            {
                Response.AppendHeader("Last-Modified", modified.ToString("r"));
                var contentType = GetContentType(filename);
                return File(filename, contentType);
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.NotModified;
                return new EmptyResult();
            }
        }

        bool HasFileChanged(DateTime modified)
        {
            DateTime ifModifiedSinceHeader;
            if (!DateTime.TryParse(Request.Headers["If-Modified-Since"], out ifModifiedSinceHeader))
            {
                return true;
            }
            return modified > ifModifiedSinceHeader;
        }

        string GetContentType(string filename)
        {
            switch (Path.GetExtension(filename).ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".css":
                    return "text/css";
                case ".js":
                    return "text/javascript";
                default: throw new Exception("Cannot determine content type for file: " + filename);
            }
        }

        // For future use maybe, when we want a theme switching admin screen...

        //[AuthorizeAdmin]
        //public void Change(string theme)
        //{
        //    var config = WebConfigurationManager.OpenWebConfiguration(null);
        //    config.AppSettings.Settings["theme"].Value = theme;
        //    config.Save();
        //}

        //[AuthorizeAdmin]
        //public ActionResult List()
        //{
        //    return Json(Directory.GetDirectories(Server.MapPath("~/themes")).Select(d => Path.GetDirectoryName(d)));
        //}
    }
}
