using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;
using RoomBookingProject.Models;
using Microsoft.AspNetCore.Authorization;


namespace RoomBookingProject.Controllers
{
    public class BookingController : Controller 
    {
        private readonly ProjectContext _context;

        public BookingController(ProjectContext context)
        {
            _context = context;
        }

        // Отображение всех броней

        [Authorize]
        public IActionResult ViewBooking()
        {
            IQueryable<Booking> bookings = _context.Bookings.Include(p => p.Room)
                                                            .Include(p => p.Customer);

            ViewBag.Rooms = new SelectList(_context.Rooms, "RoomId", "RoomType");
            ViewBag.Customers = new SelectList(_context.Customers, "CustomerId", "Name");

            return View(bookings.ToList());
        }


        [Authorize]
        // CОЗДАНИЕ Бронирования
        public IActionResult CreateBooking()
        {
            ViewBag.Rooms = new SelectList(_context.Rooms, "RoomId", "RoomType");
            return View();
        }

        /*
        [HttpPost]
        public IActionResult CreateBooking(Booking booking)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Rooms = new SelectList(_context.Rooms, "RoomId", "RoomType", booking.RoomId);
                _context.Bookings.Add(booking);
                _context.SaveChanges();
                return RedirectToAction("ViewBooking");
            }
            ViewBag.Rooms = new SelectList(_context.Rooms, "RoomId", "RoomType", booking.RoomId);
            return View(booking);
        }
        */


        [Authorize]
        [HttpPost]
        public IActionResult CreateBooking(Booking booking)
        {
            if (ModelState.IsValid)
            {              

                if (booking.CheckInDate.Date < DateTime.Now.Date)
                {
                    TempData["Message"] = "Error! CheckIn date less than current date.";
                    TempData["IsError"] = true;
                    ViewBag.Rooms = new SelectList(_context.Rooms, "RoomId", "RoomType", booking.RoomId);
                    return View(booking);
                }

                if (booking.CheckOutDate <= booking.CheckInDate)
                {
                    TempData["Message"] = "Error! CheckOut date less than CheckIn date.";
                    TempData["IsError"] = true;
                    ViewBag.Rooms = new SelectList(_context.Rooms, "RoomId", "RoomType", booking.RoomId);
                    return View(booking);
                }

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
                    return View(booking);
                }

                _context.Bookings.Add(booking);
                _context.SaveChanges();
                return RedirectToAction("ViewBooking");
            }

            ViewBag.Rooms = new SelectList(_context.Rooms, "RoomId", "RoomType", booking.RoomId);
            return View(booking);
        }

        // Редактирование броней
        [Authorize]
        public async Task<IActionResult> EditBooking(int? id)
        {
            if (id != null)
            {
                Booking? booking = await _context.Bookings.FirstOrDefaultAsync(p => p.BookingId == id);
                ViewBag.Rooms = new SelectList(_context.Rooms, "RoomId", "RoomType", booking.RoomId);
                if (booking != null) return View(booking);
            }
            return NotFound();
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditBooking(Booking booking)
        {
            if (ModelState.IsValid)
            {

                if (booking.CheckInDate.Date < DateTime.Now.Date)
                {
                    TempData["Message"] = "Error! CheckIn date less than current date.";
                    TempData["IsError"] = true;
                    ViewBag.Rooms = new SelectList(_context.Rooms, "RoomId", "RoomType", booking.RoomId);
                    return View(booking);
                }

                if (booking.CheckOutDate <= booking.CheckInDate)
                {
                    TempData["Message"] = "Error! CheckOut date less than CheckIn date.";
                    TempData["IsError"] = true;
                    ViewBag.Rooms = new SelectList(_context.Rooms, "RoomId", "RoomType", booking.RoomId);
                    return View(booking);
                }

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
                    return View(booking);
                }


                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction("ViewBooking");
            }
            ViewBag.Rooms = new SelectList(_context.Rooms, "RoomId", "RoomType", booking.RoomId);
            return View(booking);
        }

        // Удаление брони
        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                Booking? booking = await _context.Bookings.FirstOrDefaultAsync(p => p.BookingId == id);

                if (booking != null)
                {
                    _context.Bookings.Remove(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("ViewBooking");
                }
            }
            return NotFound();
        }
        

        /*
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                Booking? booking = await _context.Bookings.FirstOrDefaultAsync(p => p.BookingId == id);

                if (booking != null)
                {
                    booking.Status = "Declined"; 
                    await _context.SaveChangesAsync();
                    return RedirectToAction("ViewBooking");
                }
            }
            return NotFound();
        }
        */

        public IActionResult SearchCustomer(string name)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Name == name);
            if (customer != null)
            {
                return Content(customer.CustomerId.ToString());
            }
            return NotFound();
        }



    }
}

