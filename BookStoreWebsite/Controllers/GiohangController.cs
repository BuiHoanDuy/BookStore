using BookStoreWebsite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BookStoreWebsite.Controllers
{
    public class GiohangController : Controller
    {
        private QLBansachEntities data = new QLBansachEntities();
        // GET: Giohang
        public ActionResult Index()
        {
            return View();
        }
        public List<GioHang> LayGioHang()
        {
            List<GioHang> lstGiohang = Session["Giohang"] as List<GioHang>;
            if(lstGiohang == null)
            {
                lstGiohang = new List<GioHang>();
                Session["Giohang"] = lstGiohang;
            }
            return lstGiohang;
        }

        // Thêm hàng vào giỏ
        public ActionResult ThemGiohang(int iMasach, string strURL)
        {
            //// Kiểm tra nếu người dùng chưa đăng nhập
            //if (Session["TaiKhoan"] == null)
            //{
            //    // Chuyển hướng đến trang đăng nhập
            //    return RedirectToAction("Login", "User");
            //}
            // Lấy ra Session giỏ hàng
            List<GioHang> lstGiohang = LayGioHang();

            // Kiểm tra sách này tồn tại trong Session["Giohang"] chưa?
            GioHang sanpham = lstGiohang.Find(n => n.iMasach == iMasach);

            if (sanpham == null)
            {
                sanpham = new GioHang(iMasach);
                lstGiohang.Add(sanpham);
                return Redirect(strURL);
            }
            else
            {
                sanpham.iSoluong++;
                return Redirect(strURL);
            }
        }

        private int TongSoluong()
        {
            int iTongSoluong = 0;
            List<GioHang> lstGiohang = Session["Giohang"] as List<GioHang>;

            if (lstGiohang != null)
            {
                iTongSoluong = lstGiohang.Sum(n => n.iSoluong);
            }

            return iTongSoluong;
        }
        private double TongTien()
        {
            double dTongTien = 0;
            List<GioHang> lstGiohang = Session["Giohang"] as List<GioHang>;

            if (lstGiohang != null)
            {
                dTongTien = lstGiohang.Sum(n => n.dThanhtien);
            }

            return dTongTien;
        }
        public ActionResult GioHang()
        {
            List<GioHang> lstGiohang = LayGioHang();

            if (lstGiohang.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.TongSoluong = TongSoluong();
            ViewBag.TongTien = TongTien();

            return View(lstGiohang);
        }
        public ActionResult GioHangPartial()
        {
            ViewBag.Tongsoluong = TongSoluong();
            ViewBag.Tongtien = TongTien();
            return PartialView();
        }

        public ActionResult XoaGioHang(int iMaSP)
        {
            List<GioHang> lstGiohang = LayGioHang();
            GioHang sanpham = lstGiohang.SingleOrDefault(n => n.iMasach == iMaSP); 
            if (sanpham != null)
            {
                lstGiohang.RemoveAll(n => n.iMasach == iMaSP);
                return RedirectToAction("GioHang");
            }
            if (lstGiohang.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("GioHang");
        }

        public ActionResult CapNhatGioHang(int iMaSP, FormCollection f)
        {
            List<GioHang> lstGiohang = LayGioHang();
            GioHang sanpham = lstGiohang.SingleOrDefault(n => n.iMasach == iMaSP);
            if (sanpham != null)
            {
                int soLuongMoi = int.Parse(f["txtSoLuong"].ToString());

                if (soLuongMoi > 0)
                {
                    // Cập nhật số lượng
                    sanpham.iSoluong = soLuongMoi;
                }
                else
                {
                    // Nếu số lượng mới <= 0, xóa sản phẩm khỏi giỏ hàng
                    lstGiohang.RemoveAll(n => n.iMasach == iMaSP);
                }
            }
            if (lstGiohang != null)
            {
                sanpham.iSoluong = int.Parse(f["txtSoLuong"].ToString());
            }
            return RedirectToAction("GioHang");
        }

        public ActionResult XoaTatCa()
        {
            List<GioHang> lstGiohang = LayGioHang();
            lstGiohang.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult DatHang()
        {
            // Kiểm tra đăng nhập
            if (Session["Taikhoan"] == null || string.IsNullOrEmpty(Session["Taikhoan"].ToString()))
            {
                return RedirectToAction("Login", "User");
            }

            // Kiểm tra giỏ hàng
            if (Session["Giohang"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Lấy giỏ hàng từ Session
            List<GioHang> lstGiohang = LayGioHang();
            ViewBag.TongSoluong = TongSoluong();
            ViewBag.TongTien = TongTien();

            return View(lstGiohang);
        }

        [HttpPost]
        public ActionResult DatHang(FormCollection collection)
        {
            // Thêm Đơn hàng
            DONDATHANG ddh = new DONDATHANG();
            KHACHHANG kh = (KHACHHANG)Session["Taikhoan"];
            List<GioHang> gh = LayGioHang();
            ddh.MaKH = kh.MaKH;
            ddh.Ngaydat = DateTime.Now;
            var ngaygiao = String.Format("{0:MM/dd/yyyy}", collection["Ngaygiao"]);
            ddh.Ngaygiao = DateTime.Parse(ngaygiao);
            ddh.Tinhtranggiaohang = false;
            ddh.Dathanhtoan = false;
            data.DONDATHANGs.Add(ddh);
            data.SaveChanges();

            // Thêm chi tiết đơn hàng
            foreach (var item in gh)
            {
                CHITIETDONTHANG ctdh = new CHITIETDONTHANG();
                ctdh.MaDonHang = ddh.MaDonHang;
                ctdh.Masach = item.iMasach;
                ctdh.Soluong = item.iSoluong;
                ctdh.Dongia = (decimal)item.dDongia;
                data.CHITIETDONTHANGs.Add(ctdh);
            }

            data.SaveChanges();
            Session["Giohang"] = null;
            return RedirectToAction("Xacnhandonhang", "Giohang");
        }
        public ActionResult Xacnhandonhang()
        {
            return View();
        }
    }
}