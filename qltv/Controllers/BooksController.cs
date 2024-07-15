
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using qltv.Models;
using qltv.ViewModels;

namespace qltv.Controllers
{
    public class BooksController : Controller
    {
        private qltvEntities db = new qltvEntities();

        // GET: Books
        public ActionResult Index(string SearchString, double min = double.MinValue, double max = double.MaxValue)
        {
            var books = db.Books.Include(b => b.Genre);
            var borrowBooks = db.Books.Include(b => b.Genre).Where(b => b.BookStatus == true);
            var availableBooks = db.Books.Include(b => b.Genre).Where(b => b.BookStatus == false);

            if (!String.IsNullOrEmpty(SearchString))
            {
                books = books.Where(s => s.Title.Contains(SearchString));
            }
            if (min >= 0 && max > 0)
            {
                books = db.Books.OrderByDescending(x => x.Price).Where(p => (double)p.Price >= min && (double)p.Price <= max);
            }

            var viewModel = new BookView
            {
                Books = books.ToList(),
                BorrowBook = borrowBooks.ToList(),
                AvailableBook = availableBooks.ToList()

            };
            return View(viewModel);

        }


        // GET: Books/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // GET: Books/Create
        public ActionResult Create()
        {
            ViewBag.Author = new SelectList(db.Authors, "AuthorID", "AuthorName");
            ViewBag.GenreID = new SelectList(db.Genres, "GenreID", "GenreName");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Bookcode,Title,Author,GenreID,Price,Available,BookStatus,Publish")] Book book, HttpPostedFileBase IMG)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (IMG != null && IMG.ContentLength > 0)
                    {
                        using (var binaryReader = new BinaryReader(IMG.InputStream))
                        {
                            book.IMG = binaryReader.ReadBytes(IMG.ContentLength);
                        }
                    }
                    db.Books.Add(book);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                    }
                }
                ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu sách. Vui lòng kiểm tra chi tiết và thử lại.");
            }

            ViewBag.Author = new SelectList(db.Authors, "AuthorID", "AuthorName", book.Author);
            ViewBag.GenreID = new SelectList(db.Genres, "GenreID", "GenreName", book.GenreID);
            return View(book);
        }

        // GET: Books/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            ViewBag.Author = new SelectList(db.Authors, "AuthorID", "AuthorName", book.Author);
            ViewBag.GenreID = new SelectList(db.Genres, "GenreID", "GenreName", book.GenreID);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Bookcode,Title,Author,GenreID,Price,Available,BookStatus,Publish")] Book book, HttpPostedFileBase uploadImage)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload if MedicineImg is provided
                if (uploadImage != null && uploadImage.ContentLength > 0)
                {
                    using (var binaryReader = new BinaryReader(uploadImage.InputStream))
                    {
                        book.IMG = binaryReader.ReadBytes(uploadImage.ContentLength);
                    }
                }

                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Author = new SelectList(db.Authors, "AuthorID", "AuthorName", book.Author);
            ViewBag.GenreID = new SelectList(db.Genres, "GenreID", "GenreName", book.GenreID);
            return View(book);
        }

        // GET: Books/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Book book = db.Books.Find(id);
            db.Books.Remove(book);
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
