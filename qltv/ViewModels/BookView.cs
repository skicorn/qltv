using qltv.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace qltv.ViewModels
{
    public class BookView
    {
        public IEnumerable<Book> Books { get; set; }
        public IEnumerable<Book> BorrowBook { get; set; }
        public IEnumerable<Book> AvailableBook { get; set; }

        public string Title { get; set; }
        public Nullable<int> Author { get; set; }
        public Nullable<int> GenreID { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<int> Available { get; set; }
        public Nullable<bool> BookStatus { get; set; }
        public Nullable<System.DateTime> Publish { get; set; }
        public byte[] IMG { get; set; }
        public virtual Author Author1 { get; set; }
        public virtual Genre Genre { get; set; }
    }
}