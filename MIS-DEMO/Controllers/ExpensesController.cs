using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MIS_DEMO.Data;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MIS_DEMO.Models;
using System;

namespace MIS_DEMO.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult ExpensesEntry()
        {
            return View();
        }

        // Fetch list of pending expenses ---
        [HttpGet]
        public async Task<IActionResult> GetPendingList()
        {
            // Get the current logged-in user from the Session
            var currentUser = HttpContext.Session.GetString("Username");

            var pendingExpenses = await _context.ExpenseMasters
                .Where(e => e.Status == "Pending" && e.Username == currentUser) // Added filter here
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();

            return Json(pendingExpenses);
        }

        // Fetch a single expense by ID ---
        [HttpGet]
        public async Task<IActionResult> GetExpense(int id)
        {
            var expense = await _context.ExpenseMasters.FindAsync(id);
            if (expense == null) return NotFound();
            return Json(expense);
        }

        // UPDATED: Handle both Insert and Update ---
        [HttpPost]
        public async Task<IActionResult> SaveExpense([FromForm] ExpenseMaster expense, IFormFile fileUpload)
        {
            try
            {
                string newDocumentPath = null;

                if (fileUpload != null && fileUpload.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "expenses");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileUpload.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileUpload.CopyToAsync(fileStream);
                    }
                    newDocumentPath = $"/uploads/expenses/{uniqueFileName}";
                }

                if (expense.Id == 0)
                {
                    // INSERT NEW RECORD
                    expense.Status = "Pending";

                    // --- NEW: Attach the user to the record behind the scenes ---
                    expense.Username = HttpContext.Session.GetString("Username");
                    expense.RepCode = HttpContext.Session.GetString("SalesRepCode");

                    if (newDocumentPath != null) expense.DocumentPath = newDocumentPath;

                    _context.ExpenseMasters.Add(expense);
                }
                else
                {
                    // UPDATE EXISTING RECORD
                    var existingExpense = await _context.ExpenseMasters.FindAsync(expense.Id);
                    if (existingExpense == null) return Json(new { success = false, message = "Record not found." });

                    // Security check: ensure the user editing is the user who owns it
                    var currentUser = HttpContext.Session.GetString("Username");
                    if (existingExpense.Username != currentUser)
                    {
                        return Json(new { success = false, message = "Unauthorized to edit this record." });
                    }

                    existingExpense.ExpenseDate = expense.ExpenseDate;
                    existingExpense.TranNo = expense.TranNo;
                    existingExpense.ExpenseType = expense.ExpenseType;
                    existingExpense.Amount = expense.Amount;
                    existingExpense.Remark = expense.Remark;

                    if (newDocumentPath != null)
                    {
                        existingExpense.DocumentPath = newDocumentPath;
                    }

                    _context.Update(existingExpense);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Expense saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving expense: " + ex.Message });
            }
        }

        // Delete a single expense ---
        [HttpPost]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            try
            {
                var expense = await _context.ExpenseMasters.FindAsync(id);
                if (expense == null)
                {
                    return Json(new { success = false, message = "Record not found." });
                }

                if (!string.IsNullOrEmpty(expense.DocumentPath))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", expense.DocumentPath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.ExpenseMasters.Remove(expense);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Expense deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting expense: " + ex.Message });
            }
        }
    }
}