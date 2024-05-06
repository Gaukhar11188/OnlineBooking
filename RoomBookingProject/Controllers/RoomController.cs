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
    public class RoomController : Controller
    {
        private readonly ProjectContext _context;

        public RoomController(ProjectContext context)
        {
            _context = context;
        }


        // Отображение комнат

        [Authorize]
        public IActionResult ViewRoom()
        {
            IQueryable<Room> rooms = _context.Rooms;
            return View(rooms.ToList());
        }


        [Authorize]
        // CОЗДАНИЕ Комнаты
        public IActionResult CreateRoom()
        {
            return View();
        }

        /*
        [HttpPost]
        public IActionResult CreateRoom(Room room)
        {
            if (ModelState.IsValid)
            {
                _context.Rooms.Add(room);
                _context.SaveChanges();
                return RedirectToAction("ViewRoom");
            }
            return View(room);
        }
        */


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateRoom(Room room, IFormFile imgFile)
        {
            if (ModelState.IsValid)
            {
                if (imgFile != null && imgFile.Length > 0)
                {
                    var extension = Path.GetExtension(imgFile.FileName).ToLowerInvariant();

                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif")
                    {
                        room.Img = imgFile.FileName;

                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "room", imgFile.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imgFile.CopyToAsync(stream);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("ImgFile", "Please upload a valid image file.");
                        return View(room);
                    }
                }

                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();
                return RedirectToAction("ViewRoom");
            }
            return View(room);
        }


        [Authorize]
        public async Task<IActionResult> EditRoom(int? id)
        {
            if (id != null)
            {
                Room? room = await _context.Rooms.FirstOrDefaultAsync(p => p.RoomId == id);
                if (room != null) return View(room);
            }
            return NotFound();
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditRoom(Room room)
        {
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
            return RedirectToAction("ViewRoom");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                Room? room = await _context.Rooms.FirstOrDefaultAsync(p => p.RoomId == id);
                if (room != null)
                {
                    _context.Rooms.Remove(room);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("ViewRoom");
                }
            }
            return NotFound();
        }
    }
}
