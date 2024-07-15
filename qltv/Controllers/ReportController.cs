using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using qltv.Models;

namespace qltv.Controllers
{
    public class ReportController : Controller
    {
        private qltvEntities db = new qltvEntities();

        // GET: Report
        public ActionResult Index()
        {
            var rentals = db.Rentals.Include(r => r.Customer).Include(r => r.Staff);
            return View(rentals.ToList());
        }
    }
}
