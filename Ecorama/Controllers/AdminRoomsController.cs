using ClosedXML.Excel;
using Ecorama.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Ecorama.Controllers
{
    public class AdminRoomsController : Controller
    {
        private readonly MyDbContext _context;

        public AdminRoomsController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var rooms = _context.Rooms.ToList();
            return View(rooms);
        }



        public async Task<IActionResult> ExportRoomsToExcel()
        {
            var rooms = await _context.Rooms.ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Rooms");

                // العناوين
                var headers = new[]
                {
            "Room ID", "Name", "Description", "Capacity", "Type",
            "Status", "Created At"
        };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Cell(1, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // البيانات
                for (int i = 0; i < rooms.Count; i++)
                {
                    var room = rooms[i];
                    worksheet.Cell(i + 2, 1).Value = room.RoomId;
                    worksheet.Cell(i + 2, 2).Value = room.Name;
                    worksheet.Cell(i + 2, 3).Value = room.Description;
                    worksheet.Cell(i + 2, 4).Value = room.Capacity;
                    worksheet.Cell(i + 2, 5).Value = room.Type;
                    worksheet.Cell(i + 2, 6).Value = room.IsActive == true ? "مفعّلة" : "معطّلة";
                    worksheet.Cell(i + 2, 7).Value = room.CreatedAt?.ToString("yyyy-MM-dd");
                }

                // ضبط العرض التلقائي
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Column(i + 1).AdjustToContents();
                }

                // تصدير الملف
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "RoomsExport.xlsx");
                }
            }
        }

















        [HttpGet]
        public IActionResult AddRoom()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddRoom(string name, string description, int capacity, string type, IFormFile imageFile)
        {
            int? adminId = HttpContext.Session.GetInt32("UserId");

            if (adminId == null)
                return RedirectToAction("Login", "Login");

            if (string.IsNullOrEmpty(name) || imageFile == null || imageFile.Length == 0)
            {
                TempData["MSG"] = "الرجاء تعبئة جميع الحقول المطلوبة.";
                return View();
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(imageFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                TempData["MSG"] = "صيغة الصورة غير مدعومة.";
                return View();
            }

            if (imageFile.Length > 2 * 1024 * 1024)
            {
                TempData["MSG"] = "حجم الصورة يجب أن لا يتجاوز 2MB.";
                return View();
            }

            var fileName = Guid.NewGuid().ToString() + extension;
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/rooms");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            var imageUrl = "/uploads/rooms/" + fileName;

            var newRoom = new Room
            {
                Name = name,
                Description = description,
                Capacity = capacity,
                Type = type,
                ImageUrl = imageUrl,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Rooms.Add(newRoom);
            await _context.SaveChangesAsync();

            TempData["MSG1212"] = "تمت إضافة الغرفة بنجاح.";
            return RedirectToAction("Index");
        }


        [HttpGet]
        public IActionResult EditRoom(int id)
        {
            var room = _context.Rooms.Find(id);
            if (room == null) return NotFound();

            ViewBag.RoomTypes = new List<SelectListItem>
    {
        new SelectListItem { Text = "اجتماعات", Value = "اجتماعات" },
        new SelectListItem { Text = "فردية", Value = "فردية" },
        new SelectListItem { Text = "جماعية", Value = "جماعية" },
        new SelectListItem { Text = "دراسية", Value = "دراسية" },
        new SelectListItem { Text = "ورشة عمل", Value = "ورشة عمل" },
    };

            return View(room);
        }


        [HttpPost]
        public async Task<IActionResult> EditRoom(int id, string name, string description, int capacity, string type, IFormFile imageFile)
        {
            var room = _context.Rooms.Find(id);
            if (room == null)
                return NotFound();

            room.Name = name;
            room.Description = description;
            room.Capacity = capacity;
            room.Type = type;

            if (imageFile != null && imageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(imageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    TempData["MSG"] = "صيغة الصورة غير مدعومة.";
                    return View(room);
                }

                if (imageFile.Length > 2 * 1024 * 1024)
                {
                    TempData["MSG"] = "حجم الصورة يجب أن لا يتجاوز 2MB.";
                    return View(room);
                }

                var fileName = Guid.NewGuid().ToString() + extension;
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/rooms");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                room.ImageUrl = "/uploads/rooms/" + fileName;
            }

            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();

            TempData["MSG"] = "تم تعديل بيانات الغرفة بنجاح.";
            return RedirectToAction("Index");
        }

        public IActionResult ToggleStatus(int id)
        {
            var room = _context.Rooms.Find(id);
            if (room != null)
            {
                room.IsActive = !(room.IsActive ?? false);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var room = _context.Rooms.Find(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }



        ////////////////////////
        [HttpGet]
        public IActionResult AddAvailability(int roomId)
        {
            ViewBag.RoomId = roomId;
            var room = _context.Rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room == null) return NotFound();
            ViewBag.RoomName = room.Name;
            return View();
        }

        [HttpPost]
        public IActionResult AddAvailability(RoomAvailability model)
        {
            if (model.AvailableFromDate > model.AvailableToDate)
                ModelState.AddModelError("", "تاريخ البداية يجب أن يكون قبل تاريخ النهاية.");

            if (model.AvailableFromTime >= model.AvailableToTime)
                ModelState.AddModelError("", "وقت البداية يجب أن يكون قبل وقت النهاية.");

            // التحقق من عدم وجود تداخل في أوقات التوفر
            var existingAvailability = _context.RoomAvailabilities
                .Where(ra => ra.RoomId == model.RoomId &&
                            ((model.AvailableFromDate >= ra.AvailableFromDate && model.AvailableFromDate <= ra.AvailableToDate) ||
                             (model.AvailableToDate >= ra.AvailableFromDate && model.AvailableToDate <= ra.AvailableToDate) ||
                             (model.AvailableFromDate <= ra.AvailableFromDate && model.AvailableToDate >= ra.AvailableToDate)))
                .Any();

            if (existingAvailability)
                ModelState.AddModelError("", "يوجد تداخل في فترات التوفر للغرفة.");

            if (ModelState.IsValid)
            {
                _context.RoomAvailabilities.Add(model);
                _context.SaveChanges();
                return RedirectToAction("ViewAvailability", new { roomId = model.RoomId });
            }

            ViewBag.RoomId = model.RoomId;
            ViewBag.RoomName = _context.Rooms.Find(model.RoomId)?.Name;
            return View(model);
        }


        public IActionResult ViewAvailability(int roomId)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room == null) return NotFound();

            var availability = _context.RoomAvailabilities
                .Where(a => a.RoomId == roomId)
                .OrderBy(a => a.AvailableFromDate)
                .ToList();

            ViewBag.RoomName = room.Name;
            ViewBag.RoomId = roomId;
            return View(availability);
        }

        public IActionResult DeleteAvailability(int id, int roomId)
        {
            var record = _context.RoomAvailabilities.Find(id);
            if (record != null)
            {
                _context.RoomAvailabilities.Remove(record);
                _context.SaveChanges();
            }

            return RedirectToAction("ViewAvailability", new { roomId });
        }



        public IActionResult BookingsList()
        {
            var bookings = _context.RoomBookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();

            return View(bookings);
        }



        public IActionResult ApproveBooking(int id)
        {
            var booking = _context.RoomBookings.Find(id);
            if (booking == null) return NotFound();

            booking.Status = "Approved";
            _context.SaveChanges();
            return RedirectToAction("BookingsList");
        }

        public IActionResult RejectBooking(int id)
        {
            var booking = _context.RoomBookings.Find(id);
            if (booking == null) return NotFound();

            booking.Status = "Rejected";
            _context.SaveChanges();
            return RedirectToAction("BookingsList");
        }












        //[HttpGet]
        //[Route("api/GetRoomAvailability")]
        //public IActionResult GetRoomAvailability(int roomId, string date)
        //{
        //    try
        //    {
        //        // تحويل التاريخ من string إلى DateOnly
        //        if (!DateOnly.TryParse(date, out DateOnly selectedDate))
        //        {
        //            return BadRequest("تاريخ غير صحيح");
        //        }

        //        // البحث عن توفر الغرفة للتاريخ المحدد
        //        var availabilityRaw = _context.RoomAvailabilities
        //             .Where(ra => ra.RoomId == roomId &&
        //                          ra.AvailableFromDate <= selectedDate &&
        //                          ra.AvailableToDate >= selectedDate)
        //             .ToList(); // جلب البيانات أولاً

        //        var availability = availabilityRaw
        //            .Select(ra => new {
        //                availableFromTime = ra.AvailableFromTime?.ToString("HH:mm"),
        //                availableToTime = ra.AvailableToTime?.ToString("HH:mm")
        //            })
        //            .ToList();


        //        return Json(availability);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "خطأ في الخادم");
        //    }
        //}



        // إضافة هذه الدالة للـ Controller الخاص بالحجز (وليس AdminRoomsController)

        [HttpGet]
        [Route("api/GetBookedSlots")]
        public IActionResult GetBookedSlots(int roomId, string date)
        {
            try
            {
                if (!DateOnly.TryParse(date, out DateOnly selectedDate))
                {
                    return BadRequest("تاريخ غير صحيح");
                }

                // جلب الحجوزات المؤكدة لهذا اليوم والغرفة
                var bookedSlots = _context.RoomBookings
                    .Where(b => b.RoomId == roomId &&
                               b.BookingDate == selectedDate &&
                               (b.Status == "Approved" || b.Status == "Pending")) // الحجوزات المؤكدة أو في الانتظار
                    .Select(b => new
                    {
                        Date = b.BookingDate.ToString(),
                        From = b.BookingFrom.ToString(),
                        To = b.BookingTo.ToString()
                    })
                    .ToList();

                return Json(bookedSlots);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "خطأ في الخادم");
            }
        }

        [HttpGet]
        [Route("api/GetRoomAvailability")]
        public IActionResult GetRoomAvailability(int roomId, string date)
        {
            try
            {
                if (!DateOnly.TryParse(date, out DateOnly selectedDate))
                {
                    return BadRequest("تاريخ غير صحيح");
                }

                // البحث عن توفر الغرفة للتاريخ المحدد
                var availabilityRaw = _context.RoomAvailabilities
                     .Where(ra => ra.RoomId == roomId &&
                                  ra.AvailableFromDate <= selectedDate &&
                                  ra.AvailableToDate >= selectedDate)
                     .ToList();

                var availability = availabilityRaw
                    .Select(ra => new
                    {
                        availableFromTime = ra.AvailableFromTime?.ToString("HH:mm"),
                        availableToTime = ra.AvailableToTime?.ToString("HH:mm")
                    })
                    .ToList();

                // جلب الحجوزات المؤكدة لهذا اليوم
                var bookedSlotsRaw = _context.RoomBookings
                      .Where(b => b.RoomId == roomId &&
                                  b.BookingDate == selectedDate &&
                                  (b.Status == "Approved" || b.Status == "Pending"))
                      .ToList(); // اجلب البيانات إلى الذاكرة

                var bookedSlots = bookedSlotsRaw
                    .Select(b => new
                    {
                        From = b.BookingFrom?.ToString("HH:mm"),
                        To = b.BookingTo?.ToString("HH:mm")
                    })
                    .ToList();


                return Json(new
                {
                    availability = availability,
                    bookedSlots = bookedSlots
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "خطأ في الخادم: " + ex.Message);
            }
        }











        public IActionResult RoomCalendar()
        {
            return View();
        }


        [HttpGet]
        public IActionResult GetBookings()
        {
            var bookings = _context.RoomBookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Where(b => b.BookingDate != null && b.Status == "Approved") 

                .Select(b => new
                {
                    id = b.BookingId,
                    title = b.Purpose ?? "No Purpose",
                    start = b.BookingDate.Value.ToString("yyyy-MM-dd") + "T" + b.BookingFrom.Value.ToString("HH\\:mm"),
                    end = b.BookingDate.Value.ToString("yyyy-MM-dd") + "T" + b.BookingTo.Value.ToString("HH\\:mm"),

                    extendedProps = new
                    {
                        guests = b.NumberOfGuests,
                        notes = b.Notes,
                        room = b.Room != null ? b.Room.Name : null,
                        user = b.User != null ? b.User.FirstName + " " + b.User.LastName : null,
                        startTime = b.BookingFrom.Value.ToString(@"HH\:mm"), 
                        endTime = b.BookingTo.Value.ToString(@"HH\:mm"),
                        date = b.BookingDate
                    }
                })
                .ToList();

            return Json(bookings);
        }







    }
}
