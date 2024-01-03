using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HotelListingSystem.Models;
using Microsoft.Ajax.Utilities;

namespace HotelListingSystem.Controllers
{
    public class BusinessesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Businesses
        public ActionResult Index()
        {
            return View(db.Businesses.ToList());
        }

        // GET: Businesses/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                var currentUser = User.Identity.Name;
                var findHotel = db.Businesses.Include(x=>x.HotelUser).FirstOrDefault(x=>x.HotelUser.UserName == currentUser);
                if (findHotel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                id = findHotel.Id;
            }

            Business business = db.Businesses.Find(id) ?? throw new Exception("Not Found");
            if (business == null)
            {
                return HttpNotFound();
            }
            return View(business);
        }

        // GET: Businesses/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Businesses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Age,NameOfBank,AccountNumber,BranchCode,PhysicalAddress1,PhysicalAddress2,PhysicalAddress3,PhysicalAddress4,PhysicalAddress5,PhysicalAddressCode,BusinessEmail,CertificateOfOccupancyDocName,CertificateOfOccupancyDocContent,CertificateOfOccupancyDoContentType,CertificateOfOccupancyDoFileSize,CertificateOfOccupancyDocVerified,COADocName,COADocContent,COADocContentType,COADocFileSize,COADocVerified")] Business business)
        {
            if (ModelState.IsValid)
            {

                var context = HttpContext;

                try
                {
                    var file = context.Request.Files[0];
                    if (file.ContentLength > 0)
                    {
                        Stream fileContent = file.InputStream;
                        business.CertificateOfOccupancyDocName = file.FileName;
                        business.CertificateOfOccupancyDoContentType = file.ContentType;
                        business.CertificateOfOccupancyDocContent = new byte[fileContent.Length];
                        business.CertificateOfOccupancyDoFileSize = file.ContentLength;
                        fileContent.Read(business.CertificateOfOccupancyDocContent, 0, (int)fileContent.Length);
                    }
                    var file1 = context.Request.Files[1];
                    if (file1.ContentLength > 0)
                    {
                        Stream fileContent = file1.InputStream;
                        business.COADocName = file1.FileName;
                        business.COADocContentType = file1.ContentType;
                        business.COADocContent = new byte[fileContent.Length];
                        business.COADocFileSize = file1.ContentLength;
                        fileContent.Read(business.CertificateOfOccupancyDocContent, 0, (int)fileContent.Length);
                    }

                    business.CreatedOn = DateTime.Now;

                    db.Businesses.Add(business);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                return RedirectToAction("Index");
            }
            return View(business);
        }

        // GET: Businesses/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Business business = db.Businesses.Find(id);
            if (business == null)
            {
                return HttpNotFound();
            }
            return View(business);
        }

        // POST: Businesses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Age,NameOfBank,AccountNumber,BranchCode,PhysicalAddress1,PhysicalAddress2,PhysicalAddress3,PhysicalAddress4,PhysicalAddress5,PhysicalAddressCode,BusinessEmail,CertificateOfOccupancyDocName,CertificateOfOccupancyDocContent,CertificateOfOccupancyDoContentType,CertificateOfOccupancyDoFileSize,CertificateOfOccupancyDocVerified,COADocName,COADocContent,COADocContentType,COADocFileSize,COADocVerified")] Business business)
        {
            if (ModelState.IsValid)
            {
                db.Entry(business).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(business);
        }

        // GET: Businesses/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Business business = db.Businesses.Find(id);
            if (business == null)
            {
                return HttpNotFound();
            }
            return View(business);
        }

        // POST: Businesses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Business business = db.Businesses.Find(id);
            db.Businesses.Remove(business);
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

        public ActionResult GetDocument(int id, int doctype)
        {
            var documentDownload = db.Businesses.Find(id);
            switch (doctype)
            {
                case 1:
                    var mimTypeCOADocName = MimeMapping.GetMimeMapping(documentDownload.COADocName);
                    if (documentDownload == null)
                    {
                        throw new Exception("Document not found");

                    }
                    return File(documentDownload.COADocContent, mimTypeCOADocName, documentDownload.COADocName);

                    case 2:
                    var CertificateOfOccupancyDocName = MimeMapping.GetMimeMapping(documentDownload.CertificateOfOccupancyDocName);
                    if (documentDownload == null)
                    {
                        throw new Exception("Document not found");

                    }
                    return File(documentDownload.CertificateOfOccupancyDocContent, CertificateOfOccupancyDocName, documentDownload.CertificateOfOccupancyDocName);
                default:
                    return null;
            }
        }


        [HttpGet]
        public JsonResult VerifyBusiness(int id)
        {
            var business = db.Businesses.Find(id);
            business.CertificateOfOccupancyDocVerified = true;
            business.COADocVerified = true;
            db.Entry(business).State = EntityState.Modified;
            var result = db.SaveChanges();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
