using qltv.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace qltv.Controllers
{
    public class RentalView
    {
        public int ItemID { get; set; }  
        public string RentalId { get; set; }   
        public string BookName { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public int Total {  get; set; }
        public bool?Returned { get; set; }
    }
}