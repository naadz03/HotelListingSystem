using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using HotelListingSystem.Helpers;
using System.Configuration;

namespace HotelListingSystem.ViewModel
{
    public class Email
    {
        public void SendEmail(string Email, string subject, string Name, string status, bool isVerify = true)
        {
			try
			{
                MailMessage mail = new MailMessage();
                MailAddress from = new MailAddress("africanmagicsystem@gmail.com");
                mail.From = from;
                mail.Subject = subject;
                mail.IsBodyHtml = true;
                if (isVerify)
                {
                    mail.Body = "Hi  " + Name + "<br/>Your hotel status is  " + status + "<br/><br/>Your's Sincerely<br/><strong>Hotel Listing Team</strong> ";
                }
                else
                {
                    mail.Body = "Hi  " + Name + "<br/><br/>"+ status + " <br/><br/>Your's Sincerely<br/><strong>Hotel Listing Team</strong> ";
                }
                mail.To.Add(Email);

                //mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                NetworkCredential networkCredential = new NetworkCredential("HotelListVX@gmail.com", "ujzzmzrxomafbwkb");
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = networkCredential;
                smtp.Port = 587;
                smtp.Send(mail);
                //Dispose of email.
                mail.Dispose();
            }
			catch (Exception ex)
			{
                //alternate logic
            }
        }

        public void SendEmail(string Email, string subject, string Name, string body)
        {
            try
            {
                MailMessage mail = new MailMessage();
                MailAddress from = new MailAddress("africanmagicsystem@gmail.com");
                mail.From = from;
                mail.Subject = subject;
                mail.IsBodyHtml = true;
                mail.Body = String.Format($"Hi {Name} <br/> {body} <br/><br/>Your's Sincerely<br/><strong>Hotel Listing Team</strong>");
                mail.To.Add(Email);

                //mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                NetworkCredential networkCredential = new NetworkCredential("HotelListVX@gmail.com", "ujzzmzrxomafbwkb");
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = networkCredential;
                smtp.Port = 587;
                smtp.Send(mail);
                //Dispose of email.
                mail.Dispose();
            }
            catch (Exception ex)
            {
                //alternate logic
            }
        }
        public void SendEmail(string Email, string subject, string Name, string body, string emailKey = default,
            string Hotel = null, string status=null)
        {
            try
            {
                MailMessage mail = new MailMessage();
                MailAddress from = new MailAddress("africanmagicsystem@gmail.com");
                mail.From = from;
                mail.Subject = subject;
                mail.IsBodyHtml = true;
                var location = String.Format("{0}/Helpers/ImageHelper/UserConfig", ConfigurationManager.AppSettings["domain"]);
                switch (emailKey)
                {
                    case "e_user_config":
                        String customBody = EmailTemplates.UserConfiguration;
                        customBody = customBody.Replace("{#Name}", Name);
                        customBody = customBody.Replace("{#Body}", body);
                        customBody = customBody.Replace("#ImgLocation", location);
                        mail.Body = customBody;
                        break;
                    case "e_refund_mail":
                        String refund = EmailTemplates.Refund;
                        refund = refund.Replace("[Customer Name]", Name);
                        refund = refund.Replace("[Status]", "<strong><i>Refund Confirmed: Approved</i></strong>");
                        refund = refund.Replace("[Hotel Name]", Hotel);
                        refund = refund.Replace("[Current Date]", DateTime.Now.ToLongDateString());
                        mail.Body = refund;
                        break;
                    case "e_checkin_reminder":
                        String checkin = EmailTemplates.CheckinOrOutReminder;
                        checkin = checkin.Replace("[Hotel Name]", Hotel);
                        checkin = checkin.Replace("September", DateTime.Now.ToLongDateString());
                        mail.Body = checkin;
                        break;
                    case "e_checkout_reminder":
                        String checkout = EmailTemplates.CheckinOrOutReminder;
                        checkout = checkout.Replace("[Hotel Name]", Hotel);
                        checkout = checkout.Replace("September", DateTime.Now.ToLongDateString());
                        mail.Body = checkout;
                        break;
                    default:
                        mail.Body = String.Format("Dear {0}, <br/> {1} <br/><br/>Your's Sincerely<br/><strong>Hotel Listing Team</strong>", Name, body);
                        break;
                }

                //mail.To.Add(Email);
                mail.To.Add("ngxongosiyanda@gmail.com");

                //mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                NetworkCredential networkCredential = new NetworkCredential("HotelListVX@gmail.com", "ujzzmzrxomafbwkb");
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = networkCredential;
                smtp.Port = 587;
                smtp.Send(mail);
                //Dispose of email.
                mail.Dispose();
            }
            catch (Exception ex)
            {
                //alternate logic
            }
        }

        public void SendException(Exception ex)
        {

        }

    }
}