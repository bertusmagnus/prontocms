using System.Net.Mail;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Mvc;
using Pronto.PagePlugins;

namespace Pronto.Controllers
{
    public class ContactFormController : Controller
    {
        public ActionResult ClientScript()
        {
            return File(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(ContactFormPlugin), "ContactForm.js"),
                "text/javascript"
            );
        }

        public void Send(string emailAddress, string name, string message)
        {
            var sender = new MailAddress(emailAddress, name);
            var recipient = new MailAddress(WebConfigurationManager.AppSettings["ContactForm.Recipient"]);
            var subject = WebConfigurationManager.AppSettings["ContactForm.Subject"] ?? "Message from website contact form";
            var mailMessage = new MailMessage(sender, recipient)
            {
                Subject = subject,
                Body = message
            };
            var client = new SmtpClient();
            client.Send(mailMessage);
        }
    }
}
