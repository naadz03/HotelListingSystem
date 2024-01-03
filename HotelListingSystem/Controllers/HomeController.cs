using HotelListingSystem.Models;
using HotelListingSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Xml.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Globalization;
using HotelListingSystem.Helpers;

namespace HotelListingSystem.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string message)
        {
            var today = DateTime.Now.Date.DayOfWeek;

            VisitorsHelper.AddVisit(Session, Request);
            return View();
        }

        public static String GetDayWithNum(Int32 day, String value = null)
        {
            switch (day)
            {
                case 1:
                    value = "Mon";
                    break;
                case 2:
                    value = "Tue";
                    break;
                case 3:
                    value = "Wed";
                    break;
                case 4:
                    value = "Thu";
                    break;
                case 5:
                    value = "Fri";
                    break;
                case 6:
                    value = "Sat";
                    break;
                default:
                    value = "Sun";
                    break;
            }
            return value;
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult SendMessage(string email, string name, string body, string subject)
        {
            string b = "Your enquiry, " + body + " has been submitted and will be attended to by an agent soon.";
            new Email().SendEmail(email, "Hotel enquiry: " + subject, name, b, false);
            return Json(new { success = true, message = "Hotel updated successfully" });

        }


        public ActionResult GenerateBusinessPDFStatement(int businessUserId)    
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var StatementName = $"B{DateTime.Now.ToString("yyyyMMddHHmmss")}{businessUserId}.pdf";
            var folderName = string.Format("businesspayments");
            var root = HostingEnvironment.MapPath($"~/{folderName}/");
            var path = Path.Combine(root, StatementName);
            var contentType = "application/pdf";
            path = System.IO.Path.GetFullPath(path);
            iTextSharp.text.Document document = new iTextSharp.text.Document(PageSize.A4.Rotate());

            var businesspayments = (from payment in context.Payments
                                    join reseve in context.Reservations on payment.ReservationId equals reseve.Id
                                    join hotel in context.Hotels on reseve.HotelId equals hotel.Id
                                    join business in context.HotelUsers on hotel.HotelUserId equals business.Id
                                    join payer in context.HotelUsers on payment.HotelUserId equals payer.Id
                                    where (int)hotel.HotelUserId == businessUserId
                                    select new BusinessPDFStatement
                                    {
                                        HotelName = hotel.Name,
                                        PaymentDate = payment.CreatedDateTime,
                                        PaymentBy = payer.FirstName + " " + payer.LastName,
                                        Amountpaid = payment.Amount,
                                        TravixComission = payment.Amount
                                    }).ToList();

            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                PdfWriter writer = PdfWriter.GetInstance(document, fileStream);

                PdfPTable table = new PdfPTable(5);
                document.Open();
                // Set table properties
                table.WidthPercentage = 100f;
                float[] columnWidths = { 2f, 1f, 1f, 1f, 1f }; // Adjust column widths as needed
                table.SetWidths(columnWidths);
                //table.DefaultCell.Border = Rectangle.NO_BORDER; // Remove cell borders
                table.DefaultCell.BorderColor = BaseColor.GRAY; // Set cell border color

                // Add table headers
                AddTableCell(table, "HOTEL NAME", bold: true);
                AddTableCell(table, "PAYMENT DATE", bold: true);
                AddTableCell(table, "PAYMENT BY", bold: true);
                AddTableCell(table, "AMOUNT PAID", bold: true);
                AddTableCell(table, "COMMISSION", bold: true);
                foreach (var payment in businesspayments)
                    AddTableRow(table, payment.HotelName, payment.PaymentDate, payment.PaymentBy, payment.Amountpaid, payment.TravixComission);
                var businessUser = context.HotelUsers.Find(businessUserId);
                Font headingFont = new Font(Font.FontFamily.HELVETICA, 20f, Font.BOLD);
                Paragraph heading = new Paragraph($"Business Statement Of: {businessUser.FullName} : {StatementName.Replace(".pdf","")}", headingFont);
                heading.Alignment = Element.ALIGN_CENTER;
                heading.SpacingAfter = 20f; // Set spacing after the header
                document.Add(heading);
                document.Add(table);
                document.Close();
            }
            return File(System.IO.File.ReadAllBytes(path), contentType, StatementName);
        }
        public ActionResult ServicePaymentsPDFStatement(int businessUserId)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var StatementName = $"SV{DateTime.Now.ToString("yyyyMMddHHmmss")}{businessUserId}.pdf";
            var folderName = string.Format("businesspayments");
            var root = HostingEnvironment.MapPath($"~/{folderName}/");
            var path = Path.Combine(root, StatementName);
            var contentType = "application/pdf";
            path = System.IO.Path.GetFullPath(path);
            iTextSharp.text.Document document = new iTextSharp.text.Document(PageSize.A4.Rotate());

            var hotels = context.Hotels
                .Where(a => a.HotelUserId == businessUserId || a.ReceptionistId == businessUserId)
                .Select(a => new { ownerId = a.HotelUserId, receptId = a.ReceptionistId })
                .ToList();
            var ownerId = hotels.Select(a => a.ownerId).ToList();
            var receptId = hotels.Select(a => a.receptId).ToList();

            List<Payment> ServicePayments = context.Payments
                                            .Include("HotelUser")
                                            .Include("Hotel")
                                            .Where(c => c.Servicepayment && (ownerId.Contains(c.HotelUserId) || receptId.Contains(c.HotelUserId)))
                                            .ToList();

            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                PdfWriter writer = PdfWriter.GetInstance(document, fileStream);

                PdfPTable table = new PdfPTable(6);
                document.Open();
                // Set table properties
                table.WidthPercentage = 100f;
                float[] columnWidths = { 1f, 1f, 1f, 1f, 1f , 1f }; // Adjust column widths as needed
                table.SetWidths(columnWidths);
                //table.DefaultCell.Border = Rectangle.NO_BORDER; // Remove cell borders
                table.DefaultCell.BorderColor = BaseColor.GRAY; // Set cell border color

                // Add table headers
                AddTableCell(table, "HOTEL NAME", bold: true);
                AddTableCell(table, "PAYMENT DATE", bold: true);
                AddTableCell(table, "PAYMENT BY", bold: true);
                AddTableCell(table, "PAYMENT TYPE", bold: true);
                AddTableCell(table, "PAYMENT REF", bold: true);
                AddTableCell(table, "AMOUNT PAID", bold: true);
                foreach (var payment in ServicePayments)
                    AddTableRow2(table, (payment.Hotel == null) ? "Multiple Hotels" : payment.Hotel.Name, payment.CreatedDateTime, payment.HotelUser.FullName, payment.PaymentType, payment.RefNo, payment.Amount);
                var businessUser = context.HotelUsers.Find(businessUserId);
                Font headingFont = new Font(Font.FontFamily.HELVETICA, 20f, Font.BOLD);
                Paragraph heading = new Paragraph($"Service Statement Of: {businessUser.FullName} : {StatementName.Replace(".pdf","")}", headingFont);
                heading.Alignment = Element.ALIGN_CENTER;
                heading.SpacingAfter = 20f; // Set spacing after the header
                document.Add(heading);
                document.Add(table);
                document.Close();
            }
            return File(System.IO.File.ReadAllBytes(path), contentType, StatementName);
        }


     



        #region  statement helpers
        static void AddTableRow(PdfPTable table, string HotelName, DateTime? PaymentDate, string paymentBy, decimal amountpaid, decimal comission)
        {
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            table.AddCell(textInfo.ToTitleCase(HotelName));
            table.AddCell($"{PaymentDate}");
            table.AddCell(textInfo.ToTitleCase(paymentBy));
            table.AddCell(amountpaid.ToString("C"));
            table.AddCell((Convert.ToInt16(comission) * 0.02).ToString("C"));
        }
        static void AddTableRow2(PdfPTable table, string HotelName, DateTime? PaymentDate, string paymentBy, string PaymentType, string refNo, decimal amountpaid)
        {
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            table.AddCell(textInfo.ToTitleCase(HotelName));
            table.AddCell($"{PaymentDate}");
            table.AddCell(textInfo.ToTitleCase(paymentBy));
            table.AddCell(textInfo.ToTitleCase(PaymentType));
            table.AddCell(textInfo.ToTitleCase(refNo));
            table.AddCell(amountpaid.ToString("C"));
        }

        static void AddTableCell(PdfPTable table, string content, bool bold = false)
        {
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            content = textInfo.ToTitleCase(content);

            PdfPCell cell = new PdfPCell(new Phrase(content));
            //cell.Border = Rectangle.NO_BORDER; // Remove cell borders
            cell.BorderColor = BaseColor.GRAY; // Set cell border color
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            if (bold)
            {
                cell.Phrase.Font.SetStyle(Font.BOLD);
            }
            table.AddCell(cell);
        }
        #endregion
    }
}