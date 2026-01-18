using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIS_DEMO.Data;
using MIS_DEMO.Models;
using MIS_DEMO.Models.ViewModels;
using System.Diagnostics;

namespace MIS_DEMO.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userName = HttpContext.Session.GetString("Username");
            var userType = HttpContext.Session.GetString("UserType");
            var salesRepCode = HttpContext.Session.GetString("SalesRepCode");

            var model = new DashboardViewModel();

            //var today = new DateTime(2021, 7, 31);
            //var today = DateTime.Today;

            if (userType == "REP")
            {
                model.TodayTotalSales = _context.VW_SALES_FACT
                    .AsNoTracking()
                    .Where(x =>
                        x.SalesRepCode == salesRepCode)
                    .Sum(x => (decimal?)x.LineTotal) ?? 0;
                //model.TodayTotalSales = _context.VW_SALES_FACT
                //    .AsNoTracking()
                //    .Where(x =>
                //        x.SalesRepCode == salesRepCode &&
                //        x.RefDate >= today &&
                //        x.RefDate < today.AddDays(1))
                //    .Sum(x => (decimal?)x.LineTotal) ?? 0;
            }

            else if (userType == "ASM")
            {
                // Step 1: Check if user is actually an SM
                var isSM = _context.WKF_MAP_SM_ASM
                    .AsNoTracking()
                    .Any(x => x.UserNameSM == userName);

                if (isSM)
                {
                    // SM logic
                    // 1. Get all ASMs assigned to this SM
                    var assignedASMs = _context.WKF_MAP_SM_ASM
                        .AsNoTracking()
                        .Where(x => x.UserNameSM == userName)
                        .Select(x => x.UserNameASM)
                        .ToList();

                    // Get ASM rep codes
                    var asmRepCodes = _context.WKF_USER_REP_MAP
                        .AsNoTracking()
                        .Where(x => assignedASMs.Contains(x.UserName))
                        .Select(x => x.SalesRepCode)
                        .ToList();

                    // 2. Get all REP codes under these ASMs
                    var repRepCodes = _context.WKF_MAP_REP_ASM
                        .AsNoTracking()
                        .Where(x => assignedASMs.Contains(x.UserName))
                        .Select(x => x.SalesRepCode)
                        .ToList();

                    var allRepCodes = asmRepCodes
                        .Union(repRepCodes)
                        .ToList();

                    if (!string.IsNullOrEmpty(salesRepCode))
                    {
                        allRepCodes.Add(salesRepCode);
                    }

                    // 4. Sum all sales
                    model.TodayTotalSales = _context.VW_SALES_FACT
                        .AsNoTracking()
                        .Where(
                            x => allRepCodes.Contains(x.SalesRepCode))
                        .Sum(x => (decimal?)x.LineTotal) ?? 0;
                }
                else
                {
                    // ASM logic (existing)
                    var repCodes = _context.WKF_MAP_REP_ASM
                        .AsNoTracking()
                        .Where(x => x.UserName == userName)
                        .Select(x => x.SalesRepCode)
                        .ToList();

                    // Add ASM's own salesRepCode
                    if (!string.IsNullOrEmpty(salesRepCode) && !repCodes.Contains(salesRepCode))
                    {
                        repCodes.Add(salesRepCode);
                    }

                    // Sum sales for ASM + assigned REPs
                    model.TodayTotalSales = _context.VW_SALES_FACT
                        .AsNoTracking()
                        .Where(
                            x => repCodes.Contains(x.SalesRepCode))
                        .Sum(x => (decimal?)x.LineTotal) ?? 0;
                }

            }

            return View(model);
        }

    }
}
