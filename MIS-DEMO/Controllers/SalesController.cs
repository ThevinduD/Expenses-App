using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIS_DEMO.Data;
using MIS_DEMO.Models.ViewModels;
using MIS_DEMO.Services;

namespace MIS_DEMO.Controllers
{
    public class SalesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SalesAccessService _salesAccessService;

        public SalesController(
            AppDbContext context,
            SalesAccessService salesAccessService)
        {
            _context = context;
            _salesAccessService = salesAccessService;
        }

        public IActionResult Details()
        {
            // 1. Session check (VERY IMPORTANT)
            var userName = HttpContext.Session.GetString("UserName");
            var userType = HttpContext.Session.GetString("UserType");
            var salesRepCode = HttpContext.Session.GetString("SalesRepCode");

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userType))
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. Get accessible rep codes (REP / ASM / SM logic)
            var repCodes = _salesAccessService.GetAccessibleRepCodes(
                userType,
                userName,
                salesRepCode
            );

            if (!repCodes.Any())
            {
                return View(new SalesSummaryViewModel());
            }

            // 3. Date ranges
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            var thisMonthStart = new DateTime(today.Year, today.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = thisMonthStart.AddDays(-1);

            // 4. Base query
            var salesQuery = _context.VW_SALES_FACT
                .AsNoTracking()
                .Where(x => repCodes.Contains(x.SalesRepCode));

            // 5. Build ViewModel
            var model = new SalesSummaryViewModel
            {
                TodaySales = salesQuery
                    .Where(x => x.RefDate >= today && x.RefDate < today.AddDays(1))
                    .Sum(x => (decimal?)x.LineTotal) ?? 0,

                YesterdaySales = salesQuery
                    .Where(x => x.RefDate >= yesterday && x.RefDate < today)
                    .Sum(x => (decimal?)x.LineTotal) ?? 0,

                ThisMonthSales = salesQuery
                    .Where(x => x.RefDate >= thisMonthStart)
                    .Sum(x => (decimal?)x.LineTotal) ?? 0,

                LastMonthSales = salesQuery
                    .Where(x => x.RefDate >= lastMonthStart && x.RefDate <= lastMonthEnd)
                    .Sum(x => (decimal?)x.LineTotal) ?? 0
            };

            return View(model);
        }
    }
}
