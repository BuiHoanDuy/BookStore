using BookStoreWebsite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;
using System.IO;
using System.Web.UI.WebControls;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Net;
using System.Data.Entity.Migrations;

namespace BookStoreWebsite.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        private QLBansachEntities db = new QLBansachEntities();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Sach(int? page)
        {
            int pageNumber = (page ?? 1);
            int pageSize = 5;
            return View(db.SACHes.ToList().OrderBy(n => n.Masach).ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            var tendn = collection["username"];
            var matkhau = collection["password"];

            if (string.IsNullOrEmpty(tendn))
            {
                ViewData["Loi1"] = "Phải nhập tên đăng nhập";
            }
            else if (string.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi2"] = "Phải nhập mật khẩu";
            }
            else
            {
                Admin ad = db.Admins.SingleOrDefault(n => n.UserAdmin == tendn && n.PassAdmin == matkhau);
                if (ad != null)
                {
                    Session["Taikhoanadmin"] = ad;
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩi không đúng";
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult ThemmoiSach()
        {
            ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe");
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ThemMoiSach(SACH sach, HttpPostedFileBase fileUpload)
        {
            // Đưa dữ liệu vào dropdown
            ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe");
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");

            // Kiểm tra nếu không có file được upload
            if (fileUpload == null)
            {
                ViewBag.ThongBao = "Vui lòng chọn ảnh bìa";
                return View();
            }

            // Thêm vào CSDL
            if (ModelState.IsValid)
            {
                // Lưu tên file
                var fileName = Path.GetFileName(fileUpload.FileName);
                // Lưu đường dẫn của file
                var path = Path.Combine(Server.MapPath("~/Content/image/"), fileName);

                // Kiểm tra hình ảnh tồn tại chưa
                if (System.IO.File.Exists(path))
                {
                    ViewBag.ThongBao = "Hình ảnh đã tồn tại";
                }
                else
                {
                    // Lưu hình ảnh vào đường dẫn
                    fileUpload.SaveAs(path);

                    // Gán đường dẫn ảnh vào thuộc tính Anhbia
                    sach.Anhbia = fileName;

                    try
                    {
                        // Thêm vào CSDL
                        db.SACHes.Add(sach);
                        db.SaveChangesAsync();
                        return RedirectToAction("Sach");
                    }
                    catch (DbEntityValidationException ex)
                    {
                        // Hiển thị lỗi xác thực
                        foreach (var validationErrors in ex.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                            }
                        }
                    }
                }
            }

            return View();
        }

        public ActionResult Chitietsach(int id)
        {
            SACH sach = db.SACHes.SingleOrDefault(n => n.Masach == id);
            ViewBag.Masach = sach.Masach;
            if (sach == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sach);
        }

        [HttpGet]
        public ActionResult Xoasach(int id)
        {
            SACH sach = db.SACHes.SingleOrDefault(n => n.Masach == id);
            ViewBag.Masach = sach.Masach;
            if (sach == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sach);
        }

        [HttpPost, ActionName("Xoasach")]
        public ActionResult Xacnhanxoa(int id)
        {
            SACH sach = db.SACHes.SingleOrDefault(m => m.Masach == id);
            ViewBag.Masach = sach.Masach;
            if (sach == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            db.SACHes.Remove(sach);
            db.SaveChangesAsync();
            return RedirectToAction("Sach");
        }

        // GET: Admin/Edit/5
        public ActionResult Suasach(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SACH sach = db.SACHes.Include(s => s.CHUDE).FirstOrDefault(s => s.Masach == id);
            if (sach == null)
            {
                return HttpNotFound();
            }

            var chude = db.CHUDEs.Select(c => new { c.MaCD, c.TenChuDe }).ToList();
            ViewData["MaCD"] = new SelectList(chude, "MaCD", "TenChuDe", sach.MaCD);

            ViewData["MaNXB"] = new SelectList(db.NHAXUATBANs, "MaNXB", "TenNXB", sach.MaNXB);
            return View(sach);
        }

        [HttpGet]
        public ActionResult SuaSach(int id)
        {
            SACH sach = db.SACHes.SingleOrDefault(n => n.Masach == id);

            if (sach == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe", sach.MaCD);
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB", sach.MaNXB);
            return View(sach);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SuaSach(SACH sach, HttpPostedFileBase fileupload)
        {
            ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe", sach.MaCD);
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB", sach.MaNXB);

            if (fileupload == null)
            {
                ViewBag.Thongbao = "Vui lòng chọn ảnh bìa";
                return View(sach);
            }
            else
            {

                if (ModelState.IsValid)
                {
                    var fileName = Path.GetFileName(fileupload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/image/"), fileName);
                    if (System.IO.File.Exists(path))
                    {
                        ViewBag.Thongbao = "Hình ảnh đã tồn tại";
                        return View(sach);
                    }
                    else
                    {
                        fileupload.SaveAs(path);
                    }
                    sach.Anhbia = fileName;
                    db.SACHes.AddOrUpdate(sach);
                    //UpdateModel(sach);

                    db.SaveChanges();

                }
                return RedirectToAction("Sach");
            }
        }
        public ActionResult ThongKeSach()
        {
            var bookStatistics = db.SACHes
           .GroupBy(s => s.CHUDE.TenChuDe)
           .ToDictionary(
               g => g.Key?.ToString() ?? "Unknown", // Convert nullable int to string, handle null as "Unknown"
               g => g.Count()
           );

            return View(bookStatistics); // Pass the dictionary as a model to the view
        }
    }
}