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
            int? AdminId = HttpContext.Session.GetInt32("AdminId");
            if (AdminId == null)
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

            // Get booked dates (Approved AND Pending)
            ViewBag.BookedTimeSlots = JsonConvert.SerializeObject(
                    _context.RoomBookings
                        .Where(b => b.RoomId == id &&
                                   (b.Status == "Approved" || b.Status == "Pending"))
                        .Select(b => new {
                            Date = b.BookingDate.Value.ToString("yyyy-MM-dd"),
                            From = b.BookingFrom.Value.ToString("HH:mm"),
                            To = b.BookingTo.Value.ToString("HH:mm")
                        }).ToList()
                    );

            return View("Book", room);
        }

        [HttpPost]
        public IActionResult Book(RoomBooking booking, string BookingType)
        {
            int? userId = HttpContext.Session.GetInt32("AdminId");
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

            var bookingDateOnly = booking.BookingDate.Value;

            // معالجة حجز اليوم الكامل
            if (BookingType == "fullDay")
            {
                booking.BookingFrom = new TimeOnly(8, 0);  // 8:00 AM
                booking.BookingTo = new TimeOnly(22, 0);   // 10:00 PM

                // التحقق من وجود أي حجوزات في نفس اليوم للحجز الكامل
                bool hasConflictingBookings = _context.RoomBookings.Any(b =>
                    b.RoomId == booking.RoomId &&
                    b.BookingDate.Value == bookingDateOnly &&
                    (b.Status == "Approved" || b.Status == "Pending")
                );

                if (hasConflictingBookings)
                {
                    TempData["Error12"] = "لا يمكن حجز اليوم كاملاً، توجد حجوزات أخرى في نفس اليوم.";
                    return RedirectToAction("Book", new { id = booking.RoomId });
                }
            }
            else
            {
                // التحقق من الأوقات للحجز بالساعة
                if (!booking.BookingFrom.HasValue || !booking.BookingTo.HasValue)
                {
                    TempData["Error12"] = "يرجى اختيار أوقات صالحة.";
                    return RedirectToAction("Book", new { id = booking.RoomId });
                }

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

                // التحقق من عدم وجود حجز يوم كامل في نفس التاريخ
                bool hasFullDayBooking = _context.RoomBookings.Any(b =>
                    b.RoomId == booking.RoomId &&
                    b.BookingDate.Value == bookingDateOnly &&
                    (b.Status == "Approved" || b.Status == "Pending") &&
                    b.BookingFrom.Value.Hour == 8 && b.BookingFrom.Value.Minute == 0 &&
                    b.BookingTo.Value.Hour == 22 && b.BookingTo.Value.Minute == 0
                );

                if (hasFullDayBooking)
                {
                    TempData["Error12"] = "لا يمكن الحجز، الغرفة محجوزة لليوم كامل في هذا التاريخ.";
                    return RedirectToAction("Book", new { id = booking.RoomId });
                }

                // التحقق المحسن من التداخل في الأوقات
                bool isConflicting = _context.RoomBookings.Any(b =>
                    b.RoomId == booking.RoomId &&
                    b.BookingDate.Value == bookingDateOnly &&
                    (b.Status == "Approved" || b.Status == "Pending") &&
                    (
                        // حالات التداخل:
                        (bookingFrom < b.BookingTo && bookingTo > b.BookingFrom) ||
                        (bookingFrom <= b.BookingFrom && bookingTo >= b.BookingTo) ||
                        (bookingFrom >= b.BookingFrom && bookingTo <= b.BookingTo)
                    )
                );

                if (isConflicting)
                {
                    TempData["Error12"] = "الوقت المحدد يتداخل مع حجز موجود. يرجى اختيار أوقات أخرى.";
                    return RedirectToAction("Book", new { id = booking.RoomId });
                }
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

            // إضافة ملاحظة للحجز الكامل
            if (BookingType == "fullDay")
            {
                booking.Notes = booking.Notes + (string.IsNullOrEmpty(booking.Notes) ? "" : " - ") + "حجز يوم كامل";
            }

            try
            {
                _context.RoomBookings.Add(booking);
                _context.SaveChanges();

                string successMessage = BookingType == "fullDay"
                    ? "تم إرسال طلب حجز اليوم الكامل بنجاح، يرجى انتظار الموافقة."
                    : "تم إرسال طلب الحجز بنجاح، يرجى انتظار الموافقة.";

                TempData["MSG"] = successMessage;
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
