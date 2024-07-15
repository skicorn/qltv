using qltv.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace qltv.Controllers
{
    public class AccountController : Controller
    {
        private readonly qltvEntities db = new qltvEntities();
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Staff model)
        {
            if (ModelState.IsValid)
            {
                if (db.Staffs.Any(s => s.StaffPhone == model.StaffPhone))
                {
                    ModelState.AddModelError("", "Số điện thoại đã được sử dụng.");
                    return View(model);
                }

                db.Staffs.Add(model);
                db.SaveChanges();
                return RedirectToAction("Login");
            }

            return View(model);
        }
        // GET: Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            if (ModelState.IsValid)
            {
                double phone;
                bool isDouble = double.TryParse(username, out phone);

                if (isDouble)
                {
                    var userStore = db.Staffs.FirstOrDefault(u => u.StaffPhone == phone && u.Staffpassword == password);

                    if (userStore == null)
                    {
                        ViewBag.ErrorLog = "Bạn đã nhập sai tên đăng nhập hoặc Mật khẩu";
                        return View("Login");
                    }
                    else
                    {
                        Session["UserID"] = userStore.StaffID;
                        Session["UserEmail"] = userStore.StaffPhone;
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ViewBag.ErrorLog = "Tên đăng nhập phải là một số hợp lệ";
                    return View("Login");
                }
            }

            return View();
        }

        public ActionResult ResetPassword()
        {
            return View();
        }


        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
        // GET: Account/Profile
        [HttpGet]
        public ActionResult Profile()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int staffID = Convert.ToInt32(Session["UserID"]);
            var staff = db.Staffs.Find(staffID);
            if (staff == null)
            {
                // Xử lý trường hợp không tìm thấy nhân viên trong cơ sở dữ liệu
                return RedirectToAction("Login", "Account");
            }

            return View(staff);
        }


        public ActionResult ResetPassword(double staffPhone)
        {
            // Chuyển đổi staffPhone thành chuỗi để sử dụng trong câu truy vấn LINQ
            string phoneString = staffPhone.ToString();

            if (string.IsNullOrEmpty(phoneString))
            {
                return RedirectToAction("ResetPassword");
            }

            // Xử lý reset mật khẩu ở đây, ví dụ đặt mật khẩu mặc định
            var staff = db.Staffs.FirstOrDefault(s => s.StaffPhone.ToString() == phoneString);

            if (staff != null)
            {
                // Thiết lập mật khẩu mặc định (ví dụ là "password123")
                staff.Staffpassword = "password123"; // Hãy chuyển sang cách an toàn hơn trong sản phẩm thực tế

                // Lưu thay đổi vào cơ sở dữ liệu
                db.SaveChanges();

                // Redirect đến trang thông báo reset mật khẩu thành công
                return RedirectToAction("ResetPasswordConfirmation");
            }
            else
            {
                // Redirect về trang reset mật khẩu nếu không tìm thấy nhân viên
                return RedirectToAction("ResetPassword");
            }
        }


    }
}