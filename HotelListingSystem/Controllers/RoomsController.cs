using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HotelListingSystem.Models;

namespace HotelListingSystem.Controllers
{
    public class RoomsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Rooms
        public ActionResult IndexAll()
        {
            return View(db.Rooms.Include(x => x.Hotel).ToList());
        }
        public ActionResult Index()
        {
            var user = AppHelper.CurrentHotelUser().Id;
            var rooms = db.Rooms.Include(x => x.Hotel).Where(x => x.Hotel.HotelUserId == user).ToList();
            return View(rooms);
        }

        // GET: Rooms/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Room room = db.Rooms.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            return View(room);
        }

        // GET: Rooms/Create
        public ActionResult Create(int? hotelId)
        {
            Room room = new Room();
            if (hotelId != null)
            {
                var hotel = db.Hotels.FirstOrDefault(a => a.Id == hotelId);
                if (hotel.IsVerified == null) return View("_AwaitingVerification");

                ViewBag.AddRooms = "true";
                room.HotelId = hotelId;
            }
            var CurrentUser = AppHelper.CurrentHotelUser();
            ViewBag.HotelId = new SelectList(db.Hotels.Where(a => (bool)a.IsVerified && a.HotelUserId == CurrentUser.Id), "Id", "Name");
            return View(room);
        }

        // POST: Rooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Room room, List<HttpPostedFileBase> documents)
        {
            if (ModelState.IsValid)
            {
                room.CreatedOn = DateTime.Now;
                db.Rooms.Add(room);
                db.SaveChanges();
                if (documents != null)
                {
                    int current = 0;
                    foreach (var doc in documents)
                    {
                        var file = doc;
                        var fileContent = file.InputStream;
                        byte[] data;
                        data = new byte[fileContent.Length];
                        file.InputStream.Read(data, 0, file.ContentLength);
                        switch (current)
                        {
                            case 0:
                                #region old
                                //room.RoomImageName1 = file.FileName;
                                //room.RoomImageContentType1 = file.ContentType;
                                //room.RoomImageContent1 = data;
                                //room.RoomImageFileSize1 = (Int64)file.ContentLength;
                                //room.RoomImageName2 = file.FileName;
                                //room.RoomImageContentType2 = file.ContentType;
                                //room.RoomImageContent2 = data;
                                //room.RoomImageFileSize2 = (Int64)file.ContentLength;
                                #endregion
                                SaveHotelDocument(file.FileName, file.ContentType, data, (Int64)file.ContentLength, room.Id, "r_hotel_room_first");
                                break;
                            case 1:
                                SaveHotelDocument(file.FileName, file.ContentType, data, (Int64)file.ContentLength, room.Id, "r_hotel_room_2nd");
                                break;
                        }
                        current++;
                    }
                }
                
                return RedirectToAction("Index");
            }
            ViewBag.HotelId = new SelectList(db.Hotels, "Id", "Name");
            return View(room);
        }



        public static void SaveHotelDocument(string fileName, string fileType, byte[] fileBytes, Int64 fileSize, int roomId, string doc_key_type)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                Document document = context.Documents
                    .FirstOrDefault(c => c.RoomId == roomId && c.DocumentTypeKey == doc_key_type);
                if (document == null)
                {
                    document = new Document();
                    File ofile = new File();
                    ofile.ContentType = fileType;
                    ofile.Content = fileBytes;
                    ofile.FileName = fileName;
                    ofile.FileSize = fileSize;
                    ofile.IsActive = true;
                    ofile.IsDeleted = false;
                    ofile.CreatedDateTime = DateTime.Now;
                    ofile.ModifiedDateTime = DateTime.Now;
                    context.Files.Add(ofile);
                    context.SaveChanges();

                    document.IsActive = true;
                    document.IsDeleted = false;
                    document.CreatedDateTime = DateTime.Now;
                    document.ModifiedDateTime = DateTime.Now;
                    document.FileId = ofile.Id;
                    document.DocumentTypeKey = doc_key_type;
                    document.RoomId = roomId;
                    context.Documents.Add(document);
                    context.SaveChanges();
                }
                else
                {
                    File ofile = context.Files.Find(document.FileId);
                    ofile.ContentType = fileType;
                    ofile.Content = fileBytes;
                    ofile.FileName = fileName;
                    ofile.FileSize = fileSize;
                    ofile.ModifiedDateTime = DateTime.Now;
                    document.ModifiedDateTime = DateTime.Now;
                    context.Entry(ofile).State = EntityState.Modified;
                    context.Entry(document).State = EntityState.Modified;
                    context.SaveChanges();
                }

            }
        }
        public ActionResult DownloadRoomImageFile(int Id, string doc_key_type)
        {
            try
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    Document document = context.Documents.FirstOrDefault(a => a.RoomId == Id && a.DocumentTypeKey == doc_key_type);
                    File file = context.Files.Find(document.FileId);
                    if (document.DocumentTypeKey == "a_customer_liveness_image")
                    {
                        Response.ContentType = "image/png";
                        return File(file.Content, file.FileName);
                    }
                    return File(file.Content, file.ContentType, file.FileName);
                }
            }
            catch
            {

            }
            return null;
        }





        public ActionResult DisplayImage(int roomId, int imageType)
        {
            var hotel = db.Rooms.FirstOrDefault(h => h.Id == roomId);
            if (imageType == 1 && hotel != null && hotel.RoomImageContent1 != null)
            {
                return File(hotel.RoomImageContent1, hotel.RoomImageContentType1);
            }
            else if (imageType == 2 && hotel != null && hotel.RoomImageContent2 != null)
            {
                return File(hotel.RoomImageContent1, hotel.RoomImageContentType2);
            }
            else
            {
                // Return a default image or an error image
                // depending on your requirements
                return File("~/Content/default_image.jpg", "image/jpeg");
            }
        }


        // GET: Rooms/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Room room = db.Rooms.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            return View(room);
        }

        // POST: Rooms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Sleeps,NoOfBeds,Baths,Balcony,RoomImageName1,RoomImageContent1,RoomImageContentType1,RoomImageFileSize1,RoomImageName2,RoomImageContent2,RoomImageContentType2,RoomImageFileSize2")] Room room)
        {
            if (ModelState.IsValid)
            {
                db.Entry(room).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(room);
        }

        // GET: Rooms/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Room room = db.Rooms.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            return View(room);
        }

        // POST: Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Room room = db.Rooms.Find(id);
            db.Rooms.Remove(room);
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
    }
}
