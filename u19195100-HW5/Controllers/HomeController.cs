using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using u19195100_HW5.Models;
using u19195100_HW5.Models.ViewModels;

namespace u19195100_HW5.Controllers
{
    public class HomeController : Controller
    {
        //Public datservice
        public DataService Database = new DataService();

        //Public static lists
        public static List<OnlineBook> onlinebooks = new List<OnlineBook>();
        public static List<Student> students = new List<Student>();
        public static List<Borrow> borrowedbooks = new List<Borrow>();
        public static List<OnlineBook> QueryforSearchedBooks = new List<OnlineBook>();
        public static List<Student> QueryforSearchedStudents = new List<Student>();

        public static int InspectBook = 0;

        // GET: Home
        //Display full information of the students, authors, and books
        [HttpGet]
        public ActionResult Index()
        {
            List<OnlineBook> returnBooks = null;
            try
            {
                if (QueryforSearchedBooks.Count > 0)
                {
                    returnBooks = QueryforSearchedBooks;
                }
                else
                {
                    RefreshData();
                    returnBooks = onlinebooks;
                }

                ViewBag.Message = TempData["Message"];
                ViewBag.Types = Database.GetTypes();
                ViewBag.Authors = Database.GetAuthors();
            }
            catch (Exception message)
            {
                ViewBag.Message = message.Message;
            }
            return View(returnBooks);
        }
        //Clears the books that have been searched by the user
        [HttpGet]
        public ActionResult ClearBooks()
        {
            QueryforSearchedBooks.Clear();
            return RedirectToAction("Index");
        }

        //Method for searching books using entered name and type and author
        [HttpPost]
        public ActionResult SearchInTheBooksDbTable(string name, int? typeId, int? authorId)
        {
            try
            {    //Checks if the user has entered values into all the inputs
                if (QueryforSearchedBooks.Count > 0 && name == "" && typeId == null && authorId == null)
                {
                    TempData["Message"] = "You Didnt Search For Anything Stop Wasting my Time !!!";
                }
                else
                {
                    QueryforSearchedBooks.Clear();
                    RefreshData();
                    if (name != "" && typeId != null && authorId != null)
                    {
                        //search the name and type and author
                        QueryforSearchedBooks = onlinebooks.Where(x => x.name == name && x.typeId == typeId && x.authorId == authorId).ToList();
                    }
                    else if (name != "" && typeId != null && authorId == null)
                    {
                        // search the name and type 
                        QueryforSearchedBooks = onlinebooks.Where(x => x.name == name && x.typeId == typeId).ToList();
                    }
                    else if (name != "" && typeId == null && authorId != null)
                    {
                        // search the name and author
                        QueryforSearchedBooks = onlinebooks.Where(x => x.name == name && x.authorId == authorId).ToList();
                    }
                    else if (name == "" && typeId != null && authorId != null)
                    {
                        //search the type and author
                        QueryforSearchedBooks = onlinebooks.Where(x => x.typeId == typeId && x.authorId == authorId).ToList();
                    }
                    else if (name == "" && typeId == null && authorId != null)
                    {
                        //search the author
                        QueryforSearchedBooks = onlinebooks.Where(x => x.authorId == authorId).ToList();
                    }
                    else if (name == "" && typeId != null && authorId == null)
                    {
                        // search the type
                        QueryforSearchedBooks = onlinebooks.Where(x => x.typeId == typeId).ToList();
                    }
                    else if (name != "" && typeId == null && authorId == null)
                    {
                        // search the name 
                        QueryforSearchedBooks = onlinebooks.Where(x => x.name == name).ToList();
                    }
                }
            }
            catch (Exception message)
            {
                TempData["Message"] = message;
            }
            return RedirectToAction("Index");
        }
        //Display records
        [HttpGet]
        public ActionResult Information(int bookId)
        {
            RefreshData();
            OnlineBook bookInList = onlinebooks.Where(x => x.bookId == bookId).FirstOrDefault();
            if (bookInList != null)
            {
                var RecordsofBorrowedBooks = borrowedbooks.Where(x => x.bookId == bookId).ToList();
                bookInList.totalBorrows = RecordsofBorrowedBooks.Count();
                List<BorrowedBookOnline> RecordOfBorrowed = new List<BorrowedBookOnline>();
                for (int i = 0; i < RecordsofBorrowedBooks.Count(); i++)
                {
                    BorrowedBookOnline record = new BorrowedBookOnline();
                    record.borrowId = RecordsofBorrowedBooks[i].borrowId;
                    record.studentName = students.Where(x => x.studentId == RecordsofBorrowedBooks[i].studentId).FirstOrDefault().name;
                    record.takenDate = RecordsofBorrowedBooks[i].takenDate;
                    record.broughtDate = RecordsofBorrowedBooks[i].broughtDate;
                    RecordOfBorrowed.Add(record);
                }
                bookInList.borrowedRecords = RecordOfBorrowed;
                ViewBag.Message = TempData["Details"];
            }
            else
            {
                ViewBag.Message = "Book Not Found";
            }

            return View(bookInList);
        }

        //Display the list of the students 
        [HttpGet]
        public ActionResult DisplayStudents(int bookId)
        {
            RefreshData();

            OnlineBook onlinebook = onlinebooks.Where(x => x.bookId == bookId).FirstOrDefault();
            ViewBag.Status = onlinebook.status;

            if (onlinebook.studentId != 0)
            {
                ViewBag.studentId = onlinebook.studentId;
            }
            else
            {
                ViewBag.studentId = 0;
            }

            InspectBook = 0;
            InspectBook = bookId;

            List<Student> returnStudents = null;
            if (QueryforSearchedStudents.Count > 0)
            {
                returnStudents = QueryforSearchedStudents;
            }
            else
            {
                RefreshData();
                returnStudents = students;
            }

            ViewBag.Message = TempData["MessageStudent"];

            //This is the class variable used to retrieve all the keys for all the students
            var Classes = students.GroupBy(x => x.Class).Select(y => new { Class = y.Key }).OrderByDescending(z => z.Class).ToList();
            ViewBag.Classes = new SelectList(Classes, "Class", "Class");


            return View(returnStudents);
        }

        //Clear students on the list
        [HttpGet]
        public ActionResult ClearExistingStudents()
        {
            QueryforSearchedStudents.Clear();
            return RedirectToAction("DisplayStudents", new { bookId = InspectBook });
        }

        //Function to search students
        [HttpPost]
        public ActionResult SearchStudents(string name, string Class)
        {
            try
            {    //Checks if the user has enteres the text into input boxes
                if (QueryforSearchedStudents.Count > 0 && name == "" && Class == "")
                {
                    TempData["MessageStudent"] = "You Didn't Search For Anything!";
                }
                else
                {
                    QueryforSearchedStudents.Clear();
                    RefreshData();
                    if (name != "" && Class != "")
                    {
                        //search the name and class
                        QueryforSearchedStudents = students.Where(x => x.name == name && x.Class == Class).ToList();
                    }
                    else if (name != "" && Class == "")
                    {
                        // search the name only 
                        QueryforSearchedStudents = students.Where(x => x.name == name).ToList();
                    }
                    else if (name == "" && Class != "")
                    {
                        // search the name and author
                        QueryforSearchedStudents = students.Where(x => x.Class == Class).ToList();
                    }
                }
            }
            catch (Exception message)
            {
                TempData["MessageStudent"] = message;
            }
            return RedirectToAction("DisplayStudents", new { bookId = InspectBook });
        }

        //Function to borrow book
        [HttpGet]
        public ActionResult BorrowBookToStudent(int studentId)
        {
            string res = Database.BorrowBook(studentId, InspectBook);
            if (res == "")
            {
                TempData["MessaeStudent"] = "Failed To Borrow Book for Student";
                return RedirectToAction("DisplayStudents", new { bookId = InspectBook });
            }
            return RedirectToAction("Information", new { bookId = InspectBook });
        }

        //Function to return books
        [HttpGet]
        public ActionResult ReturnBook()
        {
            string res = Database.ReturnBook(onlinebooks.Where(x => x.bookId == InspectBook).FirstOrDefault().borrowId);
            if (res == "")
            {
                TempData["MessageStudent"] = "An Error Occourred While attempting to Return the onlinebook";
                return RedirectToAction("DisplayStudents", new { bookId = InspectBook });
            }
            TempData["Information"] = res;
            return RedirectToAction("Information", new { bookId = InspectBook });
        }


        //Method to Refresh static Lists 
        private void RefreshData()
        {
            onlinebooks.Clear();
            students.Clear();
            borrowedbooks.Clear();

            onlinebooks = Database.getBooks();
            students = Database.getAllStudents();
            borrowedbooks = Database.getAllBorrows();

            for (int i = 0; i < borrowedbooks.Count; i++)
            {
                if (borrowedbooks[i].broughtDate == null)
                {
                    OnlineBook book = onlinebooks.Where(x => x.bookId == borrowedbooks[i].bookId).FirstOrDefault();
                    book.status = false;
                    book.studentId = (int)borrowedbooks[i].studentId;
                    book.borrowId = borrowedbooks[i].borrowId;
                }
            }
        }


    }
}