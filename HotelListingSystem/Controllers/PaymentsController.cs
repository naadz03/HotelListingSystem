using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HotelListingSystem.Engines.PointSystem;
using HotelListingSystem.Models;
using HotelListingSystem.ViewModel;
using Microsoft.Graph.Models;
using PayPal.Api;
using Payment = HotelListingSystem.Models.Payment;


namespace HotelListingSystem.Controllers
{
    public class PaymentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Payments
        public ActionResult Index()
        {
            if (User.IsInRole("Customer"))
            {
                var cust = AppHelper.CurrentHotelUser().Id;
                var payments = db.Payments.Where(x => x.HotelUserId == cust).ToList();
                return View(payments);
            }
            else if (User.IsInRole("Admin"))
            {
                var payments = db.Payments.ToList();
                return View(payments);
            }
            else
            {
                return View(new List<Payment>());
            }
        }

        public ActionResult history()
        {
            return View(db.Payments.Include(d => d.Reservation.Hotel).Include(d => d.HotelUser).Where(c => c.IsPaid).ToList());
        }

        public ActionResult ReceiptDetails(int Id)
        {
            ReceiptDetailsViewModel paymentVM = new ReceiptDetailsViewModel();
            using(ApplicationDbContext context = new ApplicationDbContext())
            {
                paymentVM.Payment = context.Payments.Find(Id);
                paymentVM.Reservation = context.Reservations.Find(paymentVM.Payment.ReservationId);
                paymentVM.ReservationBy = context.HotelUsers.Find(paymentVM.Reservation.HotelUserId);
                paymentVM.BookedHotel = context.Hotels.Find(paymentVM.Reservation.HotelId);
                //paymentVM.Add_Ons = context.AddOnsRs.Where(a => a.Id == (int)paymentVM.Reservation.AddOnsId).FirstOrDefault();
                paymentVM.Room = context.Rooms.Find(paymentVM.Reservation.RoomId);
                paymentVM.Receptionist = context.HotelUsers.Find(paymentVM.BookedHotel.ReceptionistId);
            }
            return View(paymentVM);
        }
       
        public ActionResult Payments()
        {
            using(ApplicationDbContext context = new ApplicationDbContext())
            {
                var businessUser = AppHelper.CurrentHotelUser().Id;
                var businesspayments = (from payment in context.Payments
                                        join reseve in context.Reservations on payment.ReservationId equals reseve.Id
                                        join hotel in context.Hotels on reseve.HotelId equals hotel.Id
                                        join business in context.HotelUsers on hotel.HotelUserId equals business.Id
                                        join payer in context.HotelUsers on payment.HotelUserId equals payer.Id
                                        where (int)hotel.HotelUserId == businessUser
                                        select new BusinessPDFStatement
                                        {
                                            HotelName = hotel.Name,
                                            PaymentDate = payment.CreatedDateTime,
                                            PaymentBy = payer.FirstName + " " + payer.LastName,
                                            Amountpaid = payment.Amount,
                                            TravixComission = payment.Amount,
                                            PaymentId = payment.Id
                                        }).ToList();

                return View(businesspayments);
            }
           
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payments.Include(x => x.HotelUser).Include(x => x.Reservation).FirstOrDefault(x => x.Id == id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }


        public ActionResult Create(int id)
        {
            Reservation reservation = db.Reservations.Include(x => x.HotelUser).Include(x=>x.Room).Include(x=>x.Hotel).FirstOrDefault(x => x.Id == id);

            if (reservation != null)
            {
                string refno = PaymentReferenceGenerator();
                string desc = $"{reservation.Hotel.Name} {reservation.Room.Name} {(reservation.NoOfRooms > 1 ? "Tickets" : "Tickets")}";
                decimal total = reservation.TotalCost;

                //PayController pay = new PayController();
                //var payResult = PaymentWithPaypal(desc, refno, reservation.Route.Rate.ToString(), total.ToString(), reservation.NoOfReservations.ToString());

                Payment payment = new Payment();
                payment.CreatedDateTime = DateTime.Now;
                payment.ReservationId = reservation.Id;
                payment.HotelUserId = reservation.HotelUserId;
                payment.Amount = total;
                payment.RefNo = refno;
                payment.IsActive = true;
                payment.IsPaid = false;
                payment.PaymentMethod = "PayPal";
                payment.InvoiceNumber = InvReferenceGenerator();

                db.Payments.Add(payment);
                db.SaveChanges();
                var business = db.Businesses.Where(a => a.HotelUserId == reservation.HotelUserId).FirstOrDefault();
                return PaymentWithPaypal(desc, refno, (reservation.Room.PricePerRoom + reservation.AddOnsCost).ToString(), total.ToString(), reservation.NoOfRooms.ToString(), payment.InvoiceNumber, reservation.Hotel, reservation.HotelUser);

            }
            //ViewBag.CustomerId = new SelectList(db.Customers, "Id", "FirstName", payment.CustomerId);
            //ViewBag.ReservationId = new SelectList(db.Reservations, "Id", "To", payment.ReservationId);
            return View("_Error");
        }
        public ActionResult PaymentSuccess(string guid)
        {
            var payment = db.Payments.Include(x => x.HotelUser).Include(x => x.Reservation).FirstOrDefault(x => x.RefNo == guid);
            if (payment != null && payment?.IsPaid != true)
            {
                payment.IsPaid = true;
                payment.ModifiedDateTime = DateTime.Now;
                payment.Reservation.Booked = true;
                payment.Reservation.ModifiedOn = DateTime.Now;
                db.Entry(payment).State = EntityState.Modified;
                db.Entry(payment.Reservation).State = EntityState.Modified;
                db.SaveChanges();
            }
            return View(payment);
        }
        // GET: Payments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payments.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName", payment.HotelUserId);
            ViewBag.RouteId = new SelectList(db.Reservations, "Id", "To", payment.ReservationId);
            return View(payment);
        }

        // POST: Payments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CustomerId,RouteId,RefNo,IsPaid,Amount,IsActive,CreatedDateTime,ModifiedDateTime")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(payment).State = EntityState.Modified;
                db.SaveChanges();   
                return RedirectToAction("Index");
            }
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName", payment.HotelUserId);
            ViewBag.RouteId = new SelectList(db.Reservations, "Id", "To", payment.ReservationId);
            return View(payment);
        }

        // GET: Payments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payments.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Payment payment = db.Payments.Find(id);
            db.Payments.Remove(payment);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public static string PaymentReferenceGenerator()
        {
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                Random random = new Random();
                int randNumber = random.Next(10000, 99999);
                string RefPrefix = "PR";

                string refnos = string.Format("{0}{1}", RefPrefix, randNumber);
                _ = (context.Payments.Select(x => x.RefNo).Contains(refnos)) ? PaymentReferenceGenerator() : refnos;
                return refnos;
            }
            catch (Exception ex)
            {
                return PaymentReferenceGenerator();
            }
        }

        public ActionResult PaymentWithPaypal(string description, string refNo, string price, string total, string quantity, string invNo, Hotel hotel, HotelUsers User
             , string Cancel = null)
        {
            string guid = String.Empty;
            //getting the apiContext  
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {
                //A resource representing a Payer that funds a payment PayPal.Api.Payment Method as paypal  
                //Payer Id will be returned when payment proceeds or click to pay  
                string payerId = Request?.Params["PayerID"] ?? "";
                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist  
                    //it is returned by the create function call of the payment class  
                    // Creating a payment  
                    // baseURL is the url on which paypal sendsback the data.  
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Payments/PaymentWithPayPal?";
                    //here we are generating guid for storing the paymentID received in session  
                    //which will be used in the payment execution  
                    //var guid = Convert.ToString((new Random()).Next(100000));
                    guid = refNo;
                    //CreatePayment function gives us the payment approval url  
                    //on which payer is redirected for paypal account payment  
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid, description, refNo, price, total, quantity, invNo);
                    //get links returned from paypal in response to Create function call  
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment  
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    // saving the paymentID in the key guid  
                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment  
                    guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    //If executed payment failed then we will show payment failure message to user  
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("Failure");
                    }
                }
                UpdateHotelFees(hotel.Id, decimal.Parse(total));
                //hotel.AmountOwed += decimal.Parse((double.Parse(total) * 0.02).ToString());
                //db.Entry(hotel).State = EntityState.Modified;
                //db.SaveChanges();

                var body = $"Hi {User.FullName} thankyou for your payment ";
                new Email().SendEmail(User.EmailAddress, "Hotel Payment", User.FullName, body);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Failure", new { q = ex.Message });
            }

            

            //on successful payment, show success page to user.  
            return RedirectToAction("PaymentSuccess", new { guid = guid });
        }
        private PayPal.Api.Payment payment;
        private PayPal.Api.Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new PayPal.Api.Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }
        private PayPal.Api.Payment CreatePayment(APIContext apiContext, string redirectUrl, string description, string reference, string price, string total, string quantity, string invNo)
        {
            double usd = 17.61;
            //price = $"{decimal.Parse(price) * decimal.Parse(usd.ToString())}";
            //total = $"{decimal.Parse(price) * decimal.Parse(usd.ToString())}";
            decimal decTotal = decimal.Parse(price) * int.Parse(quantity);
            //create itemlist and add item objects to it  
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            //Adding Item Details like name, currency, price etc  
            itemList.items.Add(new Item()
            {
                name = description,
                currency = "USD",
                price = price.Replace(',', '.'),
                quantity = quantity,
                sku = reference
            });
            var payer = new Payer()
            {
                payment_method = "paypal"
            };
            // Configure Redirect Urls here with RedirectUrls object  
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };
            // Adding Tax, shipping and Subtotal details  
            var details = new Details()
            {
                tax = "1",
                shipping = "1",
                subtotal = total.Replace(',', '.'),
            };
            decTotal += decimal.Parse(details.tax);
            decTotal += decimal.Parse(details.shipping);

            string totalAmount = decTotal.ToString("#.##");
            //Final amount with details  
            var amount = new Amount()
            {
                currency = "USD",
                total = totalAmount, // Total must be equal to sum of tax, shipping and subtotal.  
                details = details
            };

            var transactionList = new List<Transaction>();
            // Adding description about the transaction  
            transactionList.Add(new Transaction()
            {
                description = description,
                invoice_number = invNo, //Generate an Invoice No  
                amount = amount,
                item_list = itemList
            });
            this.payment = new PayPal.Api.Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            // Create a payment using a APIContext  
            return this.payment.Create(apiContext);
        }

        public static string InvReferenceGenerator()
        {
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                Random random = new Random();
                int randNumber = random.Next(10000, 99999);
                string RefPrefix = "INV";

                string refnos = string.Format("{0}{1}", RefPrefix, randNumber);
                _ = (context.Payments.Select(x => x.RefNo).Contains(refnos)) ? InvReferenceGenerator() : refnos;
                return refnos;
            }
            catch (Exception ex)
            {
                return InvReferenceGenerator();
            }
        }

        public ActionResult Failure(string q)
        {
            ViewBag.Error = q;
            return View();
        }

        [Authorize(Roles = "Administrator, Receptionist")]
        public ActionResult HotelPyaments()
        {
            List<Payment> payments = new List<Payment>();
            List<IGrouping<int, Hotel>> owinghotels = new List<IGrouping<int, Hotel>>();

            try
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    var payment = context.Payments
                        .Include(a => a.Reservation.Hotel.HotelUser)
                        .Where(c => c.ReservationId != null)
                        .ToList();

                    if (User.IsInRole("Administrator"))
                    {
                        payments = payment;
                        owinghotels = payment
                            .Select(a => a.Reservation.Hotel)
                            .GroupBy(grp => grp.Id)
                            .ToList();
                    }
                    else
                    {
                        var user = AppHelper.CurrentHotelUser().Id;
                        payments = payment
                            .Where(a => (a.Reservation.Hotel.HotelUser.Id == user || a.Reservation.Hotel.ReceptionistId == user))
                            .ToList();
                        owinghotels = payments
                            .Select(a => a.Reservation.Hotel)
                            .GroupBy(grp => grp.Id)
                            .ToList();
                    }
                }
            }
            catch
            {
                // Handle exception
            }

            ViewBag.Amount = ((owinghotels.Count() == 0 && owinghotels.First().First().HotelUser.SystemRates == null) ? "0.00" : ((decimal)owinghotels.First().First().HotelUser.SystemRates).ToString().Replace(',', '.')).ToString();
            ViewBag.Ownerid = (owinghotels.Count() == 0) ? 0 : owinghotels.First()?.First()?.HotelUser?.Id;
            ViewBag.FullName = AppHelper.CurrentHotelUser()?.FullName ?? "";
            return View(owinghotels);
        }


        public ActionResult ServicePayments()
        {
            var ReceptOrOwner = AppHelper.CurrentHotelUser().Id;
            var hotels = db.Hotels
                .Where(a => a.HotelUserId == ReceptOrOwner || a.ReceptionistId == ReceptOrOwner)
                .Select(a => new { ownerId = a.HotelUserId, receptId = a.ReceptionistId })
                .ToList();
            var ownerId = hotels.Select(a => a.ownerId).ToList();
            var receptId = hotels.Select(a => a.receptId).ToList();
            var history = db.Payments.Include(c => c.Hotel.HotelUser).Include(c => c.HotelUser).Where(a => a.Servicepayment && (ownerId.Contains(a.HotelUserId) || receptId.Contains(a.HotelUserId))).ToList();
            return View(history);
        }




        public ActionResult CreditDebitPaymentYoco(int reservationId, string yocco_ref)
        {
            try
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    var reservation = context.Reservations.Find(reservationId);
                    Payment payment = new Payment();
                    payment.YoccoReferrence = yocco_ref;
                    payment.CreatedDateTime = DateTime.Now;
                    payment.ReservationId = reservation.Id;
                    payment.HotelUserId = reservation.HotelUserId;
                    payment.Amount = reservation.TotalCost;
                    payment.RefNo = GetpaymentReferrence("YC", context);
                    payment.IsActive = true;
                    payment.IsPaid = false;
                    payment.PaymentMethod = "YOCCO Debit/Credit";
                    payment.InvoiceNumber = InvReferenceGenerator();
                    payment.IsPaid = true;
                    payment.ModifiedDateTime = DateTime.Now;
                    context.Payments.Add(payment);
                    context.SaveChanges();

                    reservation.Booked = true;
                    reservation.Updated = true;
                    reservation.PaymentApproved = true;
                    reservation.UpdatedById = reservation.HotelUserId;
                    reservation.ModifiedOn = DateTime.Now;
                    context.Entry(reservation).State = EntityState.Modified;
                    context.SaveChanges();
                    new PointHelper(context).AddUserPoints(AppHelper.CurrentHotelUser().Id, (Int32)reservation.TotalCost);
                    UpdateHotelFees((int)reservation.HotelId, reservation.TotalCost);

                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {

            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }
        public static string GetpaymentReferrence(string Prefix, ApplicationDbContext context)
        {
            try
            {
                Random random = new Random();
                int randNumber = random.Next(10000, 99999);
                string refnos = string.Format("{0}{1}", Prefix, randNumber);
                _ = (context.Payments.Select(x => x.RefNo).Contains(refnos)) ? GetpaymentReferrence(Prefix, context) : refnos;
                return refnos;
            }
            catch 
            {
                GetpaymentReferrence(Prefix, context);
            }
            return null;
        }

        public static void UpdateHotelFees(int id, decimal amount)
        {
            try
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    var hotel = context.Hotels.Find(id);
                    var user = context.HotelUsers.Find(hotel.HotelUserId);

                    hotel.ModifiedDate = DateTime.Now;
                    hotel.AmountOwed += amount * decimal.Parse("0,02");
                    user.SystemRates = (user.SystemRates == null) ? (hotel.AmountOwed) : (user.SystemRates + hotel.AmountOwed);
                    context.Entry(hotel).State = EntityState.Modified;
                    context.Entry(user).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }
            catch
            {

            }
        }


        public ActionResult PaySystemFeesBulk(int id, string yocco_ref)
        {
            try
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    var owner = context.HotelUsers.Find(id);
                    var hotels = context.Hotels.Where(a => a.HotelUserId == owner.Id).ToList();
                    foreach(var h in hotels)
                    {
                        h.AmountOwed = 0;
                        //h.IsBlackListed = false;
                        h.PaymentDoneDate = DateTime.Now;
                        h.ModifiedDate = DateTime.Now;
                        context.Entry(h).State = EntityState.Modified;
                    }
                    _ = SaveBusinessPayment(context, $"Bulk Payment [{hotels.Count()}]", (decimal)owner.SystemRates, AppHelper.CurrentHotelUser().Id, null, yocco_ref);
                    owner.SystemRates = 0;
                    owner.LastPaymentDate = DateTime.Now;
                    context.Entry(owner).State = EntityState.Modified;
                    context.SaveChanges();
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {

            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PaySystemFeesSingle(int id, int HotelId, string yocco_ref)
        {
            try
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    var owner = context.HotelUsers.Find(id);
                    var hotel = context.Hotels.Find(HotelId);
                    //hotel.IsBlackListed = false;
                    hotel.PaymentDoneDate = DateTime.Now;
                    hotel.ModifiedDate = DateTime.Now;
                    owner.SystemRates = (owner.SystemRates - hotel.AmountOwed);
                    _ = SaveBusinessPayment(context, "Single Payment", hotel.AmountOwed, AppHelper.CurrentHotelUser().Id, HotelId, yocco_ref);
                    hotel.AmountOwed = 0;
                    owner.LastPaymentDate = DateTime.Now;
                    context.Entry(hotel).State = EntityState.Modified;
                    context.Entry(owner).State = EntityState.Modified;
                    context.SaveChanges();
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {

            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public static Payment SaveBusinessPayment(ApplicationDbContext context, string type, decimal amount,int userId, int? hotelId, string yocco_ref)
        {
            Payment payment = new Payment();
            payment.YoccoReferrence = yocco_ref;
            payment.CreatedDateTime = DateTime.Now;
            payment.HotelUserId = userId;
            payment.HotelId = hotelId;
            payment.Amount = amount;
            payment.PaymentType = type;
            payment.RefNo = GetpaymentReferrence("YC", context);
            payment.IsActive = true;
            payment.IsPaid = false;
            payment.Servicepayment = true;
            payment.PaymentMethod = "YOCCO Debit/Credit";
            payment.InvoiceNumber = InvReferenceGenerator();
            payment.IsPaid = true;
            payment.ModifiedDateTime = DateTime.Now;
            context.Payments.Add(payment);
            context.SaveChanges();
            return payment;
        }

        public ActionResult SendBlacklistEmail(int id)
        {
            try
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    var hotel = context.Hotels.Find(id);
                    var owner = context.HotelUsers.Find(hotel.HotelUserId);
                   
                    hotel.IsBlackListed = false;
                    hotel.NotificationDate = DateTime.Now;
                    hotel.IsNotified = true;
                    context.Entry(hotel).State = EntityState.Modified;
                    context.SaveChanges();

                    new Email().SendEmail(owner.EmailAddress, "Payment Due", $"{owner.FullName}", $"Your hotel {hotel.Name} has been marked for blacklist if you do not settle the overdue funds immedietly.");

                    
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {

            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Blacklisthotel(int id)
        {
            try
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    var hotel = context.Hotels.Find(id);
                    var owner = context.HotelUsers.Find(hotel.HotelUserId);
                   
                    hotel.IsBlackListed = true;
                    hotel.NotificationDate = DateTime.Now;
                    hotel.IsNotified = true;
                    context.Entry(hotel).State = EntityState.Modified;
                    context.SaveChanges();

                    new Email().SendEmail(owner.EmailAddress, "Blacklist", $"{owner.FullName}", $"Your hotel {hotel.Name} has been blacklisted due to pennding payments. To reactivate your hotel you need to sttle the payments and await approval.");

                    
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {

            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }
        public ActionResult WhitelistHotel(int id)
        {
            try
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    var hotel = context.Hotels.Find(id);
                    var owner = context.HotelUsers.Find(hotel.HotelUserId);
                   
                    hotel.IsBlackListed = false;
                    hotel.NotificationDate = DateTime.Now;
                    hotel.IsNotified = false;
                    context.Entry(hotel).State = EntityState.Modified;
                    context.SaveChanges();

                    new Email().SendEmail(owner.EmailAddress, "Whitelist", $"{owner.FullName}", $"Your hotel {hotel.Name} has been whitelisted you will now be getting bookings via our channel.");

                    
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {

            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }
    }
}
