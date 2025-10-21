using CMCS1.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CMCS.Controllers
{
    public class ClaimsController : Controller
    {
        private static List<Claim> _claims = new List<Claim>();

        [HttpGet]
        public IActionResult UpdateClaim(int id)
        {
            var claim = _claims.FirstOrDefault(c => c.GetHashCode() == id);
            if (claim == null || claim.Status != "Pending")
            {
                return NotFound("The claim is not available for update or does not exist.");
            }

            return View(claim);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateClaim(int id, Claim model, List<IFormFile> SupportingDocuments)
        {
            var existingClaim = _claims.FirstOrDefault(c => c.GetHashCode() == id);
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

            // Handle document uploads
            var uploadsBase = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            Directory.CreateDirectory(uploadsBase);
            var uploadsDate = Path.Combine(uploadsBase, DateTime.Now.ToString("yyyyMMdd"));
            Directory.CreateDirectory(uploadsDate);

            if (SupportingDocuments != null)
            {
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

            return RedirectToAction("TrackClaim", new { id = id });
        }
    }
}
