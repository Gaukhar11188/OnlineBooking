using RoomBookingProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics.Eventing.Reader;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;

namespace RoomBookingProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProjectContext _context;

        public HomeController(ProjectContext context)
        {
            _context = context;
        }

        
        public IActionResult RoomInfo()
        {
            IQueryable<Room> rooms = _context.Rooms;
            return View(rooms.ToList());
        }
        

        public IActionResult Index()
        {
            IQueryable<Blog> blogs = _context.Blogs;
            return View(blogs.ToList());
        }

        [Authorize]
        public IActionResult Room(DateTime? checkInDate, DateTime? checkOutDate, int guests)
        {
            if (checkInDate.HasValue)
            {
                HttpContext.Session.SetString("CheckInDate", checkInDate.Value.ToString("yyyy-MM-dd"));
            }
            if (checkOutDate.HasValue)
            {
                HttpContext.Session.SetString("CheckOutDate", checkOutDate.Value.ToString("yyyy-MM-dd"));
            }

            if (checkInDate.HasValue && checkOutDate.HasValue)
            {
                var checkInDateTime = checkInDate.Value;
                var checkOutDateTime = checkOutDate.Value;

                var availableRooms = _context.Rooms.Where(room =>
    !_context.Bookings.Any(booking =>
        booking.RoomId == room.RoomId &&
        ((checkInDateTime < booking.CheckOutDate && checkOutDateTime > booking.CheckInDate) ||
        (checkInDateTime <= booking.CheckInDate && checkOutDateTime >= booking.CheckOutDate)
    )))
    .Where(room => room.Capacity == guests)
.ToList();

                return View(availableRooms);
            }

            var allRooms = _context.Rooms.ToList();
            return View(allRooms);
        }

        /*
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> RoomDetails(int? id)
        {
            if (id != null)
            {
                Room room = await _context.Rooms.FirstOrDefaultAsync(p => p.RoomId == id);

                if (room != null)
                {
                    string checkInDateString = HttpContext.Session?.GetString("CheckInDate");
                    string checkOutDateString = HttpContext.Session?.GetString("CheckOutDate");

                    DateTime checkInDate = DateTime.Today.AddHours(14); 
                    DateTime checkOutDate = DateTime.Today.AddDays(1).AddHours(12);

                    if (!string.IsNullOrEmpty(checkInDateString) && !string.IsNullOrEmpty(checkOutDateString))
                    {
                        if (DateTime.TryParse(checkInDateString, out DateTime parsedCheckInDate))
                        {
                            checkInDate = parsedCheckInDate;
                        }

                        if (DateTime.TryParse(checkOutDateString, out DateTime parsedCheckOutDate))
                        {
                            checkOutDate = parsedCheckOutDate;
                        }
                    }

                    ViewBag.CheckInDate = checkInDate;
                    ViewBag.CheckOutDate = checkOutDate;

                    if (User.Identity.IsAuthenticated)
                    {
                        string userEmail = User.FindFirstValue(ClaimTypes.Name);
                        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                        if (user != null)
                        {
                            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.UserId);

                            if (customer != null)
                            {
                                ViewBag.UserName = customer.Name;
                                ViewBag.UserPhone = customer.Phone;
                            }
                        }
                    }

                    return View(room);
                }
            }

            return NotFound();
        }
        */

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> RoomDetails(int? id)
        {
            if (id != null)
            {
                Room room = await _context.Rooms.FirstOrDefaultAsync(p => p.RoomId == id);

                DateTime checkInDate = DateTime.Today.AddHours(14);
                DateTime checkOutDate = DateTime.Today.AddDays(1).AddHours(12);

                if (room != null)
                {
                    
                    string checkInDateString = HttpContext.Session?.GetString("CheckInDate");
                    string checkOutDateString = HttpContext.Session?.GetString("CheckOutDate");

                    if (!string.IsNullOrEmpty(checkInDateString) && !string.IsNullOrEmpty(checkOutDateString))
                    {
                        if (DateTime.TryParse(checkInDateString, out DateTime parsedCheckInDate))
                        {
                           
                            checkInDate = parsedCheckInDate.Date.AddHours(14);
                        }

                        if (DateTime.TryParse(checkOutDateString, out DateTime parsedCheckOutDate))
                        {
                            
                            checkOutDate = parsedCheckOutDate.Date.AddHours(12);
                        }
                    }

                    ViewBag.CheckInDate = checkInDate;
                    ViewBag.CheckOutDate = checkOutDate;

                    if (User.Identity.IsAuthenticated)
                    {
                        string userEmail = User.FindFirstValue(ClaimTypes.Name);
                        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                        if (user != null)
                        {
                            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.UserId);

                            if (customer != null)
                            {
                                ViewBag.UserName = customer.Name;
                                ViewBag.UserPhone = customer.Phone;
                            }
                        }
                    }

                    return View(room);
                }
            }

            return NotFound();
        }


        [HttpPost]
        public async Task<IActionResult> CalculateTotalPrice(DateTime checkInDate, DateTime checkOutDate, decimal pricePerNight)
        {
            var duration = (checkOutDate.Date - checkInDate.Date).TotalDays;
            var total = pricePerNight * (decimal)duration;
            return Json(total);
        }

        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RoomDetails(IFormCollection form, Booking model)
        {
            if (ModelState.IsValid)
            {

                string name = form["Name"];
                string phone = form["Phone"];
                string checkInDate = form["checkInDate"];
                string checkOutDate = form["checkOutDate"];
                string roomId = form["RoomId"];

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
                {
                    TempData["Message"] = "Please provide your name and phone number.";
                    TempData["IsError"] = true;
                    return RedirectToAction("RoomDetails", new { id = roomId });
                }

                Customer customer = await _context.Customers.FirstOrDefaultAsync(c => c.Name == name && c.Phone == phone);
                if (customer == null)
                {
                    customer = new Customer
                    {
                        Name = name,
                        Phone = phone,
                        UserId = Convert.ToInt32(User.FindFirstValue("UserId"))
                    };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }

                // Создаем запись о бронировании
                Booking booking = new Booking
                {
                    RoomId = Convert.ToInt32(roomId),
                    CustomerId = customer.CustomerId,
                    CheckInDate = DateTime.Parse(checkInDate),
                    CheckOutDate = DateTime.Parse(checkOutDate)
                };

                var isRoomAvailable = !_context.Bookings.Any(b =>
                           b.RoomId == booking.RoomId &&
                       (
                           (booking.CheckInDate < b.CheckOutDate && booking.CheckOutDate > b.CheckInDate) ||
                           (booking.CheckInDate <= b.CheckInDate && booking.CheckOutDate >= b.CheckOutDate)
                       )
                       );

                if (!isRoomAvailable)
                {
                    TempData["Message"] = "The room is not available.";
                    TempData["IsError"] = true;
                    ViewBag.Rooms = new SelectList(_context.Rooms, "RoomId", "RoomType", booking.RoomId);
                    return RedirectToAction("RoomDetails", new { id = roomId });
                }

                if (booking.CheckInDate.Date < DateTime.Now.Date)
                {
                    TempData["Message"] = "Error! CheckIn date less than current date.";
                    TempData["IsError"] = true;
                    ViewBag.Rooms = new SelectList(_context.Rooms, "RoomId", "RoomType", booking.RoomId);
                    return RedirectToAction("RoomDetails", new { id = roomId });
                }

                if (booking.CheckOutDate <= booking.CheckInDate)
                {
                    TempData["Message"] = "Error! CheckOut date less than CheckIn date.";
                    TempData["IsError"] = true;
                    ViewBag.Rooms = new SelectList(_context.Rooms, "RoomId", "RoomType", booking.RoomId);
                    return RedirectToAction("RoomDetails", new { id = roomId });
                }

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                return RedirectToAction("BookingConfirmation");
            }

            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> BookingConfirmation()
        {
            if (User.Identity.IsAuthenticated)
            {
                string userEmail = User.FindFirstValue(ClaimTypes.Name);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user != null)
                {
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.UserId);

                    if (customer != null)
                    {
                        ViewBag.CustomerId = customer.CustomerId;

                     
                        var bookings = await _context.Bookings
                            .Include(b => b.Room)
                            .Include(b => b.Customer)
                            .Where(b => b.CustomerId == customer.CustomerId)
                            .ToListAsync();

                        return View(bookings);
                    }
                }
            }

            return View();
        }
        

        

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        
        // Страница Авторизации нового клиента
        public IActionResult Registration()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Registration([Bind("Id, Email, Password")] User user)
        {
            if (ModelState.IsValid)
            {
            
                User existingUser = _context.Users.FirstOrDefault(p => p.Email == user.Email && p.Password == user.Password);

                if (existingUser == null)
                {
                    TempData["Message"] = $"User doesn't exist or wrong password!";
                    TempData["IsError"] = true;
                    return Unauthorized();
                }

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim("UserId", existingUser.UserId.ToString()),
            new Claim("Email", existingUser.Email)
        };

               
                if (existingUser.IsAdmin)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                }

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                ViewBag.UserEmail = existingUser.Email;

                return RedirectToAction("Index");
            }

            return View();
        }


        public IActionResult Unauthorized()
        {
            return View();
        }

        public IActionResult _Header()
        {
            return View();
        }


        // Страница РЕГИСТРАЦИИ ПОЛЬЗОВАТЕЛЯ

        [HttpGet]
        public IActionResult Registration2()
        {
            return View();
        }

        public async Task<IActionResult> Registration2([Bind("UserId, Email, Password")] User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
            {
                TempData["Message"] = $"User with email {user.Email} already exists!";
                TempData["IsError"] = true;
                return View(user);
            }
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Registration successful!";
            TempData["IsError"] = false;
            return RedirectToAction("Registration");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Registration");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}