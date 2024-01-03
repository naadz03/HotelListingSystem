using HotelListingSystem.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelListingSystem.Controllers
{
    public class InventoryController : Controller
    {
        private ApplicationDbContext _context;
        public HotelUsers CurrentUser { get; set; }
        public List<int> hIds { get; set; }
        public InventoryController()
        {
            _context = new ApplicationDbContext();
        }
        public void Initialise()
        {
            if (Request.IsAuthenticated)
            {
                CurrentUser = AppHelper.CurrentHotelUser();

                if (User.IsInRole("Business Owner") || User.IsInRole("Receptionist"))
                    hIds = _context.Hotels.
                        Where(a => (a.HotelUserId == CurrentUser.Id || a.ReceptionistId == CurrentUser.Id)).
                        Select(a => a.Id).ToList();

            }
        }
        public ActionResult Index()
        {
            Initialise();
            List<Inventory> inventory = _context.Inventories.Where(a => hIds.Contains(a.ForHotelId)).ToList();
            foreach(var inv in inventory)
            {
                inventory.FirstOrDefault(a => a.Id == inv.Id).ForHotel = _context.Hotels.FirstOrDefault(a => a.Id == inv.ForHotelId);
                inventory.FirstOrDefault(a => a.Id == inv.Id).AddedByUser = _context.HotelUsers.FirstOrDefault(a => a.Id == inv.AddedByUserId);
                inventory.FirstOrDefault(a => a.Id == inv.Id).Department = _context.Departments.FirstOrDefault(a => a.Id == inv.DepartmentKey);
                inventory.FirstOrDefault(a => a.Id == inv.Id).Product = _context.Products.FirstOrDefault(a => a.Id == inv.ProductKey);
            }
            return View(inventory);
        }

        public ActionResult RequestProduct()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult RequestProduct(ProductRequest collection)
        {
            Initialise();
            collection.IsAdded = true;
            collection.IsDeleted = false;
            collection.WaiterId = CurrentUser.Id;
            _context.ProductRequests.Add(collection);
            _context.SaveChanges();
            return RedirectToAction("Requests", "Inventory");
        }

        public ActionResult Requests()
        {
            Initialise();
            List<ProductRequest> products = _context.ProductRequests.ToList();
            foreach (var product in products)
            {
                products.FirstOrDefault(a => a.Id == product.Id).Waiter
                    = _context.HotelUsers.FirstOrDefault(a => a.Id == product.WaiterId);

                if (product.Waiter.HotelId != null)
                    products.FirstOrDefault(a => a.Id == product.Id).Waiter.Hotel
                        = _context.Hotels.FirstOrDefault(a => a.Id == product.Waiter.HotelId);
            }

            if (User.IsInRole("Employee"))
                products = products.Where(a => a.WaiterId == CurrentUser.Id).ToList();
            else
                products = products.Where(a => hIds.Contains((int)a.Waiter.HotelId)).ToList();

            return View(products);
        }

        public ActionResult ProductReturns()
        {
            Initialise();
            List<CheckoutHistory> chkHistory = _context.CheckoutHistories.Where(a => a.RtnDateTime == null).ToList();
            foreach (var product in chkHistory)
            {
                chkHistory.FirstOrDefault(a => a.Id == product.Id).Product
                    = _context.Products.FirstOrDefault(a => a.Id == product.ProductId);
            }
            return View(chkHistory);
        }

        public ActionResult ProductCreate()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult ProductCreate(Product collection)
        {
            Initialise();
            collection.IsActive = true;
            collection.Name = collection.Name.Replace("-", "~");
            _context.Products.Add(collection);  
            _context.SaveChanges();
            return RedirectToAction("", "Inventory");
        }

        public ActionResult AddProduct()
        {
            Initialise();
            ViewBag.Hotels = new SelectList(_context.Hotels.Where(a => hIds.Contains(a.Id)), "Id", "Name");
            ViewBag.Department = new SelectList(_context.Departments.ToList(), "Id", "Name");
            ViewBag.ProductKey = new SelectList(_context.Products.ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddProduct(Inventory collection)
        {
            Initialise();
            collection.AddedByUserId = CurrentUser.Id;
            collection.IsActive = true;
            _context.Inventories.Add(collection);  
            _context.SaveChanges();
            return RedirectToAction("", "Inventory");
        }


        public ActionResult EditProduct(Guid id)
        {
            Initialise();
            Inventory collection = _context.Inventories.Find(id);
            ViewBag.Hotels = new SelectList(_context.Hotels.Where(a => hIds.Contains(a.Id)), "Id", "Name");
            ViewBag.Department = new SelectList(_context.Departments.ToList(), "Id", "Name");
            ViewBag.ProductKey = new SelectList(_context.Products.Where(a => a.Id == collection.ProductKey).ToList(), "Id", "Name");
            return View(collection);
        }

        [HttpPost]
        [Authorize]
        public ActionResult EditProduct(Inventory collection)
        {
            Initialise();
            Inventory inventory = _context.Inventories.Find(collection.Id);
            inventory.ForHotelId = collection.ForHotelId;
            inventory.DepartmentKey = collection.DepartmentKey;
            inventory.Quantity = collection.Quantity;
            _context.Entry(inventory).State = System.Data.Entity.EntityState.Modified;
            _context.SaveChanges();
            return RedirectToAction("", "Inventory");
        }

        [Authorize(Roles = "Employee")]
        public ActionResult Products(string search = null)
        {
            Initialise();
            int WorkspaceId = (int)CurrentUser.HotelId;
            List<Guid> ints = _context.Inventories.Where(a => a.ForHotelId == WorkspaceId).Select(a => a.ProductKey).ToList();
            List<Product> products = _context.Products.Where(a => ints.Contains(a.Id)).ToList();
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(a => !string.IsNullOrEmpty(a.Tags)).ToList();
                products = products.Where(a => a.Tags.ToLower().Split('|').ToList().Contains(search.ToLower().Trim()) || a.Name.ToLower().Split(' ').ToList().Contains(search.ToLower().Trim())).ToList();
            }
            return View(products);  
        }

        public ActionResult checkoutProducts(string products)
        {
            try
            {
                Initialise();
                List<String> productsList = products.Split('|').ToList();
                List<(int quantity, string productname)> product = productsList.Select(a => splitProduct(a)).ToList();


                foreach (var item in product)
                {
                    CheckoutHistory checkoutHistory = new CheckoutHistory();
                    checkoutHistory.ProductId = _context.Products.FirstOrDefault(a => a.Name.Equals(item.productname)).Id;
                    checkoutHistory.Quantity = item.quantity;
                    checkoutHistory.ForHotelId = (int)CurrentUser.HotelId;
                    checkoutHistory.ByUserId = CurrentUser.Id;
                    checkoutHistory.ChkDateTime = DateTime.Now;
                    _context.CheckoutHistories.Add(checkoutHistory);
                    _context.SaveChanges();

                    Inventory inventory = _context.Inventories.FirstOrDefault(a => a.ProductKey == checkoutHistory.ProductId);
                    inventory.Quantity -= checkoutHistory.Quantity;
                    _context.Entry(inventory).State = System.Data.Entity.EntityState.Modified;
                    _context.SaveChanges();
                }


                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        #region helpers

        static (int, string) splitProduct(string input)
        {
            List<string> list = input.Replace("[", "").Replace("]", "").Split('-').ToList();
            if (list.Count < 1 || list.Count >=3)
                return (0, string.Empty);
            return (int.Parse(list[0]), list[1]);
        }

        public ActionResult DeleteProduct(Guid id) 
        {
            Initialise();
            
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReturnProductApi(Guid historyId, int enteredQuantity)
        {
            Initialise();
            CheckoutHistory checkoutHistory = _context.CheckoutHistories.Find(historyId);
            checkoutHistory.ByUser = _context.HotelUsers.FirstOrDefault(a => a.Id == checkoutHistory.ByUserId);
            if (checkoutHistory != null)
            {
                checkoutHistory.LogQuantity += enteredQuantity;
                checkoutHistory.Quantity -= enteredQuantity;
                if(checkoutHistory.Quantity == 0)
                    checkoutHistory.RtnDateTime = DateTime.Now;
                _context.Entry(checkoutHistory).State = System.Data.Entity.EntityState.Modified;
                _context.SaveChanges();

                Inventory inventory = _context.Inventories.FirstOrDefault(a => a.ProductKey == checkoutHistory.ProductId && a.ForHotelId == checkoutHistory.ByUser.HotelId);
                if (inventory != null)
                {
                    inventory.Quantity += enteredQuantity;
                    _context.Entry(inventory).State = System.Data.Entity.EntityState.Modified;
                    _context.SaveChanges(); 
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}