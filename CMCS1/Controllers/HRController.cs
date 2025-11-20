using CMCS1.Data;
using CMCS1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.Text;

namespace CMCS1.Controllers
{
    public class HRController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConverter _pdfConverter;

        public HRController(AppDbContext context, IConverter pdfConverter)
        {
            _context = context;
            _pdfConverter = pdfConverter;
        }

        private bool IsHR()
        {
            return HttpContext.Session.GetString("UserRole") == "HR";
        }

        private IActionResult? CheckHRAccess()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole")))
            {
                return RedirectToAction("Login", "Account");
            }
            if (!IsHR())
            {
                return RedirectToAction("Index", "Home");
            }
            return null;
        }

        // GET: HR/Dashboard
        public IActionResult Dashboard()
        {
            var accessCheck = CheckHRAccess();
            if (accessCheck != null) return accessCheck;

            return View();
        }

        // GET: HR/ManageUsers
        public async Task<IActionResult> ManageUsers()
        {
            var accessCheck = CheckHRAccess();
            if (accessCheck != null) return accessCheck;
            
            var users = await _context.Users.OrderBy(u => u.Role).ThenBy(u => u.FullName).ToListAsync();
            return View(users);
        }

        // GET: HR/CreateUser
        public IActionResult CreateUser()
        {
            var accessCheck = CheckHRAccess();
            if (accessCheck != null) return accessCheck;
            return View();
        }

        // POST: HR/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User user)
        {
            var accessCheck = CheckHRAccess();
            if (accessCheck != null) return accessCheck;
            
            if (ModelState.IsValid)
            {
                // Check if username already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Username already exists");
                    return View(user);
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"User '{user.FullName}' created successfully. Username: {user.Username}, Password: {user.Password}";
                return RedirectToAction("ManageUsers");
            }
            return View(user);
        }

        // GET: HR/EditUser/5
        public async Task<IActionResult> EditUser(int? id)
        {
            var accessCheck = CheckHRAccess();
            if (accessCheck != null) return accessCheck;
            
            if (id == null) return NotFound();
            
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            
            return View(user);
        }

        // POST: HR/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, User user)
        {
            var accessCheck = CheckHRAccess();
            if (accessCheck != null) return accessCheck;
            
            if (id != user.UserId) return NotFound();
            
            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(id);
                    if (existingUser == null) return NotFound();
                    
                    existingUser.FullName = user.FullName;
                    existingUser.Email = user.Email;
                    existingUser.Phone = user.Phone;
                    existingUser.HourlyRate = user.HourlyRate;
                    existingUser.Username = user.Username;
                    existingUser.Password = user.Password;
                    existingUser.Role = user.Role;
                    
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"User '{user.FullName}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(e => e.UserId == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("ManageUsers");
            }
            return View(user);
        }

        // GET: HR/ManageLecturers (Legacy - kept for backward compatibility)
        public async Task<IActionResult> ManageLecturers()
        {
            var accessCheck = CheckHRAccess();
            if (accessCheck != null) return accessCheck;
            
            var lecturers = await _context.Users.Where(u => u.Role == "Lecturer").ToListAsync();
            return View(lecturers);
        }

        // GET: HR/CreateLecturer (Legacy - kept for backward compatibility)
        public IActionResult CreateLecturer()
        {
            var accessCheck = CheckHRAccess();
            if (accessCheck != null) return accessCheck;
            return View();
        }

        // POST: HR/CreateLecturer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLecturer(User user)
        {
            var accessCheck = CheckHRAccess();
            if (accessCheck != null) return accessCheck;
            
            user.Role = "Lecturer";
            
            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManageLecturers");
            }
            return View(user);
        }

        // GET: HR/EditLecturer/5 (Legacy - kept for backward compatibility)
        public async Task<IActionResult> EditLecturer(int? id)
        {
            var accessCheck = CheckHRAccess();
            if (accessCheck != null) return accessCheck;
            
            if (id == null) return NotFound();
            
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Role != "Lecturer") return NotFound();
            
            return View(user);
        }

        // POST: HR/EditLecturer/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLecturer(int id, User user)
        {
            var accessCheck = CheckHRAccess();
            if (accessCheck != null) return accessCheck;
            
            if (id != user.UserId) return NotFound();
            
            user.Role = "Lecturer";
            
            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(id);
                    if (existingUser == null) return NotFound();
                    
                    existingUser.FullName = user.FullName;
                    existingUser.Email = user.Email;
                    existingUser.Phone = user.Phone;
                    existingUser.HourlyRate = user.HourlyRate;
                    existingUser.Username = user.Username;
                    existingUser.Password = user.Password;
                    
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(e => e.UserId == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("ManageLecturers");
            }
            return View(user);
        }

        // GET: HR/Reports
        public async Task<IActionResult> Reports(int? lecturerId, DateTime? fromDate, DateTime? toDate)
        {
            var accessCheck = CheckHRAccess();
            if (accessCheck != null) return accessCheck;

            var claimsQuery = _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.Status == "Approved");

            if (lecturerId.HasValue)
            {
                claimsQuery = claimsQuery.Where(c => c.LecturerId == lecturerId.Value);
            }
            if (fromDate.HasValue)
            {
                claimsQuery = claimsQuery.Where(c => c.StatusUpdateDate >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                claimsQuery = claimsQuery.Where(c => c.StatusUpdateDate <= toDate.Value);
            }

            var approvedClaims = await claimsQuery
                .OrderByDescending(c => c.StatusUpdateDate)
                .ToListAsync();

            // For lecturer filter dropdown
            var lecturers = await _context.Users
                .Where(u => u.Role == "Lecturer")
                .OrderBy(u => u.FullName)
                .ToListAsync();
            ViewBag.Lecturers = lecturers;
            ViewBag.SelectedLecturerId = lecturerId;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View(approvedClaims);
        }

        // GET: HR/DownloadReportPdf
        public async Task<IActionResult> DownloadReportPdf(int? lecturerId, DateTime? fromDate, DateTime? toDate)
        {
            var accessCheck = CheckHRAccess();
            if (accessCheck != null) return accessCheck;

            var claimsQuery = _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.Status == "Approved");

            if (lecturerId.HasValue)
            {
                claimsQuery = claimsQuery.Where(c => c.LecturerId == lecturerId.Value);
            }
            if (fromDate.HasValue)
            {
                claimsQuery = claimsQuery.Where(c => c.StatusUpdateDate >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                claimsQuery = claimsQuery.Where(c => c.StatusUpdateDate <= toDate.Value);
            }

            var claims = await claimsQuery.OrderByDescending(c => c.StatusUpdateDate).ToListAsync();

            // Build HTML for PDF
            var sb = new StringBuilder();
            sb.AppendLine("<html><head><style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
            sb.AppendLine("h2 { color: #333; border-bottom: 2px solid #007bff; padding-bottom: 10px; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
            sb.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            sb.AppendLine("th { background-color: #007bff; color: white; font-weight: bold; }");
            sb.AppendLine("tr:nth-child(even) { background-color: #f2f2f2; }");
            sb.AppendLine(".summary { background-color: #e7f3ff; padding: 15px; border-radius: 5px; margin-bottom: 20px; }");
            sb.AppendLine(".summary strong { color: #007bff; }");
            sb.AppendLine("</style></head><body>");
            
            sb.AppendLine("<h2>Approved Claims Report</h2>");
            
            // Add filter information
            if (lecturerId.HasValue || fromDate.HasValue || toDate.HasValue)
            {
                sb.AppendLine("<div class='summary'>");
                sb.AppendLine("<p><strong>Filters Applied:</strong></p>");
                if (lecturerId.HasValue)
                {
                    var lecturer = await _context.Users.FindAsync(lecturerId.Value);
                    sb.AppendLine($"<p>Lecturer: {lecturer?.FullName}</p>");
                }
                if (fromDate.HasValue)
                {
                    sb.AppendLine($"<p>From Date: {fromDate.Value:yyyy-MM-dd}</p>");
                }
                if (toDate.HasValue)
                {
                    sb.AppendLine($"<p>To Date: {toDate.Value:yyyy-MM-dd}</p>");
                }
                sb.AppendLine("</div>");
            }
            
            sb.AppendLine("<div class='summary'>");
            sb.AppendLine($"<p><strong>Total Claims:</strong> {claims.Count} | <strong>Total Payment Amount:</strong> {claims.Sum(c => c.TotalAmount):C}</p>");
            sb.AppendLine("</div>");
            
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr>");
            sb.AppendLine("<th>Claim ID</th><th>Lecturer</th><th>Hours Worked</th><th>Hourly Rate</th><th>Total Amount</th><th>Submission Date</th><th>Approval Date</th><th>Manager 1</th><th>Manager 2</th>");
            sb.AppendLine("</tr></thead><tbody>");
            
            foreach (var claim in claims)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{claim.ClaimId.ToString().Substring(0, 8)}</td>");
                sb.AppendLine($"<td>{claim.Lecturer?.FullName}</td>");
                sb.AppendLine($"<td>{claim.HoursWorked}</td>");
                sb.AppendLine($"<td>{claim.HourlyRate:C}</td>");
                sb.AppendLine($"<td><strong>{claim.TotalAmount:C}</strong></td>");
                sb.AppendLine($"<td>{claim.SubmissionDate:yyyy-MM-dd}</td>");
                sb.AppendLine($"<td>{claim.StatusUpdateDate:yyyy-MM-dd}</td>");
                sb.AppendLine($"<td>{(claim.Manager1Approved ? "✓" : "✗")} {claim.Manager1ApprovalDate?.ToString("yyyy-MM-dd")}</td>");
                sb.AppendLine($"<td>{(claim.Manager2Approved ? "✓" : "✗")} {claim.Manager2ApprovalDate?.ToString("yyyy-MM-dd")}</td>");
                sb.AppendLine("</tr>");
            }
            
            sb.AppendLine("</tbody></table>");
            sb.AppendLine($"<p style='margin-top: 30px; color: #666; font-size: 12px;'>Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine("</body></html>");

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Landscape,
                    Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 }
                },
                Objects = { new ObjectSettings { HtmlContent = sb.ToString() } }
            };
            
            var pdf = _pdfConverter.Convert(doc);
            var fileName = $"ApprovedClaimsReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdf, "application/pdf", fileName);
        }
    }
}
