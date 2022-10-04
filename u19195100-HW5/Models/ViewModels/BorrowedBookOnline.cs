using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace u19195100_HW5.Models.ViewModels
{
    public class BorrowedBookOnline
    {
        public string studentName { get; set; }

        public Nullable<int> borrowId { get; set; }

        public Nullable<DateTime> takenDate { get; set; }

        public Nullable<DateTime> broughtDate { get; set; }
    }
}