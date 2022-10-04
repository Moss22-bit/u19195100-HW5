using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data.Common;
using u19195100_HW5.Models.ViewModels;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace u19195100_HW5.Models
{
    public class DataService
    {
        //Declared instance of the dataservice
        private static DataService instance;

        //Checking if the instance of the dataservice is empty or not
        public static DataService GetDataService()
        {
            if (instance == null)
            {
                instance = new DataService();
            }
            return instance;
        }

        //Connection to the Database
        private SqlConnection EstablishConnection()
        {
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
            sqlConnectionStringBuilder["Data Source"] = "DESKTOP-2QBCM19\\SQLEXPRESS";
              sqlConnectionStringBuilder["Initial Catalog"] = "Library";
            sqlConnectionStringBuilder["Integrated Security"] = "True";
            return new SqlConnection(sqlConnectionStringBuilder.ConnectionString);
        }
        //Method to retrieve books from database
        public List<OnlineBook> getBooks()
        {
            List<OnlineBook> bookList = new List<OnlineBook>();
            String command = "SELECT book.[bookId] as bookId ,book.[name] as name ,book.[pagecount] as pagecount ,book.[point] as point,auth.[name] as authorName,auth.[surname] as authorSurname ,type.[name] typeName,  book.[authorId],book.[typeId] FROM [Library].[dbo].[books] book JOIN [Library].[dbo].[authors] auth on book.authorId = auth.authorId JOIN [Library].[dbo].[types] type on book.typeId = type.typeId";
            using (SqlConnection conn = EstablishConnection())
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(command, conn))
                    {
                        SqlDataReader readAllTheBooks = cmd.ExecuteReader();
                        while (readAllTheBooks.Read())
                        {
                            OnlineBook book = new OnlineBook();
                            book.bookId = (int)readAllTheBooks["bookId"];
                            book.name = (string)readAllTheBooks["name"];
                            book.pagecount = (int)readAllTheBooks["pagecount"];
                            book.point = (int)readAllTheBooks["point"];
                            book.authorId = (int)readAllTheBooks["authorId"];
                            book.typeId = (int)readAllTheBooks["typeId"];
                            book.authorName = (string)readAllTheBooks["authorName"];
                            book.authorSurname = (string)readAllTheBooks["authorSurname"];
                            book.typeName = (string)readAllTheBooks["typeName"];
                            book.status = true;
                            bookList.Add(book);
                        }
                    }
                    conn.Close();
                }
                catch
                {

                }
            }
            return bookList;
        }

        //Method to retrieve students from database
        public List<Student> getAllStudents()
        {
            List<Student> studentList = new List<Student>();
            try
            {
                String command = "SELECT * FROM [Library].[dbo].[students]";
                using (SqlConnection conn = EstablishConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(command, conn))
                    {
                        SqlDataReader readStudents = cmd.ExecuteReader();
                        while (readStudents.Read())
                        {
                            Student student = new Student();
                            student.studentId = (int)readStudents["studentId"];
                            student.name = (string)readStudents["name"];
                            student.surname = (string)readStudents["surname"];
                            student.birthdate = (DateTime)readStudents["birthdate"];
                            student.gender = (string)readStudents["gender"];
                            student.Class = (string)readStudents["class"];
                            student.point = (int)readStudents["point"];
                            studentList.Add(student);
                        }
                    }
                    conn.Close();
                }
            }
            catch
            {

            }
            return studentList;
        }

        //Method to retrieve borrowedbooks from database
        public List<Borrow> getAllBorrows()
        {
            List<Borrow> borrowList = new List<Borrow>();
            try
            {
                String command = "SELECT * FROM [Library].[dbo].[borrows]";
                using (SqlConnection conn = EstablishConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(command, conn))
                    {
                        SqlDataReader readAllTheBorrows = cmd.ExecuteReader();
                        while (readAllTheBorrows.Read())
                        {
                            Borrow borrow = new Borrow();
                            borrow.borrowId = (int)readAllTheBorrows["borrowId"];
                            borrow.studentId = (int)readAllTheBorrows["studentId"];
                            borrow.bookId = (int)readAllTheBorrows["bookId"];
                            borrow.takenDate = Convert.ToDateTime(readAllTheBorrows["takenDate"]);
                            var broughtDate = readAllTheBorrows["broughtDate"].ToString();
                            if (broughtDate != "")
                            {
                                borrow.broughtDate = Convert.ToDateTime(readAllTheBorrows["broughtDate"]);
                            }
                            else
                            {
                                borrow.broughtDate = null;
                            }

                            borrowList.Add(borrow);
                        }
                    }
                    conn.Close();
                }
            }
            catch
            {

            }
            return borrowList;
        }

        ////Method to retrieve types of books from database
        public SelectList GetTypes()
        {
            List<Type> typesList = new List<Type>();
            try
            {
                String command = "SELECT * FROM [Library].[dbo].[types]";
                using (SqlConnection conn = EstablishConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(command, conn))
                    {
                        SqlDataReader readAllTheTypes = cmd.ExecuteReader();
                        while (readAllTheTypes.Read())
                        {
                            Type type = new Type();
                            type.typeId = (int)readAllTheTypes["typeId"];
                            type.name = (string)readAllTheTypes["name"];
                            typesList.Add(type);
                        }
                    }
                    conn.Close();
                }
            }
            catch
            {

            }
            return new SelectList(typesList, "typeId", "name");
        }

        //Method to retrieve authors from database
        public SelectList GetAuthors()
        {
            List<Author> authorsList = new List<Author>();
            try
            {
                String command = "SELECT * FROM [Library].[dbo].[authors]";
                using (SqlConnection conn = EstablishConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(command, conn))
                    {
                        SqlDataReader readAllTheAuthors = cmd.ExecuteReader();
                        while (readAllTheAuthors.Read())
                        {
                            Author author = new Author();
                            author.authorId = (int)readAllTheAuthors["authorId"];
                            author.name = (string)readAllTheAuthors["name"] + " " + (string)readAllTheAuthors["surname"];
                            authorsList.Add(author);
                        }
                    }
                    conn.Close();
                }
            }
            catch
            {

            }
            return new SelectList(authorsList, "authorId", "name");
        }

        //Method to borrow book using student id and book id
        public string BorrowBook(int studentId, int bookId)
        {
            string res = "";
            try
            {
                String command = "Insert Into [Library].[dbo].[borrows] (studentId,bookId,takenDate) values ('" + studentId + "','" + bookId + "',GETDATE())";
                using (SqlConnection conn = EstablishConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(command, conn))
                    {
                        cmd.ExecuteNonQuery();
                        res = "Successfully Borrowed Book";
                    }
                    conn.Close();
                }

            }
            catch
            {

            }
            return res;
        }

        //Method to return borrowed books using the borrow id
        public string ReturnBook(int borrowId)
        {
            string res = "";
            try
            {
                String command = "Update [Library].[dbo].[borrows] set broughtDate = GETDATE() where borrowId =" + borrowId;
                using (SqlConnection conn = EstablishConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(command, conn))
                    {
                        cmd.ExecuteNonQuery();
                        res = "Successfully Returned Book";
                    }
                    conn.Close();
                }
            }
            catch
            {

            }
            return res;
        }
    }
}