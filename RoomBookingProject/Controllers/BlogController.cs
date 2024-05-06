using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomBookingProject.Models;

namespace RoomBookingProject.Controllers
{
    public class BlogController : Controller
    {

        private readonly ProjectContext _context;

        public BlogController(ProjectContext context)
        {
            _context = context;
        }

        // Просмотр всех блогов
        [Authorize]
        public IActionResult ViewBlog()
        {
            IQueryable<Blog> blogs = _context.Blogs;
            return View(blogs.ToList());
        }


        [Authorize]
        // CОЗДАНИЕ БЛОГА
        public IActionResult CreateBlog()
        {
            return View();
        }

        /*
        [HttpPost]
        public IActionResult CreateBlog(Blog blog)
        {
            if (ModelState.IsValid)
            {
                _context.Blogs.Add(blog);
                _context.SaveChanges();
                return RedirectToAction("ViewBlog");
            }
            return View(blog);
        }
        */


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateBlog(Blog blog, IFormFile imgFile)
        {
            if (ModelState.IsValid)
            {
                if (imgFile != null && imgFile.Length > 0)
                {
                    var extension = Path.GetExtension(imgFile.FileName).ToLowerInvariant();

                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif")
                    {
                        blog.Img = imgFile.FileName;

                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "blog", imgFile.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imgFile.CopyToAsync(stream);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("ImgFile", "Please upload a valid image file.");
                        return View(blog);
                    }
                }

                _context.Blogs.Add(blog);
                await _context.SaveChangesAsync();
                return RedirectToAction("ViewBlog");
            }
            return View(blog);
        }

        [Authorize]
        // Редактирование БЛОГА
        public async Task<IActionResult> EditBlog(int? id)
        {
            if (id != null)
            {
                Blog? blog = await _context.Blogs.FirstOrDefaultAsync(p => p.BlogId == id);
                if (blog != null) return View(blog);
            }
            return NotFound();
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditBlog(Blog blog)
        {
            _context.Blogs.Update(blog);
            await _context.SaveChangesAsync();
            return RedirectToAction("ViewBlog");
        }

        // Удаление БЛОГА
        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                Blog? blog = await _context.Blogs.FirstOrDefaultAsync(p => p.BlogId == id);
                if (blog != null)
                {
                    _context.Blogs.Remove(blog);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("ViewBlog");
                }
            }
            return NotFound();
        }
    }
}

