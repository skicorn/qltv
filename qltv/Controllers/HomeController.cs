using qltv.Models;
using qltv.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace qltv.Controllers
{
    public class HomeController : Controller
    {
        private qltvEntities db = new qltvEntities();

        public ActionResult Index(string timeFrame = "week")
        {
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;

            switch (timeFrame)
            {
                case "day":
                    startDate = DateTime.Today;
                    break;
                case "week":
                    startDate = DateTime.Today.AddDays(-7);
                    break;
                case "month":
                    startDate = DateTime.Today.AddMonths(-1);
                    break;
                case "year":
                    startDate = DateTime.Today.AddYears(-1);
                    break;
                default:
                    startDate = DateTime.Today.AddDays(-7);
                    break;
            }

            // Tổng số hóa đơn đã cho thuê
            int totalVisitors = db.Rentals.Count();

            // Số thành viên 
            int newMembers = db.Customers.Count();

            // Số sách đã cho mượn
            int borrowedBooks = db.RentalItems.Sum(b => b.Quantity) ?? 0;

            // Số sách quá hạn
            int overdueBooks = db.Rentals.Count(r => r.DateReturn < DateTime.Now);
            // Tổng doanh thu
            double totalRevenue = db.Rentals
                                    .Sum(r => (double?)r.Total) ?? 0;

            // Lấy danh sách thành viên
            var members = db.Customers
                            .Select(c => new CustomerViewModel
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

            // Lấy sách nổi bật ( theo số lượng mượn nhiều nhất)
            //var trendingBookIds = db.RentalItems
            //                        .GroupBy(ri => ri.Bookcode)
            //                        .OrderByDescending(g => g.Count())
            //                        .Select(g => g.Key)
            //                        .Take(5)
            //                        .ToList();

            //var trendingBooks = db.Books
            //                      .Where(b => trendingBookIds.Contains(b.Bookcode))
            //                      .Select(b => new TrendingBooksViewModel
            //                      {
            //                          Bookcode = b.Bookcode,
            //                          IMG = b.IMG
            //                      })
            //                      .ToList();

            ViewBag.TotalVisitors = totalVisitors;
            ViewBag.NewMembers = newMembers;
            ViewBag.BorrowedBooks = borrowedBooks;
            ViewBag.OverdueBooks = overdueBooks;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.Members = members;
            ViewBag.Books = books;
            //ViewBag.TrendingBooks = trendingBooks;

            return View();
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
    }
}
