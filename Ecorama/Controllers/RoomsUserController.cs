using Ecorama.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Ecorama.Controllers
{
    public class RoomsUserController : Controller
    {
        private readonly MyDbContext _context;

        public RoomsUserController(MyDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var rooms = _context.Rooms.Where(r => r.IsActive == true).ToList();
            return View(rooms); // this looks for Views/RoomsUser/Index.cshtml
        }




        public IActionResult Book(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Login");

            var room = _context.Rooms.FirstOrDefault(r => r.RoomId == id);
            if (room == null) return NotFound();

            // Get availability for the room
            var availability = _context.RoomAvailabilities
                .Where(a => a.RoomId == id)
                .ToList();

            var minDate = availability.Min(a => a.AvailableFromDate);
            var maxDate = availability.Max(a => a.AvailableToDate);

            ViewBag.AvailableFrom = minDate;
            ViewBag.AvailableTo = maxDate;

            // ✅ Get booked dates (Approved AND Pending) - تم التحديث هنا
            ViewBag.BookedTimeSlots = JsonConvert.SerializeObject(
                    _context.RoomBookings
                        .Where(b => b.RoomId == id &&
                                   (b.Status == "Approved" || b.Status == "Pending")) // شمل الحجوزات المعلقة أيضاً
                        .Select(b => new {
                            Date = b.BookingDate.Value.ToString("yyyy-MM-dd"),
                            From = b.BookingFrom.Value.ToString("HH:mm"),
                            To = b.BookingTo.Value.ToString("HH:mm")
                        }).ToList()
                    );

            return View("Book", room);
        }




        [HttpPost]
        public IActionResult Book(RoomBooking booking)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Login");

            var room = _context.Rooms.FirstOrDefault(r => r.RoomId == booking.RoomId);
            if (room == null)
            {
                TempData["Error12"] = "الغرفة غير موجودة.";
                return RedirectToAction("Index");
            }

            // التحقق من صحة البيانات المدخلة
            if (!booking.BookingDate.HasValue)
            {
                TempData["Error12"] = "يرجى اختيار تاريخ صالح.";
                return RedirectToAction("Book", new { id = booking.RoomId });
            }

            if (!booking.BookingFrom.HasValue || !booking.BookingTo.HasValue)
            {
                TempData["Error12"] = "يرجى اختيار أوقات صالحة.";
                return RedirectToAction("Book", new { id = booking.RoomId });
            }

            var bookingDateOnly = booking.BookingDate.Value;
            var bookingFrom = booking.BookingFrom.Value;
            var bookingTo = booking.BookingTo.Value;

            // التحقق من منطقية الأوقات
            if (bookingFrom >= bookingTo)
            {
                TempData["Error12"] = "وقت البداية يجب أن يكون قبل وقت النهاية.";
                return RedirectToAction("Book", new { id = booking.RoomId });
            }

            // التحقق من أن الحجز لا يقل عن 30 دقيقة
            if (bookingTo.ToTimeSpan() - bookingFrom.ToTimeSpan() < TimeSpan.FromMinutes(30))
            {
                TempData["Error12"] = "مدة الحجز يجب أن تكون 30 دقيقة على الأقل.";
                return RedirectToAction("Book", new { id = booking.RoomId });
            }

            // التحقق من التاريخ المتاح
            bool isDateAvailable = _context.RoomAvailabilities.Any(a =>
                a.RoomId == booking.RoomId &&
                a.AvailableFromDate <= bookingDateOnly &&
                a.AvailableToDate >= bookingDateOnly
            );

            if (!isDateAvailable)
            {
                TempData["Error12"] = "التاريخ غير متاح للحجز.";
                return RedirectToAction("Book", new { id = booking.RoomId });
            }

            // التحقق المحسن من التداخل في الأوقات
            bool isConflicting = _context.RoomBookings.Any(b =>
                b.RoomId == booking.RoomId &&
                b.BookingDate.Value == bookingDateOnly &&
                (b.Status == "Approved" || b.Status == "Pending") &&
                (
                    // حالات التداخل:
                    // 1. الحجز الجديد يبدأ قبل انتهاء حجز موجود وينتهي بعد بداية حجز موجود
                    (bookingFrom < b.BookingTo && bookingTo > b.BookingFrom) ||
                    // 2. الحجز الجديد يحتوي بالكامل على حجز موجود
                    (bookingFrom <= b.BookingFrom && bookingTo >= b.BookingTo) ||
                    // 3. الحجز الجديد محتوى بالكامل داخل حجز موجود
                    (bookingFrom >= b.BookingFrom && bookingTo <= b.BookingTo)
                )
            );

            if (isConflicting)
            {
                TempData["Error12"] = "الوقت المحدد يتداخل مع حجز موجود. يرجى اختيار أوقات أخرى.";
                return RedirectToAction("Book", new { id = booking.RoomId });
            }

            // التحقق من عدد الضيوف
            if (booking.NumberOfGuests <= 0 || booking.NumberOfGuests > room.Capacity)
            {
                TempData["Error12"] = $"عدد الضيوف يجب أن يكون بين 1 و {room.Capacity}.";
                return RedirectToAction("Book", new { id = booking.RoomId });
            }

            // حفظ الحجز
            booking.UserId = userId.Value;
            booking.Status = "Pending";
            booking.CreatedAt = DateTime.Now;

            try
            {
                _context.RoomBookings.Add(booking);
                _context.SaveChanges();
                TempData["MSG"] = "تم إرسال طلب الحجز بنجاح، يرجى انتظار الموافقة.";
            }
            catch (Exception ex)
            {
                TempData["Error12"] = "حدث خطأ أثناء حفظ الحجز. يرجى المحاولة مرة أخرى.";
            }

            return RedirectToAction("Book", new { id = booking.RoomId });
        }







        public IActionResult MyBookings()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Login");

            var bookings = _context.RoomBookings
                .Include(b => b.Room)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();

            return View(bookings);
        }

    }
}
