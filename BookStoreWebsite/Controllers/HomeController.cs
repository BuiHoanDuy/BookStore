using BookStoreWebsite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PagedList;
using PagedList.Mvc;
namespace BookStoreWebsite.Controllers
{
    public class HomeController : Controller
    {
        private QLBansachEntities db = new QLBansachEntities();
        public ActionResult Index(int? page)
        {
            int pageSize = 5;
            int pageNum = (page ?? 1);
            var books = db.SACHes.Include("CHUDE").Include("NHAXUATBAN").Where(b => b.Anhbia != null && b.Tensach != null).ToList();
            //Session.Clear();
            return View(books.ToPagedList(pageNum, pageSize));
        }
        public ActionResult GetNewestBook()
        {
            var books = db.SACHes
              .Include("CHUDE")
              .Include("NHAXUATBAN")
              .OrderByDescending(b => b.Ngaycapnhat) // Replace 'DateAdded' with your actual date field name
              .Take(5)
              .ToList();
            return View(books);
        }
        public ActionResult GetCategories()
        {
            var categories = db.CHUDEs.ToList();
            return PartialView("GetCategories", categories);
        }
        public ActionResult GetPublishers()
        {
            var publishers = db.NHAXUATBANs.ToList();
            return PartialView("GetPublishers", publishers);
        }

        public ActionResult FilteredBooks(int? MaCD, int? MaNXB)
        {
            var books = db.SACHes.AsQueryable();

            // Lọc theo chủ đề nếu có
            if (MaCD.HasValue)
            {
                books = books.Where(b => b.MaCD == MaCD.Value);
            }

            // Lọc theo nhà xuất bản nếu có
            if (MaNXB.HasValue)
            {
                books = books.Where(b => b.MaNXB == MaNXB.Value);
            }

            // Trả về danh sách sách lọc vào View mới
            return View("FilteredBooks", books.ToList());
        }
        public ActionResult ProductDetails(int id)
        {
            var book = db.SACHes
                         .Include("NHAXUATBAN") // để bao gồm thông tin nhà xuất bản
                         .Include("CHUDE")      // để bao gồm thông tin chủ đề
                         .FirstOrDefault(b => b.Masach == id);

            if (book == null)
            {
                return HttpNotFound();
            }

            return View(book);
        }

    }
}