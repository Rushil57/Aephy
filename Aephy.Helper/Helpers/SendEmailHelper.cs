using System.Net.Mail;
using System.Net;

namespace Aephy.Helper.Helpers;

public class SendEmailHelper
{
    public static bool SendEmail(string mailTo, string mailSubject, string mailBody)
    {
        try
        {
            using (MailMessage mail = new MailMessage())
            {
                string emailFrom = "noreply@ephylink.com";
                mail.From = new MailAddress(emailFrom);
                mail.To.Add(mailTo);
                mail.Subject = mailSubject;
                mail.Body = mailBody;
                mail.IsBodyHtml = true;
                using (SmtpClient smtp = new SmtpClient("smtppro.zoho.eu", 587))
                {
                    smtp.Credentials = new NetworkCredential(emailFrom, "xWg4KhtVHBN1");
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Send(mail);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}
