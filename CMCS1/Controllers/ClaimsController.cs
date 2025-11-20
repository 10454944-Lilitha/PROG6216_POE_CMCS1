using CMCS1.Data;
using CMCS1.Models;
using CMCS1.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CMCS1.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly AppDbContext _context;

        public ClaimsController(AppDbContext context)
        {
            _context = context;
        }

        private string GetUserRole()
        {
            return HttpContext.Session.GetString("UserRole") ?? "";
        }

        private bool IsLecturer()
        {
            return GetUserRole() == "Lecturer";
        }

        private bool IsManager()
        {
            var role = GetUserRole();
            return role == "Manager1" || role == "Manager2";
        }

        private IActionResult? CheckRoleAccess()
        {
            var role = GetUserRole();
            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction("SelectRole", "Account");
            }
            return null;
        }

        // GET: Claims/Index - Display all claims
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roleCheck = CheckRoleAccess();
            if (roleCheck != null) return roleCheck;

            if (!IsLecturer())
            {
                return RedirectToAction("ReviewClaims");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var claims = await _context.Claims
                .Where(c => c.LecturerId == userId)
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();

            return View(claims);
        }

        // GET: Claims/SubmitClaim - Display submit form
        [HttpGet]
        public async Task<IActionResult> SubmitClaim()
        {
            var roleCheck = CheckRoleAccess();
            if (roleCheck != null) return roleCheck;

            if (!IsLecturer())
            {
                return RedirectToAction("ReviewClaims");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var user = await _context.Users.FindAsync(userId);

            var model = new ClaimViewModel();
            if (user != null && user.HourlyRate.HasValue)
            {
                model.HourlyRate = user.HourlyRate.Value;
            }

            return View(model);
        }

        // POST: Claims/SubmitClaim - Handle claim submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(ClaimViewModel model, List<IFormFile> SupportingDocuments)
        {
            var roleCheck = CheckRoleAccess();
            if (roleCheck != null) return roleCheck;

            if (!IsLecturer())
            {
                return RedirectToAction("ReviewClaims");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return RedirectToAction("Login", "Account");

            // Auto-set hourly rate
            if (user.HourlyRate.HasValue)
            {
                model.HourlyRate = user.HourlyRate.Value;
            }

            // Re-validate model after setting rate
            ModelState.Clear();
            TryValidateModel(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var claim = new Claim
            {
                ClaimId = Guid.NewGuid(),
                LecturerId = userId.Value,
                HoursWorked = model.HoursWorked,
                HourlyRate = model.HourlyRate,
                Notes = model.Notes,
                Status = "Pending",
                SubmissionDate = DateTime.Now,
                UploadedFileNames = new List<string>()
            };

            // Handle document uploads
            if (SupportingDocuments != null && SupportingDocuments.Any())
            {
                var uploadsBase = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsBase);
                var uploadsDate = Path.Combine(uploadsBase, DateTime.Now.ToString("yyyyMMdd"));
                Directory.CreateDirectory(uploadsDate);

                foreach (var file in SupportingDocuments)
                {
                    if (file.Length > 0)
                    {
                        if (file.Length > 10 * 1024 * 1024)
                        {
                            ModelState.AddModelError("SupportingDocuments", "One or more files exceed 10MB limit.");
                            return View(model);
                        }

                        if (!new[] { ".pdf", ".docx", ".xlsx" }.Contains(Path.GetExtension(file.FileName).ToLower()))
                        {
                            ModelState.AddModelError("SupportingDocuments", "Unsupported file type. Use PDF, DOCX, or XLSX.");
                            return View(model);
                        }

                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine(uploadsDate, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        claim.UploadedFileNames.Add(fileName);
                    }
                }
            }

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();
            return RedirectToAction("Success");
        }

        // GET: Claims/Success - Display success message
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }

        // GET: Claims/TrackClaim - Display claim details
        [HttpGet]
        public async Task<IActionResult> TrackClaim(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return View(null);
            }

            Claim? claim = null;

            // Try to parse as Guid first
            if (Guid.TryParse(id, out Guid claimGuid))
            {
                claim = await _context.Claims
                    .Include(c => c.FeedbackList)
                    .FirstOrDefaultAsync(c => c.ClaimId == claimGuid);
            }

            return View(claim);
        }

        // GET: Claims/UpdateClaim - Display update form
        [HttpGet]
        public async Task<IActionResult> UpdateClaim(string id)
        {
            var roleCheck = CheckRoleAccess();
            if (roleCheck != null) return roleCheck;

            if (!IsLecturer())
            {
                return RedirectToAction("ReviewClaims");
            }

            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Invalid claim ID.");
            }

            Claim? claim = null;

            // Try to parse as Guid first
            if (Guid.TryParse(id, out Guid claimGuid))
            {
                claim = await _context.Claims.FirstOrDefaultAsync(c => c.ClaimId == claimGuid);
            }

            if (claim == null || claim.Status != "Pending")
            {
                return NotFound("The claim is not available for update or does not exist.");
            }

            return View(claim);
        }

        // POST: Claims/UpdateClaim - Handle claim update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateClaim(string id, Claim model, List<IFormFile> SupportingDocuments)
        {
            var roleCheck = CheckRoleAccess();
            if (roleCheck != null) return roleCheck;

            if (!IsLecturer())
            {
                return RedirectToAction("ReviewClaims");
            }

            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Invalid claim ID.");
            }

            Claim? existingClaim = null;

            // Try to parse as Guid first
            if (Guid.TryParse(id, out Guid claimGuid))
            {
                existingClaim = await _context.Claims.FirstOrDefaultAsync(c => c.ClaimId == claimGuid);
            }

            if (existingClaim == null || existingClaim.Status != "Pending")
            {
                return NotFound("The claim is not available for update or does not exist.");
            }

            // Auto-set hourly rate from user profile to prevent tampering
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = await _context.Users.FindAsync(userId);
            if (user != null && user.HourlyRate.HasValue)
            {
                model.HourlyRate = user.HourlyRate.Value;
            }
            
            // Re-validate model
            ModelState.Clear();
            TryValidateModel(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Update claim
            existingClaim.HoursWorked = model.HoursWorked;
            existingClaim.HourlyRate = model.HourlyRate;
            existingClaim.Notes = model.Notes;
            existingClaim.StatusUpdateDate = DateTime.Now;

            // Handle document uploads
            if (SupportingDocuments != null && SupportingDocuments.Any())
            {
                var uploadsBase = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsBase);
                var uploadsDate = Path.Combine(uploadsBase, DateTime.Now.ToString("yyyyMMdd"));
                Directory.CreateDirectory(uploadsDate);

                foreach (var file in SupportingDocuments)
                {
                    if (file.Length > 0)
                    {
                        if (file.Length > 10 * 1024 * 1024)
                        {
                            ModelState.AddModelError("SupportingDocuments", "One or more files exceed 10MB limit.");
                            return View(model);
                        }

                        if (!new[] { ".pdf", ".docx", ".xlsx" }.Contains(Path.GetExtension(file.FileName).ToLower()))
                        {
                            ModelState.AddModelError("SupportingDocuments", "Unsupported file type. Use PDF, DOCX, or XLSX.");
                            return View(model);
                        }

                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine(uploadsDate, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        existingClaim.UploadedFileNames.Add(fileName);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("TrackClaim", new { id = existingClaim.ClaimId.ToString() });
        }

        // GET: Claims/ReviewClaims - Display all claims for managers to review
        [HttpGet]
        public async Task<IActionResult> ReviewClaims()
        {
            var roleCheck = CheckRoleAccess();
            if (roleCheck != null) return roleCheck;

            if (!IsManager())
            {
                return RedirectToAction("Index");
            }

            // Show all claims for managers
            var allClaims = await _context.Claims
                .Include(c => c.Lecturer)
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();
                
            ViewBag.CurrentUser = GetUserRole();
            return View(allClaims);
        }

        // POST: Claims/ApproveClaim - Handle claim approval/rejection with feedback
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClaim(string id, string action, string feedback)
        {
            var roleCheck = CheckRoleAccess();
            if (roleCheck != null) return roleCheck;

            if (!IsManager())
            {
                return RedirectToAction("Index");
            }
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Invalid claim ID.");
            }

            Claim? claim = null;

            // Try to parse as Guid first
            if (Guid.TryParse(id, out Guid claimGuid))
            {
                claim = await _context.Claims
                    .Include(c => c.FeedbackList)
                    .FirstOrDefaultAsync(c => c.ClaimId == claimGuid);
            }

            if (claim == null)
            {
                return NotFound("Claim not found.");
            }

            var currentManager = GetUserRole();

            if (action == "Reject")
            {
                // If either manager rejects, claim is rejected
                claim.Status = "Rejected";
                claim.StatusUpdateDate = DateTime.Now;
            }
            else if (action == "Approve")
            {
                // Track which manager approved
                if (currentManager == "Manager1")
                {
                    claim.Manager1Approved = true;
                    claim.Manager1ApprovalDate = DateTime.Now;
                }
                else if (currentManager == "Manager2")
                {
                    claim.Manager2Approved = true;
                    claim.Manager2ApprovalDate = DateTime.Now;
                }

                // Check if both managers have approved
                if (claim.Manager1Approved && claim.Manager2Approved)
                {
                    claim.Status = "Approved";
                    claim.StatusUpdateDate = DateTime.Now;
                }
                else
                {
                    // Partially approved - waiting for other manager
                    claim.Status = "Pending Approval";
                    claim.StatusUpdateDate = DateTime.Now;
                }
            }

            // Add feedback if provided
            if (!string.IsNullOrWhiteSpace(feedback))
            {
                var feedbackEntry = new Feedback
                {
                    FeedbackId = Guid.NewGuid(),
                    ClaimId = claim.ClaimId,
                    Message = feedback,
                    ReviewedBy = currentManager,
                    ReviewDate = DateTime.Now
                };
                _context.Feedbacks.Add(feedbackEntry);
                claim.FeedbackList.Add(feedbackEntry);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = action == "Approve" 
                ? (claim.Status == "Approved" 
                    ? "Claim approved by both managers!" 
                    : $"Claim approved by {currentManager}. Waiting for other manager's approval.")
                : "Claim rejected.";

            return RedirectToAction("ReviewClaims");
        }

        // GET: Claims/Delete - Display delete confirmation
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var roleCheck = CheckRoleAccess();
            if (roleCheck != null) return roleCheck;

            if (!IsLecturer())
            {
                return RedirectToAction("ReviewClaims");
            }

            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Invalid claim ID.");
            }

            Claim? claim = null;

            // Try to parse as Guid first
            if (Guid.TryParse(id, out Guid claimGuid))
            {
                claim = await _context.Claims.FirstOrDefaultAsync(c => c.ClaimId == claimGuid);
            }

            if (claim == null)
            {
                return NotFound("Claim not found.");
            }

            return View(claim);
        }

        // POST: Claims/DeleteConfirmed - Handle claim deletion
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var roleCheck = CheckRoleAccess();
            if (roleCheck != null) return roleCheck;

            if (!IsLecturer())
            {
                return RedirectToAction("ReviewClaims");
            }

            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Invalid claim ID.");
            }

            Claim? claim = null;

            // Try to parse as Guid first
            if (Guid.TryParse(id, out Guid claimGuid))
            {
                claim = await _context.Claims.FirstOrDefaultAsync(c => c.ClaimId == claimGuid);
            }

            if (claim != null)
            {
                _context.Claims.Remove(claim);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
