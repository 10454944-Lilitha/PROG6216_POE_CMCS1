using CMCS1.Models;
using Microsoft.AspNetCore.Mvc;

namespace CMCS1.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account/SelectRole
        [HttpGet]
        public IActionResult SelectRole()
        {
            return View();
        }

        // POST: Account/SetRole
        [HttpPost]
        public IActionResult SetRole(string role)
        {
            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction("SelectRole");
            }

            // Store the role in session
            HttpContext.Session.SetString("UserRole", role);

            // Redirect based on role
            if (role == "Lecturer")
            {
                return RedirectToAction("Index", "Claims");
            }
            else // Manager1 or Manager2
            {
                return RedirectToAction("ReviewClaims", "Claims");
            }
        }

        // GET: Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("SelectRole");
        }
    }
}
