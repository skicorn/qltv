using System;
using System.Linq;
using System.Web.Mvc;
using qltv.Models;
using qltv.ViewModels;

public class DashboardController : Controller
{
    private qltvEntities db = new qltvEntities();

    public ActionResult Index()
    {
        // Tổng số khách truy cập
        int totalVisitors = db.Rentals.Count();

        // Số thành viên mới
        int newMembers = db.Customers.Count();

        // Số sách đã mượn
        int borrowedBooks = db.RentalItems.Count();

        // Số sách quá hạn
        int overdueBooks = db.Rentals.Count(r => r.DateReturn < DateTime.Now);

        // Lấy danh sách thành viên
        var members = db.Customers.Select(c => new CustomerViewModel
        {
            Name = c.Name,
            Phone = c.Phone,
            Email = c.Email
        })
                        .Take(10)
                        .ToList();

        // Lấy danh sách sách
        var books = db.Books
                      .Select(b => new BooksViewModel
                      {
                          Title = b.Title,
                          Bookcode = b.Bookcode,
                          BookStatus = (bool)b.BookStatus
                      })
                      .Take(10)
                      .ToList();

        // Lấy sách nổi bật (giả sử theo số lượng mượn nhiều nhất)
        var trendingBookIds = db.RentalItems
                                .GroupBy(ri => ri.BookID)
                                .OrderByDescending(g => g.Count())
                                .Select(g => g.Key)
                                .Take(5)
                                .ToList();

        var trendingBooks = db.Books
                              .Where(b => trendingBookIds.Contains(b.Bookcode))
                              .Select(b => new TrendingBooksViewModel
                              {
                                  Bookcode = b.Bookcode,
                                  IMG = b.IMG
                              })
                              .ToList();

        ViewBag.TotalVisitors = totalVisitors;
        ViewBag.NewMembers = newMembers;
        ViewBag.BorrowedBooks = borrowedBooks;
        ViewBag.OverdueBooks = overdueBooks;
        ViewBag.Members = members;
        ViewBag.Books = books;
        ViewBag.TrendingBooks = trendingBooks;

        return View();
    }
}

