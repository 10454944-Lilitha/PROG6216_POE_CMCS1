using CMCS1.Models;
using CMCS1.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CMCS1.Controllers
{
    public class ClaimsController : Controller
    {
        private static List<Claim> _claims = new List<Claim>();
        private static List<Feedback> _feedbacks = new List<Feedback>();

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
        public IActionResult Index()
        {
            var roleCheck = CheckRoleAccess();
            if (roleCheck != null) return roleCheck;

            if (!IsLecturer())
            {
                return RedirectToAction("ReviewClaims");
            }

            return View(_claims);
        }

        // GET: Claims/SubmitClaim - Display submit form
        [HttpGet]
        public IActionResult SubmitClaim()
        {
            var roleCheck = CheckRoleAccess();
            if (roleCheck != null) return roleCheck;

            if (!IsLecturer())
            {
                return RedirectToAction("ReviewClaims");
            }

            return View(new ClaimViewModel());
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var claim = new Claim
            {
                ClaimId = Guid.NewGuid(),
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

            _claims.Add(claim);
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
        public IActionResult TrackClaim(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return View(null);
            }

            // Try to parse as Guid first
            if (Guid.TryParse(id, out Guid claimGuid))
            {
                var claim = _claims.FirstOrDefault(c => c.ClaimId == claimGuid);
                return View(claim);
            }

            // Fall back to hash code for backward compatibility
            if (int.TryParse(id, out int hashCode))
            {
                var claim = _claims.FirstOrDefault(c => c.GetHashCode() == hashCode);
                return View(claim);
            }

            return View(null);
        }

        // GET: Claims/UpdateClaim - Display update form
        [HttpGet]
        public IActionResult UpdateClaim(string id)
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
                claim = _claims.FirstOrDefault(c => c.ClaimId == claimGuid);
            }
            // Fall back to hash code for backward compatibility
            else if (int.TryParse(id, out int hashCode))
            {
                claim = _claims.FirstOrDefault(c => c.GetHashCode() == hashCode);
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
                existingClaim = _claims.FirstOrDefault(c => c.ClaimId == claimGuid);
            }
            // Fall back to hash code for backward compatibility
            else if (int.TryParse(id, out int hashCode))
            {
                existingClaim = _claims.FirstOrDefault(c => c.GetHashCode() == hashCode);
            }

            if (existingClaim == null || existingClaim.Status != "Pending")
            {
                return NotFound("The claim is not available for update or does not exist.");
            }

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

            return RedirectToAction("TrackClaim", new { id = existingClaim.ClaimId.ToString() });
        }

        // GET: Claims/ReviewClaims - Display all claims for managers to review
        [HttpGet]
        public IActionResult ReviewClaims()
        {
            var roleCheck = CheckRoleAccess();
            if (roleCheck != null) return roleCheck;

            if (!IsManager())
            {
                return RedirectToAction("Index");
            }

            // Show all claims for managers
            var allClaims = _claims.OrderByDescending(c => c.SubmissionDate).ToList();
            ViewBag.CurrentUser = GetUserRole();
            return View(allClaims);
        }

        // POST: Claims/ApproveClaim - Handle claim approval/rejection with feedback
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveClaim(string id, string action, string feedback)
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
                claim = _claims.FirstOrDefault(c => c.ClaimId == claimGuid);
            }
            // Fall back to hash code for backward compatibility
            else if (int.TryParse(id, out int hashCode))
            {
                claim = _claims.FirstOrDefault(c => c.GetHashCode() == hashCode);
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
                _feedbacks.Add(feedbackEntry);
                claim.FeedbackList.Add(feedbackEntry);
            }

            TempData["SuccessMessage"] = action == "Approve" 
                ? (claim.Status == "Approved" 
                    ? "Claim approved by both managers!" 
                    : $"Claim approved by {currentManager}. Waiting for other manager's approval.")
                : "Claim rejected.";

            return RedirectToAction("ReviewClaims");
        }

        // GET: Claims/Delete - Display delete confirmation
        [HttpGet]
        public IActionResult Delete(string id)
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
                claim = _claims.FirstOrDefault(c => c.ClaimId == claimGuid);
            }
            // Fall back to hash code for backward compatibility
            else if (int.TryParse(id, out int hashCode))
            {
                claim = _claims.FirstOrDefault(c => c.GetHashCode() == hashCode);
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
        public IActionResult DeleteConfirmed(string id)
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
                claim = _claims.FirstOrDefault(c => c.ClaimId == claimGuid);
            }
            // Fall back to hash code for backward compatibility
            else if (int.TryParse(id, out int hashCode))
            {
                claim = _claims.FirstOrDefault(c => c.GetHashCode() == hashCode);
            }

            if (claim != null)
            {
                _claims.Remove(claim);
            }

            return RedirectToAction("Index");
        }
    }
}
