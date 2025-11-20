using CMCS1.Data;
using CMCS1.Models;
using CMCS1.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMCS1.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);

                if (user != null)
                {
                    // Store user info in session
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("Username", user.Username);

                    if (user.Role == "Lecturer")
                    {
                        return RedirectToAction("Index", "Claims");
                    }
                    else if (user.Role == "HR")
                    {
                        return RedirectToAction("Dashboard", "HR");
                    }
                    else // Manager1 or Manager2
                    {
                        return RedirectToAction("ReviewClaims", "Claims");
                    }
                }

                ModelState.AddModelError("", "Invalid username or password");
            }

            return View(model);
        }

        // GET: Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
