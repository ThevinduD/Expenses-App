using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIS_DEMO.Data;
using MIS_DEMO.Models.ViewModels;

namespace MIS_DEMO.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAdminUser()
        {
            var u = HttpContext.Session.GetString("Username");
            return u == "IT01" || u == "b";
        }

        // Page 1: list users with #NA# password
        [HttpGet]
        public IActionResult NaPasswords()
        {
            if (!IsAdminUser()) return Forbid();

            var users = _context.USERS
                .AsNoTracking()
                .Where(u => u.Password == "#NA#")
                .OrderBy(u => u.UserName)
                .Select(u => new NaPasswordUserVm
                {
                    UserName = u.UserName,
                    Description = u.Description,
                    Email = u.Email // rename if your field name differs
                })
                .ToList();

            return View(users);
        }

        // Page 2 (optional): separate screen to set password for one user
        [HttpGet]
        public IActionResult SetPassword(string userName)
        {
            if (!IsAdminUser()) return Forbid();
            if (string.IsNullOrWhiteSpace(userName)) return RedirectToAction(nameof(NaPasswords));

            // only allow editing if still #NA#
            var exists = _context.USERS.AsNoTracking().Any(x => x.UserName == userName && x.Password == "#NA#");
            if (!exists) return RedirectToAction(nameof(NaPasswords));

            return View(new SetUserPasswordVm { UserName = userName });
        }

        // Submit: update password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetPassword(SetUserPasswordVm vm)
        {
            if (!IsAdminUser()) return Forbid();

            if (!ModelState.IsValid)
                return View(vm);

            var user = _context.USERS
                .AsTracking()
                .FirstOrDefault(u => u.UserName == vm.UserName);

            if (user == null) return NotFound();

            // Only change if currently #NA#
            if (user.Password != "#NA#")
                return RedirectToAction(nameof(NaPasswords));

            user.Password = vm.NewPassword;

            _context.SaveChanges();

            TempData["Msg"] = $"Password set for {vm.UserName}";
            return RedirectToAction(nameof(NaPasswords));
        }
    }
}
