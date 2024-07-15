using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using qltv.Models;
using Microsoft.Ajax.Utilities;


namespace qltv.Controllers
{
    public class RentalsController : Controller
    {
        private qltvEntities db = new qltvEntities();

        // GET: Rentals

        public ActionResult Index(string phone, DateTime? startDate, DateTime? endDate)
        {
            Utils.UpdateRentalStatusToOverdue();

            IQueryable<Rental> rentals = db.Rentals.Include(r => r.Customer).Include(r => r.Staff);

            if (!String.IsNullOrEmpty(phone))
            {
                rentals = rentals.Where(r => r.CusPhone.ToString().Contains(phone));
            }

            // Lọc theo ngày mượn (nếu chỉ có date create)
            if (startDate != null && endDate == null)
            {
                rentals = rentals.Where(r => r.DateCreate >= startDate);
            }

            // Lọc theo ngày mượn (nếu chỉ có date return)
            if (startDate == null && endDate != null)
            {
                rentals = rentals.Where(r => r.DateReturn <= endDate);
            }

            // Lọc theo ngày mượn (nếu có cả date create và endDate)
            if (startDate != null && endDate != null)
            {
                rentals = rentals.Where(r => r.DateCreate >= startDate && r.DateReturn <= endDate);
            }
            rentals = rentals.OrderByDescending(r => r.DateCreate);

            return View(rentals.ToList());
        }
        // GET: Rentals/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Rental rental = db.Rentals.Find(id);
            if (rental == null)
            {
                return HttpNotFound();
            }
            return View("Details", rental);
        }

        // GET: Rentals/Create
        public ActionResult Create()
        {
            ViewBag.CusPhone = new SelectList(db.Customers, "Phone", "Name");
            ViewBag.StaffID = new SelectList(db.Staffs, "StaffID", "Staffpassword");
            return View();
        }

        // POST: Rentals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(List<RentalItem> RentalItems, [Bind(Include = "RentailID,CusPhone,StaffID,Total,DateCreate,DateReturn, Status, Discount")] Rental rental)
        {

            if (ModelState.IsValid)
            {
                string id = Guid.NewGuid().ToString();
                // Add BillDetails to database context
                if (RentalItems != null && RentalItems.Count > 0)
                {
                    foreach (var detail in RentalItems)
                    {
                        db.RentalItems.Add(detail);
                    }
                    db.SaveChanges();
                }
                rental.RentailID = id;
                rental.Rental_status = "Đang mượn";
                rental.DateCreate = DateTime.Now;
                rental.StaffID = 1;
                rental.Discount = (rental.Discount != null) ? rental.Discount : 0; 

                db.Rentals.Add(rental);
                db.SaveChanges();

                // Associate BillDetails with the newly created Bill
                if (RentalItems != null && RentalItems.Count > 0)
                {

                    int? billtotal = 0;
                    int? totalquantity = 0;
                    foreach (var detail in RentalItems)
                    {
                        updatebookstock(detail.Bookcode, detail.Quantity, false);
                        detail.Price = db.Books.Where(de => de.Bookcode == detail.Bookcode).FirstOrDefault().Price;
                        detail.Total = detail.Price * detail.Quantity;
                        detail.Returned = false;
                        //add to bill
                        billtotal = billtotal + detail.Total;
                        totalquantity = totalquantity + detail.Quantity;
                        detail.BookName = db.Books.Where(de => de.Bookcode == detail.Bookcode).FirstOrDefault().Title;
                        detail.RentalID = rental.RentailID;
                        rental.Total = billtotal;
                    }
                    rental.Total = rental.Total - rental.Discount;
                    db.SaveChanges();
                }
                TempData["Message"] = "Tạo thành công";
                return RedirectToAction("Details", new { id = id });
            }

            // If ModelState is not valid, reload the view with ViewBag 
            ViewBag.CusPhone = new SelectList(db.Customers, "Phone", "Name", rental.CusPhone);
            ViewBag.StaffID = new SelectList(db.Staffs, "StaffID", "Staffpassword", rental.StaffID);
            return View(rental);
        }


        // GET: Rentals/Edit/5
        // GET: Rentals/EditDiscount/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Rental rental = db.Rentals.Find(id);
            if (rental == null)
            {
                return HttpNotFound();
            }

            ViewBag.RentalID = rental.RentailID; // Pass the RentalID to the view

            return View(rental);
        }

        // POST: Rentals/EditDiscount/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "RentailID, Discount")] Rental rental)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the existing rental from the database
                var existingRental = db.Rentals.Find(rental.RentailID);

                if (existingRental == null)
                {
                    return HttpNotFound();
                }

                // Update only the discount
                existingRental.Discount = rental.Discount;
                existingRental.Total = (existingRental.Discount > existingRental.Total) ? 0 : existingRental.Total - existingRental.Discount;

                // Save changes
                db.Entry(existingRental).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(rental);
        }
        // GET: Rentals/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Rental rental = db.Rentals.Find(id);
            if (rental == null)
            {
                return HttpNotFound();
            }
            return View(rental);
        }

        // POST: Rentals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Rental rental = db.Rentals.Find(id);
            var rentalitem = rental.RentalItems.Where(item => item.RentalID == id).ToList();
            foreach(var item in rentalitem)
            {   
                db.RentalItems.Remove(item);
            }
            db.Rentals.Remove(rental);
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

        public void updatebookstock(int?code, int?quantity, bool isIncrease)
        {
            var book = db.Books.FirstOrDefault(b => b.Bookcode == code);
            book.Available = (book!=null && isIncrease == false) ? book.Available - quantity : book.Available+quantity;
            db.SaveChanges();
        }

        public JsonResult checkcount(int? code, int? quantity)
        {
            int?available = db.Books.Where(b => b.Bookcode == code).FirstOrDefault().Available;
            bool check = available >= quantity;
            return Json(new { success = true, check }, JsonRequestBehavior.AllowGet); 
        }

        [HttpGet]
        public JsonResult GetRentalDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "Invalid information" }, JsonRequestBehavior.AllowGet);
            }

            var detail = db.RentalItems
                .Where(de => de.RentalID == id)
                .Select(de => new RentalView
                {
                    ItemID = de.ID,
                    RentalId = de.RentalID,
                    BookName = de.BookName,
                    Price = (int)de.Price,
                    Quantity = (int)de.Quantity,
                    Total = (int)de.Total,
                    Returned = de.Returned
                })
                .ToList();

            if (detail == null || detail.Count == 0)
            {
                return Json(new { success = false, message = "No details found" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true, detail }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult SearchBook(string name)
        {
            if (name == null)
            {
                return Json(new { success = false, message = "Invalid medicine" }, JsonRequestBehavior.AllowGet);
            }

            var books = db.Books
                              .Where(cu => cu.Title.Contains(name))
                              .Select(me => new BookViewModel
                              {
                                  Bookcode = me.Bookcode,
                                  Title = me.Title,
                                  Author = db.Authors.Where(a=> a.AuthorID==a.AuthorID).FirstOrDefault().AuthorName
                              })
                              .ToList();

            return Json(books, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetBookInfo(int bookid)
        {
            if (bookid == null)
            {
                return Json(new { success = false, message = "Invalid book" }, JsonRequestBehavior.AllowGet);
            }

            var books = db.Books.Find(bookid);
            if (books != null)
            {
                return Json(new { success = true, price = books.Price }, JsonRequestBehavior.AllowGet);
            }

            return Json(books, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetBillInfo(string rentalID)
        {
            var rental = db.Rentals.Find(rentalID);
            if (rental != null)
            {
                return Json(new { success = true, rentalid = rental.RentailID, rentaltotal = rental.Total, datecreate = rental.DateCreate.ToString(), datereturn = rental.DateReturn.ToString(), customer= rental.Customer.Phone, cashier = rental.Staff.StaffName, discount = rental.Discount}, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = "Invalid rental" }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult UpdateReturnStatus(int rentalItemId, bool? returned)
        {
            try
            {
                var rentalItem = db.RentalItems.Find(rentalItemId);

                if (rentalItem == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phiếu thuê" });
                }

                rentalItem.Returned = returned;
                db.SaveChanges();

                return Json(new { success = true, message = "Cập nhập thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error occurred while updating return status: " + ex.Message });
            }
        }
        [HttpPost]
        public void RentalReturned(string id)
        {
            int countquantity = 0;
            var book = db.Rentals.Where(r=>r.RentailID == id).FirstOrDefault(); 
            var item = book.RentalItems.Where(i=> i.RentalID == id).ToList();
            book.Rental_status = "Đã trả";

            foreach(var i in item)
            {
               i.Returned = true;
               updatebookstock(i.Bookcode, i.Quantity, true);
            }
            db.SaveChanges();
        }
        [HttpPost]
        public void updatebookquantity(int id, int quantity)
        {
            var book = db.Books.Find(id);
            if (book != null)
            {
                book.Available = book.Available - quantity;
                db.SaveChanges();
            }
        }
    }
}
