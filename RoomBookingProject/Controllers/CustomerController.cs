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
    public class CustomerController : Controller
    {
        private readonly ProjectContext _context;

        public CustomerController(ProjectContext context)
        {
            _context = context;
        }

        // Просмотр всех клиентов

        [Authorize]
        public IActionResult ViewCustomer()
        {
            IQueryable<Customer> customers = _context.Customers.Include(c => c.User);
            return View(customers.ToList());
        }

        [Authorize]
        // CОЗДАНИЕ КЛИЕНТА
        public IActionResult CreateCustomer()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult CreateCustomer(Customer customer)
        {
            if (ModelState.IsValid)
            {
                var existingCustomerWithUserId = _context.Customers.FirstOrDefault(c => c.UserId == customer.UserId);
                if (existingCustomerWithUserId != null)
                {
                    ModelState.AddModelError("UserId", "A customer with the same UserId already exists.");
                }

                var existingCustomerWithName = _context.Customers.FirstOrDefault(c => c.Name == customer.Name);
                if (existingCustomerWithName != null)
                {
                    ModelState.AddModelError("Name", "A customer with the same Name already exists.");
                }

                var existingUser = _context.Users.Any(u => u.UserId == customer.UserId);
                if (!existingUser)
                {
                    ModelState.AddModelError("UserId", "The specified UserId does not belong to an existing user.");
                }

                if (ModelState.ErrorCount > 0)
                {
                    return View(customer);
                }

                _context.Customers.Add(customer);
                _context.SaveChanges();
                return RedirectToAction("ViewCustomer");
            }
            return View(customer);
        }


        [Authorize]
        public async Task<IActionResult> EditCustomer(int? id)
        {
            if (id != null)
            {
                Customer? customer = await _context.Customers.FirstOrDefaultAsync(p => p.CustomerId == id);
                if (customer != null) return View(customer);
            }
            return NotFound();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditCustomer(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction("ViewCustomer");
        }
        

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                Customer? customer = await _context.Customers.FirstOrDefaultAsync(p => p.CustomerId == id);
                if (customer != null)
                {
                    _context.Customers.Remove(customer);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("ViewCustomer");
                }
            }
            return NotFound();
        }

        [Authorize]
        public IActionResult SearchUser(string email)
        {
            var user = _context.Users.FirstOrDefault(c => c.Email == email);
            if (user != null)
            {
                return Content(user.UserId.ToString());
            }
            return NotFound();
        }

        [Authorize]
        public IActionResult SearchByEmail(string email)
        {
            var customers = _context.Customers.Include(c => c.User).Where(c => c.User.Email == email).ToList();
            return View("ViewCustomer", customers);
        }

        [Authorize]
        public IActionResult Search(string email, string name)
        {
            IQueryable<Customer> customers = _context.Customers.Include(c => c.User);

            if (!string.IsNullOrEmpty(email))
            {
                customers = customers.Where(c => c.User.Email == email);
            }

            if (!string.IsNullOrEmpty(name))
            {
                customers = customers.Where(c => c.Name.Contains(name));
            }

            return View("ViewCustomer", customers.ToList());
        }




    }
}
