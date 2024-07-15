using qltv.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace qltv.Controllers
{
    public static class Utils
    {
        public static void UpdateRentalStatusToOverdue()
        {
            using (var context = new qltvEntities())
            {
                var currentDate = DateTime.Today;

                var overdueRentals = context.Rentals
                    .Where(r => r.DateReturn < currentDate && r.Rental_status == "Đang mượn")
                    .ToList();

                foreach (var rental in overdueRentals)
                {
                    rental.Rental_status = "Quá hạn";
                }

                context.SaveChanges();
            }
        }

    }
}